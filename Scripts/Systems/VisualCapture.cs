using Godot;
using System;
using System.IO;
namespace Realmshift;
/// <summary>Offscreen screenshot harness for visual QA. Not used by normal game startup.</summary>
public partial class VisualCapture : Node
{
    private int _stage,_frames;
    private string _directory="";
    private GameManager G=>GameManager.Instance;
    public override void _Ready()
    {
        _directory=Array.Find(OS.GetCmdlineUserArgs(),s=>s.StartsWith("--capture="))![10..];Directory.CreateDirectory(_directory);
        SaveManager.Data.Shards=240;SaveManager.Data.Heroes=new(){0,1,2,3,4,5,6,7};SaveManager.Data.Weapons=new(){0,1,2,3,4,5,6,7};SaveManager.Data.Bosses=new(){0,1,2,3};SaveManager.Data.Tutorial=true;SaveManager.Data.Settings.AutoAim=true;SaveManager.Data.Endless=false;
        G.UI.ShowMenu();
    }
    public override void _Process(double delta)
    {
        _frames++;if(G.State==RunState.Upgrade){G.UI.Close();G.State=RunState.Combat;G.Experience.ConsumeAll();}
        int wait=_stage is >=4 and <=8?250:60;if(_frames<wait)return;_frames=0;
        string[] names={"00-main-menu","01-characters","02-customization","03-loadout","04-greenward","05-emberglass","06-frostveil","07-astral","08-boss","09-world-shift","10-armory","11-settings"};
        var img=GetViewport().GetTexture().GetImage();img.SavePng(Path.Combine(_directory,names[_stage]+".png"));GD.Print("CAPTURE / "+names[_stage]);_stage++;
        switch(_stage)
        {
            case 1:G.UI.ShowCharacters();break;case 2:G.UI.ShowCreator();break;case 3:G.UI.ShowLoadout();break;
            case 4:G.StartRun(0,1,Element.Fire);G.Player!.Position=G.World.SafePoint(new Vector2(1130,1056));Input.ActionPress("attack");break;
            case 5:G.Level.SkipTo(11);break;case 6:G.Level.SkipTo(21);break;case 7:G.Level.SkipTo(31);G.Player!.Weapons.Equip(0,5,Element.Arcane,4);break;
            case 8:G.Level.SkipTo(40);G.Player!.Weapons.Equip(0,0,Element.Lightning,0);break;
            case 9:Input.ActionRelease("attack");G.Enemies.Boss!.Damage(999999,Element.Arcane);break;
            case 10:G.ReturnToMenu();G.UI.ShowArmory(5);break;case 11:G.UI.ShowSettings(G.UI.ShowMenu);break;
            default:SetProcess(false);G.Quit();break;
        }
    }
}
