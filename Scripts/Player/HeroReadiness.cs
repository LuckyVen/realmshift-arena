using Godot;
namespace Realmshift;

/// <summary>Reads the real hero timer; no extra ability state or saved fields.</summary>
public partial class HeroReadiness : Node2D
{
    public PlayerController Player {get;set;}=null!;
    public bool IsReady=>!Player.Dead&&Player.HeroTimer<=0;
    private float _time;
    private bool _wasReady=true;
    public static Element ElementFor(int hero)=>hero is 0 or 1?Element.Nature:hero==2?Element.Fire:hero==4?Element.Frost:hero==5?Element.Lightning:Element.Arcane;
    public override void _Ready(){ZIndex=-1;TextureFilter=TextureFilterEnum.Nearest;}
    public override void _Process(double delta)
    {
        var g=GameManager.Instance;if(g.Running)_time+=(float)delta;
        if(IsReady&&!_wasReady&&g.Running)g.Effects.Sigil(Player.Position,22,ElementFor(Player.Avatar.Config.Hero),.45f);
        _wasReady=IsReady;QueueRedraw();
    }
    public override void _Draw()
    {
        if(!IsReady)return;var g=GameManager.Instance;if(g.State is RunState.Menu or RunState.Defeat or RunState.Victory)return;
        Color color=Palette.ElementColor(ElementFor(Player.Avatar.Config.Hero));float pulse=g.Settings.ReducedFlash?0:(Mathf.Sin(_time*2)+1)*.5f;
        DrawSetTransform(new Vector2(0,1),0,new Vector2(1,.55f));
        for(int i=0;i<3;i++){float a=i*Mathf.Tau/3+.3f;DrawArc(Vector2.Zero,13,a,a+.55f,7,new Color(color,.32f+.12f*pulse),1);}
        DrawSetTransform(Vector2.Zero);
        for(int i=0;i<3;i++)
        {float a=_time*.65f+i*Mathf.Tau/3;Vector2 q=new(Mathf.Cos(a)*14,-9+Mathf.Sin(a)*5);DrawRect(new Rect2(q.Round(),1,2),new Color(color,.45f));}
    }
}
