using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GUIController : MonoBehaviour
{
    World world;
    FlyCam flyCam;
    public List<GameObject> collapses;
    public GameObject collapseHolder;
    public GameObject uiHolder;

    private int collapseCount;
    private void Awake()
    {

        world = GameObject.Find("World Holder").GetComponent<World>();
        flyCam = GameObject.Find("player").GetComponent<FlyCam>();
        RegisterLisners();
        foreach (var go in collapses)
        {
            Button b = go.GetComponent<Button>();
            b.onClick.AddListener(CollapseCall);
            go.transform.parent.GetChild(1).transform.gameObject.SetActive(false);
        }
        collapseCount = collapses.Count;
    }

    private void Update()
    {
        if (flyCam.inFly)
        {
            uiHolder.SetActive(false);
        }
        else
        {
            uiHolder.SetActive(true);
        }
    }


    void RegisterLisners()
    {
        Slider temp;
        InputField field;
        Toggle toggle;
        // General
        
        temp = GameObject.Find("MIN_H").GetComponent<Slider>();
        temp.value = world.surficeSettings.minHeight;
        temp.onValueChanged.AddListener((v) => { world.SetMinH(Mathf.CeilToInt(v)); });
        temp = GameObject.Find("MAX_H").GetComponent<Slider>();
        temp.value = world.surficeSettings.maxHeight;
        temp.onValueChanged.AddListener((v) => { world.SetMaxH(Mathf.CeilToInt(v)); });
        toggle = GameObject.Find("TRACK").GetComponent<Toggle>();
        toggle.isOn = world.trackPlayer;
        toggle.onValueChanged.AddListener((b) => { world.SetTrack(b); });
        toggle = GameObject.Find("INTERPOLATE").GetComponent<Toggle>();
        toggle.isOn = world.marchCubeSettings.interpolate;
        toggle.onValueChanged.AddListener((b) => { world.SetInterploate(b); });
        toggle = GameObject.Find("SHADING").GetComponent<Toggle>();
        toggle.isOn = world.shaderSettings.useFlatShading;
        toggle.onValueChanged.AddListener((b) => { world.SetShading(b); });
        
        // Chunk settings
        temp = GameObject.Find("CHUNK_SIZE").GetComponent<Slider>();
        temp.value = world.numberOfVoxelsPerAxis;
        temp.onValueChanged.AddListener((v) => {world.SetChunkSize(Mathf.CeilToInt(v));});
        temp = GameObject.Find("RENDER_DISANCE").GetComponent<Slider>();
        temp.value = world.renderingDistance;
        temp.onValueChanged.AddListener((v) => {world.SetRenderDistance(Mathf.CeilToInt(v));});
        
        // Surf noise settings
        field = GameObject.Find("NUM_OF_L").GetComponent<InputField>();
        field.text = world.surficeSettings.numberOfLayers.ToString();
        field.onEndEdit.AddListener((s) => { world.SetNumbOfSurfLayers(int.Parse(s)); });
        temp = GameObject.Find("BASE_FREQ_SURF").GetComponent<Slider>();
        temp.value = world.surficeSettings.baseFrequency;
        temp.onValueChanged.AddListener((v) => { world.SetBaseFreqSurf(v); }); 
        temp = GameObject.Find("BASE_AMPL_SURF").GetComponent<Slider>();
        temp.value = world.surficeSettings.baseAmplitude;
        temp.onValueChanged.AddListener((v) => { world.SetBaseAmplSurf(v); });
        temp = GameObject.Find("MULT_FREQ_SURF").GetComponent<Slider>();
        temp.value = world.surficeSettings.frequencyMultiplier;
        temp.onValueChanged.AddListener((v) => { world.SetMultFreqSurf(v); });
        temp = GameObject.Find("MULT_AMPL_SURF").GetComponent<Slider>();
        temp.value = world.surficeSettings.amplitudeMultiplier;
        temp.onValueChanged.AddListener((v) => { world.SetMultAmplSurf(v); });
        
        // Surf noise settings
        field = GameObject.Find("NUM_OF_L_C").GetComponent<InputField>();
        field.text = world.caveSettings.numberOfLayers.ToString();
        field.onEndEdit.AddListener((s) => { world.SetNumbOfCaveLayers(int.Parse(s)); });
        temp = GameObject.Find("BASE_FREQ_CAVE").GetComponent<Slider>();
        temp.value = world.caveSettings.baseFrequency;
        temp.onValueChanged.AddListener((v) => { world.SetBaseFreqCave(v); });
        temp = GameObject.Find("BASE_AMPL_CAVE").GetComponent<Slider>();
        temp.value = world.caveSettings.baseAmplitude;
        temp.onValueChanged.AddListener((v) => { world.SetBaseAmplCave(v); });
        temp = GameObject.Find("MULT_FREQ_CAVE").GetComponent<Slider>();
        temp.value = world.caveSettings.frequencyMultiplier;
        temp.onValueChanged.AddListener((v) => { world.SetMultFreqCave(v); });
        temp = GameObject.Find("MULT_AMPL_CAVE").GetComponent<Slider>();
        temp.value = world.caveSettings.amplitudeMultiplier;
        temp.onValueChanged.AddListener((v) => { world.SetMultAmplCave(v); });
        // Brush settings
        temp = GameObject.Find("BRUSH_SIZE").GetComponent<Slider>();
        temp.value = world.brushSettings.BrushRadius;
        temp.onValueChanged.AddListener((v) => { world.SetBrushSize(v); });
        temp = GameObject.Find("BRUSH_STR").GetComponent<Slider>();
        temp.value = world.brushSettings.brushStrength;
        temp.onValueChanged.AddListener((v) => { world.SetBrushStrength(v); });
        

        //COLOR settings
        GetColorSettings(GameObject.Find("TER_COLOR").GetComponent<InputField>());
        //BTNS
        Button b = GameObject.Find("EXIT").GetComponent<Button>();
        b.onClick.AddListener(() => { Application.Quit(); }); 
        b = GameObject.Find("RESET").GetComponent<Button>();
        b.onClick.AddListener(() => { SceneManager.LoadScene(SceneManager.GetActiveScene().name); });
    }

    void CollapseCall()
    {
        int c = 0;
        string name = EventSystem.current.currentSelectedGameObject.name;
        bool found = false;
        foreach (Transform ch in collapseHolder.transform)
        {
            if (ch.gameObject.name == "BTNS") continue;
            if(name == ch.gameObject.transform.GetChild(0).name)
            {
                found = true;
                ch.gameObject.transform.GetChild(1).gameObject.SetActive(true);
                Text t = ch.gameObject.transform.GetChild(0).gameObject.GetComponent<Text>();
                t.text = "="+t.text.Substring(1);
                RectTransform rt = ch.gameObject.GetComponent<RectTransform>();
                rt.offsetMin = new Vector2(rt.offsetMin.x, 0);
                rt.offsetMax = new Vector2(rt.offsetMax.x, -c * 50);
                continue;
            }
            else
            {
                ch.gameObject.transform.GetChild(1).gameObject.SetActive(false);
                Text t = ch.gameObject.transform.GetChild(0).gameObject.GetComponent<Text>();
                t.text = "+" + t.text.Substring(1);
                if (found)
                {

                    RectTransform rt = ch.gameObject.GetComponent<RectTransform>();
                    rt.offsetMin = new Vector2(rt.offsetMin.x, 0);
                    rt.offsetMax = new Vector2(rt.offsetMax.x, -975 + (-c + collapseCount-1) * 50);
                }
                else
                {
                    RectTransform rt = ch.gameObject.GetComponent<RectTransform>();
                    rt.offsetMin = new Vector2(rt.offsetMin.x, 0);
                    rt.offsetMax = new Vector2(rt.offsetMax.x, - c * 50);
                }
            }
            c++;

            print(ch.gameObject.transform.GetChild(0).name);
        }
    }
    public void GetColorSettings(InputField txt)
    {
        txt.text = world.shaderSettings.gradToString();
        txt.onEndEdit.AddListener((t) => { world.setTerColor(t); });
    }
}
