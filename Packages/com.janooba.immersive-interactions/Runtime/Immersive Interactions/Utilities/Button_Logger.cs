
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Button_Logger : UdonSharpBehaviour
{
    public UdonBehaviour button;

    [Header("-- UI --")] 
    public TextMeshProUGUI displayText;
    
    public void Update()
    {
        if (Networking.LocalPlayer == null || !Networking.LocalPlayer.IsValid()) return;
        if (!button) return;
        
        string text = "";
        bool isLocked = (bool)button.GetProgramVariable("isLocked");
        bool isToggled = (bool)button.GetProgramVariable("_isToggled");
        float progress = (float)button.GetProgramVariable("_currentUnitProgress");
        bool isSleeping = (bool)button.GetProgramVariable("_sleeping");
        int idleFrames = (int)button.GetProgramVariable("_framesIdle");
        float penetration = (int)button.GetProgramVariable("_penetration");
        var owner = Networking.GetOwner(button.gameObject);

        text += $"Is Locked: {isLocked}\n";
        text += $"Is Toggled: {isToggled}\n";
        text += $"Progress: {progress}\n";
        text += $"Sleeping: {isSleeping}\n";
        text += $"Frames Idle: {idleFrames}\n";
        text += $"Penetration: {penetration}\n";
        text += $"Owner: {owner.playerId} {owner.displayName}";

        displayText.text = text;
    }
}
