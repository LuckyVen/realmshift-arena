using Godot;
using System.Collections.Generic;
namespace Realmshift;
public sealed class Projectile
{
    public bool Active,Enemy,Return;
    public Vector2 Position,Previous,Velocity,Origin;
    public float Damage,Life,Age,Radius,TrailTimer;
    public int TrailHead,TrailCount;
    public readonly Vector2[] Trail=new Vector2[8];
    public Element Element;
    public int Pierce,Style;
    public readonly HashSet<EnemyBase> Hit=new();
}
public partial class ProjectilePool : Node2D
{
    public const int Capacity=700;
    private readonly Projectile[] _items=new Projectile[Capacity];
    public int ActiveCount {get;private set;}
    public int LiveCount {get{int count=0;foreach(var p in _items)if(p.Active)count++;return count;}}
    public override void _Ready(){ZIndex=22;for(int i=0;i<Capacity;i++)_items[i]=new();}
    public Projectile? Spawn(Vector2 p,Vector2 velocity,float damage,Element element,bool enemy,float life=1.4f,int pierce=0,int style=0,float radius=3,bool returns=false)
    {
        foreach(var s in _items)if(!s.Active)
        {s.Active=true;s.Position=s.Previous=s.Origin=p;s.Velocity=velocity;s.Damage=damage;s.Element=element;s.Enemy=enemy;s.Life=life;s.Pierce=pierce;s.Style=style;s.Radius=radius;s.Return=returns;s.Age=0;s.TrailTimer=0;s.TrailCount=s.TrailHead=0;s.Hit.Clear();return s;}return null;
    }
    public override void _PhysicsProcess(double delta)
    {
        var g=GameManager.Instance;if(!g.Running)return;float dt=(float)delta;ActiveCount=0;
        foreach(var s in _items)
        {
            if(!s.Active)continue;s.Age+=dt;s.Life-=dt;if(s.Life<=0){s.Active=false;continue;}
            s.Previous=s.Position;
            if(s.Return&&s.Age>.45f){s.Velocity=(g.Player!.Position+new Vector2(0,-12)-s.Position).Normalized()*260;if(s.Age>.6f&&s.Position.DistanceTo(g.Player.Position)<16){s.Active=false;continue;}}
            s.Position+=s.Velocity*dt;
            s.TrailTimer-=dt;if(s.TrailTimer<=0){s.TrailTimer=s.Enemy?.04f:.022f;s.Trail[s.TrailHead]=s.Position;s.TrailHead=(s.TrailHead+1)%8;s.TrailCount=Mathf.Min(8,s.TrailCount+1);}
            if(s.Position.X<20||s.Position.Y<20||s.Position.X>WorldManager.Size.X-20||s.Position.Y>WorldManager.Size.Y-20){s.Active=false;continue;}
            // Physical environment query uses the same authoritative map used to build colliders.
            if(!s.Enemy&&!g.World.CanStand(s.Position,0)&&!s.Return){s.Active=false;g.Effects.Impact(s.Position,s.Element,.65f,s.Velocity,false);continue;}
            if(s.Enemy)
            {if(SegmentDistance(s.Previous,s.Position,g.Player!.Position+new Vector2(0,-9))<s.Radius+7){g.Player.TakeDamage(s.Damage,s.Position);g.Effects.Impact(s.Position,s.Element,.8f,null,false);s.Active=false;}}
            else
            {
                Vector2 middle=(s.Previous+s.Position)/2;float search=s.Previous.DistanceTo(s.Position)/2+s.Radius+35;
                foreach(var e in g.Enemies.Query(middle,search))
                {
                    if(s.Hit.Contains(e)||SegmentDistance(s.Previous,s.Position,e.Position+new Vector2(0,-9))>e.Radius+s.Radius)continue;
                    s.Hit.Add(e);g.Weapons.Hit(e,s.Damage,s.Element,s.Position);
                    if(s.Style==3||g.Player!.Stats.Explosive){g.Weapons.Area(s.Position,35*g.Player!.Stats.Area,s.Damage*.5f,s.Element,e);}
                    if(s.Pierce--<=0){s.Active=false;break;}
                }
            }
            if(s.Active)ActiveCount++;
        }
        QueueRedraw();
    }
    public static float SegmentDistance(Vector2 a,Vector2 b,Vector2 p)
    {Vector2 d=b-a;float t=d.LengthSquared()>.001f?Mathf.Clamp((p-a).Dot(d)/d.LengthSquared(),0,1):0;return p.DistanceTo(a+d*t);}
    public void Clear(){foreach(var s in _items)s.Active=false;ActiveCount=0;QueueRedraw();}
    public override void _Draw()
    {
        foreach(var s in _items)if(s.Active)
        {
            int quality=SaveManager.Data.Settings.Particles;int count=Mathf.Min(s.TrailCount,quality==0?2:quality==1?5:8);
            for(int i=count-1;i>=1;i--)
            {Vector2 p=s.Trail[Mathf.PosMod(s.TrailHead-1-i,8)];float alpha=(1-i/(float)count)*(s.Enemy?.22f:.42f);VfxSprites.Frame(this,"Trails/"+VfxSprites.Name(s.Element),p,16,16,6,(s.Age*20+i)%6,s.Style==3?.9f:.55f,s.Velocity.Angle(),new Color(1,1,1,alpha));}
            VfxSprites.Bolt(this,s.Position,s.Velocity,s.Element,s.Style,s.Age,s.Enemy);
        }
    }
}
