using Godot;
namespace Realmshift;
public partial class MenuBackdrop : Control
{
    public float Time;
    public override void _Ready(){MouseFilter=MouseFilterEnum.Ignore;Size=new(640,360);}
    public override void _Process(double delta){Time+=(float)delta;QueueRedraw();}
    public override void _Draw()
    {
        DrawRect(new Rect2(0,0,640,360),new Color("051623a9"));
        for(int x=0;x<330;x+=8)DrawRect(new Rect2(x,0,8,360),new Color(0.02f,.07f,.1f,.55f*(1-x/330f)));
        for(int i=0;i<22;i++){Vector2 p=new(Mathf.PosMod(i*41+Mathf.Sin(Time+i)*8,640),Mathf.PosMod(i*67-Time*(3+i%4),360));DrawRect(new Rect2(p,2,2),new Color(Palette.Teal,.2f+.2f*Mathf.Sin(Time+i)));}
    }
}
