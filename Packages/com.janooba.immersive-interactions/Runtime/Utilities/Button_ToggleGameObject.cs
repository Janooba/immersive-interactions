
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Button_ToggleGameObject : UdonSharpBehaviour
{
    public GameObject gameObjectToToggle;
    public bool networkSync = false;
    
    [UdonSynced]
    public bool isActive;

    private bool _startState;

    private void Start()
    {
        _startState = isActive;
        gameObjectToToggle.SetActive(isActive);
    }

    public override void OnDeserialization()
    {
        if (networkSync)
            gameObjectToToggle.SetActive(isActive);
    }

    public void Toggle()
    {
        gameObjectToToggle.SetActive(!gameObjectToToggle.activeSelf);
        isActive = gameObjectToToggle.activeSelf;
        if (networkSync) RequestSerialization();
    }
    
    public void Toggle_On()
    {
        gameObjectToToggle.SetActive(!_startState);
        isActive = !_startState;
        if (networkSync) RequestSerialization();
    }
    
    public void Toggle_Off()
    {
        gameObjectToToggle.SetActive(_startState);
        isActive = _startState;
        if (networkSync) RequestSerialization();
    }
}