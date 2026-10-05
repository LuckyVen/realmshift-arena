using Godot;
using System.Collections.Generic;
namespace Realmshift;
public static class InputBindings
{
    public static readonly Dictionary<string,Key> Defaults=new(){{"up",Key.W},{"down",Key.S},{"left",Key.A},{"right",Key.D},{"dash",Key.Space},{"hero",Key.Q},{"interact",Key.E},{"slot1",Key.Key1},{"slot2",Key.Key2},{"pause",Key.Escape}};
    public static void Setup()
    {
        foreach(var kv in Defaults)
        {if(!InputMap.HasAction(kv.Key))InputMap.AddAction(kv.Key);InputMap.ActionEraseEvents(kv.Key);var key=SaveManager.Data.Settings.Keys.TryGetValue(kv.Key,out long code)?(Key)code:kv.Value;InputMap.ActionAddEvent(kv.Key,new InputEventKey{PhysicalKeycode=key});}
        AddMouse("attack",MouseButton.Left);AddMouse("secondary",MouseButton.Right);
        AddAxis("left",JoyAxis.LeftX,-1);AddAxis("right",JoyAxis.LeftX,1);AddAxis("up",JoyAxis.LeftY,-1);AddAxis("down",JoyAxis.LeftY,1);
        AddJoy("dash",JoyButton.A);AddJoy("hero",JoyButton.Y);AddJoy("interact",JoyButton.X);AddJoy("pause",JoyButton.Start);AddJoy("slot1",JoyButton.LeftShoulder);AddJoy("slot2",JoyButton.RightShoulder);
        AddAxis("attack",JoyAxis.TriggerRight,1);AddAxis("secondary",JoyAxis.TriggerLeft,1);
    }
    private static void AddMouse(string s,MouseButton b){if(!InputMap.HasAction(s))InputMap.AddAction(s);InputMap.ActionEraseEvents(s);InputMap.ActionAddEvent(s,new InputEventMouseButton{ButtonIndex=b});}
    private static void AddJoy(string s,JoyButton b)=>InputMap.ActionAddEvent(s,new InputEventJoypadButton{ButtonIndex=b});
    private static void AddAxis(string s,JoyAxis a,float value)=>InputMap.ActionAddEvent(s,new InputEventJoypadMotion{Axis=a,AxisValue=value});
    public static void Rebind(string action,Key key)
    {
        string? duplicate=null;foreach(var kv in Defaults){long current=SaveManager.Data.Settings.Keys.GetValueOrDefault(kv.Key,(long)kv.Value);if(kv.Key!=action&&current==(long)key){duplicate=kv.Key;break;}}
        if(duplicate!=null)SaveManager.Data.Settings.Keys[duplicate]=SaveManager.Data.Settings.Keys.GetValueOrDefault(action,(long)Defaults[action]);
        SaveManager.Data.Settings.Keys[action]=(long)key;Setup();SaveManager.Save();
    }
    public static string Label(string action)
    {
        if(action=="attack")return "LMB";if(action=="secondary")return "RMB";
        return Defaults.TryGetValue(action,out var fallback)?OS.GetKeycodeString((Key)SaveManager.Data.Settings.Keys.GetValueOrDefault(action,(long)fallback)):action.ToUpperInvariant();
    }
}
