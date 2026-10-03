using Godot;
using System.Collections.Generic;
using System.Linq;
namespace Realmshift;
public partial class EnemyDirector : Node2D
{
    public List<EnemyBase> Active {get;}=new();
    public BossBase? Boss {get;private set;}
    public int Count=>Active.Count;
    private readonly Stack<EnemyBase> _pool=new();
    private readonly Dictionary<Vector2I,List<EnemyBase>> _grid=new();
    public override void _Ready()
    {Name="Enemies";YSortEnabled=true;for(int i=0;i<140;i++){var enemy=new EnemyBase();AddChild(enemy);_pool.Push(enemy);}}
    public void Spawn(int index,Vector2 position,bool elite=false,bool miniboss=false,bool summoned=false)
    {
        if(_pool.Count==0)return;var g=GameManager.Instance;var enemy=_pool.Pop();enemy.Spawn(Catalog.Enemies[index],g.World.SafePoint(position),g.Level.Level,elite,miniboss);Active.Add(enemy);
    }
    public BossBase SpawnBoss(int realm)
    {Boss=new BossBase();AddChild(Boss);Boss.SpawnBoss(realm,GameManager.Instance.World.SafePoint(GameManager.Instance.Player!.Position+new Vector2(0,-155)),GameManager.Instance.Level.Level);Active.Add(Boss);return Boss;}
    public override void _PhysicsProcess(double delta)
    {
        _grid.Clear();foreach(var e in Active)
        {if(!e.Active)continue;var cell=new Vector2I(Mathf.FloorToInt(e.Position.X/64),Mathf.FloorToInt(e.Position.Y/64));if(!_grid.TryGetValue(cell,out var bucket))_grid[cell]=bucket=new();bucket.Add(e);}
    }
    public IEnumerable<EnemyBase> Query(Vector2 point,float radius)
    {
        int x0=Mathf.FloorToInt((point.X-radius)/64),x1=Mathf.FloorToInt((point.X+radius)/64),y0=Mathf.FloorToInt((point.Y-radius)/64),y1=Mathf.FloorToInt((point.Y+radius)/64);
        for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)if(_grid.TryGetValue(new Vector2I(x,y),out var bucket))foreach(var e in bucket)if(e.Active&&e.Position.DistanceSquaredTo(point)<Mathf.Pow(radius+e.Radius,2))yield return e;
    }
    public EnemyBase? Nearest(Vector2 p,float radius,HashSet<EnemyBase>? excluded=null)
    {EnemyBase? best=null;float d=radius*radius;foreach(var e in Query(p,radius)){if(excluded?.Contains(e)==true)continue;float dd=p.DistanceSquaredTo(e.Position);if(dd<d){d=dd;best=e;}}return best;}
    public Vector2 Separation(EnemyBase e)
    {Vector2 result=Vector2.Zero;int n=0;foreach(var other in Query(e.Position,20)){if(other==e||other.IsBoss)continue;Vector2 d=e.Position-other.Position;if(d.LengthSquared()>.1f)result+=d.Normalized();if(++n>=4)break;}return result;}
    public void Defeated(EnemyBase e)
    {
        Active.Remove(e);var g=GameManager.Instance;g.Kills++;g.Score+=e.IsBoss?1000:e.Miniboss?200:e.Elite?60:10;g.Combo++;g.ComboTimer=3;SaveManager.Data.TotalKills++;
        if(g.Player!.Stats.Leech)g.Player.Heal(1);
        if(e.IsBoss){g.Level.BossDefeated();e.QueueFree();Boss=null;return;}
        g.Pickups.Spawn(e.Position,PickupKind.Experience,e.Data.Experience*(e.Elite?3:1)*(e.Miniboss?5:1));
        if(e.Elite||GD.Randf()<.2f)g.Pickups.Spawn(e.Position+new Vector2(5,0),PickupKind.Shard,e.Miniboss?8:e.Elite?3:1);
        float roll=GD.Randf();if(roll<.04f)g.Pickups.Spawn(e.Position+new Vector2(-5,0),PickupKind.Health,12);else if(roll<.055f)g.Pickups.Spawn(e.Position,PickupKind.Shield,18);else if(roll<.065f)g.Pickups.Spawn(e.Position,PickupKind.Magnet,1);else if(roll<.075f)g.Pickups.Spawn(e.Position,PickupKind.Haste,5);
        if(e.Miniboss)g.Pickups.Spawn(e.Position+new Vector2(0,8),PickupKind.Crystal,1);
        _pool.Push(e);
    }
    public void Clear()
    {foreach(var e in Active.ToArray()){e.Deactivate();if(e.IsBoss)e.QueueFree();else _pool.Push(e);}Active.Clear();Boss=null;_grid.Clear();}
}
