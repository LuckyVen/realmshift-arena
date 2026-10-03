using Godot;
namespace Realmshift;
[GlobalClass] public partial class WeaponData : Resource
{
    [Export] public string DisplayName {get;set;}="Wand";
    [Export(PropertyHint.MultilineText)] public string Description {get;set;}="";
    [Export] public WeaponKind Kind {get;set;}
    [Export] public float Damage {get;set;}=14;
    [Export] public float Interval {get;set;}=.25f;
    [Export] public float Speed {get;set;}=300;
    [Export] public float Range {get;set;}=300;
    [Export] public float AbilityCooldown {get;set;}=7;
    [Export] public int UnlockBoss {get;set;}=-1;
}
