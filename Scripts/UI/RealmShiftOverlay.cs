using Godot;
namespace Realmshift;

/// <summary>Short pixel-energy sweep; sits below menu text and never intercepts input.</summary>
public partial class RealmShiftOverlay : Control
{
    public override void _Ready(){MouseFilter=MouseFilterEnum.Ignore;Size=new(640,360);TextureFilter=TextureFilterEnum.Nearest;}
    public override void _Process(double delta){Visible=GameManager.Instance.State==RunState.Shift;if(Visible)QueueRedraw();}
    public override void _Draw()
    {
        var g=GameManager.Instance;if(g.State!=RunState.Shift)return;
        float t=Mathf.Clamp(g.Shifts.Progress,0,1),energy=Mathf.Sin(t*Mathf.Pi);int next=g.World.NextRealm;
        Element element=next==0?Element.Nature:next==1?Element.Fire:next==2?Element.Frost:Element.Arcane;Color color=Palette.Realm(next);
        for(int i=0;i<7;i++)
        {float alpha=energy*(7-i)*.009f;DrawRect(new Rect2(i*4,i*4,640-i*8,360-i*8),new Color(color,alpha),false,4);}
        int count=SaveManager.Data.Settings.Particles==0?10:SaveManager.Data.Settings.Particles==2?28:18;
        for(int i=0;i<count;i++)
        {
            float angle=i*Mathf.Tau/count+.35f;float radius=25+t*420+i%4*16;Vector2 p=new Vector2(320,180)+Vector2.FromAngle(angle)*radius*new Vector2(1,.65f);
            VfxSprites.Frame(this,"Particles/"+VfxSprites.Name(element),p,8,8,4,(t*32+i)%4,1.2f,angle,new Color(1,1,1,energy*.75f));
        }
        if(!g.Settings.ReducedFlash)
        {float pulse=Mathf.Max(0,1-Mathf.Abs(t-.74f)/.055f)*.10f;DrawRect(new Rect2(0,0,640,360),new Color(color,pulse));}
    }
}
