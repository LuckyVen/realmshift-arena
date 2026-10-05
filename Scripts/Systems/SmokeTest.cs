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
        System.IO.File.WriteAllText(SaveManager.Path,"{\"Version\":1,\"Shards\":1419,\"BestLevel\":21,\"Heroes\":[0,1,5],\"Weapons\":[0,1,2,3],\"Bosses\":[0],\"Relics\":[\"runesteel\"],\"Avatar\":{\"Hero\":5,\"Body\":2,\"Hair\":10},\"Settings\":{\"Shake\":false,\"Particles\":2},\"Tutorial\":true}");
        SaveManager.Load();Check(SaveManager.Data.Version==1&&SaveManager.Data.Shards==1419&&SaveManager.Data.BestLevel==21&&SaveManager.Data.Heroes.Contains(5)&&SaveManager.Data.Bosses.Contains(0)&&SaveManager.Data.Relics.Contains("runesteel")&&!SaveManager.Data.Settings.Shake,"legacy version-1 profile preserves progression and settings");
        Check(Catalog.Weapons.Length==8,"eight weapon resources");Check(Catalog.Enemies.Length==28,"28 enemy variants");Check(Catalog.Upgrades.Count>=50,"50+ upgrade resources");Check(Catalog.Heroes.Length==8,"eight heroes");
        G.UI.ShowMenu();G.UI.ShowCharacters();G.UI.ShowCreator();G.UI.ShowLoadout();G.UI.ShowArmory(7);foreach(var tab in new[]{"HEROES","WEAPONS","ENEMIES","BOSSES","RELICS","REALMS","LORE"})G.UI.ShowCollection(tab);G.UI.ShowSettings(G.UI.ShowMenu);G.UI.ShowControls(G.UI.ShowMenu);Check(true,"all menu pages construct");
        SaveManager.Data.Settings.AutoAim=true;SaveManager.Data.Avatar.Hair=10;SaveManager.Data.Avatar.Body=2;SaveManager.Data.Shards=123;SaveManager.Save();SaveManager.Load();Check(SaveManager.Data.Shards==123&&SaveManager.Data.Avatar.Hair==10,"save roundtrip");
        G.StartRun(0,1,Element.Lightning);G.Player!.Stats.MaxHealth=G.Player.Stats.Health=10000;Input.ActionPress("attack");Input.ActionPress("right");
        Check(G.World.Dressing.Plants.Count>100&&G.World.Dressing.Plants.Count<=GroundDressing.Capacity,"forest dressing is present and bounded");
        Check(G.World.Dressing.Plants.All(x=>G.World.CanStand(x.Position,12)&&G.World.GroundKindAt(x.Position)==0),"foliage leaves paths and collisions clear");
        var east=CircularMinimap.Project(G.Player.Position+new Vector2(100,0),G.Player.Position);Check(east.X>CircularMinimap.Center.X&&CircularMinimap.Inside(east),"radar keeps east to the right of the player");
        Check(CircularMinimap.Inside(CircularMinimap.Edge(CircularMinimap.Project(G.Player.Position+new Vector2(2500,1800),G.Player.Position))),"offscreen objective fits inside the circular radar");
    }
    public override void _PhysicsProcess(double delta)
    {
        _frames++;
        try
        {
            if(_step==0&&_frames==8){Check(G.Player!.Position.X>G.World.Spawn.X,"movement responds to input");Input.ActionRelease("right");Input.ActionPress("dash");}
            if(_step==0&&_frames==9){Check(G.Player!.DashTimer>0&&G.Player.Invincible>0,"dash grants invulnerability and starts its motion");Check(G.Effects.LiveCount>0,"dash visual feedback spawns");Input.ActionRelease("dash");Check(G.Player.Readiness.IsReady,"hero aura starts ready");Input.ActionPress("hero");}
            if(_step==0&&_frames==10){Check(G.Player!.HeroTimer>0&&!G.Player.Readiness.IsReady,"casting hero skill clears readiness during cooldown");Input.ActionRelease("hero");}
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
                var boss=G.Enemies.Boss!;Check(boss!=null&&boss.Realm==_boss,"scheduled guardian "+_boss);
                Check(boss!.MaxHealth>1100+_boss*750,"guardian "+_boss+" has increased durability");
                float before=boss.Health;boss.Damage(100,Element.Frost,false);Check(before-boss.Health>80&&before-boss.Health<100,"guardian "+_boss+" has bounded damage resistance");
                Check(BossTuning.Interval(_boss,3)>1.3f&&BossTuning.Interval(_boss,3)<BossTuning.Interval(_boss,1),"guardian "+_boss+" escalates pressure with recovery time");
                boss.Damage(boss.MaxHealth*.4f,Element.Frost);_step++;_frames=0;
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
                {
                    G.StartRun((int)kind,0,Element.Arcane);G.Player!.Weapons.Primary(.5f,Vector2.Right);G.Player.Weapons.Release(Vector2.Right);
                    Check(Art.Weapon(kind).GetWidth()==32&&Art.Weapon(kind).GetHeight()==32,kind+" redesigned icon imports");
                    Check(kind is WeaponKind.Blade or WeaponKind.Gauntlets?G.Effects.LiveCount>0:G.Projectiles.LiveCount>0,kind+" primary produces its attack");
                    G.Player.Weapons.Secondary(Vector2.Right);Check(G.Player.Weapons.AbilityTimer>0,kind+" secondary executes");
                }
                G.Player!.Weapons.Switch(1);Check(G.Player.Weapons.Slot==1,"unlocked hotbar switches to its secondary slot");G.Player.Weapons.Switch(0);G.Player.Stats.SecondSlot=false;G.Player.Weapons.Switch(1);Check(G.Player.Weapons.Slot==0,"locked hotbar refuses secondary slot");G.Player.Stats.SecondSlot=true;
                G.Projectiles.Clear();G.Weapons.Clear();G.Enemies.Clear();G.Effects.Clear();G.State=RunState.Portal;
                for(int i=0;i<300;i++)G.Effects.Particles(G.Player!.Position,Element.Fire,10);
                Check(G.Effects.LiveCount==EffectsManager.Capacity,"effect pool saturates at its fixed capacity");Check(G.Depth.YSortEnabled&&G.Player!.GetParent()==G.Depth&&G.Player.ZIndex==0,"player uses shared scenery depth sorting");_step++;_frames=0;
            }
            else if(_step==6&&_frames>70)
            {
                Check(G.Effects.LiveCount==0,"all burst effects expire and return to the pool");SaveManager.Save();G.ReturnToMenu();Check(G.State==RunState.Menu,"return to menu clears run");GD.Print("SMOKE PASS / "+_checks+" integration checks");SetPhysicsProcess(false);G.Quit();
            }
            if(_frames>1000)throw new Exception("Smoke stage timed out: "+_step);
        }
        catch(Exception ex){GD.PushError("SMOKE FAIL / "+ex);GetTree().Quit(1);}
    }
    private void Check(bool condition,string label)
    {if(!condition){SetPhysicsProcess(false);GetTree().Quit(1);throw new Exception(label);}_checks++;GD.Print("PASS / "+label);}
}
