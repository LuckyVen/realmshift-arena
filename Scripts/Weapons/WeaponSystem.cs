using Godot;
namespace Realmshift;
public sealed class WeaponInstance
{
    public WeaponData Data=null!;
    public Element Element=Element.Arcane;
    public int Rarity;
    public float Multiplier=>1+Rarity*.12f;
}
public partial class WeaponSystem : Node2D
{
    public PlayerController Player {get;set;}=null!;
    public WeaponInstance[] Slots {get;private set;}=new WeaponInstance[2];
    public WeaponInstance Current=>Slots[Slot];
    public int Slot {get;private set;}
    public float AbilityTimer {get;private set;}
    public float AbilityDuration {get;private set;}
    public float Charge {get;private set;}
    private float _cooldown,_passive;
    private GameManager G=>GameManager.Instance;
    public override void _Ready(){Equip(0,0,Element.Arcane,0);Equip(1,1,Element.Nature,0);ZIndex=20;}
    public void Equip(int slot,int kind,Element element,int rarity)
    {Slots[slot]=new(){Data=Catalog.Weapons[kind],Element=element,Rarity=rarity};SaveManager.Data.Weapons.Add(kind);}
    public void Switch(int slot){if(slot is <0 or >1||!Player.Stats.SecondSlot&&slot==1)return;Release(Player.Aim);Slot=slot;G.Audio.Play("ui");}
    public override void _PhysicsProcess(double delta)
    {
        if(!G.Running)return;float dt=(float)delta;_cooldown=Mathf.Max(0,_cooldown-dt);AbilityTimer=Mathf.Max(0,AbilityTimer-dt);_passive-=dt;
        if(Current.Data.Kind==WeaponKind.Spellbook&&_passive<=0)
        {_passive=1.1f/Player.Stats.AttackRate;G.Weapons.Area(Player.Position,72*Player.Stats.Area,Damage*.65f,Current.Element);G.Effects.Sigil(Player.Position,72*Player.Stats.Area,Current.Element,.4f);}
        if(Current.Data.Kind==WeaponKind.Orbs&&_passive<=0)
        {
            _passive=.24f;for(int i=0;i<Player.Stats.OrbCount;i++){Vector2 p=Player.Position+Vector2.FromAngle((float)Time.GetTicksMsec()/650f+i*Mathf.Tau/Player.Stats.OrbCount)*30;var e=G.Enemies.Nearest(p,12);if(e!=null)G.Weapons.Hit(e,Damage*.35f,Current.Element,p);}
        }
        QueueRedraw();
    }
    private float Damage=>Current.Data.Damage*Player.Stats.Damage*Current.Multiplier*(Player.Power>0?1.6f:1);
    public void Primary(float dt,Vector2 aim)
    {
        if(Current.Data.Kind==WeaponKind.Bow){Charge=Mathf.Min(1.2f,Charge+dt);return;}
        if(_cooldown>0)return;_cooldown=Current.Data.Interval/Player.Stats.AttackRate;Fire(aim,1);
    }
    public void Release(Vector2 aim)
    {if(Charge<=0)return;if(_cooldown<=0){_cooldown=Current.Data.Interval/Player.Stats.AttackRate;Fire(aim,.7f+Charge*1.5f);}Charge=0;}
    private void Fire(Vector2 aim,float charged)
    {
        var s=Player.Stats;var data=Current.Data;Vector2 start=Player.Position+new Vector2(0,-11)+aim*12;float damage=Damage*charged;float range=data.Range*s.Range;Player.Avatar.Attack=1;
        SaveManager.Data.Mastery[(int)data.Kind]=SaveManager.Data.Mastery.GetValueOrDefault((int)data.Kind)+1;
        switch(data.Kind)
        {
            case WeaponKind.Blade:
                G.Weapons.Arc(Player.Position,aim,54*s.Range*s.Area,damage,Current.Element,1.8f);G.Audio.Play("blade");break;
            case WeaponKind.Gauntlets:
                G.Weapons.Arc(Player.Position,aim,40*s.Range,damage,Current.Element,1.4f);G.Effects.Impact(start+aim*12,Current.Element,.85f,aim,false);Player.Velocity+=aim*45;G.Audio.Play("blade");break;
            case WeaponKind.Staff:
                G.Projectiles.Spawn(start,aim*data.Speed,damage,Current.Element,false,range/data.Speed,s.Pierce,3,5);G.Audio.Play("cast");break;
            case WeaponKind.Chakram:
                G.Projectiles.Spawn(start,aim*data.Speed,damage,Current.Element,false,1.7f,s.Pierce+8,4,7,true);G.Audio.Play("blade");break;
            case WeaponKind.Spellbook:
                for(int i=0;i<3+s.ExtraProjectiles;i++)G.Projectiles.Spawn(start,Vector2.FromAngle(aim.Angle()+(i-(2+s.ExtraProjectiles)/2f)*.15f)*data.Speed,damage,Current.Element,false,range/data.Speed,s.Pierce,0,3);G.Audio.Play("cast");break;
            default:
                int n=1+s.ExtraProjectiles+(data.Kind==WeaponKind.Orbs?Mathf.Max(0,s.OrbCount-2):0);
                for(int i=0;i<n;i++)
                {var dir=aim.Rotated((i-(n-1)/2f)*.13f);G.Projectiles.Spawn(start,dir*data.Speed,damage,Current.Element,false,range/data.Speed,s.Pierce+(data.Kind==WeaponKind.Bow?(charged>1.5f?2:1):0),data.Kind==WeaponKind.Bow?1:data.Kind==WeaponKind.Orbs?5:0,data.Kind==WeaponKind.Orbs?4:3,s.ReturnShots);}
                G.Audio.Play(data.Kind==WeaponKind.Bow?"bow":"shoot");break;
        }
        G.Effects.Cast(start,Current.Element,aim,data.Kind is WeaponKind.Staff or WeaponKind.Blade||charged>1.8f);
    }
    public void Secondary(Vector2 aim)
    {
        if(AbilityTimer>0)return;AbilityDuration=Current.Data.AbilityCooldown;AbilityTimer=AbilityDuration;float d=Damage;Vector2 pos=Player.Position;var s=Player.Stats;
        switch(Current.Data.Kind)
        {
            case WeaponKind.Wand:for(int i=0;i<14;i++)G.Projectiles.Spawn(pos,Vector2.FromAngle(i*Mathf.Tau/14)*250,d*.9f,Current.Element,false,1.2f,s.Pierce);break;
            case WeaponKind.Bow:for(int i=0;i<8;i++)G.Weapons.Zone(G.World.SafePoint(pos+aim*130+new Vector2(GD.Randf()*80-40,GD.Randf()*60-30)),25,1.2f,d*1.2f,Current.Element);break;
            case WeaponKind.Blade:G.Weapons.Area(pos,90*s.Area,d*2,Current.Element);G.Effects.Ring(pos,90*s.Area,Palette.ElementColor(Current.Element),.5f);break;
            case WeaponKind.Staff:G.Weapons.Zone(G.World.SafePoint(pos+aim*120),80*s.Area,4*s.Duration,d*.8f,Current.Element);break;
            case WeaponKind.Chakram:for(int i=0;i<6;i++)G.Projectiles.Spawn(pos,aim.Rotated((i-2.5f)*.35f)*230,d,Current.Element,false,2,9,4,6,true);break;
            case WeaponKind.Orbs:Player.Stats.Shield+=22;G.Weapons.Area(pos,100,d*1.5f,Current.Element);break;
            case WeaponKind.Spellbook:Player.Stats.Shield+=18;G.Weapons.Zone(pos,95*s.Area,5*s.Duration,d*.6f,Current.Element);break;
            case WeaponKind.Gauntlets:Player.Invincible=.8f;Player.Velocity=aim*450;G.Weapons.Arc(pos,aim,140*s.Range,d*2,Current.Element,1);G.Effects.Line(pos,pos+aim*140,Palette.ElementColor(Current.Element),.35f);break;
        }
        Player.Avatar.Attack=1;G.Effects.Cast(pos+new Vector2(0,-10),Current.Element,aim,true);G.Audio.Play("cast");G.Camera.Shake(2);
    }
    public override void _Draw()
    {
        if(Current==null)return;
        if(Current.Data.Kind==WeaponKind.Orbs)
        {for(int i=0;i<Player.Stats.OrbCount;i++){float age=(float)Time.GetTicksMsec()/1000f;var p=Vector2.FromAngle(age*1000/650f+i*Mathf.Tau/Player.Stats.OrbCount)*30+new Vector2(0,-10);VfxSprites.Bolt(this,p,Vector2.Right,Current.Element,5,age+i*.1f);}}
        if(Current.Data.Kind==WeaponKind.Spellbook)
        {VfxSprites.Field(this,new Vector2(0,-4),22,Current.Element,(float)Time.GetTicksMsec()/1000f,.45f);DrawTextureRect(Art.Weapon(WeaponKind.Spellbook),new Rect2(-25,-29+Mathf.Sin((float)Time.GetTicksMsec()/700f)*2,18,18),false);}
        if(Charge>0){DrawArc(new Vector2(0,-13),18,-Mathf.Pi/2,-Mathf.Pi/2+Mathf.Tau*Charge/1.2f,24,Palette.Gold,2);VfxSprites.Bolt(this,Player.Aim*19+new Vector2(0,-11),Player.Aim,Current.Element,1,0,false,.7f);}
    }
}
