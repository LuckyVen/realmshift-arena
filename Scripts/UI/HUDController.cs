using Godot;
namespace Realmshift;
public partial class HUDController : Control
{
    public static readonly Rect2[] WeaponSlots={new(273,300,44,44),new(325,300,44,44)};
    public CircularMinimap Radar {get;private set;}=null!;
    public float HealthRatio {get;private set;}=1;
    private Font _font=null!;
    private PlayerController? _tracked;
    private float _ghost=1,_damageHold,_time;
    public override void _Ready()
    {
        MouseFilter=MouseFilterEnum.Ignore;_font=UIFactory.SmallFont;Size=new(640,360);
        Radar=new CircularMinimap{Name="CircularRadar",Position=new(548,8)};AddChild(Radar);
    }
    public override void _Process(double delta)
    {
        if(!Visible)return;var p=GameManager.Instance.Player;if(p==null)return;
        float dt=(float)delta;float ratio=Mathf.Clamp(p.Stats.Health/Mathf.Max(1,p.Stats.MaxHealth),0,1);
        if(!ReferenceEquals(p,_tracked)){_tracked=p;HealthRatio=_ghost=ratio;}
        if(ratio<HealthRatio)_damageHold=.4f;HealthRatio=ratio;_damageHold=Mathf.Max(0,_damageHold-dt);
        if(ratio>_ghost)_ghost=ratio;else if(_damageHold<=0)_ghost=Mathf.MoveToward(_ghost,ratio,dt*.45f);
        if(GameManager.Instance.Running)_time+=dt;QueueRedraw();
    }
    private void Text(string s,Vector2 p,Color c,int size=8,float width=-1)=>DrawString(_font,p,s,HorizontalAlignment.Left,width,size,c);
    private void CenterText(string s,Vector2 p,Color c,int size=8)=>HUDWidgets.CenterText(this,_font,s,p,size,c);
    private void Bar(Rect2 rect,float value,float max,Color c)
    {DrawRect(rect,Palette.Ink);DrawRect(new Rect2(rect.Position+Vector2.One,new Vector2((rect.Size.X-2)*Mathf.Clamp(value/Mathf.Max(1,max),0,1),rect.Size.Y-2)),c);DrawRect(rect,new Color("617a6a"),false,1);}
    private void Health(PlayerController p)
    {
        HUDWidgets.Frame(this,new Rect2(29,17,127,16),new Color("2c3135"),new Color("c3c29d"));
        DrawRect(new Rect2(34,21,117*_ghost,8),new Color("e9bb72"));
        float pulse=GameManager.Instance.Settings.ReducedFlash?0:Mathf.Sin(_time*2)*.04f;
        Color red=new Color("d86562");if(HealthRatio<.25f)red=red.Lightened(.12f+pulse);
        DrawRect(new Rect2(34,21,117*HealthRatio,8),red);DrawRect(new Rect2(34,21,117*HealthRatio,2),red.Lightened(.25f));
        HUDWidgets.Frame(this,new Rect2(10,11,26,26),new Color("ede0b8"),new Color("849684"));
        DrawTextureRect(Art.Get("UI/HUD/heart.svg"),new Rect2(14,15,18,18),false);
        if(p.Stats.Shield>0){DrawRect(new Rect2(38,36,113,2),Palette.Ink);DrawRect(new Rect2(38,36,113*Mathf.Clamp(p.Stats.Shield/p.Stats.MaxHealth,0,1),2),Palette.Teal);}
        DrawTextureRect(Art.Get("UI/HUD/dash.svg"),new Rect2(164,17,14,14),false);
        int visible=Mathf.Min(6,p.Stats.DashCharges);
        for(int i=0;i<visible;i++)
        {
            var r=new Rect2(181+i*10,20,8,10);DrawRect(r,Palette.Ink);DrawRect(new Rect2(r.Position+Vector2.One,r.Size-Vector2.One*2),new Color("395a58"));
            float charge=i<p.Charges?1:i==p.Charges?1-Mathf.Clamp(p.DashRecharge/Mathf.Max(.001f,p.Stats.DashCooldown),0,1):0;
            if(charge>0)DrawRect(new Rect2(r.Position+new Vector2(1,9-8*charge),6,8*charge),i<p.Charges?Palette.Teal:Palette.Teal.Darkened(.28f));
            if(i<p.Charges)DrawRect(new Rect2(r.Position+new Vector2(2,2),4,1),Palette.Paper);
        }
        if(p.Stats.DashCharges>6)Text("+"+(p.Stats.DashCharges-6),new(183,40),Palette.Teal,7);
    }
    private void Hotbar(PlayerController p)
    {
        for(int i=0;i<WeaponSlots.Length;i++)
        {
            var rect=WeaponSlots[i];var w=p.Weapons.Slots[i];bool locked=i==1&&!p.Stats.SecondSlot,active=p.Weapons.Slot==i;
            var color=locked?new Color("435254"):active?Palette.Gold:new Color("738979");
            HUDWidgets.Frame(this,rect,active?new Color("334943"):new Color("1b3036"),color,active);
            DrawTextureRect(Art.Weapon(w.Data.Kind),new Rect2(rect.Position+new Vector2(6,5),32,32),false,locked?new Color(.4f,.4f,.4f):Colors.White);
            Text(InputBindings.Label(i==0?"slot1":"slot2"),rect.Position+new Vector2(5,10),locked?Palette.Muted:Palette.Paper,7,14);
            var e=rect.Position+new Vector2(36,36);HUDWidgets.Diamond(this,e,7,Palette.Ink);
            DrawTextureRect(Art.Get($"UI/HUD/element_{(int)w.Element}.svg"),new Rect2(e-new Vector2(5,5),10,10),false,locked?new Color(.4f,.4f,.4f):Colors.White);
            if(locked){DrawRect(new Rect2(rect.Position+new Vector2(16,16),12,10),Palette.Ink);DrawRect(new Rect2(rect.Position+new Vector2(19,13),6,7),Palette.Gold,false,2);DrawRect(new Rect2(rect.Position+new Vector2(20,19),4,3),Palette.Gold);}
            else if(w.Rarity>0)for(int dot=0;dot<Mathf.Min(5,w.Rarity);dot++)DrawRect(new Rect2(rect.Position+new Vector2(8+dot*4,40),2,1),Palette.Gold);
        }
        var current=p.Weapons.Current;Color element=Palette.ElementColor(current.Element);
        HUDWidgets.Ability(this,new(248,324),Art.Get("UI/HUD/skill.svg"),element,p.Weapons.AbilityTimer,p.Weapons.AbilityDuration,GameManager.Instance.Settings.ReducedFlash,_time);
        HUDWidgets.Ability(this,new(394,324),Art.Get("UI/HUD/hero.svg"),Palette.ElementColor(HeroReadiness.ElementFor(p.Avatar.Config.Hero)),p.HeroTimer,p.Stats.HeroCooldown,GameManager.Instance.Settings.ReducedFlash,_time);
        CenterText(InputBindings.Label("secondary"),new(248,349),Palette.Muted,7);CenterText(InputBindings.Label("hero"),new(394,349),Palette.Muted,7);
        var xp=GameManager.Instance.Experience;DrawRect(new Rect2(273,348,96,2),new Color("304946"));DrawRect(new Rect2(273,348,96*Mathf.Clamp(xp.Experience/Mathf.Max(1,xp.Required),0,1),2),Palette.Teal);
        CenterText(current.Element.ToString().ToUpper()+" / "+current.Data.DisplayName.ToUpper(),new(321,359),element,7);
    }
    private static string Compact(int value)=>value>=1000000?(value/1000000f).ToString("0.#")+"M":value>=10000?(value/1000f).ToString("0")+"K":value>=1000?(value/1000f).ToString("0.#")+"K":value.ToString();
    public override void _Draw()
    {
        var g=GameManager.Instance;var p=g.Player;if(p==null)return;Health(p);Hotbar(p);
        string wave=g.State==RunState.Tutorial?"TRAINING":$"LEVEL {g.Level.Level:00}"+(g.Level.Level%10==0?" / BOSS":"");
        CenterText(wave,new(320,19),Palette.Paper,10);CenterText(Catalog.Realms[g.World.Realm].DisplayName,new(320,33),Palette.Realm(g.World.Realm),8);
        HUDWidgets.Diamond(this,new(554,99),3,Palette.Gold);Text(Compact(SaveManager.Data.Shards),new(561,102),Palette.Gold,7,37);
        DrawRect(new Rect2(606,96,4,4),CircularMinimap.EnemyColor);Text(Compact(g.Enemies.Count+g.Level.RemainingSpawns),new(615,102),Palette.Muted,7,21);
        if(g.Combo>1)Text($"{g.Combo} CHAIN",new(15,57),Palette.Gold,9);
        if(g.Enemies.Boss is { } boss)
        {CenterText(boss.BossName.ToUpper(),new(320,55),Palette.Paper,9);Bar(new Rect2(193,62,254,6),boss.Health,boss.MaxHealth,Palette.Realm(boss.Realm));CenterText("PHASE "+boss.Phase,new(320,79),Palette.Gold,8);}
        Text(InputBindings.Label("pause")+" / PAUSE",new(12,354),Palette.Muted,7);
        if(!g.Running)return;
        string prompt="";foreach(var c in g.World.Chests)if(c.DistanceTo(p.Position)<42)prompt=InputBindings.Label("interact")+" / OPEN REALM CACHE";
        foreach(var s in g.World.Shrines)if(s.DistanceTo(p.Position)<35)prompt=InputBindings.Label("interact")+" / AWAKEN SHRINE";
        if(g.World.PortalActive&&p.Position.DistanceTo(g.World.PortalPosition)<55)prompt=InputBindings.Label("interact")+" / ENTER NEXT REALM";
        if(prompt.Length>0)CenterText(prompt,new(320,287),Palette.Gold,9);
    }
}
