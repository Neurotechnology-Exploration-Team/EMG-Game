using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface BCIMenuI
{
    public void Pause();
    public void Resume();

    public List<string> GetKeybindNames();
    public void SetKeybindNames(List<string> keybinds);

    public bool GetInputForKeybind(string keybind);
    public double GetRawInputForKeybind(string keybind);

    public void ResetThresholdSliders();
    public void SetAllKeybinds();
    public void SetAllThresholdParamters();
}
