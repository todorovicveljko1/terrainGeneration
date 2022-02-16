using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
	public Vector3Int id;
	public readonly int numVoxelsPerAxis;
	public bool active;

	public Mesh mesh;
	public MeshFilter filter;
	public GameObject go;
	MeshRenderer renderer;
	MeshCollider collider;
	
	// Buffers
	ComputeBuffer densityBuffer;
	ComputeBuffer triangleBuffer;
	ComputeBuffer triangleCountBuffer;

	// Mesh processing
	Dictionary<Vector2Int, int> vertexIndexMap;
	List<Vector3> processedVertices;
	List<Vector3> processedNormals;
	List<int> processedTriangles;

	public Vector3 Coord{ get =>new Vector3(id.x, id.y, id.z) * numVoxelsPerAxis; }
	public int NumDPPerAxis { get => numVoxelsPerAxis + 3; }
	public ComputeBuffer DensityBuffer { get => densityBuffer; set => densityBuffer = value; }
    public ComputeBuffer TriangleBuffer { get => triangleBuffer; set => triangleBuffer = value; }
    public ComputeBuffer TriangleCountBuffer { get => triangleCountBuffer; set => triangleCountBuffer = value; }

    public Chunk(Vector3Int pos, int numVoxels, GameObject meshHolder)
    {
        this.id = pos;
        this.numVoxelsPerAxis = numVoxels;
		this.active = true;

		// Initialize buffers
		int densityPoints = (numVoxels + 3) * (numVoxels + 3) * (numVoxels + 3);
		this.DensityBuffer = new ComputeBuffer(densityPoints, sizeof(float));
		int maxVertexCount = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis * 5;
		this.TriangleBuffer = new ComputeBuffer(maxVertexCount, 32 * 3, ComputeBufferType.Append);
		this.TriangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
		// Create mesh object and add mesh renderer
		this.mesh = new Mesh();
        this.mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
		go = meshHolder;
		filter = meshHolder.AddComponent<MeshFilter>();
        renderer = meshHolder.AddComponent<MeshRenderer>();
		collider = meshHolder.AddComponent<MeshCollider>();

		filter.mesh = mesh;

		// Fields for deduping triangles
		vertexIndexMap = new Dictionary<Vector2Int, int>();
		processedVertices = new List<Vector3>();
		processedNormals = new List<Vector3>();
		processedTriangles = new List<int>();
	}

	public void Dispose()
	{
		// Free GPU Buffers
		collider.sharedMesh = null;
		this.mesh.Clear();
		this.DensityBuffer.Release();
		this.TriangleBuffer.Release();
		this.TriangleCountBuffer.Release();

		vertexIndexMap.Clear();
		processedVertices.Clear();
		processedNormals.Clear();
		processedTriangles.Clear();
	}
	public void setMaterial(Material m)
    {
		renderer.material = m;
	}
    public void CreateMesh(bool useflatShading)
    {
		vertexIndexMap.Clear();
		processedVertices.Clear();
		processedNormals.Clear();
		processedTriangles.Clear(); 

		// Get VertexData from GPU
		int[] vertexCountData = new int[1];
		TriangleCountBuffer.SetData(vertexCountData);
		ComputeBuffer.CopyCount(triangleBuffer, TriangleCountBuffer, 0);

		TriangleCountBuffer.GetData(vertexCountData);
		int numVertices = vertexCountData[0] * 3;
		VertexData[] vertexData = new VertexData[numVertices];

		triangleBuffer.GetData(vertexData, 0, 0, numVertices);
		
		int triangleIndex = 0;
		int save = 0;
		int sharedVertexIndex;
		for (int i = 0; i < numVertices; i++)
		{
			VertexData data = vertexData[i];
			
			if (!useflatShading && vertexIndexMap.TryGetValue(data.id, out sharedVertexIndex))
			{
				processedTriangles.Add(sharedVertexIndex);
				save += 1;
			}
			else
			{
				if(!useflatShading)
					vertexIndexMap.Add(data.id, triangleIndex);
				processedVertices.Add(data.position);
				processedNormals.Add(data.normal);
				processedTriangles.Add(triangleIndex);
				triangleIndex++;
			}
		}
		collider.sharedMesh = null;
		mesh.Clear(false);

		mesh.SetVertices(processedVertices);
		mesh.SetTriangles(processedTriangles, 0, true);
		if (useflatShading)
			mesh.RecalculateNormals();
		else
			mesh.SetNormals(processedNormals);
		collider.sharedMesh = mesh;
		/*
		triangleBuffer.Dispose();
		int maxVertexCount = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis * 5;
		this.TriangleBuffer = new ComputeBuffer(maxVertexCount, 32 * 3, ComputeBufferType.Append);
		*/

	}



	public class ChunkToEdit
    {
		public Vector3Int key;
		public Vector3Int point;
		public bool remove;
		public ChunkToEdit(Vector3Int key, Vector3Int point, bool remove)
        {
			this.key = key;
			this.point = point;
			this.remove = remove;
        }
    }
}
