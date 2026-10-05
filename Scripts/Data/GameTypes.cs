using Godot;
namespace Realmshift;
public enum WeaponKind { Wand, Bow, Blade, Staff, Chakram, Orbs, Spellbook, Gauntlets }
public enum Element { Fire, Frost, Lightning, Nature, Arcane }
public enum EnemyKind { Chaser, Caster, Dasher, Swarm, Tank, Summoner, Support }
public enum RunState { Menu, Tutorial, Combat, Upgrade, Rest, BossIntro, Shift, Portal, Pause, Defeat, Victory }
public enum PickupKind { Experience, Health, Shard, Shield, Magnet, Haste, Crystal }
public static class Palette
{
    public static readonly Color Ink=new("101e2d"), Paper=new("ecdcbc"), Muted=new("94b5b0"), Gold=new("e6bd78"), Teal=new("80d8be");
    public static Color ElementColor(Element e)=>new(new[]{"ed9b63","91d9e9","f2d779","98d682","b79eeb"}[(int)e]);
    public static Color Realm(int r)=>new(new[]{"88b786","ed9b63","91d9e9","b79eeb"}[Mathf.PosMod(r,4)]);
}
public static class Art
{
    private static readonly System.Collections.Generic.Dictionary<string,Texture2D> Textures=new();
    public static void Release(){foreach(var tex in Textures.Values)tex.Dispose();Textures.Clear();}
    public static Texture2D Get(string path)
    {
        if(!Textures.TryGetValue(path,out var tex)) Textures[path]=tex=GD.Load<Texture2D>("res://Assets/Art/"+path);
        return tex;
    }
    public static Texture2D Weapon(WeaponKind kind)=>Get($"Weapons/Polished/weapon_{(int)kind}.svg");
}
