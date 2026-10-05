using Godot;
using System.Collections.Generic;
using System.Linq;
namespace Realmshift;
public sealed class CombatZone
{
    public Vector2 Position,End;
    public float Radius,Life,Warning,Tick,Damage,Age,InitialWarning;
    public Element Element;
    public bool Enemy,Beam,Resolved;
}
public partial class CombatSystem : Node2D
{
    private readonly List<CombatZone> _zones=new();
    private GameManager G=>GameManager.Instance;
    public override void _Ready(){ZIndex=-2;TextureFilter=TextureFilterEnum.Nearest;}
    public void Hit(EnemyBase enemy,float damage,Element element,Vector2 at)
    {
        if(!enemy.Active)return;bool critical=GD.Randf()<G.Player!.Stats.Crit;if(element==Element.Arcane)damage*=1+(G.Player.Stats.ElementPower-1)*.2f;
        enemy.Damage(damage*(critical?1.75f:1),element,true,critical);
        if(element==Element.Lightning)
        {var visited=new HashSet<EnemyBase>{enemy};Vector2 origin=enemy.Position;int count=1+G.Player.Stats.Chain;for(int i=0;i<count;i++){var next=G.Enemies.Nearest(origin,85,visited);if(next==null)break;visited.Add(next);G.Effects.Line(origin,next.Position,Palette.ElementColor(element),.17f);next.Damage(damage*.55f,element);origin=next.Position;}}
        if(critical){G.Camera.Shake(.6f);G.HitStop(.022f);}G.Audio.Play("hit",.25f);
    }
    public void Area(Vector2 p,float radius,float damage,Element element,EnemyBase? except=null)
    {foreach(var enemy in G.Enemies.Query(p,radius).ToArray())if(enemy!=except)Hit(enemy,damage,element,p);G.Effects.Sigil(p,radius,element,.3f);}
    public void Arc(Vector2 p,Vector2 direction,float radius,float damage,Element element,float angle)
    {foreach(var e in G.Enemies.Query(p,radius).ToArray())if(Mathf.Abs(direction.AngleTo(e.Position-p))<angle/2)Hit(e,damage,element,e.Position);G.Effects.Arc(p+new Vector2(0,-8),radius,direction.Angle(),angle,Palette.ElementColor(element));}
    public void Zone(Vector2 p,float radius,float duration,float damage,Element element)
    {if(_zones.Count>=64)return;_zones.Add(new(){Position=p,Radius=radius,Life=duration,Damage=damage,Element=element});G.Effects.Sigil(p,radius,element,.4f);}
    public void Hazard(Vector2 p,float radius,float warning,float duration,float damage,Element element)
    {if(_zones.Count>=64)return;_zones.Add(new(){Position=p,Radius=radius,Warning=warning,InitialWarning=warning,Life=warning+duration,Damage=damage,Element=element,Enemy=true});G.Effects.Telegraph(p,element,radius,warning);}
    public void Beam(Vector2 start,Vector2 end,float width,float warning,float damage,Element element)
    {if(_zones.Count>=64)return;_zones.Add(new(){Position=start,End=end,Radius=width,Warning=warning,InitialWarning=warning,Life=warning+.3f,Damage=damage,Element=element,Enemy=true,Beam=true});G.Effects.Cast(start,element,(end-start).Normalized(),true);G.Effects.Trajectory(start,end,warning);}
    public void EnemyShot(Vector2 from,Vector2 target,float damage,int realm,int count=1)
    {for(int i=0;i<count;i++){Vector2 dir=(target-from).Normalized().Rotated((i-(count-1)/2f)*.2f);G.Projectiles.Spawn(from,dir*135,damage,(Element)(realm==0?3:realm==1?0:realm==2?1:4),true,3,0,0,4);}}
    public void Radial(Vector2 from,int count,float speed,float damage,int realm,float rotation=0)
    {for(int i=0;i<count;i++)G.Projectiles.Spawn(from,Vector2.FromAngle(i*Mathf.Tau/count+rotation)*speed,damage,(Element)(realm==0?3:realm==1?0:realm==2?1:4),true,4,0,0,4);}
    public override void _PhysicsProcess(double delta)
    {
        if(!G.Running)return;float dt=(float)delta;
        for(int i=_zones.Count-1;i>=0;i--)
        {
            var z=_zones[i];z.Age+=dt;z.Life-=dt;if(z.Life<=0){_zones.RemoveAt(i);continue;}
            if(z.Warning>0){z.Warning-=dt;continue;}z.Tick-=dt;if(z.Tick>0)continue;z.Tick=.45f;
            if(z.Enemy)
            {
                bool hits=z.Beam?ProjectilePool.SegmentDistance(z.Position,z.End,G.Player!.Position)<z.Radius:z.Position.DistanceTo(G.Player!.Position)<z.Radius;
                if(hits)G.Player!.TakeDamage(z.Damage,z.Position);
                if(!z.Resolved){G.Effects.Impact(z.Position,z.Element,1.6f,null,false);if(z.Beam)G.Effects.Line(z.Position,z.End,Palette.ElementColor(z.Element),.28f);G.Audio.Play("cast",.45f);z.Resolved=true;}
            }
            else{Area(z.Position,z.Radius,z.Damage,z.Element);if(z.Element==Element.Nature&&G.Player!.Position.DistanceTo(z.Position)<z.Radius)G.Player.Heal(G.Player.Stats.ElementPower);if(!G.Running)break;}
        }
        QueueRedraw();
    }
    public void Clear(){_zones.Clear();QueueRedraw();}
    public override void _Draw()
    {
        foreach(var z in _zones)
        {
            Color c=z.Enemy?new("fc9fbe"):Palette.ElementColor(z.Element);float alpha=z.Warning>0?.10f:.12f;
            if(z.Beam)
            {
                DrawLine(z.Position,z.End,new Color(c,alpha),z.Radius*2);
                Vector2 normal=(z.End-z.Position).Normalized().Orthogonal()*z.Radius;
                DrawLine(z.Position+normal,z.End+normal,new Color(c,.85f),1);DrawLine(z.Position-normal,z.End-normal,new Color(c,.85f),1);
                if(z.Warning<=0)
                {int count=Mathf.Clamp((int)(z.Position.DistanceTo(z.End)/24),1,24);for(int i=0;i<count;i++)VfxSprites.Bolt(this,z.Position.Lerp(z.End,(i+.5f)/count),z.End-z.Position,z.Element,0,z.Age+i*.02f,false,.75f);}
                else for(int i=0;i<5;i++)DrawLine(z.Position.Lerp(z.End,i/5f),z.Position.Lerp(z.End,(i+.5f)/5f),c,1);
            }
            else
            {
                DrawCircle(z.Position,z.Radius,new Color(c,alpha));DrawArc(z.Position,z.Radius,0,Mathf.Tau,48,new Color(c,.85f),z.Enemy?2:1);
                VfxSprites.Field(this,z.Position,z.Radius,z.Element,z.Age,z.Warning>0?.28f:.65f);
                if(z.Warning>0)
                {float progress=1-z.Warning/Mathf.Max(.001f,z.InitialWarning);DrawArc(z.Position,z.Radius+3,-Mathf.Pi/2,-Mathf.Pi/2+Mathf.Tau*progress,48,c,2);DrawLine(z.Position-new Vector2(4,0),z.Position+new Vector2(4,0),c);DrawLine(z.Position-new Vector2(0,4),z.Position+new Vector2(0,4),c);}
            }
        }
    }
}
