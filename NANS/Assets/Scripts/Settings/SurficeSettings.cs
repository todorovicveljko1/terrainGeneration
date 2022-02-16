using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SurficeSettings {
    public int numberOfLayers = 4;
    public float baseAmplitude = 1;
    public float baseFrequency = 1;
    public float amplitudeMultiplier = 0.5f;
    public float frequencyMultiplier = 2;

    public float minHeight = 0;
    public float maxHeight = 16;

}
