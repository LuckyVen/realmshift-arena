using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Realmshift;

/// <summary>Deterministic visual/structural QA. Uses an isolated save and is inactive during normal play.</summary>
public partial class PolishCapture : Node
{
    private readonly List<(string Name,Action Setup,int Frames)> _shots=new();
    private int _stage,_frames,_checks,_video;
    private string _dir="";
    private bool _shoot,_melee;
    private WorldProp? _tree;
    private GameManager G=>GameManager.Instance;
    public override void _Ready()
    {
        _dir=Array.Find(OS.GetCmdlineUserArgs(),x=>x.StartsWith("--polish-capture="))![17..];Directory.CreateDirectory(_dir);
        SaveManager.Data.Shards=1240;SaveManager.Data.Heroes=new(){0,1,2,3,4,5,6,7};SaveManager.Data.Weapons=new(){0,1,2,3,4,5,6,7};SaveManager.Data.Bosses=new(){0,1,2,3};SaveManager.Data.Tutorial=true;SaveManager.Data.Settings.AutoAim=false;SaveManager.Data.Settings.Shake=false;SaveManager.Data.Settings.Vsync=false;SaveManager.Data.Settings.Particles=2;SaveManager.Data.Avatar=AvatarConfig.FromHero(5);SaveManager.Save();G.ApplySettings();
        void Shot(string name,Action setup,int frames=30)=>_shots.Add((name,setup,frames));
        Shot("00-main-menu",G.UI.ShowMenu);Shot("01-credits",G.UI.ShowCredits);
        Shot("02-characters",()=>{G.UI.ShowCharacters();Click("SOREN");});Shot("03-customization",G.UI.ShowCreator);
        Shot("04-loadout",()=>{G.UI.ShowLoadout();Click("ELEMENTAL GAUNTLETS");});Shot("05-armory",()=>G.UI.ShowArmory(7));
        Shot("06-lore",()=>G.UI.ShowCollection("LORE"));Shot("07-settings",()=>G.UI.ShowSettings(G.UI.ShowMenu));Shot("08-controls",()=>G.UI.ShowControls(G.UI.ShowMenu));
        Shot("09-pause",()=>{Arena(0,Element.Fire);G.UI.TogglePause();});Shot("10-relics",()=>{Arena(0,Element.Arcane);G.UI.ShowUpgrades();});Shot("11-cache",()=>{Arena(0,Element.Fire);G.UI.ShowCache();});
        Shot("12-defeat",()=>{G.UI.Close();G.State=RunState.Defeat;G.UI.ShowResult(false);});Shot("13-victory",()=>{G.UI.Close();G.State=RunState.Victory;G.UI.ShowResult(true);});
        Shot("14-tree-behind",()=>Tree(new Vector2(0,-45)),80);Shot("15-tree-front",()=>Tree(new Vector2(0,22)),80);Shot("16-tree-side",()=>Tree(new Vector2(36,-22)),80);
        foreach(Element e in Enum.GetValues<Element>()){Element element=e;Shot("17-element-"+e.ToString().ToLowerInvariant(),()=>Effects(element),30);}
        foreach(WeaponKind w in Enum.GetValues<WeaponKind>()){WeaponKind weapon=w;Shot("18-weapon-"+w.ToString().ToLowerInvariant(),()=>Weapon(weapon),24);}
        for(int i=0;i<4;i++){int realm=i;Shot("19-boss-"+realm,()=>{Arena(0,(Element)(realm==0?3:realm==1?0:realm==2?1:4));G.Level.SkipTo((realm+1)*10);G.Player!.Position=G.World.SafePoint(G.Enemies.Boss!.Position+new Vector2(0,90));G.Camera.Focus=G.Enemies.Boss.Position+new Vector2(0,15);_shoot=false;},375);}
        Shot("20-realm-shift",()=>{G.Enemies.Boss!.Damage(999999,Element.Arcane);_shoot=false;},155);
        Shot("21-low-vfx",()=>{SaveManager.Data.Settings.Particles=0;Effects(Element.Fire);},30);
        Shot("22-credits-640",()=>{G.ReturnToMenu();SaveManager.Data.Settings.Resolution=0;G.ApplySettings();G.UI.ShowCredits();});
        Shot("23-relics-640",()=>{Arena(0,Element.Fire);G.UI.ShowUpgrades();});
        Shot("24-menu-1920",()=>{G.ReturnToMenu();SaveManager.Data.Settings.Resolution=2;G.ApplySettings();G.UI.ShowMenu();});
        Shot("26-hud-ready",()=>{SaveManager.Data.Settings.Resolution=1;SaveManager.Data.Settings.Particles=2;G.ApplySettings();Arena(0,Element.Nature);Enemies();});
        Shot("27-hud-cooldowns",()=>{Arena(1,Element.Frost);G.Player!.Stats.Health=47;G.Player.Stats.Shield=22;G.Player.Weapons.Secondary(Vector2.Right);Input.ActionPress("hero");Input.ActionPress("dash");Enemies();});
        Shot("28-hotbar-secondary",()=>{Arena(2,Element.Arcane);G.Player!.Weapons.Switch(1);Enemies();});
        Shot("29-hotbar-locked",()=>{SaveManager.Data.Bosses.Clear();Arena(0,Element.Fire);});
        Shot("30-radar-objectives",()=>{SaveManager.Data.Bosses=new(){0,1,2,3};Arena(4,Element.Lightning);G.World.PortalActive=true;Enemies();});
        for(int i=0;i<4;i++){int realm=i;Shot("31-foliage-realm-"+realm,()=>Foliage(realm));}
        Shot("32-reduced-flash",()=>{SaveManager.Data.Settings.ReducedFlash=true;Arena(5,Element.Arcane);Enemies();});
        Shot("33-hud-640",()=>{SaveManager.Data.Settings.ReducedFlash=false;SaveManager.Data.Settings.Resolution=0;G.ApplySettings();Arena(6,Element.Nature);Enemies();});
        Shot("34-hud-1920",()=>{SaveManager.Data.Settings.Resolution=2;G.ApplySettings();Arena(3,Element.Fire);Enemies();});
        Shot("25-combat-video",()=>{SaveManager.Data.Settings.Resolution=1;SaveManager.Data.Settings.Particles=2;G.ApplySettings();Arena(0,Element.Fire);Enemies();foreach(var e in G.Enemies.Active)e.Spawn(e.Data,e.Position,2000);_shoot=true;},600);
        _shots[0].Setup();
    }
    private IEnumerable<Node> All(Node root){yield return root;foreach(Node child in root.GetChildren())foreach(var n in All(child))yield return n;}
    private void Click(string label){var b=All(G.UI).OfType<Button>().First(x=>x.Text==label);b.EmitSignal(Button.SignalName.Pressed);}
    private void Arena(int weapon,Element element)
    {
        _shoot=_melee=false;Input.ActionRelease("attack");Input.ActionRelease("hero");Input.ActionRelease("dash");G.StartRun(weapon,1,element);G.Enemies.Clear();G.Weapons.Clear();G.Player!.Invincible=1000;G.Player.Stats.Crit=0;G.Player.Position=G.World.Spawn;G.Player.Weapons.Equip(0,weapon,element,0);G.Camera.Focus=G.Player.Position;G.Camera.Position=G.Player.Position;G.State=RunState.Portal;G.UI.Announce("","",0);
    }
    private void Enemies(){for(int i=0;i<8;i++)G.Enemies.Spawn(G.World.Realm*7,G.World.SafePoint(G.Player!.Position+Vector2.FromAngle(i*Mathf.Tau/8)*180));}
    private void Foliage(int realm)
    {Arena(0,(Element)(realm==0?3:realm==1?0:realm==2?1:4));G.World.Build(realm);G.Player!.Position=G.World.SafePoint(G.World.Spawn+new Vector2(-220,-170));G.Camera.Focus=G.Player.Position;G.Camera.Position=G.Player.Position;Enemies();}
    private void Tree(Vector2 offset)
    {
        if(_tree==null){Arena(0,Element.Nature);_tree=All(G.Depth).OfType<WorldProp>().First(p=>p.HasCanopy&&G.World.CanStand(p.Position+new Vector2(0,-45)));}
        G.State=RunState.Rest;G.Level.SkipTo(1);G.Enemies.Clear();G.State=RunState.Portal;G.Player!.Position=_tree.Position+offset;G.Player.Velocity=Vector2.Zero;G.Camera.Focus=_tree.Position+new Vector2(0,-34);G.Camera.Position=G.Camera.Focus.Value;G.UI.Announce("","",0);
    }
    private void Effects(Element element)
    {
        Arena(0,element);G.State=RunState.Portal;var p=G.Player!.Position;
        G.Weapons.Zone(p+new Vector2(85,35),42,2,0,element);G.Effects.Arc(p+new Vector2(0,-8),55,0,1.8f,Palette.ElementColor(element));
        G.Effects.Impact(p+new Vector2(90,-50),element,1.3f,null,false);_shoot=true;
    }
    private void Weapon(WeaponKind weapon)
    {
        Arena((int)weapon,weapon==WeaponKind.Bow?Element.Fire:weapon==WeaponKind.Chakram?Element.Lightning:weapon==WeaponKind.Gauntlets?Element.Frost:Element.Arcane);G.State=RunState.Portal;_shoot=true;_melee=weapon is WeaponKind.Blade or WeaponKind.Gauntlets;
        G.Player!.Weapons.Secondary(Vector2.Right);
    }
    private void Check(bool condition,string detail){if(!condition)throw new Exception(detail);_checks++;GD.Print("POLISH PASS / "+detail);}
    private void Bounds()
    {
        foreach(var n in All(G.UI).OfType<Label>())
        {
            if(!n.IsVisibleInTree()||n.Text.Length==0)continue;var parent=n.GetParent() as Control;if(parent==null)continue;
            if(parent is ScrollContainer)continue;
            float width=n.Size.X;Check(width<=parent.Size.X+1,"label width / "+n.Text.Split('\n')[0]);
            Check(n.Position.X>=-.5f&&n.Position.Y>=-.5f,"label origin / "+n.Text.Split('\n')[0]);
            float natural=n.GetLineCount()*n.GetThemeFont("font").GetHeight(n.GetThemeFontSize("font_size"));
            if(n.AutowrapMode!=TextServer.AutowrapMode.Off)Check(natural<=n.Size.Y+2,"wrapped text height / "+n.Text.Split('\n')[0]);
        }
        if(_shots[_stage].Name.Contains("tree"))Check(G.Depth.YSortEnabled&&G.Player!.GetParent()==G.Depth&&G.Player.ZIndex==_tree!.ZIndex,"shared foot-position tree sorting");
        var hud=All(G.UI).OfType<HUDController>().First();
        if(hud.IsVisibleInTree()&&G.Player!=null)
        {
            Check(!HUDController.WeaponSlots[0].Intersects(HUDController.WeaponSlots[1]),"equipment slots have independent bounds");
            foreach(var r in HUDController.WeaponSlots)Check(r.Position.X>=0&&r.End.X<=640&&r.Position.Y>=0&&r.End.Y<=360,"hotbar inside logical viewport");
            Check(G.World.Dressing.Plants.Count<=GroundDressing.Capacity,"dressing capacity");
            if(_shots[_stage].Name.Contains("hud-")||_shots[_stage].Name.Contains("radar-")||_shots[_stage].Name.Contains("foliage-"))Check(hud.Radar.VisibleEnemyMarkers>0,"live red enemy markers on radar");
        }
        Check(G.Effects.ActiveCount<=EffectsManager.Capacity,"VFX capacity");
        if(_shots[_stage].Name=="25-combat-video")Check(G.Player!=null&&!G.Player.Dead&&G.State==RunState.Portal,"showcase remains in live gameplay");
    }
    public override void _Process(double delta)
    {
        try
        {
            _frames++;
            if(_shots[_stage].Name=="25-combat-video"&&G.Player!=null)G.Player.Invincible=1000;
            if(_shots[_stage].Name.StartsWith("19-boss")&&G.Enemies.Boss!=null)G.Camera.Focus=G.Enemies.Boss.Position;
            if(_shoot&&G.Player!=null&&G.Running)
            {
                int cycle=_shots[_stage].Name=="25-combat-video"?Math.Min(4,(_frames-1)/120):0;
                if(_shots[_stage].Name=="25-combat-video")
                {
                    Element element=(Element)cycle;int[] kinds={(int)WeaponKind.Wand,(int)WeaponKind.Bow,(int)WeaponKind.Chakram,(int)WeaponKind.Spellbook,(int)WeaponKind.Gauntlets};
                    if(G.Player.Weapons.Current.Element!=element){G.Player.Weapons.Release(Vector2.Right);G.Player.Weapons.Equip(0,kinds[cycle],element,0);G.Effects.Clear();G.Projectiles.Clear();G.Weapons.Clear();}
                    if(_frames==120){Input.ActionPress("hero");Input.ActionPress("dash");G.Player.Weapons.Secondary(Vector2.Right);}
                    if(_frames==121){Input.ActionRelease("hero");Input.ActionRelease("dash");}
                    if(_frames%20==0)G.Effects.Impact(G.Player.Position+new Vector2(115,-40),element,1.3f,null,false);
                    if(_frames%90==0)G.Weapons.Zone(G.Player.Position+new Vector2(85,30),38,1,0,element);
                }
                G.Player.Weapons.Primary((float)delta,Vector2.Right);if(G.Player.Weapons.Current.Data.Kind==WeaponKind.Bow&&_frames%18==0)G.Player.Weapons.Release(Vector2.Right);
                if(_melee&&_frames%6==0)G.Effects.Arc(G.Player.Position+new Vector2(0,-8),55,0,1.8f,Palette.ElementColor(G.Player.Weapons.Current.Element));
                if(_shots[_stage].Name.StartsWith("17-element")&&_frames%12==0)G.Effects.Impact(G.Player.Position+new Vector2(100,-40),G.Player.Weapons.Current.Element,1.2f,null,false);
            }
            if(_shots[_stage].Name=="25-combat-video"&&_frames%2==0)
            {var video=Path.Combine(_dir,"video-frames");Directory.CreateDirectory(video);using var frame=GetViewport().GetTexture().GetImage();frame.SavePng(Path.Combine(video,$"{_video++:0000}.png"));}
            if(_frames<_shots[_stage].Frames)return;
            Bounds();using(var image=GetViewport().GetTexture().GetImage())image.SavePng(Path.Combine(_dir,_shots[_stage].Name+".png"));GD.Print("POLISH CAPTURE / "+_shots[_stage].Name);
            _frames=0;_stage++;
            if(_stage>=_shots.Count){G.ReturnToMenu();GD.Print("POLISH FINISHED / "+_checks+" structural checks / "+_shots.Count+" screens");CallDeferred(nameof(Finish));return;}
            _shots[_stage].Setup();
        }
        catch(Exception ex){GD.PushError("POLISH FAILED / "+ex);GetTree().Quit(1);}
    }
    private void Finish(){_shoot=false;SetProcess(false);GC.Collect();GC.WaitForPendingFinalizers();G.Quit();}
}
