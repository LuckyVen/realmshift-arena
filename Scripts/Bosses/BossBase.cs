using Godot;
namespace Realmshift;
public partial class BossBase : EnemyBase
{
    public int Realm {get;private set;}
    public int Phase {get;private set;}=1;
    public string BossName=>Catalog.Realms[Realm].BossName;
    private int _pattern;
    private float _phaseFlash;
    public void SpawnBoss(int realm,Vector2 position,int level)
    {
        Realm=realm;Spawn(Catalog.Enemies[realm*6+4],position,level);Scale=Vector2.One;Radius=32;
        MaxHealth=1100+realm*750+Mathf.Max(0,level-40)*80;Health=MaxHealth;Phase=1;Cooldown=2.5f;ZIndex=16;
    }
    public override void _PhysicsProcess(double delta)
    {
        var g=GameManager.Instance;if(!g.Running||!Active)return;
        float dt=(float)delta;Age+=dt;Flash=Mathf.Max(0,Flash-dt*5);Slow=Mathf.Max(0,Slow-dt);Burn=Mathf.Max(0,Burn-dt);_phaseFlash=Mathf.Max(0,_phaseFlash-dt);Cooldown-=dt;
        int newPhase=Health<MaxHealth*.3f?3:Health<MaxHealth*.65f?2:1;
        if(newPhase>Phase){Phase=newPhase;_phaseFlash=1;Cooldown=1.1f;g.UI.Announce("PHASE "+Phase,"The guardian changes its rhythm.",2);g.Camera.Shake(4);g.Audio.Play("boss");}
        var player=g.Player!;Vector2 dir=g.World.PathDirection(Position,player.Position);float dist=Position.DistanceTo(player.Position);
        float speed=Realm==2?22:Realm==1?18:26;Velocity=dir*(dist>115?speed:0)*(Slow>0?.7f:1);MoveAndSlide();
        if(dist<42)player.TakeDamage(18+Realm*4,Position);
        if(Cooldown<=0){AttackPattern();_pattern++;Cooldown=(Realm==3?2.1f:2.7f)/(1+(Phase-1)*.18f);}
        QueueRedraw();
    }
    private void AttackPattern()
    {
        var g=GameManager.Instance;var p=g.Player!.Position;Element element=Realm==0?Element.Nature:Realm==1?Element.Fire:Realm==2?Element.Frost:Element.Arcane;
        float damage=15+Realm*3;
        switch(Realm)
        {
            case 0:
                if(_pattern%3==0)
                {for(int i=0;i<3+Phase;i++)g.Weapons.Hazard(g.World.SafePoint(p+Vector2.FromAngle(i*2.3f)*40),23,.95f,1,damage,element);}
                else if(_pattern%3==1)g.Weapons.Radial(Position-new Vector2(0,24),10+Phase*3,85+Phase*10,damage,Realm,Age*.3f);
                else
                {g.Weapons.Beam(Position,p+(p-Position).Normalized()*90,13,1,damage+5,element);if(Phase>1)for(int i=0;i<Phase;i++)g.Enemies.Spawn(3,g.World.SafePoint(Position+Vector2.FromAngle(i*3)*80),false);}
                break;
            case 1:
                if(_pattern%3==0)
                {for(int i=0;i<6+Phase*2;i++)g.Weapons.Hazard(g.World.SafePoint(p+Vector2.FromAngle(i*2.4f)*((i+1)*16)),22,1.1f,1.7f,damage,element);}
                else if(_pattern%3==1){g.Weapons.Radial(Position,12+Phase*4,125,damage,Realm,Age);if(Phase>1)g.Weapons.Radial(Position,8,75,damage,Realm,Age+.2f);}
                else{for(int i=-Phase;i<=Phase;i++)g.Weapons.Beam(Position+new Vector2(i*45,-20),p+new Vector2(i*45,80),11,1.2f,damage+7,element);}
                break;
            case 2:
                if(_pattern%3==0)
                {Vector2 dir=(p-Position).Normalized();for(int i=0;i<7+Phase*2;i++)g.Projectiles.Spawn(Position-new Vector2(0,22),dir.Rotated((i-(6+Phase*2)/2f)*.17f)*105,damage,element,true,4,0,0,4);}
                else if(_pattern%3==1)
                {for(int i=0;i<8+Phase;i++){Vector2 pt=p+Vector2.FromAngle(i*Mathf.Tau/(8+Phase))*80;g.Weapons.Hazard(g.World.SafePoint(pt),20,1.2f,1.3f,damage,element);}g.UI.Announce("FROST CROWN","Stay within the eye, then dash out.",1.3f);}
                else{g.Weapons.Beam(Position,p+(p-Position).Normalized()*110,17,.95f,damage+8,element);if(Phase>1)g.Weapons.Radial(Position,8,80,damage,Realm,Age);}
                break;
            case 3:
                if(_pattern%4==0)
                {for(int i=0;i<Phase+2;i++){float a=i*Mathf.Pi/(Phase+2)+Age;Vector2 d=Vector2.FromAngle(a);g.Weapons.Beam(p-d*240,p+d*240,9,1.3f,damage,element);}}
                else if(_pattern%4==1)
                {g.Weapons.Radial(Position,16+Phase*4,100,damage,Realm,Age);g.Weapons.Radial(Position,8,155,damage,Realm,Age+.2f);}
                else if(_pattern%4==2)
                {for(int i=0;i<Phase+3;i++)g.Weapons.Hazard(g.World.SafePoint(p+Vector2.FromAngle(i*2.4f)*i*35),26,.85f,1.8f,damage,element);}
                else{Position=g.World.SafePoint(p+Vector2.FromAngle(Age)*155);g.Effects.Burst(Position,Palette.Realm(Realm),25);g.Weapons.Hazard(Position,60,1,1,damage+6,element);}
                break;
        }
        g.Audio.Play("cast",.65f);
    }
    public override void Damage(float amount,Element element,bool apply=true,bool critical=false)
    {base.Damage(amount,element,apply,critical);}
    public override void _Draw()
    {
        if(!Active)return;DrawTextureRectRegion(Art.Get($"Bosses/boss_{Realm}.png"),new Rect2(-48,-82,96,96),new Rect2((int)(Age*5)%4*96,0,96,96),Flash>0?new Color(1.6f,1.6f,1.6f):Colors.White);
        if(_phaseFlash>0)DrawArc(new Vector2(0,-28),52+(1-_phaseFlash)*25,0,Mathf.Tau,48,new Color(Palette.Realm(Realm),_phaseFlash),2);
    }
}
