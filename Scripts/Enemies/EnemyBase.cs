using Godot;
namespace Realmshift;
public partial class EnemyBase : CharacterBody2D
{
    public EnemyData Data {get;protected set;}=null!;
    public bool Active {get;protected set;}
    public bool Elite {get;protected set;}
    public bool Miniboss {get;private set;}
    public float Health {get;protected set;}
    public float MaxHealth {get;protected set;}
    public float Radius {get;protected set;}=9;
    public bool IsBoss=>this is BossBase;
    public float Burn,Slow,Root,Stun;
    protected float Age,Flash,Cooldown,Windup;
    protected Vector2 RushDirection;
    protected float _levelScale=1;
    private float _burnTick;
    private float _contact,_eliteTimer;
    public int Modifier {get;private set;}
    public void Heal(float value)=>Health=Mathf.Min(MaxHealth,Health+value);
    private CollisionShape2D _shape=null!;
    public override void _Ready()
    {
        CollisionLayer=2;CollisionMask=4;
        _shape=new CollisionShape2D{Shape=new CircleShape2D{Radius=7},Position=new Vector2(0,-5)};AddChild(_shape);Deactivate();
    }
    public virtual void Spawn(EnemyData data,Vector2 position,int level,bool elite=false,bool miniboss=false)
    {
        Data=data;Position=position;_levelScale=1+(level-1)*.075f;Elite=elite;Miniboss=miniboss;Radius=miniboss?17:elite?12:9;
        MaxHealth=data.Health*_levelScale*(miniboss?5:elite?2.1f:1);Health=MaxHealth;
        Modifier=elite?(int)(GD.Randi()%7):-1;_eliteTimer=3;
        Active=true;Visible=true;SetPhysicsProcess(true);_shape.SetDeferred(CollisionShape2D.PropertyName.Disabled,false);
        Burn=Slow=Root=Stun=Age=Flash=Windup=_burnTick=_contact=0;Cooldown=GD.Randf()*1.8f;Scale=Vector2.One*(miniboss?1.6f:elite?1.18f:1);ZIndex=15;
        SaveManager.Data.SeenEnemies.Add(data.Realm*7+(int)data.Kind);GameManager.Instance.Effects.Burst(position,Palette.Realm(data.Realm),8);
    }
    public void Deactivate(){Active=false;Visible=false;SetPhysicsProcess(false);if(_shape!=null)_shape.SetDeferred(CollisionShape2D.PropertyName.Disabled,true);}
    public override void _PhysicsProcess(double delta)
    {
        var g=GameManager.Instance;if(!Active||!g.Running)return;
        float dt=(float)delta;Age+=dt;Flash=Mathf.Max(0,Flash-dt*5);_contact=Mathf.Max(0,_contact-dt);
        Slow=Mathf.Max(0,Slow-dt);Root=Mathf.Max(0,Root-dt);Stun=Mathf.Max(0,Stun-dt);
        if(Burn>0){Burn-=dt;_burnTick+=dt;if(_burnTick>.4f){_burnTick=0;Damage(3*g.Player!.Stats.ElementPower,Element.Fire,false,false);if(!Active)return;}}
        var player=g.Player!;float distance=Position.DistanceTo(player.Position);Vector2 direction=g.World.PathDirection(Position,player.Position);float speed=Data.Speed*(Elite?1.15f:1)*(1+Mathf.Min(.5f,(g.Level.Level-1)*.006f));
        Cooldown-=dt;if(Elite){_eliteTimer-=dt;if(_eliteTimer<=0){_eliteTimer=4;switch(Modifier){case 0:g.Weapons.Hazard(Position,18,.7f,1,8,Element.Fire);break;case 1:g.Weapons.EnemyShot(Position,player.Position,10,3,3);break;case 2:g.Weapons.Hazard(player.Position,18,1,.6f,9,Element.Frost);break;case 3:g.Weapons.Radial(Position,8,100,9,3);break;}}if(Modifier==4)speed*=1.4f;}
        if(Stun>0||Root>0)Velocity=Vector2.Zero;
        else switch(Data.Kind)
        {
            case EnemyKind.Caster:
                Velocity=direction*(distance>190?speed:distance<115?-speed*.7f:0);
                if(Cooldown<=0&&distance<370){Cooldown=Elite?1.4f:2.2f;g.Weapons.EnemyShot(Position+new Vector2(0,-10),player.Position,Data.Damage*_levelScale,Data.Realm,Elite?3:1);}
                break;
            case EnemyKind.Dasher:
                if(Windup>0)
                {Windup-=dt;Velocity=Vector2.Zero;if(Windup<=0){Cooldown=-.4f;Velocity=RushDirection*210;}}
                else if(Cooldown<0&&Cooldown>-.4f)Velocity=RushDirection*210;
                else if(Cooldown<=-.4f){Cooldown=2.9f;Windup=.65f;RushDirection=direction;g.Effects.Line(Position,Position+direction*135,Palette.Realm(Data.Realm),.65f);Velocity=Vector2.Zero;}
                else Velocity=direction*speed;
                break;
            case EnemyKind.Support:
                Velocity=direction*(distance>210?speed:distance<130?-speed*.6f:0);
                if(Cooldown<=0){Cooldown=3.5f;foreach(var ally in g.Enemies.Query(Position,95))ally.Heal(5*_levelScale);g.Effects.Ring(Position,95,Palette.Teal,.5f);}
                break;
            case EnemyKind.Summoner:
                Velocity=direction*(distance>180?speed:0);
                if(Cooldown<=0&&g.Enemies.Count<100){Cooldown=6;g.Enemies.Spawn(Data.Realm*6+3,g.World.SafePoint(Position+new Vector2(25,0)),false,false,true);g.Effects.Ring(Position,26,Palette.Realm(Data.Realm),.4f);}
                break;
            default:Velocity=direction*speed;break;
        }
        if(Slow>0)Velocity*=.45f;
        // Mild deterministic separation keeps a swarm readable without N^2 crowd scans.
        if(!IsBoss&&Velocity!=Vector2.Zero)Velocity+=g.Enemies.Separation(this)*18;
        MoveAndSlide();if(IsOnWall()&&Velocity.LengthSquared()>1){Velocity=Velocity.Rotated(Mathf.Pi/2)*.7f;MoveAndSlide();}
        if(distance<Radius+9&&_contact<=0){_contact=.75f;player.TakeDamage(Data.Damage*_levelScale*(Elite?1.25f:1),Position);}
        if(Miniboss&&Cooldown<=0){Cooldown=2;g.Weapons.Radial(Position,10,115,Data.Damage*_levelScale,Data.Realm);}
        QueueRedraw();
    }
    public virtual void Damage(float amount,Element element,bool apply=true,bool critical=false)
    {
        if(!Active)return;if(Elite&&Modifier==5)amount*=.65f;Health-=amount;Flash=.8f;var g=GameManager.Instance;
        if(apply)
        {switch(element){case Element.Fire:Burn=2.5f*g.Player!.Stats.ElementPower;break;case Element.Frost:Slow=2.5f*g.Player!.Stats.ElementPower;break;case Element.Nature:Root=(IsBoss?.18f:.7f)*g.Player!.Stats.ElementPower;break;case Element.Lightning:Stun=IsBoss?0:.15f;break;}}
        if(g.Settings.Numbers)g.Effects.Number(Position-new Vector2(0,14),(int)amount,critical?Palette.Gold:Palette.Paper);
        g.Effects.Burst(Position-new Vector2(0,12),Palette.ElementColor(element),critical?7:3);
        if(Health<=0)Die();
    }
    protected virtual void Die()
    {
        Active=false;if(Elite&&Modifier==6){for(int i=0;i<2;i++)GameManager.Instance.Enemies.Spawn(Data.Realm*6+3,GameManager.Instance.World.SafePoint(Position+new Vector2(i==0?-20:20,0)));}
        GameManager.Instance.Enemies.Defeated(this);GameManager.Instance.Effects.Burst(Position,Palette.Realm(Data.Realm),Elite?20:9);Deactivate();
    }
    public override void _Draw()
    {
        if(!Active||Data==null)return;
        var color=Flash>0?new Color(1.7f,1.7f,1.7f):Colors.White;
        if(Elite){DrawArc(new Vector2(0,-12),18,0,Mathf.Tau,24,Modifier is >=0 and <4?Palette.ElementColor((Element)(Modifier==0?0:Modifier==2?1:Modifier==1?2:4)):Palette.Gold,1);color=new Color(1.15f,1.08f,.9f);}
        DrawTextureRectRegion(Art.Get($"Enemies/enemy_{Data.Realm}_{(int)Data.Kind}.png"),new Rect2(-16,-29,32,32),new Rect2(((int)(Age*7)%4)*32,0,32,32),color);
        if(Health<MaxHealth){DrawRect(new Rect2(-12,-33,24,2),Palette.Ink);DrawRect(new Rect2(-12,-33,24*Health/MaxHealth,2),Elite?Palette.Gold:Palette.Teal);}
        if(Burn>0)DrawRect(new Rect2(-1,-32,2,3),Palette.ElementColor(Element.Fire));
        if(Root>0)DrawArc(new Vector2(0,-2),12,0,Mathf.Tau,12,Palette.ElementColor(Element.Nature),1);
    }
}
