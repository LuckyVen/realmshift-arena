using Godot;
namespace Realmshift;
public partial class PixelPanel : Control
{
    public Color Border {get;set;}=new("5b8179");
    public bool Solid {get;set;}
    public override void _Ready(){MouseFilter=MouseFilterEnum.Ignore;}
    public override void _Draw()
    {
        DrawRect(new Rect2(Vector2.Zero,Size),new Color("071a26e8"));
        DrawRect(new Rect2(1,1,Size.X-2,Size.Y-2),Border,false,1);
        DrawRect(new Rect2(3,3,Size.X-6,Size.Y-6),new Color("24413e"),false,1);
        foreach(var p in new[]{new Vector2(0,0),new Vector2(Size.X-4,0),new Vector2(0,Size.Y-4),Size-new Vector2(4,4)})DrawRect(new Rect2(p,4,4),Palette.Gold);
        if(Solid)DrawLine(new Vector2(10,7),new Vector2(Size.X-10,7),new Color(Border,.25f));
    }
}
