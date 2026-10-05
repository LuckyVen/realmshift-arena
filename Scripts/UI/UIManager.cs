using Godot;
using System;
using System.Linq;
namespace Realmshift;
public partial class UIManager : CanvasLayer
{
    public int StartWeapon {get;private set;}
    public int SecondWeapon {get;private set;}=1;
    public Element StartElement {get;private set;}=Element.Arcane;
    private Control _root=null!,_page=null!;
    private HUDController _hud=null!;
    private ColorRect _hurt=null!;
    private Label _announce=null!,_sub=null!;
    private float _announceTime,_introTime;
    private bool _intro,_endless;
    private RunState _resume;
    private string? _binding;
    private Action? _settingsBack,_rebindRefresh;
    private float _captureGuard;
    public bool CapturingInput=>_binding!=null||_captureGuard>0;
    private AvatarConfig _editing=new();
    private int _armory;
    private int _loadoutSlot;
    private GameManager G=>GameManager.Instance;
    public override void _Ready()
    {
        Layer=50;_root=new Control{Name="Root",Size=new(640,360),MouseFilter=Control.MouseFilterEnum.Ignore};AddChild(_root);
        _hud=new HUDController();_root.AddChild(_hud);_hud.Visible=false;
        _root.AddChild(new RealmShiftOverlay());
        _hurt=new ColorRect{Size=new(640,360),Color=new Color(.7f,.05f,.08f,0),MouseFilter=Control.MouseFilterEnum.Ignore};_root.AddChild(_hurt);
        _announce=UIFactory.Text(_root,"",110,110,420,29,16,Palette.Gold);_announce.HorizontalAlignment=HorizontalAlignment.Center;
        _sub=UIFactory.Text(_root,"",95,143,450,48,10,Palette.Paper,true);_sub.HorizontalAlignment=HorizontalAlignment.Center;
        _editing=SaveManager.Data.Avatar.Copy();
    }
    public void Close()
    {if(_page!=null){_root.RemoveChild(_page);_page.QueueFree();_page=null!;}_intro=false;_binding=null;if(_hud!=null)_hud.Visible=G.Player!=null;}
    public void ShowHud(bool show){_hud.Visible=show;}
    public void Announce(string title,string subtitle,float seconds=3){_announce.Text=title;_sub.Text=subtitle;_announceTime=seconds;}
    public void HurtFlash(){if(!G.Settings.ReducedFlash)_hurt.Color=new Color(.55f,.06f,.08f,.16f);}
    public override void _Process(double delta)
    {
        float dt=(float)delta;_captureGuard=Mathf.Max(0,_captureGuard-dt);if(G.State is not RunState.Pause and not RunState.Upgrade)_announceTime=Mathf.Max(0,_announceTime-dt);
        _announce.Visible=_sub.Visible=_announceTime>0&&_page==null;_announce.Modulate=_sub.Modulate=new Color(1,1,1,Mathf.Clamp(_announceTime,0,1));
        _hurt.Color=new Color(_hurt.Color,Mathf.Max(0,_hurt.Color.A-dt*.6f));
        if(_intro){_introTime+=dt;if(_introTime>=3.7f||Input.IsKeyPressed(Key.Enter)||Input.IsMouseButtonPressed(MouseButton.Left))ShowMenu();}
    }
    public override void _UnhandledInput(InputEvent e)
    {if(_binding!=null&&e is InputEventKey {Pressed:true,Echo:false} key){InputBindings.Rebind(_binding,key.PhysicalKeycode);_binding=null;_captureGuard=.2f;_rebindRefresh?.Invoke();GetViewport().SetInputAsHandled();}}
    private Control Page(string title,Action? back=null)
    {
        Close();_hud.Visible=false;_page=new Control{Name="Page",Size=new(640,360),MouseFilter=Control.MouseFilterEnum.Stop};_root.AddChild(_page);_page.AddChild(new MenuBackdrop());
        if(title.Length>0){UIFactory.Text(_page,title,24,15,448,25,17,Palette.Gold);UIFactory.Text(_page,"REALMSHIFT / "+SaveManager.Data.Shards+" REALM SHARDS",25,39,450,13,9,Palette.Muted);}
        if(back!=null)UIFactory.Button(_page,"BACK",538,17,78,26,back);
        return _page;
    }
    public void ShowIntro()
    {
        Page("");_intro=true;_introTime=0;UIFactory.Text(_page,"A  SHATTERED  COMPASS  GAME",170,114,390,26,15,Palette.Teal);UIFactory.Icon(_page,Art.Get("UI/logo.png"),160,153,320);UIFactory.Text(_page,"PRESS ENTER TO BEGIN",249,318,240,18,10,Palette.Muted);
    }
    public void ShowMenu()
    {
        G.State=RunState.Menu;ShowHud(false);Page("");_announceTime=0;
        var logo=new TextureRect{Texture=Art.Get("UI/logo.png"),Position=new(25,24),Size=new(400,87),ExpandMode=TextureRect.ExpandModeEnum.IgnoreSize,StretchMode=TextureRect.StretchModeEnum.KeepAspectCentered,MouseFilter=Control.MouseFilterEnum.Ignore};_page.AddChild(logo);
        UIFactory.Text(_page,"FOUR REALMS. ONE UNFINISHED WORLD.",45,112,410,16,9,Palette.Muted);
        var start=UIFactory.Button(_page,"START  /  ENTER THE REALM",45,138,215,32,()=>{_endless=false;ShowCharacters();},true);start.GrabFocus();
        UIFactory.Button(_page,"CHARACTERS",45,180,174,23,ShowCharacters);UIFactory.Button(_page,"ARMORY",45,207,174,23,()=>ShowArmory(0));
        UIFactory.Button(_page,"COLLECTION",45,234,174,23,()=>ShowCollection("REALMS"));UIFactory.Button(_page,"SETTINGS",45,261,174,23,()=>ShowSettings(ShowMenu));
        UIFactory.Button(_page,"CREDITS",45,288,84,23,ShowCredits);UIFactory.Button(_page,"EXIT",135,288,84,23,G.Quit);
        Avatar(_page,SaveManager.Data.Avatar,new Vector2(456,262),3.5f);
        UIFactory.Text(_page,"YOUR NEXT JOURNEY",368,135,226,18,11,Palette.Gold);UIFactory.Text(_page,"Best level  "+SaveManager.Data.BestLevel+"   /   Best score  "+SaveManager.Data.BestScore,325,282,280,15,9,Palette.Muted);
        if(SaveManager.Data.Endless)UIFactory.Button(_page,"REALM COLLAPSE / ENDLESS",334,308,262,27,()=>{_endless=true;ShowCharacters();});else UIFactory.Text(_page,"Defeat the Realmbreaker to unlock endless.",327,310,284,24,9,Palette.Muted,true);
        UIFactory.Text(_page,"v1.2  /  WILDS & WARDENS",25,341,450,14,8,Palette.Muted);
        if(SaveManager.Warning!=null)UIFactory.Text(_page,SaveManager.Warning,280,40,327,36,9,new Color("ee8e9c"),true);
    }
    private void Avatar(Control parent,AvatarConfig config,Vector2 pos,float scale)
    {var avatar=new AvatarRenderer{Config=config,Position=pos,Scale=Vector2.One*scale,Running=true};parent.AddChild(avatar);}
    public void ShowCharacters()
    {
        Page("CHOOSE YOUR TRAVELER",ShowMenu);UIFactory.Panel(_page,23,65,594,239);
        int selected=_editing.Hero;
        for(int i=0;i<8;i++)
        {
            int id=i;bool unlocked=SaveManager.Data.Heroes.Contains(i);var h=Catalog.Heroes[i];string label=unlocked?h.DisplayName:h.DisplayName+" / 60";
            UIFactory.Button(_page,label,35+(i%2)*124,81+(i/2)*45,116,37,()=>{if(SaveManager.UnlockHero(id)){_editing=AvatarConfig.FromHero(id);SaveManager.Data.Avatar=_editing.Copy();SaveManager.Save();ShowCharacters();}else {ShowCharacters();UIFactory.Text(_page,"Need 60 shards. Collect realm shards in a run.",35,290,540,16,9,new Color("eea19b"));}},selected==i);
        }
        Avatar(_page,_editing,new Vector2(352,240),4);var hero=Catalog.Heroes[selected];
        UIFactory.Text(_page,hero.DisplayName,441,93,162,24,17,Palette.Gold);UIFactory.Text(_page,hero.Title,441,122,162,27,10,Palette.Muted,true);
        UIFactory.Text(_page,hero.Ability,441,162,162,20,11,Palette.Teal);UIFactory.Text(_page,hero.Description,441,189,161,69,10,Palette.Paper,true);
        UIFactory.Text(_page,"Cosmetic choices do not change combat stats.",35,271,384,20,9,Palette.Muted);
        UIFactory.Button(_page,"CUSTOMIZE APPEARANCE",24,317,190,28,ShowCreator);UIFactory.Button(_page,"SELECT LOADOUT >",430,317,187,28,ShowLoadout,true);
    }
    private static readonly string[] Hair={"Short","Medium","Long","Messy","Spiky","Ponytail","Tied","Curly","Layered","Warrior","Mage"};
    private static readonly string[] Outfits={"Wanderer","Ranger","Mage","Warden","Rogue","Explorer","Scholar","Elementalist"};
    public void ShowCreator()
    {
        Page("SHAPE YOUR OWN STORY",ShowCharacters);UIFactory.Panel(_page,22,62,194,239);Avatar(_page,_editing,new Vector2(118,224),5);UIFactory.Text(_page,Catalog.Heroes[_editing.Hero].Ability,37,267,165,25,10,Palette.Teal);
        Cycle("Body",new[]{"Slim","Classic","Broad"},_editing.Body,254,66,v=>_editing.Body=v);
        Cycle("Hair",Hair,_editing.Hair,254,94,v=>_editing.Hair=v);
        Cycle("Outfit",Outfits,_editing.Outfit,254,122,v=>_editing.Outfit=v);
        Cycle("Accessory",new[]{"None","Scarf","Cape","Shoulders","Charm","Circlet"},_editing.Accessory,254,150,v=>_editing.Accessory=v);
        Cycle("Eyes",new[]{"Classic","Soft","Sharp"},_editing.Eyes,254,178,v=>_editing.Eyes=v);
        Cycle("Expression",new[]{"Calm","Focused","Smiling"},_editing.Expression,254,206,v=>_editing.Expression=v);
        string[] auras={"None","Flame","Frost","Storm","Leaves","Arcane","Stars","Shadow"};int count=Math.Min(8,1+SaveManager.Data.Bosses.Count*2);
        Cycle("Aura",auras.Take(count).ToArray(),Math.Min(_editing.Aura,count-1),254,234,v=>_editing.Aura=v);
        Swatches("Hair",254,270,_editing.HairTint,v=>_editing.HairTint=v,new[]{"efd39d","5e4c55","e2e2d7","c87452","99c1c0","b299da","7d94ba","cb9c9f"});
        Swatches("Outfit",254,295,_editing.OutfitTint,v=>_editing.OutfitTint=v,new[]{"86bfa5","bb8178","85a8c5","b9a1ce","d7c592","697381","b4c8b9","da9c69"});
        Swatches("Skin",25,309,_editing.Skin,v=>_editing.Skin=v,new[]{"f3dcc2","d3ac81","ac805f","815c4d","644a44"});
        Swatches("Eyes",25,333,_editing.EyeTint,v=>_editing.EyeTint=v,new[]{"223746","327f6e","688ec1","976e4e","9272ae"});
        UIFactory.Button(_page,"SAVE TRAVELER >",444,328,173,25,()=>{SaveManager.Data.Avatar=_editing.Copy();SaveManager.Save();ShowLoadout();},true);
    }
    private void Cycle(string label,string[] options,int current,float x,float y,Action<int> set)
    {
        UIFactory.Text(_page,label.ToUpper(),x,y+4,100,20,9,Palette.Muted);
        UIFactory.Button(_page,"<",x+100,y,24,23,()=>{set((current+options.Length-1)%options.Length);ShowCreator();});UIFactory.Text(_page,options[Math.Clamp(current,0,options.Length-1)].ToUpper(),x+134,y+3,139,20,9,Palette.Paper);
        UIFactory.Button(_page,">",x+328,y,24,23,()=>{set((current+1)%options.Length);ShowCreator();});
    }
    private void Swatches(string label,float x,float y,string current,Action<string> set,string[] colors)
    {
        UIFactory.Text(_page,label.ToUpper(),x,y,52,18,8,Palette.Muted);
        for(int i=0;i<colors.Length;i++){string c=colors[i];var b=UIFactory.Button(_page,c==current?"+":"",x+55+i*25,y-2,21,18,()=>{set(c);ShowCreator();});foreach(string state in new[]{"normal","hover","pressed","focus"})b.AddThemeStyleboxOverride(state,new StyleBoxFlat{BgColor=new Color(c),BorderColor=c==current?Palette.Gold:Palette.Ink,BorderWidthTop=1,BorderWidthBottom=1,BorderWidthLeft=1,BorderWidthRight=1,ContentMarginTop=0,ContentMarginBottom=0,ContentMarginLeft=0,ContentMarginRight=0});}
    }
    public void ShowLoadout()
    {
        Page(_endless?"REALM COLLAPSE / LOADOUT":"PREPARE YOUR LOADOUT",ShowCharacters);UIFactory.Panel(_page,23,64,193,225);Avatar(_page,_editing,new Vector2(120,212),4);UIFactory.Text(_page,Catalog.Heroes[_editing.Hero].DisplayName,38,79,160,21,13,Palette.Gold);
        UIFactory.Button(_page,"PRIMARY",244,66,174,28,()=>{_loadoutSlot=0;ShowLoadout();},_loadoutSlot==0);
        var sec=UIFactory.Button(_page,SaveManager.Data.Bosses.Count>0?"SECONDARY":"SLOT 2 / BOSS 1",432,66,174,28,()=>{_loadoutSlot=1;ShowLoadout();},_loadoutSlot==1);sec.Disabled=SaveManager.Data.Bosses.Count==0;
        for(int i=0;i<8;i++)
        {
            int id=i;bool unlocked=SaveManager.Data.Weapons.Contains(i);float x=244+(i%2)*188,y=101+i/2*33;
            var b=UIFactory.Button(_page,unlocked?Catalog.Weapons[i].DisplayName:"??? / GUARDIAN "+(Catalog.Weapons[i].UnlockBoss+1),x,y,174,28,()=>{if(_loadoutSlot==0)StartWeapon=id;else SecondWeapon=id;ShowLoadout();},(_loadoutSlot==0?StartWeapon:SecondWeapon)==i);b.Disabled=!unlocked;
        }
        int selected=_loadoutSlot==0?StartWeapon:SecondWeapon;UIFactory.Text(_page,Catalog.Weapons[selected].Description,244,239,369,43,10,Palette.Muted,true);
        UIFactory.Text(_page,"ELEMENT",25,299,92,20,9,Palette.Muted);
        for(int i=0;i<5;i++){int e=i;var b=UIFactory.Button(_page,((Element)i).ToString().ToUpper(),114+i*102,294,95,24,()=>{StartElement=(Element)e;ShowLoadout();},(int)StartElement==e);b.AddThemeColorOverride("font_color",Palette.ElementColor((Element)i));}
        UIFactory.Text(_page,"WASD / MOVE   SPACE / DASH   LMB / ATTACK",25,335,386,17,9,Palette.Muted);
        UIFactory.Button(_page,"ENTER THE REALM >",429,327,187,27,()=>{SaveManager.Data.Avatar=_editing.Copy();SaveManager.Save();G.StartRun(StartWeapon,SecondWeapon,StartElement,_endless);},true);
    }
    public void ShowArmory(int id)
    {
        _armory=id;Page("THE ARMORY",ShowMenu);UIFactory.Panel(_page,22,64,196,280);UIFactory.Panel(_page,234,64,383,280);
        for(int i=0;i<8;i++){int n=i;bool unlocked=SaveManager.Data.Weapons.Contains(i);UIFactory.Button(_page,unlocked?Catalog.Weapons[i].DisplayName:"UNKNOWN RELIC",33,77+i*32,174,26,()=>ShowArmory(n),i==id);}
        var weapon=Catalog.Weapons[id];bool known=SaveManager.Data.Weapons.Contains(id);UIFactory.Icon(_page,Art.Weapon((WeaponKind)id),256,83,72,known?Colors.White:new Color(0,0,0,.8f));
        UIFactory.Text(_page,known?weapon.DisplayName:"UNDISCOVERED WEAPON",342,86,255,26,14,Palette.Gold);UIFactory.Text(_page,known?"CLASS / "+weapon.Kind.ToString().ToUpper():"GUARDIAN "+(weapon.UnlockBoss+1)+" REWARD",342,115,256,28,10,Palette.Muted);
        UIFactory.Text(_page,known?weapon.Description:"Defeat the guardian to reveal this relic and add it to your starting armory.",257,168,337,55,11,Palette.Paper,true);
        UIFactory.Text(_page,"ELEMENTS / FIRE - FROST - STORM - NATURE - ARCANE",257,232,337,29,9,Palette.Teal,true);
        UIFactory.Text(_page,"MASTERY / "+SaveManager.Data.Mastery.GetValueOrDefault(id)+" ATTACKS",257,272,337,19,10,Palette.Gold);
        int seen=Catalog.Upgrades.Count(x=>x.Weapon==id&&SaveManager.Data.Relics.Contains(x.Id));UIFactory.Text(_page,"MODIFIERS DISCOVERED / "+seen+" OF 4",257,297,337,19,9,Palette.Muted);
        UIFactory.Text(_page,"DISCOVERY / "+SaveManager.Data.Weapons.Count+" OF 8 CLASSES",257,320,337,17,9,Palette.Muted);
    }
    public void ShowCollection(string category)
    {
        Page("THE REALM ARCHIVE",ShowMenu);string[] tabs={"HEROES","WEAPONS","ENEMIES","BOSSES","RELICS","REALMS","LORE"};
        for(int i=0;i<tabs.Length;i++){string tab=tabs[i];UIFactory.Button(_page,tab,24+i*85,64,79,24,()=>ShowCollection(tab),category==tab);}
        UIFactory.Panel(_page,23,100,594,243);var scroll=new ScrollContainer{Position=new(35,111),Size=new(570,220)};_page.AddChild(scroll);var box=new VBoxContainer{SizeFlagsHorizontal=Control.SizeFlags.ExpandFill};scroll.AddChild(box);
        void Entry(string title,string detail,bool known=true)
        {
            var p=new Control{CustomMinimumSize=new(546,category=="LORE"?75:46)};box.AddChild(p);UIFactory.Text(p,title,3,2,542,18,11,known?Palette.Gold:Palette.Muted);UIFactory.Text(p,detail,3,21,541,category=="LORE"?49:25,9,Palette.Paper,true);
        }
        switch(category)
        {
            case "HEROES":for(int i=0;i<8;i++)Entry(Catalog.Heroes[i].DisplayName,SaveManager.Data.Heroes.Contains(i)?Catalog.Heroes[i].Title+" / "+Catalog.Heroes[i].Ability:"Unlock for 60 realm shards in Characters.",SaveManager.Data.Heroes.Contains(i));break;
            case "WEAPONS":foreach(var w in Catalog.Weapons)Entry(SaveManager.Data.Weapons.Contains((int)w.Kind)?w.DisplayName:"UNKNOWN RELIC",SaveManager.Data.Weapons.Contains((int)w.Kind)?w.Description:"Defeat guardian "+(w.UnlockBoss+1)+".",SaveManager.Data.Weapons.Contains((int)w.Kind));break;
            case "ENEMIES":foreach(var e in Catalog.Enemies)Entry(SaveManager.Data.SeenEnemies.Contains(e.Realm*7+(int)e.Kind)?e.DisplayName.ToUpper():"UNKNOWN CREATURE",SaveManager.Data.SeenEnemies.Contains(e.Realm*7+(int)e.Kind)?Catalog.Realms[e.Realm].DisplayName+" / "+e.Kind:"Explore the realms to discover this creature.",SaveManager.Data.SeenEnemies.Contains(e.Realm*7+(int)e.Kind));break;
            case "BOSSES":for(int i=0;i<4;i++)Entry(Catalog.Realms[i].BossName.ToUpper(),SaveManager.Data.Bosses.Contains(i)?"GUARDIAN DEFEATED / WORLD MEMORY RESTORED":"Encounter at level "+((i+1)*10)+". Three combat phases.",SaveManager.Data.Bosses.Contains(i));break;
            case "RELICS":foreach(var u in Catalog.Upgrades)Entry(SaveManager.Data.Relics.Contains(u.Id)?u.DisplayName:"UNRECORDED RELIC",SaveManager.Data.Relics.Contains(u.Id)?u.Description:"Choose this relic during a run to record it.",SaveManager.Data.Relics.Contains(u.Id));break;
            case "REALMS":for(int i=0;i<4;i++)Entry(Catalog.Realms[i].DisplayName,"LEVELS "+(i*10+1)+" - "+((i+1)*10)+" / "+Catalog.Realms[i].Subtitle);foreach(var a in SaveManager.Data.Achievements)Entry("ACHIEVEMENT / "+a.ToUpper(),"A permanent record of your travels.");break;
            default:for(int i=0;i<4;i++)Entry(Catalog.Realms[i].DisplayName,SaveManager.Data.BestLevel>=i*10+1?Catalog.Realms[i].Lore:"Reach this realm to recover its memory.",SaveManager.Data.BestLevel>=i*10+1);break;
        }
    }
    public void TogglePause()
    {
        if(G.State==RunState.Upgrade)return;
        if(G.State==RunState.Pause){Close();G.State=_resume;return;}
        _resume=G.State;G.State=RunState.Pause;ShowPause();
    }
    public void ShowPause()
    {
        Page("");UIFactory.Panel(_page,202,54,236,266);UIFactory.Text(_page,"PAUSED",272,76,145,25,18,Palette.Gold);
        UIFactory.Button(_page,"RESUME",220,117,200,27,()=>{Close();G.State=_resume;},true).GrabFocus();
        UIFactory.Button(_page,"SETTINGS",220,150,200,24,()=>ShowSettings(ShowPause));UIFactory.Button(_page,"CONTROLS",220,180,200,24,()=>ShowControls(ShowPause));
        UIFactory.Button(_page,"RESTART RUN",220,210,200,24,G.Restart);UIFactory.Button(_page,"RETURN TO MENU",220,240,200,24,G.ReturnToMenu);UIFactory.Button(_page,"QUIT GAME",220,270,200,24,G.Quit);
    }
    public void ShowSettings(Action back)
    {
        _settingsBack=back;Page("SETTINGS",()=>{SaveManager.Save();back();});UIFactory.Panel(_page,23,64,594,278);
        Slider("MASTER",G.Settings.Master,39,82,v=>{G.Settings.Master=v;G.ApplySettings();});Slider("MUSIC",G.Settings.Music,39,118,v=>{G.Settings.Music=v;G.ApplySettings();});Slider("SFX",G.Settings.Sfx,39,154,v=>{G.Settings.Sfx=v;G.ApplySettings();});
        Toggle("FULLSCREEN",G.Settings.Fullscreen,39,198,v=>{G.Settings.Fullscreen=v;G.ApplySettings();ShowSettings(back);});Toggle("VSYNC",G.Settings.Vsync,39,230,v=>{G.Settings.Vsync=v;G.ApplySettings();ShowSettings(back);});
        UIFactory.Button(_page,"WINDOW / "+((G.Settings.Resolution+1)*640)+"x"+((G.Settings.Resolution+1)*360),39,271,259,25,()=>{G.Settings.Resolution=(G.Settings.Resolution+1)%3;G.ApplySettings();ShowSettings(back);});
        Toggle("SCREEN SHAKE",G.Settings.Shake,331,82,v=>{G.Settings.Shake=v;ShowSettings(back);});Toggle("DAMAGE NUMBERS",G.Settings.Numbers,331,118,v=>{G.Settings.Numbers=v;ShowSettings(back);});Toggle("REDUCED FLASH",G.Settings.ReducedFlash,331,154,v=>{G.Settings.ReducedFlash=v;ShowSettings(back);});Toggle("AUTO AIM",G.Settings.AutoAim,331,190,v=>{G.Settings.AutoAim=v;ShowSettings(back);});
        UIFactory.Button(_page,"VFX QUALITY / "+new[]{"LOW","MEDIUM","HIGH"}[G.Settings.Particles],331,234,265,25,()=>{G.Settings.Particles=(G.Settings.Particles+1)%3;ShowSettings(back);});UIFactory.Button(_page,"CONTROLS / REBIND KEYS",331,271,265,25,()=>ShowControls(()=>ShowSettings(back)));
        UIFactory.Text(_page,"Gamepad: left stick moves, right stick aims. RT/LT attack and weapon skill.",39,310,563,18,8,Palette.Muted);
    }
    private void Slider(string label,float value,float x,float y,Action<float> change)
    {UIFactory.Text(_page,label,x,y,112,22,10,Palette.Muted);var s=new HSlider{Position=new(x+100,y+4),Size=new(158,14),MinValue=0,MaxValue=1,Step=.05,Value=value};_page.AddChild(s);s.ValueChanged+=v=>change((float)v);}
    private void Toggle(string label,bool current,float x,float y,Action<bool> set)=>UIFactory.Button(_page,label+" / "+(current?"ON":"OFF"),x,y,265,25,()=>set(!current));
    public void ShowControls(Action back)
    {
        _rebindRefresh=()=>ShowControls(back);Page("CONTROLS / KEY BINDINGS",back);UIFactory.Panel(_page,23,64,594,278);
        int i=0;foreach(var kv in InputBindings.Defaults)
        {string action=kv.Key;float x=39+(i%2)*285,y=82+i/2*38;UIFactory.Button(_page,action.ToUpper()+" / "+InputBindings.Label(action),x,y,264,27,()=>{_binding=action;_sub.Text="";UIFactory.Text(_page,"PRESS A KEY FOR "+action.ToUpper(),40,305,553,23,11,Palette.Gold);});i++;}
        UIFactory.Text(_page,"Mouse: LMB primary / RMB weapon ability. Bow: hold LMB, release to fire.",40,286,553,17,9,Palette.Muted);UIFactory.Text(_page,"A / dash   Y / hero   X / interact   LB,RB / switch   START / pause",40,324,553,16,8,Palette.Muted);
    }
    public void ShowUpgrades()
    {
        _resume=G.State;G.State=RunState.Upgrade;Page("RANK "+G.Experience.Rank+" / CHOOSE A RELIC");var choices=G.Upgrades.Offer();
        var row=new HBoxContainer{Position=new(23,88),Size=new(594,218)};row.AddThemeConstantOverride("separation",9);_page.AddChild(row);
        for(int i=0;i<3;i++)
        {
            var data=choices[i];var card=new PixelPanel{SizeFlagsHorizontal=Control.SizeFlags.ExpandFill};row.AddChild(card);
            var margin=new MarginContainer();card.AddChild(margin);margin.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);foreach(string side in new[]{"left","right","top","bottom"})margin.AddThemeConstantOverride("margin_"+side,12);
            var body=new VBoxContainer();body.AddThemeConstantOverride("separation",10);margin.AddChild(body);
            UIFactory.FlowText(body,data.Weapon>=0?Catalog.Weapons[data.Weapon].Kind.ToString().ToUpper():data.Element>=0?((Element)data.Element).ToString().ToUpper():"TRAVELER RELIC",9,Palette.Teal,18);
            UIFactory.FlowText(body,data.DisplayName,14,Palette.Gold,46);
            UIFactory.FlowText(body,data.Description,10,Palette.Paper,52);
            body.AddChild(new Control{SizeFlagsVertical=Control.SizeFlags.ExpandFill});
            UIFactory.FlowButton(body,"TAKE RELIC >",()=>{G.Upgrades.Apply(data);Close();G.State=_resume;G.Experience.TryOffer();},i==0);
        }
        var note=UIFactory.Text(_page,"Time is paused. Choose one. Every relic lasts for this run.",40,324,560,23,9,Palette.Muted,true);note.HorizontalAlignment=HorizontalAlignment.Center;
    }
    public void ShowCache()
    {
        _resume=G.State;G.State=RunState.Upgrade;Page("REALM CACHE / CHOOSE A WEAPON");
        int[] choices=Enumerable.Range(0,8).OrderBy(_=>GD.Randi()).Take(3).ToArray();
        for(int i=0;i<3;i++)
        {
            int id=choices[i],rarity=(int)(GD.Randi()%6);Element element=(Element)(GD.Randi()%5);float x=23+i*201;
            UIFactory.Panel(_page,x,86,192,207);UIFactory.Icon(_page,Art.Weapon((WeaponKind)id),x+62,100,62);UIFactory.Text(_page,Catalog.Weapons[id].DisplayName,x+12,169,165,31,12,Palette.Gold,true);UIFactory.Text(_page,new[]{"COMMON","UNCOMMON","RARE","EPIC","LEGENDARY","MYTHIC"}[rarity]+" / "+element,x+12,208,165,25,9,Palette.ElementColor(element),true);
            UIFactory.Button(_page,"EQUIP / ACTIVE SLOT",x+12,253,168,26,()=>{G.Player!.Weapons.Equip(G.Player.Weapons.Slot,id,element,rarity);G.Audio.Play("level");Close();G.State=_resume;SaveManager.Save();});
        }
        UIFactory.Button(_page,"KEEP LOADOUT / TAKE 8 SHARDS",170,318,304,26,()=>{SaveManager.Data.Shards+=8;G.RunShards+=8;Close();G.State=_resume;});
    }
    public void ShowResult(bool victory)
    {
        Page("");UIFactory.Panel(_page,122,60,396,256);UIFactory.Text(_page,victory?"THE REALM REMEMBERS":"YOUR JOURNEY ENDS",149,85,343,34,19,Palette.Gold,true);UIFactory.Text(_page,victory?"The engine is silent. Four worlds find their way home.":"Your discoveries remain. A new journey is waiting.",148,130,340,39,10,Palette.Paper,true);
        UIFactory.Text(_page,$"LEVEL {G.Level.Level:00}     SCORE {G.Score}\nDEFEATS {G.Kills}     SHARDS +{G.RunShards}\nTIME {(int)G.RunTime/60:00}:{(int)G.RunTime%60:00}",149,181,340,59,11,Palette.Teal);
        UIFactory.Button(_page,"TRY AGAIN",148,263,161,28,G.Restart);UIFactory.Button(_page,"RETURN TO MENU",321,263,170,28,G.ReturnToMenu,true);
        if(victory)UIFactory.Text(_page,"REALM COLLAPSE / ENDLESS IS NOW UNLOCKED",153,299,342,14,8,Palette.Gold);
    }
    public void ShowCredits()
    {
        Page("CREDITS / REALMSHIFT ARENA",ShowMenu);UIFactory.Panel(_page,23,65,594,278);
        var body=UIFactory.Flow(_page,30,73,580,260,true);
        UIFactory.FlowText(body,"A GAME FOR JOHN GABRIEL VENENOSO",15,Palette.Gold,30);
        UIFactory.FlowText(body,"Original characters, pixel assets, realms, bosses, and synthesized music created for this project.",11,Palette.Paper,36);
        UIFactory.FlowText(body,"Built with Godot 4.5.1 and C# / .NET 8.",11,Palette.Paper,24);
        UIFactory.FlowText(body,"Arcade arena games inspired the pacing and readability. All game identities, artwork, level layouts, and audio here are original.",11,Palette.Paper,40);
        UIFactory.FlowText(body,"Source and asset generation tools are included. See Docs for setup, architecture, testing, and release preparation.",11,Palette.Paper,40);
        UIFactory.FlowText(body,"SHATTERED COMPASS / REALMSHIFT ARENA 1.1",9,Palette.Teal,20);
    }
}
