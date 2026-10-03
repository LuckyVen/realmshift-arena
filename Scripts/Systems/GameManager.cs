using Godot;
namespace Realmshift;
public partial class GameManager : Node2D
{
    public static GameManager Instance {get;private set;}=null!;
    public RunState State {get;set;}=RunState.Menu;
    public bool Running=>(State is RunState.Combat or RunState.Tutorial or RunState.Rest or RunState.Portal)&&_stop<=0;
    public SettingsData Settings=>SaveManager.Data.Settings;
    public WorldManager World {get;private set;}=null!;
    public CameraController Camera {get;private set;}=null!;
    public PlayerController? Player {get;private set;}
    public EnemyDirector Enemies {get;private set;}=null!;
    public ProjectilePool Projectiles {get;private set;}=null!;
    public CombatSystem Weapons {get;private set;}=null!;
    public PickupManager Pickups {get;private set;}=null!;
    public EffectsManager Effects {get;private set;}=null!;
    public AudioManager Audio {get;private set;}=null!;
    public UIManager UI {get;private set;}=null!;
    public LevelManager Level {get;private set;}=new();
    public ExperienceSystem Experience {get;private set;}=new();
    public UpgradeSystem Upgrades {get;private set;}=new();
    public WorldShiftManager Shifts {get;private set;}=new();
    public int Score,Combo,Kills,RunShards;
    public float ComboTimer,RunTime;
    private float _stop,_tutorialTime;
    private bool _ended,_endless;
    private int _tutorialStep;
    private bool _automation;
    public override void _Ready()
    {
        Instance=this;Catalog.Load();SaveManager.Load();InputBindings.Setup();
        World=new WorldManager{Name="World"};AddChild(World);World.Build(0);
        Camera=new CameraController{Name="Camera",Position=World.Spawn};AddChild(Camera);
        Enemies=new EnemyDirector();AddChild(Enemies);Projectiles=new ProjectilePool{Name="Projectiles"};AddChild(Projectiles);
        Weapons=new CombatSystem{Name="CombatZones"};AddChild(Weapons);Pickups=new PickupManager{Name="Pickups"};AddChild(Pickups);Effects=new EffectsManager{Name="Effects"};AddChild(Effects);
        Audio=new AudioManager{Name="Audio"};AddChild(Audio);UI=new UIManager{Name="Interface"};AddChild(UI);
        ApplySettings();Audio.Music(5);UI.ShowIntro();
        var args=OS.GetCmdlineUserArgs();_automation=System.Array.Exists(args,x=>x.StartsWith("--smoke")||x.StartsWith("--capture="));
        if(System.Array.Exists(args,x=>x.StartsWith("--smoke"))){CallDeferred(nameof(StartSmoke));}else if(System.Array.Exists(args,x=>x.StartsWith("--capture="))){CallDeferred(nameof(StartCapture));}
    }
    public void ApplySettings()
    {
        if(DisplayServer.GetName()!="headless")
        {DisplayServer.WindowSetMode(Settings.Fullscreen?DisplayServer.WindowMode.Fullscreen:DisplayServer.WindowMode.Windowed);DisplayServer.WindowSetVsyncMode(Settings.Vsync?DisplayServer.VSyncMode.Enabled:DisplayServer.VSyncMode.Disabled);if(!Settings.Fullscreen)DisplayServer.WindowSetSize(new Vector2I(640,360)*(Settings.Resolution+1));}
        Audio.ApplyVolumes();
    }
    public void StartRun(int weapon,int second,Element element,bool endless=false)
    {
        ClearRun();_endless=endless;_ended=false;World.Endless=endless;World.Build(endless?0:0);Experience=new();Upgrades=new();Level=new();Shifts=new();Score=Combo=Kills=RunShards=0;ComboTimer=RunTime=0;
        Player=GD.Load<PackedScene>("res://Scenes/Characters/Player.tscn").Instantiate<PlayerController>();AddChild(Player);Player.Position=World.Spawn;Player.Stats.SecondSlot=SaveManager.Data.Bosses.Count>0;
        Player.Weapons.Equip(0,weapon,element,0);Player.Weapons.Equip(1,second,(Element)(((int)element+2)%5),0);Camera.Target=Player;Camera.Focus=null;Camera.Position=World.Spawn;
        UI.Close();UI.ShowHud(true);SaveManager.Data.Runs++;
        if(!SaveManager.Data.Tutorial&&!endless&&!_automation){State=RunState.Tutorial;_tutorialStep=0;_tutorialTime=0;UI.Announce("WELCOME TO THE SHATTERED REALM","Move with WASD / left stick. Cross the courtyard.",5);}
        else Level.Start(endless);
    }
    public override void _Process(double delta)
    {
        float dt=(float)delta;_stop=Mathf.Max(0,_stop-dt);
        if(Running){RunTime+=dt;ComboTimer-=dt;if(ComboTimer<=0)Combo=0;}
        if(State==RunState.Tutorial)
        {
            _tutorialTime+=dt;
            if(_tutorialStep==0&&Player!.Position.DistanceTo(World.Spawn)>32){_tutorialStep++;UI.Announce("DASH THROUGH DANGER","Press "+InputBindings.Label("dash")+" / gamepad A while moving. Dash gives brief invulnerability.",6);}
            if(_tutorialStep==1&&Player!.DashTimer>0){_tutorialStep++;Enemies.Spawn(0,World.SafePoint(Player.Position+new Vector2(140,0)));UI.Announce("MAKE YOUR FIRST STRIKE","Aim with mouse / right stick. LMB or RT attacks. Bow: hold, then release.",6);}
            if(_tutorialStep==2&&Kills>0||_tutorialTime>40||Input.IsKeyPressed(Key.Enter))
            {SaveManager.Data.Tutorial=true;SaveManager.Save();Enemies.Clear();Level.Start(false);UI.Announce("BEGIN YOUR JOURNEY","RMB / LT: weapon skill. Q / Y: hero skill. E / X: caches and gates.",6);}
        }
        Level.Tick(dt);Shifts.Tick(dt);
        if(Input.IsActionJustPressed("pause")&&!_automation&&!UI.CapturingInput&&State!=RunState.Menu&&State!=RunState.Defeat&&State!=RunState.Victory&&State!=RunState.Shift)UI.TogglePause();
    }
    public void HitStop(float duration){if(!Settings.ReducedFlash)_stop=Mathf.Max(_stop,duration);}
    public void Interact()
    {
        if(Player==null)return;
        if(World.PortalActive&&Player.Position.DistanceTo(World.PortalPosition)<55)
        {World.PortalActive=false;Pickups.Vacuum();Level.BeginNext();Audio.Play("shift");return;}
        for(int i=0;i<World.Chests.Count;i++)if(Player.Position.DistanceTo(World.Chests[i])<42)
        {Vector2 p=World.Chests[i];World.Chests.RemoveAt(i);Effects.Burst(p,Palette.Gold,16);UI.ShowCache();return;}
        for(int i=0;i<World.Shrines.Count;i++)if(Player.Position.DistanceTo(World.Shrines[i])<35)
        {Player.Heal(20);Player.Stats.Shield+=10;World.Shrines.RemoveAt(i);Effects.Ring(Player.Position,35,Palette.Teal,.8f);Audio.Play("level");UI.Announce("A MEMORY OF THE OLD WORLD","The shrine restores your health and grants a shield.",2);SaveManager.Data.Achievements.Add("Ancient Favor");return;}
    }
    public void EndRun(bool victory)
    {
        if(_ended)return;_ended=true;State=victory?RunState.Victory:RunState.Defeat;
        SaveManager.Data.BestScore=Mathf.Max(SaveManager.Data.BestScore,Score);if(Kills>=100)SaveManager.Data.Achievements.Add("Hundredfold");if(Combo>=20)SaveManager.Data.Achievements.Add("Unbroken Rhythm");SaveManager.Save();UI.ShowResult(victory);
    }
    public void ReturnToMenu()
    {if(!_ended&&Player!=null)SaveManager.Save();ClearRun();State=RunState.Menu;Camera.Focus=World.Spawn;World.Endless=false;World.Build(0);Audio.Music(5);UI.ShowMenu();}
    private void ClearRun()
    {
        UI.Close();UI.ShowHud(false);Enemies.Clear();Projectiles.Clear();Weapons.Clear();Pickups.Clear();Effects.Clear();
        if(Player!=null){RemoveChild(Player);Player.QueueFree();Player=null;}Camera.Target=null;
    }
    public void Restart()=>StartRun(UI.StartWeapon,UI.SecondWeapon,UI.StartElement,_endless);
    public override void _ExitTree(){Art.Release();Catalog.Release();}
    public override void _Notification(int what){if(what==NotificationWMCloseRequest){SaveManager.Save();GetTree().Quit();}}
    private void StartCapture(){AddChild(new VisualCapture{Name="VisualQA"});}
    private void StartSmoke()
    {AddChild(new SmokeTest{Name="SmokeTests"});}
}
