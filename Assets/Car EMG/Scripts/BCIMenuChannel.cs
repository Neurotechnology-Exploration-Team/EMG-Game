using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BCIMenuChannel : MonoBehaviour
{
    public GameObject bciMenuCanvas;
    private BCIMenuI bciMenuI;
    
    public Slider slider;
    public Slider bar;
    public double barMax;
    public GameObject advanced;
    public TextMeshProUGUI debugOne;
    public TextMeshProUGUI debugTwo;
    public TextMeshProUGUI debugThree;
    public GameObject parameter;
    public Dropdown keybind;

    private void Start()
    {
        bciMenuI = bciMenuCanvas.GetComponent<BCIMenuI>();
    }

    public void ResetAllSliders()
    {
        bciMenuI.ResetThresholdSliders();
    }

    public void SetAllKeybinds()
    {
        bciMenuI.SetAllKeybinds();
    }

    public void SetAllThresholdParameters()
    {
        bciMenuI.SetAllThresholdParamters();
    }
}
