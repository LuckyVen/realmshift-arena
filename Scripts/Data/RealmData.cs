using Godot;
namespace Realmshift;
[GlobalClass] public partial class RealmData : Resource
{
    [Export] public string DisplayName {get;set;}="Greenward Basin";
    [Export] public string Subtitle {get;set;}="The old forest remembers.";
    [Export] public string BossName {get;set;}="Rootbound Colossus";
    [Export] public string Lore {get;set;}="";
    [Export] public string ShiftText {get;set;}="";
}
