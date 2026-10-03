using Godot;
namespace Realmshift;
public partial class HUDController : Control
{
    private Font _font=null!;
    public override void _Ready(){MouseFilter=MouseFilterEnum.Ignore;_font=UIFactory.Font;Size=new(640,360);}
    public override void _Process(double delta){if(Visible)QueueRedraw();}
    private void Text(string s,Vector2 p,Color c,int size=10)=>DrawString(_font,p,s,HorizontalAlignment.Left,-1,size,c);
    private void Bar(Rect2 rect,float value,float max,Color c){DrawRect(rect,new Color("0b2026dc"));DrawRect(new Rect2(rect.Position+Vector2.One,new Vector2((rect.Size.X-2)*Mathf.Clamp(value/Mathf.Max(1,max),0,1),rect.Size.Y-2)),c);DrawRect(rect,new Color("4d6b60"),false,1);}
    public override void _Draw()
    {
        var g=GameManager.Instance;var p=g.Player;if(p==null)return;
        DrawRect(new Rect2(8,8,155,41),new Color("081f2bdb"));DrawRect(new Rect2(8,8,155,41),new Color("547668"),false,1);
        Text(Catalog.Heroes[p.Avatar.Config.Hero].DisplayName,new(15,21),Palette.Gold);Text($"{(int)p.Stats.Health}/{(int)p.Stats.MaxHealth}",new(103,21),Palette.Paper,9);
        Bar(new Rect2(15,28,139,6),p.Stats.Health,p.Stats.MaxHealth,new Color("b9c978"));
        if(p.Stats.Shield>0)Text("SHIELD "+(int)p.Stats.Shield,new(15,44),Palette.Teal,9);else Text("RANK "+g.Experience.Rank,new(15,44),Palette.Muted,9);
        Bar(new Rect2(15,47,139,2),g.Experience.Experience,g.Experience.Required,new Color("98c9d1"));
        string wave=g.State==RunState.Tutorial?"TRAINING":$"LEVEL {g.Level.Level:00}"+(g.Level.Level%10==0?" / BOSS":"");Text(wave,new(268,19),Palette.Paper,11);
        Text(Catalog.Realms[g.World.Realm].DisplayName,new(244,33),Palette.Realm(g.World.Realm),9);
        DrawRect(new Rect2(511,8,121,41),new Color("081f2bdb"));Text($"SHARDS {SaveManager.Data.Shards}",new(520,21),Palette.Gold,9);Text("FOES "+(g.Enemies.Count+g.Level.RemainingSpawns),new(520,34),Palette.Paper,9);Text("SCORE "+g.Score,new(520,45),Palette.Muted,8);
        if(g.Combo>1)Text($"{g.Combo} CHAIN",new(17,67),Palette.Gold,11);
        if(g.Enemies.Boss is { } boss)
        {Text(boss.BossName.ToUpper(),new(210,55),Palette.Paper,9);Bar(new Rect2(193,62,254,6),boss.Health,boss.MaxHealth,Palette.Realm(boss.Realm));Text("PHASE "+boss.Phase,new(291,79),Palette.Gold,9);}
        DrawRect(new Rect2(166,315,309,37),new Color("081f2bdf"));DrawRect(new Rect2(166,315,309,37),new Color("4d7167"),false,1);
        for(int i=0;i<2;i++)
        {
            float x=174+i*99;var w=p.Weapons.Slots[i];DrawTextureRect(Art.Get($"Weapons/weapon_{(int)w.Data.Kind}.png"),new Rect2(x,321,22,22),false,i==1&&!p.Stats.SecondSlot?new Color(.35f,.35f,.35f):Colors.White);
            Text(i==1&&!p.Stats.SecondSlot?"LOCKED":w.Data.DisplayName,new(x+25,330),p.Weapons.Slot==i?Palette.Gold:Palette.Muted,8);Text(i==1&&!p.Stats.SecondSlot?"BEAT BOSS 1":InputBindings.Label(i==0?"slot1":"slot2")+" / "+w.Element,new(x+25,342),Palette.ElementColor(w.Element),8);
        }
        float cd=p.Weapons.AbilityTimer;Text(cd>0?$"RMB {cd:0.0}s":"RMB READY",new(376,329),cd>0?Palette.Muted:Palette.Teal,8);Text(p.HeroTimer>0?$"{InputBindings.Label("hero")} {p.HeroTimer:0}s":InputBindings.Label("hero")+" HERO READY",new(376,342),Palette.Paper,8);
        DrawRect(new Rect2(541,315,91,37),new Color("081f2bdf"));Text("DASH "+p.Charges+"/"+p.Stats.DashCharges,new(550,329),Palette.Teal,9);Bar(new Rect2(550,337,73,5),p.Charges>0?1:p.Stats.DashCooldown-p.DashRecharge,p.Charges>0?1:p.Stats.DashCooldown,Palette.Teal);
        // Compact minimap communicates the scale and bridge routes.
        var map=new Rect2(9,268,100,75);DrawRect(map,new Color("081f2bdd"));DrawRect(map,new Color("4d7167"),false,1);float rx=g.World.RiverX/WorldManager.Size.X*100;
        DrawRect(new Rect2(map.Position+new Vector2(rx,1),3,73),Palette.Realm(g.World.Realm).Darkened(.4f));
        foreach(var s in g.World.Shrines){Vector2 q=map.Position+s/WorldManager.Size*map.Size;DrawRect(new Rect2(q,2,2),Palette.Teal);}
        foreach(var chest in g.World.Chests){Vector2 q=map.Position+chest/WorldManager.Size*map.Size;DrawRect(new Rect2(q,1,1),Palette.Gold);}
        Vector2 player=map.Position+p.Position/WorldManager.Size*map.Size;DrawRect(new Rect2(player-new Vector2(1,1),3,3),Palette.Paper);
        if(g.World.PortalActive){Vector2 portal=map.Position+g.World.PortalPosition/WorldManager.Size*map.Size;DrawRect(new Rect2(portal,3,3),new Color("cb9bea"));Text("E / GATE",new(15,263),Palette.Gold,9);}
        Text(InputBindings.Label("pause")+" / PAUSE",new(15,355),Palette.Muted,8);
        if(g.Running)
        {
            string prompt="";foreach(var c in g.World.Chests)if(c.DistanceTo(p.Position)<42)prompt=InputBindings.Label("interact")+" / OPEN REALM CACHE";
            foreach(var s in g.World.Shrines)if(s.DistanceTo(p.Position)<35)prompt=InputBindings.Label("interact")+" / AWAKEN SHRINE";
            if(g.World.PortalActive&&p.Position.DistanceTo(g.World.PortalPosition)<55)prompt=InputBindings.Label("interact")+" / ENTER NEXT REALM";
            if(prompt.Length>0)Text(prompt,new(236,301),Palette.Gold,10);
        }
    }
}
