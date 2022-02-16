using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UISliderStep : MonoBehaviour
{
    public int stepAmount = 16;
    public GameObject valueDisplay = null;
    Slider mySlider = null;
    int numberOfSteps = 0;

    // Start is called before the first frame update
    void Start()
    {
        mySlider = GetComponent<Slider>();
        if (stepAmount != 0)
        {
            numberOfSteps = (int)mySlider.maxValue / stepAmount;
        }
        mySlider.onValueChanged.AddListener(UpdateStep);
        if (valueDisplay != null)
            valueDisplay.GetComponent<Text>().text = mySlider.value.ToString();
    }

    public void UpdateStep(float value)
    {   
        if(stepAmount != 0) { 
            float range = (mySlider.value / mySlider.maxValue) * numberOfSteps;
            int ceil = Mathf.CeilToInt(range);
            mySlider.value = ceil * stepAmount;
        }
        if (valueDisplay != null)
            valueDisplay.GetComponent<Text>().text = mySlider.value.ToString();
    }
}

