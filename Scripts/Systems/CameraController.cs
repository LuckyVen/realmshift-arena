using Godot;
namespace Realmshift;
public partial class CameraController : Camera2D
{
    public PlayerController? Target {get;set;}
    public Vector2? Focus {get;set;}
    private float _shake;
    public override void _Ready(){PositionSmoothingEnabled=false;Zoom=Vector2.One;MakeCurrent();}
    public void Shake(float value){if(SaveManager.Data.Settings.Shake)_shake=Mathf.Min(6,_shake+value);}
    public override void _Process(double delta)
    {
        var point=Focus??(Target!=null?Target.Position+Target.Aim*24:WorldManager.Size/2);
        Position=Position.Lerp(point,1-Mathf.Exp(-(float)delta*8));
        Position=Position.Clamp(new Vector2(320,180),WorldManager.Size-new Vector2(320,180));
        _shake=Mathf.Max(0,_shake-(float)delta*16);Offset=SaveManager.Data.Settings.Shake?(new Vector2(GD.Randf()-.5f,GD.Randf()-.5f)*_shake).Round():Vector2.Zero;
    }
}
