using Godot;
namespace Realmshift;
[GlobalClass] public partial class HeroData : Resource
{
    [Export] public string DisplayName {get;set;}="Aster";
    [Export] public string Title {get;set;}="Realm Wanderer";
    [Export] public string Ability {get;set;}="Rejuvenation";
    [Export] public string Description {get;set;}="";
    [Export] public int Hair {get;set;}
    [Export] public int Outfit {get;set;}
    [Export] public int Accessory {get;set;}
    [Export] public Color HairColor {get;set;}=new("eed7a0");
    [Export] public Color OutfitColor {get;set;}=new("79bca1");
}
