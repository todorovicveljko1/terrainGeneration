using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    // Start is called before the first frame update
    Dictionary<Vector3Int, Chunk> chunks;

    [Header("References")]
    public ComputeShader NoiseCompute;
    public ComputeShader MarchCubeCompute;
    public ComputeShader BrushCompute;
    public Transform Player;
    [Header("General")]
    public bool trackPlayer = true;
    [Header("Chunk Settings")]
    public int numberOfVoxelsPerAxis = 16;
    public int renderingDistance = 2;

    public SurficeSettings surficeSettings;
    public CaveSettings caveSettings;
    public ShaderSettings shaderSettings;
    public MarchCubeSettings marchCubeSettings;
    public BrushSettings brushSettings;

    public TerrainShading terrainShading = new TerrainShading();

    private int ChunkY = 1;
    private Vector3Int activePosition = new Vector3Int(0, 0, 0);
    private Queue<Vector3Int> chunksToRemove = new Queue<Vector3Int>();
    private Queue<Vector3Int> chunksToGenerate = new Queue<Vector3Int>();
    private Queue<Chunk.ChunkToEdit> chunksToEdit = new Queue<Chunk.ChunkToEdit>();

    void Start()
    {
        this.chunks = new Dictionary<Vector3Int, Chunk>();
        this.brushSettings.BrushRadius = 3;
        this.InitilizeBuffers();
        this.terrainShading.Initialize();
        this.terrainShading.updateSettings(shaderSettings);
        this.ChunkY = Mathf.CeilToInt(this.surficeSettings.maxHeight / this.numberOfVoxelsPerAxis);
        this.ChunksRender();
        while(chunksToGenerate.Count > 0)
        {
            Vector3Int key = this.chunksToGenerate.Dequeue();
            this.ChunkGenerate(this.chunks[key]);
        }
    }
    private void OnDestroy()
    {
        // Clear buffers;
        foreach (Chunk c in chunks.Values)
        {
            c.Dispose();
        }
        chunks.Clear();
    }
    // Update is called once per frame
    void Update()
    {
        if (trackPlayer)
        {
            if (this.activePosition != Vector3Int.FloorToInt(this.Player.position / this.numberOfVoxelsPerAxis))
            {
                this.activePosition = Vector3Int.FloorToInt(this.Player.position / this.numberOfVoxelsPerAxis);
                this.ChunksRender();
            }
        }
        if (chunksToGenerate.Count > 0)
        {
            Vector3Int key = this.chunksToGenerate.Dequeue();
            if (this.chunks.ContainsKey(key) && this.chunks[key].active)
            {
                this.ChunkGenerate(this.chunks[key]);
            }
        }
        while(chunksToEdit.Count > 0)
        {
            this.ChunkEdit(chunksToEdit.Dequeue());
        }
    }
    void ChunksRender()
    {
        foreach(Chunk c in this.chunks.Values)
        {
            c.active = false;
        }
        for (int i =  - this.renderingDistance; i <= this.renderingDistance; i++)
        {
            int Y =Mathf.FloorToInt(Mathf.Sqrt(this.renderingDistance * this.renderingDistance - i * i)-0.001f);
            for (int j = - Y; j <= Y; j++)
            {
                for(int k = 0; k < this.ChunkY; k++) { 
                    Vector3Int v3Int = new Vector3Int(this.activePosition.x+i, k, this.activePosition.z+j);
                    if (this.chunks.ContainsKey(v3Int))
                    {
                        this.chunks[v3Int].active = true;
                    }
                    else
                    {
                        GameObject go = new GameObject("CHUNK_" + v3Int.ToString());
                        go.transform.parent = this.gameObject.transform;
                        Chunk c = new Chunk(v3Int, numberOfVoxelsPerAxis, go);
                        c.setMaterial(terrainShading.shaderSettings.material);
                        this.chunks.Add(v3Int, c);
                        chunksToGenerate.Enqueue(v3Int);
                    }
                    
                }
            }
        }
        foreach (KeyValuePair<Vector3Int, Chunk> pair in this.chunks)
        {
            if (!pair.Value.active)
            {
                this.chunksToRemove.Enqueue(pair.Key);
            }
        }

        while(this.chunksToRemove.Count > 0)
        {
            Vector3Int key = this.chunksToRemove.Dequeue();
            this.chunks[key].Dispose();
            GameObject.Destroy(this.chunks[key].go);
            this.chunks.Remove(key);
        }
        // Temporary for testing
    }
    void ChunkGenerate(Chunk chunk)
    {
        
        NoiseCompute.SetBuffer(0, "DensityBuffer", chunk.DensityBuffer);
        NoiseCompute.SetInt("textureSize", chunk.NumDPPerAxis);
        NoiseCompute.SetVector("chunkCoord", chunk.Coord - new Vector3(1,1,1));
        NoiseCompute.Dispatch(0, Mathf.CeilToInt(chunk.NumDPPerAxis / 8f), Mathf.CeilToInt(chunk.NumDPPerAxis / 8f), 1);
        this.ChunkCreateMesh(chunk);
    }
    void ChunkCreateMesh(Chunk chunk)
    {
        // Build Triangles
        chunk.TriangleBuffer.SetCounterValue(0);
        MarchCubeCompute.SetBuffer(0, "DensityBuffer", chunk.DensityBuffer);
        MarchCubeCompute.SetInt("textureSize", chunk.NumDPPerAxis);
        MarchCubeCompute.SetInt("numberOfVoxels", chunk.numVoxelsPerAxis);
        MarchCubeCompute.SetBool("interpolate", marchCubeSettings.interpolate);
        MarchCubeCompute.SetFloat("isoLevel", marchCubeSettings.isoLevel);
        MarchCubeCompute.SetVector("chunkCoord", chunk.Coord);
        MarchCubeCompute.SetBuffer(0, "triangles", chunk.TriangleBuffer);
        MarchCubeCompute.Dispatch(0, Mathf.CeilToInt(chunk.numVoxelsPerAxis / 4f), Mathf.CeilToInt(chunk.numVoxelsPerAxis / 4f), Mathf.CeilToInt(chunk.numVoxelsPerAxis / 4f));
        chunk.CreateMesh(shaderSettings.useFlatShading);
    }
    void ChunkEdit(Chunk.ChunkToEdit edit)
    {
        if (!this.chunks.ContainsKey(edit.key)) return;
        Chunk c = this.chunks[edit.key];
        int br = this.brushSettings.BrushRadiusCeil;
        BrushCompute.SetBuffer(0, "DensityBuffer", c.DensityBuffer);
        BrushCompute.SetInts("pi", edit.point.x, edit.point.y, edit.point.z);
        BrushCompute.SetBool("remove", edit.remove);
        BrushCompute.Dispatch(0, Mathf.CeilToInt((2*br+1) / 4f), Mathf.CeilToInt((2*br+1) / 4f), Mathf.CeilToInt((2*br+1) / 4f));
        this.ChunkCreateMesh(c);
    }

    void InitilizeBuffers()
    {
        this.SetParamsForNoiseCompute();
        this.SetParamsForBrushCompute();
    }

    void SetParamsForNoiseCompute()
    {
        float maxNoiseH = 0;
        float ba = this.surficeSettings.baseAmplitude;
        for(int i = 0; i< this.surficeSettings.numberOfLayers; i++)
        {
            maxNoiseH += (2 * ba);
            ba *= this.surficeSettings.amplitudeMultiplier;
        }
        this.shaderSettings.material.SetFloat("_MaxHeight", this.surficeSettings.maxHeight);
        this.shaderSettings.material.SetFloat("_MinHeight", this.surficeSettings.minHeight);
        NoiseCompute.SetInt("sNumberOfLayers", this.surficeSettings.numberOfLayers);
        NoiseCompute.SetFloat("sBaseAmplitude", this.surficeSettings.baseAmplitude);
        NoiseCompute.SetFloat("sBaseFrequency", this.surficeSettings.baseFrequency);
        NoiseCompute.SetFloat("sAmplitudeMultiplier", this.surficeSettings.amplitudeMultiplier);
        NoiseCompute.SetFloat("sFrequencyMultiplier", this.surficeSettings.frequencyMultiplier);
        NoiseCompute.SetFloat("sMinHeight", this.surficeSettings.minHeight);
        NoiseCompute.SetFloat("sHeightMult", (this.surficeSettings.maxHeight - this.surficeSettings.minHeight));


        NoiseCompute.SetInt("cNumberOfLayers", this.caveSettings.numberOfLayers);
        NoiseCompute.SetFloat("cBaseAmplitude", this.caveSettings.baseAmplitude);
        NoiseCompute.SetFloat("cBaseFrequency", this.caveSettings.baseFrequency);
        NoiseCompute.SetFloat("cAmplitudeMultiplier", this.caveSettings.amplitudeMultiplier);
        NoiseCompute.SetFloat("cFrequencyMultiplier", this.caveSettings.frequencyMultiplier);
    }
    void SetParamsForBrushCompute()
    {
        BrushCompute.SetFloat("r",this.brushSettings.BrushRadius);
        BrushCompute.SetInt("rc", this.brushSettings.BrushRadiusCeil);
        BrushCompute.SetInt("textureSize", this.numberOfVoxelsPerAxis + 3);
        BrushCompute.SetFloat("bs", this.brushSettings.brushStrength);
    }
    public void RemoveGround(Vector3 point)
    {
        foreach(var ch in brushSettings.getEffectedChunks(this.numberOfVoxelsPerAxis, point))
        {
            if (ch.y < 0 || ch.y >= ChunkY) continue;
            chunksToEdit.Enqueue(new Chunk.ChunkToEdit(ch, Vector3Int.FloorToInt(point - ch * numberOfVoxelsPerAxis), true));
        }
        //print(point);
    }
    public void AddGround(Vector3 point)
    {
        foreach (var ch in brushSettings.getEffectedChunks(this.numberOfVoxelsPerAxis, point))
        {
            if (ch.y < 0 || ch.y >= ChunkY) continue;
            chunksToEdit.Enqueue(new Chunk.ChunkToEdit(ch, Vector3Int.FloorToInt(point - ch * numberOfVoxelsPerAxis), false));
        }
        //print(point);
    }

    // Setters for UI Controller
    public void SetChunkSize(int s)
    {
        if (this.numberOfVoxelsPerAxis == s) return;
        this.numberOfVoxelsPerAxis = s;
        this.ChunkY = Mathf.CeilToInt(this.surficeSettings.maxHeight / this.numberOfVoxelsPerAxis);
        BrushCompute.SetInt("textureSize", this.numberOfVoxelsPerAxis + 3);
        foreach (Chunk c in chunks.Values)
        {
            c.Dispose();
        }
        chunks.Clear();
        ChunksRender();
    }
    private void regenerateChunks()
    {
        foreach (Chunk c in chunks.Values)
        {
            this.ChunkGenerate(c);
        }
    }
    private void regenerateChunkMesh()
    {
        foreach (Chunk c in chunks.Values)
        {
            this.ChunkCreateMesh(c);
        }
    }
    public void SetMinH(int d)
    {
        if (d == this.surficeSettings.minHeight) return;
        this.surficeSettings.minHeight = d;
        this.shaderSettings.material.SetFloat("_MinHeight", this.surficeSettings.minHeight);
        NoiseCompute.SetFloat("sMinHeight", this.surficeSettings.minHeight);
        NoiseCompute.SetFloat("sHeightMult", (this.surficeSettings.maxHeight - this.surficeSettings.minHeight));
        this.regenerateChunks();
    }
    public void SetMaxH(int d)
    {
        if (d == this.surficeSettings.maxHeight) return;
        this.surficeSettings.maxHeight = d;
        this.shaderSettings.material.SetFloat("_MaxHeight", this.surficeSettings.maxHeight);
        NoiseCompute.SetFloat("sHeightMult", (this.surficeSettings.maxHeight - this.surficeSettings.minHeight));
        this.ChunkY = Mathf.CeilToInt(this.surficeSettings.maxHeight / this.numberOfVoxelsPerAxis);
        foreach (Chunk c in chunks.Values)
        {
            c.Dispose();
        }
        chunks.Clear();
        ChunksRender();
    }
    public void SetTrack(bool b)
    {
        if (b == this.trackPlayer) return;
        this.trackPlayer = b;
        ChunksRender();
    }
    public void SetInterploate(bool b)
    {
        if (b == this.marchCubeSettings.interpolate) return;
        this.marchCubeSettings.interpolate = b;
        regenerateChunkMesh();
    }
    public void SetShading(bool b)
    {
        if (b == this.shaderSettings.useFlatShading) return;
        this.shaderSettings.useFlatShading = b;
        this.regenerateChunks();
    }
    public void SetRenderDistance(int d)
    {
        if (this.renderingDistance == d) return;
        this.renderingDistance = d;
        ChunksRender();
    }
    public void SetNumbOfSurfLayers(int s)
    {
        if (this.surficeSettings.numberOfLayers == s) return;
        this.surficeSettings.numberOfLayers = s;
        NoiseCompute.SetInt("sNumberOfLayers", this.surficeSettings.numberOfLayers);
        this.regenerateChunks();
    }
    public void SetBaseFreqSurf(float v)
    {
        if (this.surficeSettings.baseFrequency == v) return;
        this.surficeSettings.baseFrequency = v;
        NoiseCompute.SetFloat("sBaseFrequency", this.surficeSettings.baseFrequency);
        this.regenerateChunks();
    }
    public void SetBaseAmplSurf(float v)
    {
        if (this.surficeSettings.baseAmplitude == v) return;
        this.surficeSettings.baseAmplitude = v;
        NoiseCompute.SetFloat("sBaseAmplitude", this.surficeSettings.baseAmplitude);
        this.regenerateChunks();
    }
    public void SetMultFreqSurf(float v)
    {
        if (this.surficeSettings.frequencyMultiplier == v) return;
        this.surficeSettings.frequencyMultiplier = v;
        NoiseCompute.SetFloat("sFrequencyMultiplier", this.surficeSettings.frequencyMultiplier);
        this.regenerateChunks();
    }
    public void SetMultAmplSurf(float v)
    {
        if (this.surficeSettings.amplitudeMultiplier == v) return;
        this.surficeSettings.amplitudeMultiplier = v;
        NoiseCompute.SetFloat("sAmplitudeMultiplier", this.surficeSettings.amplitudeMultiplier);
        this.regenerateChunks();
    }
    // CAVE
    public void SetNumbOfCaveLayers(int s)
    {
        if (this.caveSettings.numberOfLayers == s) return;
        this.caveSettings.numberOfLayers = s;
        NoiseCompute.SetInt("cNumberOfLayers", this.caveSettings.numberOfLayers);
        this.regenerateChunks();
    }
    public void SetBaseFreqCave(float v)
    {
        if (this.caveSettings.baseFrequency == v) return;
        this.caveSettings.baseFrequency = v;
        NoiseCompute.SetFloat("cBaseFrequency", this.caveSettings.baseFrequency);
        this.regenerateChunks();
    }
    public void SetBaseAmplCave(float v)
    {
        if (this.caveSettings.baseAmplitude == v) return;
        this.caveSettings.baseAmplitude = v;
        NoiseCompute.SetFloat("cBaseAmplitude", this.caveSettings.baseAmplitude);
        this.regenerateChunks();
    }
    public void SetMultFreqCave(float v)
    {
        if (this.caveSettings.frequencyMultiplier == v) return;
        this.caveSettings.frequencyMultiplier = v;
        NoiseCompute.SetFloat("cFrequencyMultiplier", this.caveSettings.frequencyMultiplier);
        this.regenerateChunks();
    }
    public void SetMultAmplCave(float v)
    {
        if (this.caveSettings.amplitudeMultiplier == v) return;
        this.caveSettings.amplitudeMultiplier = v;
        NoiseCompute.SetFloat("cAmplitudeMultiplier", this.caveSettings.amplitudeMultiplier);
        this.regenerateChunks();
    }

    public void SetBrushSize(float s)
    {
        if (this.brushSettings.BrushRadius == s) return;
        this.brushSettings.BrushRadius = s;

        BrushCompute.SetFloat("r", this.brushSettings.BrushRadius);
        BrushCompute.SetInt("rc", this.brushSettings.BrushRadiusCeil);
    }
    public void SetBrushStrength(float s)
    {
        if (this.brushSettings.brushStrength == s) return;
        this.brushSettings.brushStrength = s;
        BrushCompute.SetFloat("bs", this.brushSettings.brushStrength);
    }

    public void setTerColor(string txt)
    {
        this.shaderSettings.loadStringToGrad(txt);
        this.terrainShading.updateSettings(this.shaderSettings);
    }
}
