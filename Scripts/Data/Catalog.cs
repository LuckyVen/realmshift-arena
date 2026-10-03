using Godot;
using System.Collections.Generic;
namespace Realmshift;
public static class Catalog
{
    public static WeaponData[] Weapons {get;private set;}=System.Array.Empty<WeaponData>();
    public static EnemyData[] Enemies {get;private set;}=System.Array.Empty<EnemyData>();
    public static HeroData[] Heroes {get;private set;}=System.Array.Empty<HeroData>();
    public static RealmData[] Realms {get;private set;}=System.Array.Empty<RealmData>();
    public static readonly List<UpgradeData> Upgrades=new();
    public static void Release()
    {foreach(var x in Weapons)x.Dispose();foreach(var x in Enemies)x.Dispose();foreach(var x in Heroes)x.Dispose();foreach(var x in Realms)x.Dispose();foreach(var x in Upgrades)x.Dispose();Upgrades.Clear();}
    public static void Load()
    {
        Weapons=new WeaponData[8]; for(int i=0;i<8;i++)Weapons[i]=GD.Load<WeaponData>($"res://Resources/Weapons/weapon_{i}.tres");
        Enemies=new EnemyData[28];for(int i=0;i<28;i++)Enemies[i]=GD.Load<EnemyData>($"res://Resources/Enemies/enemy_{i}.tres");
        Heroes=new HeroData[8];for(int i=0;i<8;i++)Heroes[i]=GD.Load<HeroData>($"res://Resources/Characters/hero_{i}.tres");
        Realms=new RealmData[4];for(int i=0;i<4;i++)Realms[i]=GD.Load<RealmData>($"res://Resources/Battlegrounds/realm_{i}.tres");
        Upgrades.Clear(); foreach(string f in ResourceLoader.ListDirectory("res://Resources/Upgrades"))if(f.EndsWith(".tres"))Upgrades.Add(GD.Load<UpgradeData>("res://Resources/Upgrades/"+f));
        Upgrades.Sort((a,b)=>string.CompareOrdinal(a.Id,b.Id));
    }
}
