using Godot;
namespace Realmshift;
public sealed class Pickup
{public bool Active;public Vector2 Position;public PickupKind Kind;public int Value;public float Age;}
public partial class PickupManager : Node2D
{
    private readonly Pickup[] _items=new Pickup[360];
    public bool Magnet {get;set;}
    public override void _Ready(){ZIndex=8;for(int i=0;i<_items.Length;i++)_items[i]=new();}
    public void Spawn(Vector2 position,PickupKind kind,int value)
    {
        foreach(var p in _items)if(!p.Active){p.Active=true;p.Position=position;p.Kind=kind;p.Value=value;p.Age=0;return;}
        // At capacity, merge XP and currency instead of silently dropping rewards.
        foreach(var p in _items)if(p.Kind==kind){p.Value+=value;return;}
    }
    public override void _PhysicsProcess(double delta)
    {
        var g=GameManager.Instance;if(!g.Running)return;float dt=(float)delta;
        foreach(var p in _items)
        {if(!p.Active)continue;p.Age+=dt;var target=g.Player!.Position;float d=p.Position.DistanceTo(target);if(d<g.Player.Stats.Magnet||Magnet||p.Age>35)p.Position=p.Position.MoveToward(target,dt*(150+(Magnet?250:0)));if(d<12){p.Active=false;Collect(p);}}
        Magnet=false;QueueRedraw();
    }
    private void Collect(Pickup p)
    {
        var g=GameManager.Instance;switch(p.Kind)
        {case PickupKind.Experience:g.Experience.Add(p.Value);break;case PickupKind.Health:g.Player!.Heal(p.Value);break;case PickupKind.Shard:SaveManager.Data.Shards+=p.Value;g.RunShards+=p.Value;break;case PickupKind.Shield:g.Player!.Stats.Shield+=p.Value;break;case PickupKind.Haste:g.Player!.Haste=p.Value;break;case PickupKind.Magnet:Magnet=true;foreach(var s in _items)if(s.Active)s.Age=100;break;case PickupKind.Crystal:g.Experience.Add(g.Experience.Required);break;}
        g.Audio.Play("pickup",.4f);
    }
    public void Vacuum(){foreach(var p in _items)if(p.Active)p.Age=100;}
    public void Clear(){foreach(var p in _items)p.Active=false;QueueRedraw();}
    public override void _Draw()
    {
        foreach(var p in _items)if(p.Active)
        {
            var pos=p.Position+new Vector2(0,Mathf.Sin(p.Age*4)*2-5);Color c=p.Kind switch{PickupKind.Experience=>new("a7ccda"),PickupKind.Health=>new("ee8e9c"),PickupKind.Shard=>Palette.Gold,PickupKind.Shield=>Palette.Teal,_=>new("be9ede")};
            DrawRect(new Rect2(pos-new Vector2(2,2),4,4),c);DrawRect(new Rect2(pos-new Vector2(1,3),2,6),c);DrawRect(new Rect2(pos-new Vector2(1,1),1,1),Palette.Paper);
        }
    }
}
