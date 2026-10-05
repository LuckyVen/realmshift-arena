using Godot;
namespace Realmshift;
public partial class PlayerController : CharacterBody2D
{
    public PlayerStats Stats {get;private set;}=new();
    public AvatarRenderer Avatar {get;private set;}=null!;
    public WeaponSystem Weapons {get;private set;}=null!;
    public HeroReadiness Readiness {get;private set;}=null!;
    public Vector2 Aim {get;private set;}=Vector2.Right;
    public float DashTimer {get;private set;}
    public float DashRecharge {get;private set;}
    public float HeroTimer {get;private set;}
    public float Invincible {get;set;}
    public int Charges {get;private set;}=1;
    public float Haste {get;set;}
    public float Power {get;set;}
    public bool Dead {get;private set;}
    private Vector2 _dashDir,_lastMove=Vector2.Down;
    private float _afterimage,_regen;
    public override void _Ready()
    {
        Avatar=GetNode<AvatarRenderer>("Avatar");Avatar.Config=SaveManager.Data.Avatar.Copy();Weapons=new WeaponSystem{Name="WeaponSystem",Player=this};AddChild(Weapons);
        Readiness=new HeroReadiness{Name="HeroReadiness",Player=this};AddChild(Readiness);
        ZIndex=0;TextureFilter=TextureFilterEnum.Nearest;
    }
    public override void _PhysicsProcess(double delta)
    {
        var g=GameManager.Instance;if(!g.Running||Dead){Velocity=Vector2.Zero;Avatar.Running=false;return;}
        float dt=(float)delta;Invincible=Mathf.Max(0,Invincible-dt);HeroTimer=Mathf.Max(0,HeroTimer-dt);Haste=Mathf.Max(0,Haste-dt);Power=Mathf.Max(0,Power-dt);
        if(Charges<Stats.DashCharges){DashRecharge-=dt;if(DashRecharge<=0){Charges++;DashRecharge=Stats.DashCooldown;}}
        _regen+=dt;if(_regen>=1){Stats.Health=Mathf.Min(Stats.MaxHealth,Stats.Health+Stats.Regen);_regen=0;}
        Vector2 move=Input.GetVector("left","right","up","down");if(move.LengthSquared()>.02f)_lastMove=move;
        Vector2 stick=new(Input.GetJoyAxis(0,JoyAxis.RightX),Input.GetJoyAxis(0,JoyAxis.RightY));
        if(stick.Length()>.25f)Aim=stick.Normalized();else if(SaveManager.Data.Settings.AutoAim||Input.GetConnectedJoypads().Count>0)
        {var e=g.Enemies.Nearest(Position,420);Aim=e!=null?(e.Position-Position).Normalized():_lastMove;}
        else Aim=(GetGlobalMousePosition()-Position-new Vector2(0,-12)).Normalized();
        if(Input.IsActionJustPressed("dash")&&Charges>0)
        {Charges--;DashRecharge=Stats.DashCooldown;DashTimer=.17f*Stats.DashLength;_dashDir=move.LengthSquared()>.02f?move.Normalized():Aim;Invincible=.24f;g.Audio.Play("dash");g.Effects.Dash(Position,_dashDir,Weapons.Current.Element);}
        if(DashTimer>0)
        {DashTimer-=dt;Velocity=_dashDir*460;_afterimage-=dt;if(_afterimage<=0){_afterimage=.035f;g.Effects.Ghost(Position,Avatar.Config,Avatar.Facing);}if(DashTimer<=0)g.Effects.Dash(Position,_dashDir,Weapons.Current.Element,true);}
        else Velocity=Velocity.MoveToward(move*Stats.MoveSpeed*(Haste>0?1.45f:1),dt*(move==Vector2.Zero?1900:1600));
        MoveAndSlide();Position=Position.Clamp(new Vector2(24,24),WorldManager.Size-new Vector2(24,24));
        if(Input.IsActionPressed("attack"))Weapons.Primary(dt,Aim);else Weapons.Release(Aim);
        if(Input.IsActionJustPressed("secondary"))Weapons.Secondary(Aim);
        if(Input.IsActionJustPressed("hero")&&HeroTimer<=0)CharacterAbility();
        if(Input.IsActionJustPressed("slot1"))Weapons.Switch(0);if(Input.IsActionJustPressed("slot2"))Weapons.Switch(1);
        if(Input.IsActionJustPressed("interact"))g.Interact();
        Avatar.Running=Velocity.LengthSquared()>12;Avatar.Dash=DashTimer;
        if(Mathf.Abs(Aim.X)>Mathf.Abs(Aim.Y))Avatar.Facing=Aim.X>0?3:2;else Avatar.Facing=Aim.Y>0?0:1;
        QueueRedraw();
    }
    public void TakeDamage(float damage,Vector2 from)
    {
        if(Invincible>0||Dead||!GameManager.Instance.Running)return;
        float hit=Mathf.Max(1,damage-Stats.Armor);float absorb=Mathf.Min(hit,Stats.Shield);Stats.Shield-=absorb;hit-=absorb;Stats.Health-=hit;Invincible=.65f;Avatar.Hit=1;
        Velocity+=(Position-from).Normalized()*55;var g=GameManager.Instance;g.Audio.Play("hurt");g.Camera.Shake(3);g.UI.HurtFlash();g.Effects.Number(Position,(int)hit,new Color("ef9592"));
        if(Stats.Thorns)g.Weapons.Area(Position,65,20,Element.Nature);
        if(Stats.Health<=0){Stats.Health=0;Dead=true;Avatar.Defeated=true;g.EndRun(false);}
    }
    public void Heal(float amount)=>Stats.Health=Mathf.Min(Stats.MaxHealth,Stats.Health+amount);
    public void Recharge(){Charges=Stats.DashCharges;DashRecharge=0;}
    private void CharacterAbility()
    {
        HeroTimer=Stats.HeroCooldown;var g=GameManager.Instance;int h=Avatar.Config.Hero;float power=Stats.HeroPower;
        switch(h)
        {
            case 0:Heal(22*power);Stats.Shield+=12*power;g.Effects.Burst(Position,Palette.Teal,25);break;
            case 1:g.Weapons.Area(Position,130*power,35*power,Element.Nature);Haste=5;break;
            case 2:g.Weapons.Zone(Position,100*power,5*Stats.Duration,15*power,Element.Fire);break;
            case 3:Stats.Shield+=40*power;g.Weapons.Area(Position,100,25*power,Element.Arcane);break;
            case 4:g.Weapons.Area(Position,150*power,30*power,Element.Frost);Invincible=1.5f;break;
            case 5:for(int i=0;i<10;i++)g.Projectiles.Spawn(Position,Vector2.FromAngle(i*Mathf.Tau/10)*250,28*power,Element.Lightning,false,1.2f,2);break;
            case 6:Power=8;g.Weapons.Area(Position,85,40*power,Element.Arcane);break;
            case 7:Haste=8;Invincible=1.1f;g.Weapons.Area(Position,80,35*power,Element.Arcane);break;
        }
        g.Audio.Play("cast");g.Effects.Ring(Position,100*power,Palette.Teal,.6f);Avatar.Attack=1;
    }
    public override void _Draw()
    {
        DrawSetTransform(new Vector2(0,-1),0,new Vector2(1,.35f));DrawCircle(Vector2.Zero,9,new Color(0,0,0,.28f));DrawSetTransform(Vector2.Zero);
        if(Stats.Shield>0)DrawArc(new Vector2(0,-12),17,0,Mathf.Tau,24,new Color(Palette.Teal,.7f),1);
        if(Weapons!=null&&!Dead&&Weapons.Current.Data.Kind!=WeaponKind.Orbs&&Weapons.Current.Data.Kind!=WeaponKind.Spellbook)
        {DrawSetTransform(new Vector2(0,-11)+Aim*10,Aim.Angle()+Mathf.Pi/4+Avatar.Attack*.6f);DrawTextureRect(Art.Weapon(Weapons.Current.Data.Kind),new Rect2(-12,-12,24,24),false,new Color(1,1,1,.92f));DrawSetTransform(Vector2.Zero);}
    }
}
