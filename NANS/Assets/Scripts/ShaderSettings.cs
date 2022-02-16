using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShaderSettings
{
    public Gradient gradient;
    public Material material;

    public bool useFlatShading;

    public string gradToString()
    {
        string output = "";
        foreach(var v in gradient.colorKeys)
        {
            output += (((int)(100-System.Math.Round(v.time, 2) * 100)).ToString() + "|" + ColorUtility.ToHtmlStringRGB(v.color)+ "\n");
        }

        return output;
    }

    public void loadStringToGrad(string txt)
    {
        GradientColorKey[] colorKeys = new GradientColorKey[8];
        int i = 0;
        string[] lines = txt.Split(new string[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);
        
        foreach(var line in lines)
        {
            if (i > 7) break;
            string[] pair = line.Split('|');
            if (pair.Length != 2) continue;
            float key = (100 - float.Parse(pair[0])) / 100;
            Color c;
            ColorUtility.TryParseHtmlString("#"+pair[1], out c);
            colorKeys[i++] = new GradientColorKey(c, key);
        }
        gradient.SetKeys(colorKeys, gradient.alphaKeys);
    }
}
