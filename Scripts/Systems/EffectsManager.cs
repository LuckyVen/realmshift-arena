using Godot;
namespace Realmshift;

public enum VisualKind { Particle, Ring, Lightning, Slash, Number, Ghost, Impact, Cast, Dash, Sigil, Telegraph, TelegraphLine }
public sealed class VisualEffect
{
    public bool Active;
    public Vector2 Position,End,Velocity;
    public Color Color;
    public float Life,MaxLife,Radius,Angle,Span,Scale,Seed;
    public VisualKind Kind;
    public Element Element;
    public int Number,Facing;
    public AvatarConfig? Avatar;
    public readonly Vector2[] Points=new Vector2[10];
}

/// <summary>One fixed pool, no per-particle nodes, bounded lifetimes and element-specific animated artwork.</summary>
public partial class EffectsManager : Node2D
{
    public const int Capacity=512;
    private readonly VisualEffect[] _items=new VisualEffect[Capacity];
    private Font _font=null!;
    private float _impactFeedback;
    public int ActiveCount {get;private set;}
    public int LiveCount {get{int count=0;foreach(var e in _items)if(e.Active)count++;return count;}}
    public override void _Ready(){ZIndex=18;TextureFilter=TextureFilterEnum.Nearest;_font=UIFactory.SmallFont;for(int i=0;i<Capacity;i++)_items[i]=new();}
    private VisualEffect? Add(VisualKind kind,Vector2 p,Color color,float life,Element element=Element.Arcane,float scale=1)
    {
        foreach(var e in _items)if(!e.Active)
        {e.Active=true;e.Kind=kind;e.Position=p;e.Color=color;e.Life=e.MaxLife=life;e.Velocity=e.End=Vector2.Zero;e.Radius=e.Angle=e.Span=0;e.Scale=scale;e.Element=element;e.Avatar=null;e.Seed=GD.Randf()*100;return e;}
        if(kind is VisualKind.Telegraph or VisualKind.TelegraphLine)
        {foreach(var e in _items)if(e.Kind is VisualKind.Particle or VisualKind.Ghost or VisualKind.Ring){e.Active=false;return Add(kind,p,color,life,element,scale);}}
        return null;
    }
    private static Element Guess(Color color)
    {Element closest=Element.Arcane;float d=float.MaxValue;foreach(Element e in System.Enum.GetValues<Element>()){Color c=Palette.ElementColor(e);float s=Mathf.Abs(c.R-color.R)+Mathf.Abs(c.G-color.G)+Mathf.Abs(c.B-color.B);if(s<d){d=s;closest=e;}}return closest;}
    public void Burst(Vector2 p,Color c,int count)=>Particles(p,Guess(c),count);
    public void Particles(Vector2 p,Element element,int count,Vector2? direction=null)
    {
        int quality=SaveManager.Data.Settings.Particles;count=quality==0?Mathf.Max(1,count/3):quality==2?Mathf.Min(20,count+count/2):Mathf.Min(14,count);
        for(int i=0;i<count;i++)
        {var e=Add(VisualKind.Particle,p,Colors.White,(float)GD.RandRange(.25,.55),element,(float)GD.RandRange(.45,.8));if(e==null)break;float angle=direction?.Angle()??GD.Randf()*Mathf.Tau;angle+=direction.HasValue?(float)GD.RandRange(-1.2,1.2):0;e.Velocity=Vector2.FromAngle(angle)*(float)GD.RandRange(20,85);e.Angle=GD.Randf()*Mathf.Tau;}
    }
    public void Impact(Vector2 p,Element element,float strength=1,Vector2? direction=null,bool feedback=true)
    {
        Add(VisualKind.Impact,p,Colors.White,.32f,element,Mathf.Clamp(strength,.6f,2.5f));Particles(p,element,strength>1.3f?9:4,direction);
        if(feedback&&strength>1.3f&&_impactFeedback<=0)
        {GameManager.Instance.Camera.Shake(Mathf.Min(2,strength*.65f));GameManager.Instance.HitStop(strength>1.8f?.04f:.025f);_impactFeedback=.1f;}
    }
    public void Ring(Vector2 p,float radius,Color c,float life)
    {var e=Add(VisualKind.Ring,p,c,life,Guess(c));if(e!=null)e.Radius=radius;}
    public void Sigil(Vector2 p,float radius,Element element,float life=.5f)
    {var e=Add(VisualKind.Sigil,p,Colors.White,life,element);if(e!=null)e.Radius=radius;}
    public void Line(Vector2 p,Vector2 end,Color c,float life)
    {
        var e=Add(VisualKind.Lightning,p,c,life,Guess(c));if(e==null)return;e.End=end;
        Vector2 dir=(end-p).Normalized(),perp=dir.Orthogonal();for(int i=0;i<10;i++)e.Points[i]=p.Lerp(end,i/9f)+perp*(i is 0 or 9?0:(GD.Randf()-.5f)*14);
    }
    public void Arc(Vector2 p,float radius,float angle,float span,Color c)
    {var e=Add(VisualKind.Slash,p,Colors.White,.18f,Guess(c));if(e!=null){e.Radius=radius;e.Angle=angle;e.Span=span;}}
    public void Cast(Vector2 p,Element element,Vector2 direction,bool heavy=false)
    {var e=Add(VisualKind.Cast,p,Palette.ElementColor(element),.16f,element,heavy?1.35f:.75f);if(e!=null)e.Angle=direction.Angle();Particles(p,element,heavy?6:2,direction);}
    public void Dash(Vector2 p,Vector2 direction,Element element,bool ending=false)
    {var e=Add(VisualKind.Dash,p,Colors.White,.22f,element,ending?.7f:1);if(e!=null)e.Angle=direction.Angle();Particles(p,element,ending?4:6,-direction);}
    public void Telegraph(Vector2 p,Element element,float radius,float life)
    {var e=Add(VisualKind.Telegraph,p,Colors.White,life,element);if(e!=null)e.Radius=radius;}
    public void Trajectory(Vector2 p,Vector2 end,float life)
    {var e=Add(VisualKind.TelegraphLine,p,new Color("fc9fbe"),life);if(e!=null)e.End=end;}
    public void Number(Vector2 p,int n,Color c){if(SaveManager.Data.Settings.Numbers){var e=Add(VisualKind.Number,p,c,.65f);if(e!=null){e.Velocity=new Vector2(GD.Randf()*12-6,-24);e.Number=n;}}}
    public void Ghost(Vector2 p,AvatarConfig config,int facing)
    {var e=Add(VisualKind.Ghost,p,Palette.Teal,.22f);if(e!=null){e.Avatar=config;e.Facing=facing;}}
    public override void _Process(double delta)
    {
        if(GameManager.Instance.State is RunState.Pause or RunState.Upgrade)return;float dt=(float)delta;ActiveCount=0;_impactFeedback=Mathf.Max(0,_impactFeedback-dt);
        foreach(var e in _items)
        {if(!e.Active)continue;e.Life-=dt;if(e.Life<=0){e.Active=false;e.Avatar=null;continue;}e.Position+=e.Velocity*dt;if(e.Kind==VisualKind.Particle){e.Velocity*=Mathf.Exp(-dt*4);e.Angle+=dt*3;}ActiveCount++;}QueueRedraw();
    }
    public void Clear(){foreach(var e in _items){e.Active=false;e.Avatar=null;}ActiveCount=0;QueueRedraw();}
    public override void _Draw()
    {
        foreach(var e in _items)
        {
            if(!e.Active)continue;float a=e.Life/e.MaxLife,t=1-a;Color c=new(e.Color,a);string name=VfxSprites.Name(e.Element);float age=e.MaxLife-e.Life;
            switch(e.Kind)
            {
                case VisualKind.Particle:VfxSprites.Frame(this,"Particles/"+name,e.Position,8,8,4,(age*18)%4,e.Scale,e.Angle,new Color(1,1,1,a));break;
                case VisualKind.Impact:VfxSprites.Glow(this,e.Position,e.Element,45*e.Scale,.45f*a);VfxSprites.Frame(this,"Impacts/"+name,e.Position,48,48,8,t*8,e.Scale);break;
                case VisualKind.Cast:VfxSprites.Frame(this,"Casts/release",e.Position,32,32,6,t*6,e.Scale,e.Angle,c);break;
                case VisualKind.Dash:VfxSprites.Frame(this,"Dash/streak",e.Position,48,32,6,t*6,e.Scale,e.Angle,new Color(Palette.ElementColor(e.Element),a));break;
                case VisualKind.Sigil:VfxSprites.Field(this,e.Position,e.Radius,e.Element,age,.7f*a);break;
                case VisualKind.Ring:
                    DrawArc(e.Position,e.Radius*(.7f+t*.3f),0,Mathf.Tau,48,new Color(e.Color,a*.6f),1);
                    for(int i=0;i<7;i++){Vector2 p=e.Position+Vector2.FromAngle(i*Mathf.Tau/7+e.Seed+t*.3f)*e.Radius*(.7f+t*.3f);VfxSprites.Frame(this,"Particles/"+name,p,8,8,4,t*4,.7f,0,c);}break;
                case VisualKind.Telegraph:VfxSprites.Field(this,e.Position,e.Radius,e.Element,age,.25f+.2f*t);break;
                case VisualKind.TelegraphLine:
                    for(int i=0;i<12;i++)DrawLine(e.Position.Lerp(e.End,i/12f),e.Position.Lerp(e.End,(i+.5f)/12f),new Color(e.Color,.6f+.35f*t),1);
                    Vector2 d=(e.End-e.Position).Normalized();DrawLine(e.End,e.End-d.Rotated(.5f)*9,e.Color,2);DrawLine(e.End,e.End-d.Rotated(-.5f)*9,e.Color,2);break;
                case VisualKind.Lightning:
                    for(int i=1;i<10;i++){DrawLine(e.Points[i-1].Round(),e.Points[i].Round(),new Color(e.Color,a*.3f),3);DrawLine(e.Points[i-1].Round(),e.Points[i].Round(),new Color(Palette.Paper,a),1);if(i%3==0)DrawLine(e.Points[i],e.Points[i]+(e.Points[i]-e.Points[i-1]).Rotated(.9f).Normalized()*8,c,1);}break;
                case VisualKind.Slash:VfxSprites.Frame(this,"Slashes/"+name,e.Position,96,96,6,t*6,e.Radius/43f,e.Angle);break;
                case VisualKind.Number:DrawString(_font,e.Position.Round(),e.Number.ToString(),HorizontalAlignment.Center,-1,10,c);break;
                case VisualKind.Ghost:
                    if(e.Avatar==null)break;var dst=new Rect2(e.Position-new Vector2(16,29),32,32);var src=new Rect2(0,e.Facing*32,32,32);Color ghost=new(Palette.Teal,a*.42f);
                    DrawTextureRectRegion(Art.Get($"Characters/body_{e.Avatar.Body}.png"),dst,src,ghost);DrawTextureRectRegion(Art.Get($"Characters/outfit_{e.Avatar.Outfit}.png"),dst,src,ghost);DrawTextureRectRegion(Art.Get($"Characters/hair_{e.Avatar.Hair}.png"),dst,src,ghost);break;
            }
        }
    }
}
