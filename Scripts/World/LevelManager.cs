using Godot;
namespace Realmshift;
public sealed class LevelManager
{
    public int Level {get;private set;}
    public int Realm=>Mathf.PosMod((Level-1)/10,4);
    public int RemainingSpawns {get;private set;}
    public float Timer {get;private set;}
    public float WaveTime {get;private set;}
    public bool Endless {get;private set;}
    private int _spawned;
    private float _hazardTimer=5;
    public void Start(bool endless)
    {Endless=endless;Level=endless?40:0;BeginNext();}
    public void BeginNext()
    {
        var g=GameManager.Instance;Level++;SaveManager.Data.BestLevel=Mathf.Max(SaveManager.Data.BestLevel,Level);WaveTime=0;_spawned=0;_hazardTimer=5;
        if(g.World.Realm!=Realm)g.World.Build(Realm);g.Audio.Music(Realm);
        if(Level%10==0)
        {g.State=RunState.BossIntro;Timer=3;g.UI.Announce(Catalog.Realms[Realm].BossName.ToUpper(),"GUARDIAN OF "+Catalog.Realms[Realm].DisplayName.ToUpper(),3);var boss=g.Enemies.SpawnBoss(Realm);g.Camera.Focus=boss.Position;g.Audio.Play("boss");g.Audio.Music(4);RemainingSpawns=0;}
        else
        {RemainingSpawns=Mathf.Min(65,8+Level*2);Timer=.3f;g.State=RunState.Combat;g.UI.Announce("LEVEL "+Level.ToString("00"),Catalog.Realms[Realm].DisplayName.ToUpper(),1.9f);}
    }
    public void Tick(float dt)
    {
        var g=GameManager.Instance;
        if(g.State==RunState.BossIntro){Timer-=dt;if(Timer<=0){g.Camera.Focus=null;g.State=RunState.Combat;}return;}
        if(g.State==RunState.Rest)
        {Timer-=dt;if(Timer<=0){g.Experience.TryOffer();if(g.State==RunState.Rest)BeginNext();}return;}
        if(g.State!=RunState.Combat)return;WaveTime+=dt;_hazardTimer-=dt;
        if(Realm!=0&&_hazardTimer<=0)
        {_hazardTimer=Realm==1?7:Realm==2?9:6;Vector2 p=g.World.SafePoint(g.Player!.Position+new Vector2(GD.Randf()*120-60,GD.Randf()*90-45));g.Weapons.Hazard(p,Realm==3?27:22,1.15f,1.3f,10+Realm*2,Realm==1?Element.Fire:Realm==2?Element.Frost:Element.Arcane);}
        if(RemainingSpawns>0)
        {
            Timer-=dt;if(Timer<=0&&g.Enemies.Count<95)
            {
                Timer=Mathf.Max(.15f,.65f-Level*.011f);Vector2 p=FindSpawn();
                int local=(int)(GD.Randi()%(uint)Mathf.Min(7,2+Level/3));bool elite=Level>3&&GD.Randf()<Mathf.Min(.2f,Level*.005f);bool mini=Level%5==0&&_spawned==0;
                g.Enemies.Spawn(local==6?24+Realm:Realm*6+local,p,elite,mini);_spawned++;RemainingSpawns--;
            }
        }
        if(RemainingSpawns==0&&g.Enemies.Count==0&&Level%10!=0)
        {
            g.State=RunState.Rest;Timer=4;g.Player!.Heal(4);g.Pickups.Vacuum();g.UI.Announce("LEVEL CLEARED","Gather your shards. The next wave approaches.",2.5f);SaveManager.Save();
            if(Level%3==0)g.World.Chests.Add(g.World.SafePoint(g.Player.Position+new Vector2(75,35)));
            g.Experience.TryOffer();
        }
    }
    private Vector2 FindSpawn()
    {
        var g=GameManager.Instance;for(int i=0;i<36;i++){Vector2 p=g.Player!.Position+Vector2.FromAngle(GD.Randf()*Mathf.Tau)*(float)GD.RandRange(310,440);if(g.World.CanStand(p,14))return p;}
        // Fallback remains outside the player's immediate danger radius.
        for(float a=0;a<Mathf.Tau;a+=.25f){Vector2 p=g.Player!.Position+Vector2.FromAngle(a)*200;if(g.World.CanStand(p,14))return p;}return g.World.SafePoint(g.World.Spawn+new Vector2(0,-250));
    }
    public void BossDefeated()
    {
        var g=GameManager.Instance;SaveManager.BossDefeated(Realm);g.Player!.Stats.SecondSlot=true;g.Player.Heal(35);g.Player.Stats.Shield+=20;SaveManager.Data.Shards+=30;g.RunShards+=30;
        g.Enemies.Clear();g.Projectiles.Clear();g.Weapons.Clear();g.Pickups.Vacuum();g.Shifts.Begin(Realm,Level==40&&!Endless);
    }
    public void SkipTo(int level)
    {Level=level-1;GameManager.Instance.Enemies.Clear();GameManager.Instance.Projectiles.Clear();BeginNext();}
}
