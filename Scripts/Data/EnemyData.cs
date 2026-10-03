using Godot;
namespace Realmshift;
[GlobalClass] public partial class EnemyData : Resource
{
    [Export] public string DisplayName {get;set;}="Mossling";
    [Export] public EnemyKind Kind {get;set;}
    [Export] public int Realm {get;set;}
    [Export] public float Health {get;set;}=25;
    [Export] public float Speed {get;set;}=45;
    [Export] public float Damage {get;set;}=10;
    [Export] public int Experience {get;set;}=4;
}
