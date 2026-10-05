using Godot;
namespace Realmshift;

/// <summary>Scenery shares its foot-position sort key with characters. Canopies fade only when occluding the hero.</summary>
public partial class WorldProp : Node2D
{
    public Texture2D Texture {get;set;}=null!;
    public Vector2 Offset {get;set;}
    public bool HasCanopy {get;set;}
    private float _canopyAlpha=1;
    public Rect2 CanopyRect=>new(GlobalPosition+Offset,new Vector2(Texture.GetWidth(),64));
    public override void _Ready(){TextureFilter=TextureFilterEnum.Nearest;ZIndex=0;}
    public override void _Process(double delta)
    {
        if(!HasCanopy)return;
        var p=GameManager.Instance.Player;
        bool hidden=p!=null&&p.GlobalPosition.Y<GlobalPosition.Y&&CanopyRect.Intersects(new Rect2(p.GlobalPosition-new Vector2(10,27),20,27));
        float target=hidden?.82f:1;
        _canopyAlpha=Mathf.MoveToward(_canopyAlpha,target,(float)delta*1.8f);QueueRedraw();
    }
    public override void _Draw()
    {
        if(HasCanopy)
        {
            // Existing art includes the grounding shadow in its lower layer.
            DrawTextureRectRegion(Texture,new Rect2(Offset+new Vector2(0,64),Texture.GetWidth(),Texture.GetHeight()-64),new Rect2(0,64,Texture.GetWidth(),Texture.GetHeight()-64));
            DrawTextureRectRegion(Texture,new Rect2(Offset,Texture.GetWidth(),64),new Rect2(0,0,Texture.GetWidth(),64),new Color(1,1,1,_canopyAlpha));
        }
        else DrawTexture(Texture,Offset);
    }
}
