using Godot;
using System;
using System.Linq;
namespace Realmshift;
/// <summary>Headless integration harness. Run with -- --smoke. Uses an isolated save profile.</summary>
public partial class SmokeTest : Node
{
    private int _step,_frames,_boss;
    private Vector2 _pauseEnemy;
    private int _killsBefore;
    private int _checks;
    private GameManager G=>GameManager.Instance;
    public override void _Ready()
    {
        Check(Catalog.Weapons.Length==8,"eight weapon resources");Check(Catalog.Enemies.Length==28,"28 enemy variants");Check(Catalog.Upgrades.Count>=50,"50+ upgrade resources");Check(Catalog.Heroes.Length==8,"eight heroes");
        G.UI.ShowMenu();G.UI.ShowCharacters();G.UI.ShowCreator();G.UI.ShowLoadout();G.UI.ShowArmory(7);foreach(var tab in new[]{"HEROES","WEAPONS","ENEMIES","BOSSES","RELICS","REALMS","LORE"})G.UI.ShowCollection(tab);G.UI.ShowSettings(G.UI.ShowMenu);G.UI.ShowControls(G.UI.ShowMenu);Check(true,"all menu pages construct");
        SaveManager.Data.Settings.AutoAim=true;SaveManager.Data.Avatar.Hair=10;SaveManager.Data.Avatar.Body=2;SaveManager.Data.Shards=123;SaveManager.Save();SaveManager.Load();Check(SaveManager.Data.Shards==123&&SaveManager.Data.Avatar.Hair==10,"save roundtrip");
        G.StartRun(0,1,Element.Lightning);G.Player!.Stats.MaxHealth=G.Player.Stats.Health=10000;Input.ActionPress("attack");
    }
    public override void _PhysicsProcess(double delta)
    {
        _frames++;
        try
        {
            if(_step==0&&_frames>420)
            {
                Check(G.Kills>0,"projectiles damage and defeat enemies");Check(G.Projectiles.ActiveCount<ProjectilePool.Capacity,"projectile pool bounded");Input.ActionRelease("attack");
                G.Enemies.Spawn(4,G.World.SafePoint(G.Player!.Position+new Vector2(80,0)));G.UI.Close();G.State=RunState.Combat;G.UI.TogglePause();_pauseEnemy=G.Enemies.Active.Last().Position;_step++;_frames=0;
            }
            else if(_step==1&&_frames>65)
            {
                Check(G.Enemies.Active.Last().Position==_pauseEnemy,"pause freezes enemy simulation");G.UI.TogglePause();G.Experience.Add(G.Experience.Required);Check(G.State==RunState.Upgrade,"XP freezes the run for upgrade choice");var choices=G.Upgrades.Offer();Check(choices.Length==3,"three upgrade choices");G.Upgrades.Apply(choices[0]);G.UI.Close();G.State=RunState.Combat;G.Experience.ConsumeAll();
                G.Enemies.Clear();G.Level.SkipTo(10);_step++;_frames=0;
            }
            else if(_step==2&&_frames>200)
            {
                var boss=G.Enemies.Boss!;Check(boss!=null&&boss.Realm==_boss,"scheduled guardian "+_boss);boss!.Damage(boss.MaxHealth*.4f,Element.Frost);_step++;_frames=0;
            }
            else if(_step==3&&_frames>10)
            {
                Check(G.Enemies.Boss!.Phase==2,"boss phase transition");G.Enemies.Boss.Damage(999999,Element.Arcane);Check(G.State==RunState.Shift,"boss victory triggers world shift");_step++;_frames=0;
            }
            else if(_step==4&&_frames>345)
            {
                if(_boss<3)
                {Check(G.World.Realm==_boss+1&&G.State==RunState.Portal,"world geometry and portal transition");G.Player!.Position=G.World.PortalPosition;G.Interact();Check(G.Level.Level==(_boss+1)*10+1,"portal advances campaign");_boss++;G.Level.SkipTo((_boss+1)*10);_step=2;_frames=0;}
                else
                {Check(G.State==RunState.Victory&&SaveManager.Data.Endless,"level 40 victory unlocks endless");G.StartRun(7,6,Element.Fire,true);Check(G.Level.Level==41,"endless starts beyond campaign");G.Enemies.Clear();G.UI.Close();G.State=RunState.Combat;_step++;_frames=0;}
            }
            else if(_step==5&&_frames>2)
            {
                foreach(var kind in Enum.GetValues<WeaponKind>())
                {G.Player!.Weapons.Equip(0,(int)kind,Element.Arcane,3);G.Player.Weapons.Primary(.5f,Vector2.Right);G.Player.Weapons.Release(Vector2.Right);G.Player.Weapons.Secondary(Vector2.Right);}
                Check(true,"eight weapon classes execute");SaveManager.Save();G.ReturnToMenu();Check(G.State==RunState.Menu,"return to menu clears run");GD.Print("SMOKE PASS / "+_checks+" integration checks");GetTree().Quit();
            }
            if(_frames>1000)throw new Exception("Smoke stage timed out: "+_step);
        }
        catch(Exception ex){GD.PushError("SMOKE FAIL / "+ex);GetTree().Quit(1);}
    }
    private void Check(bool condition,string label)
    {if(!condition)throw new Exception(label);_checks++;GD.Print("PASS / "+label);}
}
