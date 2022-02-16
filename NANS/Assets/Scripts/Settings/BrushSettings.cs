using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BrushSettings
{
    private float brushRadius = 3;
    public float BrushRadius { 
        get { return brushRadius; } 
        set {
            brushRadius = value;
            PreComputeBrushCorners();
            PreComputeLookupBuffer();
        } 
    }
    public int BrushRadiusCeil
    {
        get { return Mathf.CeilToInt(brushRadius); }
        set { }
    }
    public float brushStrength = 0.1f;
    private List<Vector3Int> brushCorners = new List<Vector3Int>();

    private void PreComputeBrushCorners()
    {
        int br = Mathf.CeilToInt(brushRadius) + 1;
        brushCorners.Clear();
        brushCorners.Add(new Vector3Int(-br, -br, -br));
        brushCorners.Add(new Vector3Int(-br, -br, +br));
        brushCorners.Add(new Vector3Int(-br, +br, -br));
        brushCorners.Add(new Vector3Int(-br, +br, +br));
        brushCorners.Add(new Vector3Int(+br, -br, -br));
        brushCorners.Add(new Vector3Int(+br, -br, +br));
        brushCorners.Add(new Vector3Int(+br, +br, -br));
        brushCorners.Add(new Vector3Int(+br, +br, +br));

    }
    private void PreComputeLookupBuffer()
    {
        int br = Mathf.CeilToInt(brushRadius);
        
    }

    public HashSet<Vector3Int> getEffectedChunks(int n, Vector3 point)
    {
        HashSet<Vector3Int> chunks_pos = new HashSet<Vector3Int>();

        foreach(var c in brushCorners)
        {
            chunks_pos.Add(Vector3Int.FloorToInt((point + c) / n));
        }

        return chunks_pos;
    }
}
