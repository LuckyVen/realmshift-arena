using Godot;
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
namespace Realmshift;
public sealed class AvatarConfig
{
    public int Hero {get;set;}
    public int Body {get;set;}
    public int Hair {get;set;}
    public int Outfit {get;set;}
    public int Accessory {get;set;}=1;
    public int Aura {get;set;}
    public int Eyes {get;set;}
    public int Expression {get;set;}
    public string Skin {get;set;}="e8d4b2";
    public string HairTint {get;set;}="f1d69c";
    public string OutfitTint {get;set;}="86bfa5";
    public string EyeTint {get;set;}="263947";
    public AvatarConfig Copy()=>JsonSerializer.Deserialize<AvatarConfig>(JsonSerializer.Serialize(this))!;
    public static AvatarConfig FromHero(int i)
    {var h=Catalog.Heroes[i];return new(){Hero=i,Hair=h.Hair,Outfit=h.Outfit,Accessory=h.Accessory,HairTint=h.HairColor.ToHtml(false),OutfitTint=h.OutfitColor.ToHtml(false)};}
}
public sealed class SettingsData
{
    public float Master {get;set;}=.7f;
    public float Music {get;set;}=.6f;
    public float Sfx {get;set;}=.7f;
    public bool Fullscreen {get;set;}
    public bool Vsync {get;set;}=true;
    public bool Shake {get;set;}=true;
    public bool Numbers {get;set;}=true;
    public bool ReducedFlash {get;set;}
    public bool AutoAim {get;set;}
    public int Particles {get;set;}=1;
    public int Resolution {get;set;}=1;
    public Dictionary<string,long> Keys {get;set;}=new();
}
public sealed class SaveData
{
    public int Version {get;set;}=1;
    public int Shards {get;set;}
    public int BestLevel {get;set;}
    public int BestScore {get;set;}
    public int Runs {get;set;}
    public int TotalKills {get;set;}
    public bool Endless {get;set;}
    public bool Tutorial {get;set;}
    public HashSet<int> Heroes {get;set;}=new(){0,1};
    public HashSet<int> Weapons {get;set;}=new(){0,1,2};
    public HashSet<int> Bosses {get;set;}=new();
    public HashSet<int> SeenEnemies {get;set;}=new();
    public HashSet<string> Relics {get;set;}=new();
    public Dictionary<int,int> Mastery {get;set;}=new();
    public HashSet<string> Achievements {get;set;}=new();
    public AvatarConfig Avatar {get;set;}=new();
    public SettingsData Settings {get;set;}=new();
}
public static class SaveManager
{
    public static SaveData Data {get;private set;}=new();
    public static string Path=>ProjectSettings.GlobalizePath(System.Array.Exists(OS.GetCmdlineUserArgs(),x=>x.StartsWith("--smoke")||x.StartsWith("--capture="))?"user://smoke-save.json":"user://realmshift-save.json");
    public static string? Warning {get;private set;}
    public static void Load()
    {
        try
        {
            if(File.Exists(Path))Data=JsonSerializer.Deserialize<SaveData>(File.ReadAllText(Path))??new();
            Data.Settings??=new();Data.Avatar??=new();Data.Heroes??=new(){0,1};Data.Weapons??=new(){0,1,2};Data.Bosses??=new();Data.SeenEnemies??=new();Data.Relics??=new();Data.Mastery??=new();Data.Achievements??=new();Data.Settings.Keys??=new();
            Data.Settings.Master=Math.Clamp(Data.Settings.Master,0,1);Data.Settings.Music=Math.Clamp(Data.Settings.Music,0,1);Data.Settings.Sfx=Math.Clamp(Data.Settings.Sfx,0,1);
            Data.Settings.Resolution=Math.Clamp(Data.Settings.Resolution,0,2);Data.Settings.Particles=Math.Clamp(Data.Settings.Particles,0,2);
            Data.Avatar.Hero=Math.Clamp(Data.Avatar.Hero,0,7);Data.Avatar.Body=Math.Clamp(Data.Avatar.Body,0,2);Data.Avatar.Hair=Math.Clamp(Data.Avatar.Hair,0,10);Data.Avatar.Outfit=Math.Clamp(Data.Avatar.Outfit,0,7);Data.Avatar.Accessory=Math.Clamp(Data.Avatar.Accessory,0,5);Data.Avatar.Aura=Math.Clamp(Data.Avatar.Aura,0,7);
        }
        catch(Exception ex){GD.PushWarning("Save recovery: "+ex.Message);Data=new();Warning="Your save could not be read. A fresh profile is active.";try{File.Copy(Path,Path+".corrupt",true);}catch{} }
    }
    public static void Save()
    {
        try{Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!);File.WriteAllText(Path+".tmp",JsonSerializer.Serialize(Data,new JsonSerializerOptions{WriteIndented=true}));File.Move(Path+".tmp",Path,true);}
        catch(Exception ex){GD.PushError("Save failed: "+ex.Message);Warning="Progress could not be saved. Check folder permissions.";}
    }
    public static bool UnlockHero(int i)
    {if(Data.Heroes.Contains(i))return true;if(Data.Shards<60)return false;Data.Shards-=60;Data.Heroes.Add(i);Save();return true;}
    public static void BossDefeated(int realm)
    {
        Data.Bosses.Add(realm);Data.Achievements.Add("Guardian "+(realm+1));
        foreach(var w in Catalog.Weapons)if(w.UnlockBoss<=realm)Data.Weapons.Add((int)w.Kind);
        if(realm==3){Data.Endless=true;Data.Achievements.Add("Realm Restorer");}Save();
    }
}
