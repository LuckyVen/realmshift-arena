using Godot;
namespace Realmshift;

/// <summary>A small number of tall non-colliding plants use the existing actor/scenery depth root.</summary>
public partial class FoliageProp : Node2D
{
    public Texture2D Texture {get;set;}=null!;
    public int WindOffset {get;set;}
    private float _time,_opacity=1;
    public override void _Ready(){ZIndex=0;TextureFilter=TextureFilterEnum.Nearest;}
    public override void _Process(double delta)
    {
        var g=GameManager.Instance;_time+=(float)delta;var p=g.Player;
        float target=p!=null&&p.Position.DistanceSquaredTo(Position)<22*22?.48f:1;
        _opacity=Mathf.MoveToward(_opacity,target,(float)delta*3);QueueRedraw();
    }
    public override void _Draw()
    {
        int frame=GameManager.Instance.Settings.Particles==0?1:(int)(_time*2+WindOffset)%3;
        DrawTextureRectRegion(Texture,new Rect2(-12,-21,24,24),new Rect2(frame*24,0,24,24),new Color(1,1,1,_opacity));
    }
}
