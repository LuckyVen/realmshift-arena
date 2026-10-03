using Godot;
namespace Realmshift;
[GlobalClass] public partial class UpgradeData : Resource
{
    [Export] public string Id {get;set;}="";
    [Export] public string DisplayName {get;set;}="";
    [Export] public string Description {get;set;}="";
    [Export] public string Effect {get;set;}="";
    [Export] public float Amount {get;set;}=1;
    [Export] public int Weapon {get;set;}=-1;
    [Export] public int Element {get;set;}=-1;
    [Export] public int MaxRank {get;set;}=3;
}
