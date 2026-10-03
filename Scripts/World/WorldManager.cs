using Godot;
using System;
using System.Collections.Generic;
namespace Realmshift;
public partial class WorldManager : Node2D
{
    public static readonly Vector2 Size=new(2816,2112);
    public int Realm {get;private set;}
    public Vector2 Spawn=>Size/2;
    public Vector2 PortalPosition=>Spawn+new Vector2(180,-40);
    public List<Vector2> Chests {get;}=new();
    public List<Vector2> Shrines {get;}=new();
    private readonly List<(Vector2 P,float R)> _blocked=new();
    private Node2D _props=null!;
    private Node2D _collision=null!;
    private float _time;
    public float Shift {get;set;}
    public int NextRealm {get;set;}
    public bool PortalActive {get;set;}
    public bool Endless {get;set;}
    public int RemainingCaches=>Chests.Count;
    public override void _Ready(){ZIndex=-10;}
    public void Build(int realm)
    {
        Realm=Mathf.PosMod(realm,4);PortalActive=false;Shift=0;_blocked.Clear();Chests.Clear();Shrines.Clear();
        if(_props!=null){RemoveChild(_props);_props.QueueFree();RemoveChild(_collision);_collision.QueueFree();}
        _props=new Node2D{Name="Scenery",ZIndex=10,YSortEnabled=true};AddChild(_props);
        _collision=new Node2D{Name="EnvironmentCollisions"};AddChild(_collision);
        AddWall(new Rect2(-32,-32,Size.X+64,48));AddWall(new Rect2(-32,Size.Y-16,Size.X+64,48));AddWall(new Rect2(-32,0,48,Size.Y));AddWall(new Rect2(Size.X-16,0,48,Size.Y));
        float river=RiverX;
        if(Realm!=2){float prev=16;foreach(float bridge in BridgeYs){AddWall(new Rect2(river,prev,80,bridge-36-prev));prev=bridge+36;}AddWall(new Rect2(river,prev,80,Size.Y-16-prev));}
        var rng=new RandomNumberGenerator{Seed=(ulong)(7045+Realm*71)};
        for(int i=0;i<260;i++)
        {
            var p=new Vector2(rng.RandfRange(64,Size.X-64),rng.RandfRange(72,Size.Y-64));
            if(!CanStand(p,24)||p.DistanceTo(Spawn)<180||OnPath(p))continue;
            int kind=i%7;string name=kind<3?(Realm==1||Realm==3?"crystal":"tree"):kind==3?"crystal":kind==4?"rock":"shrub";
            var tex=Art.Get($"Props/{name}_{Realm}.png");var sprite=new Sprite2D{Texture=tex,Position=p,Offset=new Vector2(0,-tex.GetHeight()/2f+8)};_props.AddChild(sprite);
            if(kind<5)AddObstacle(p,kind<3?11:kind==3?6:10);
        }
        Vector2[] landmarks={new(500,420),new(1870,420),new(560,1650),new(2150,1660),new(2110,1040),new(1450,1830)};
        foreach(var p in landmarks)
        {
            var spr=new Sprite2D{Texture=Art.Get($"Props/shrine_{Realm}.png"),Position=p,Offset=new Vector2(0,-24)};_props.AddChild(spr);AddObstacle(p,12);Shrines.Add(p+new Vector2(0,28));
            for(int x=-4;x<=4;x++)if(x!=0&&x!=2)PlaceWall(p+new Vector2(x*16,-62));
            for(int y=-2;y<=2;y++)if(y!=0)PlaceWall(p+new Vector2(-64,y*16-32));
        }
        for(int i=0;i<9;i++)
        {Vector2 p=landmarks[i%landmarks.Length]+new Vector2((i%3-1)*60,66);Chests.Add(p);}
        Chests.Add(Spawn+new Vector2(90,45));QueueRedraw();
    }
    public float RiverX=>Size.X*new[]{.34f,.57f,.26f,.45f}[Realm];
    private static readonly float[] BridgeYs={448,1056,1664};
    private bool OnPath(Vector2 p)=>Mathf.Abs(p.X-Spawn.X)<34||Mathf.Abs(p.Y-Spawn.Y)<34||Mathf.Abs(p.Y-448)<22||Mathf.Abs(p.Y-1664)<22;
    private bool OnBridge(float y){foreach(var b in BridgeYs)if(Mathf.Abs(y-b)<36)return true;return false;}
    public bool CanStand(Vector2 p,float margin=8)
    {
        if(p.X<32+margin||p.Y<32+margin||p.X>Size.X-32-margin||p.Y>Size.Y-32-margin)return false;
        if(Realm!=2&&p.X>RiverX-margin&&p.X<RiverX+80+margin&&!OnBridge(p.Y))return false;
        foreach(var b in _blocked)if(p.DistanceSquaredTo(b.P)<Mathf.Pow(b.R+margin,2))return false;
        return true;
    }
    public Vector2 SafePoint(Vector2 desired)
    {if(CanStand(desired))return desired;for(int i=0;i<24;i++){var p=desired+Vector2.FromAngle(i*2.4f)*((i+1)*8);if(CanStand(p))return p;}return Spawn;}
    public Vector2 PathDirection(Vector2 from,Vector2 target)
    {
        if(Realm!=2&&((from.X<RiverX&&target.X>RiverX+80)||(target.X<RiverX&&from.X>RiverX+80)))
        {float best=BridgeYs[0],dist=float.MaxValue;foreach(var y in BridgeYs){float d=Mathf.Abs(from.Y-y)+Mathf.Abs(target.Y-y);if(d<dist){dist=d;best=y;}}if(Mathf.Abs(from.Y-best)>24)target=new Vector2(from.X,best);}
        return (target-from).Normalized();
    }
    private void AddWall(Rect2 r)
    {if(r.Size.X<=0||r.Size.Y<=0)return;var b=new StaticBody2D{CollisionLayer=4,CollisionMask=0,Position=r.GetCenter()};b.AddChild(new CollisionShape2D{Shape=new RectangleShape2D{Size=r.Size}});_collision.AddChild(b);}
    private void AddObstacle(Vector2 p,float r)
    {_blocked.Add((p,r));var b=new StaticBody2D{Position=p,CollisionLayer=4,CollisionMask=0};b.AddChild(new CollisionShape2D{Shape=new CircleShape2D{Radius=r}});_collision.AddChild(b);}
    private void PlaceWall(Vector2 p)
    {_props.AddChild(new Sprite2D{Texture=Art.Get($"Props/wall_{Realm}.png"),Position=p,Offset=new Vector2(0,-8)});AddObstacle(p,7);}
    public override void _Process(double delta)
    { _time+=(float)delta;if(_props!=null){_props.Modulate=new Color(1+Shift*.35f,1-Shift*.12f,1+Shift*.25f);_props.Position=new Vector2(Mathf.Sin(_time*21)*Shift*1.5f,-Shift*(NextRealm==3?8:1));}QueueRedraw();}
    private static int Hash(int x,int y)=>unchecked((x*73856093)^(y*19349663))&0x7fffffff;
    private int Ground(int x,int y)
    {Vector2 p=new(x*16+8,y*16+8);if(p.X>RiverX&&p.X<RiverX+80)return OnBridge(p.Y)?1:2;if(OnPath(p))return 1;return 0;}
    public override void _Draw()
    {
        var game=GameManager.Instance;if(game==null)return;Vector2 center=game.Camera?.GlobalPosition??Spawn;
        int x0=Math.Clamp((int)(center.X-380)/16,0,(int)Size.X/16-1),x1=Math.Clamp((int)(center.X+380)/16,0,(int)Size.X/16-1);
        int y0=Math.Clamp((int)(center.Y-230)/16,0,(int)Size.Y/16-1),y1=Math.Clamp((int)(center.Y+230)/16,0,(int)Size.Y/16-1);
        var tex=Art.Get($"Tilesets/realm_{Realm}.png");Texture2D? next=Shift>0?Art.Get($"Tilesets/realm_{NextRealm}.png"):null;
        for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)
        {
            int kind=Ground(x,y),variant=(Hash(x,y)+((kind==2)?(int)(_time*3):0))%16;var dst=new Rect2(x*16,y*16,16,16);var src=new Rect2(variant*16,kind*16,16,16);
            var groundTex=Endless&&Hash(x/6,y/6)%7<3?Art.Get($"Tilesets/realm_{Hash(x/6+31,y/6)%4}.png"):tex;
            DrawTextureRectRegion(groundTex,dst,src);
            if(next!=null){float radial=(new Vector2(x*16,y*16)-Spawn).Length()/1800f;float reveal=Mathf.Clamp(Shift*1.8f-radial,0,1);DrawTextureRectRegion(next,dst,src,new Color(1,1,1,reveal));}
            if(kind==0&&Hash(x+41,y)%21==0){Color c=Palette.Realm(Realm).Darkened(.12f);DrawRect(new Rect2(x*16+3,y*16+8+Mathf.Sin(_time*2+x)*1,1,4),c);DrawRect(new Rect2(x*16+5,y*16+10,1,3),c);}
        }
        if(Shift>0){for(int i=0;i<10;i++){Vector2 end=Spawn+Vector2.FromAngle(i*Mathf.Tau/10+.17f)*(Shift*1200);DrawLine(Spawn,end,new Color(Palette.Realm(NextRealm),Shift*.7f),2);}}
        // The world edge is a proper thick stone rampart.
        for(int x=x0;x<=x1;x++)foreach(int y in new[]{0,(int)Size.Y/16-1})DrawTexture(Art.Get($"Props/wall_{Realm}.png"),new Vector2(x*16,y*16-16));
        for(int y=y0;y<=y1;y++)foreach(int x in new[]{0,(int)Size.X/16-1})DrawTexture(Art.Get($"Props/wall_{Realm}.png"),new Vector2(x*16,y*16-16));
        foreach(var p in Chests){DrawTexture(Art.Get("Props/chest.png"),p-new Vector2(16,27));if(p.DistanceTo(game.Player?.Position??Spawn)<45)DrawCircle(p+new Vector2(0,-34),3,Palette.Gold);}
        foreach(var p in Shrines){float glow=.5f+Mathf.Sin(_time*2+p.X)*.2f;DrawArc(p,20,0,Mathf.Tau,24,new Color(Palette.Realm(Realm),glow),1);}
        if(PortalActive)
        {
            DrawTexture(Art.Get("Props/portal.png"),PortalPosition-new Vector2(32,65));
            for(int i=0;i<10;i++){var p=PortalPosition+new Vector2(Mathf.Sin(_time*1.5f+i)*14,-18-Mathf.PosMod(_time*14+i*7,40));DrawRect(new Rect2(p,2,2),Palette.Realm(NextRealm));}
        }
        // Ambient flecks use world coordinates and stay consistent as the camera moves.
        for(int i=0;i<20;i++)
        {var p=new Vector2(center.X-360+Mathf.PosMod(i*103+_time*(Realm==2?8:4),720),center.Y-220+Mathf.PosMod(i*73+_time*(Realm==2?16:7),440));DrawRect(new Rect2(p,Realm==2?2:1,1),new Color(Palette.Realm(Realm),.35f));}
    }
}
