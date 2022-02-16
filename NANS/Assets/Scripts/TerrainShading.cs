using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainShading
{
    public ShaderSettings shaderSettings;
    public Texture2D texture;
    const int textureResolution = 50;

    public void Initialize()
    {
        if (texture == null)
        {
            texture = new Texture2D(textureResolution, 1, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
        }
    }
    public void updateSettings(ShaderSettings ss)
    {
        shaderSettings = ss;
        this.UpdateColours();
    }
    public void UpdateColours()
    {
        Color[] colours = new Color[textureResolution];
        for (int i = 0; i < textureResolution; i++)
        {
            colours[i] = shaderSettings.gradient.Evaluate(i / (textureResolution - 1f));
        }
        texture.SetPixels(colours);
        texture.Apply();
        shaderSettings.material.SetTexture("_Texture", texture);
    }


}
