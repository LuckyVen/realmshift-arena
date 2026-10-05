using Godot;
namespace Realmshift;

/// <summary>Pixel-scale HUD drawing shared by health, equipment and ability cues.</summary>
public static class HUDWidgets
{
    public static void Frame(CanvasItem c,Rect2 r,Color fill,Color rim,bool active=false)
    {
        c.DrawRect(r,Palette.Ink);
        c.DrawRect(new Rect2(r.Position+Vector2.One,r.Size-Vector2.One*2),rim);
        c.DrawRect(new Rect2(r.Position+Vector2.One*3,r.Size-Vector2.One*6),fill);
        c.DrawLine(r.Position+new Vector2(3,2),r.Position+new Vector2(r.Size.X-4,2),rim.Lightened(.18f),1);
        c.DrawLine(r.End-new Vector2(r.Size.X-3,2),r.End-new Vector2(3,2),rim.Darkened(.3f),1);
        foreach(var p in new[]{r.Position,r.Position+new Vector2(r.Size.X-2,0),r.Position+new Vector2(0,r.Size.Y-2),r.End-Vector2.One*2})c.DrawRect(new Rect2(p,2,2),Palette.Ink);
        if(active)
        {
            c.DrawLine(r.Position+new Vector2(5,4),r.Position+new Vector2(12,4),Palette.Paper,1);
            c.DrawLine(r.Position+new Vector2(4,5),r.Position+new Vector2(4,12),Palette.Paper,1);
            c.DrawRect(new Rect2(r.Position+new Vector2(r.Size.X/2-2,-5),4,2),rim);
            c.DrawRect(new Rect2(r.Position+new Vector2(r.Size.X/2-1,-3),2,2),rim);
        }
    }
    public static void Diamond(CanvasItem c,Vector2 p,float radius,Color color)
    {c.DrawColoredPolygon(new[]{p+new Vector2(0,-radius),p+new Vector2(radius,0),p+new Vector2(0,radius),p+new Vector2(-radius,0)},color);}
    public static void CenterText(CanvasItem c,Font font,string text,Vector2 baseline,int size,Color color,float width=-1)
    {c.DrawString(font,baseline-new Vector2(font.GetStringSize(text,HorizontalAlignment.Left,-1,size).X/2,0),text,HorizontalAlignment.Left,width,size,color);}
    public static void Ability(CanvasItem c,Vector2 center,Texture2D icon,Color color,float cooldown,float maximum,bool reducedFlash,float time)
    {
        bool ready=cooldown<=0;float pulse=reducedFlash?0:(Mathf.Sin(time*2.2f)+1)*.5f;
        c.DrawCircle(center,13,Palette.Ink);c.DrawCircle(center,11,new Color("193039"));
        c.DrawTextureRect(icon,new Rect2(center-new Vector2(9,9),18,18),false,new Color(color,ready?1:.35f));
        c.DrawArc(center,12,0,Mathf.Tau,32,new Color(color,ready?.5f+.2f*pulse:.22f),1);
        if(ready)
        {
            for(int i=0;i<4;i++)
            {float a=i*Mathf.Pi/2+.3f;c.DrawArc(center,15,a,a+.45f,6,new Color(color,.65f+.15f*pulse),1);}
        }
        else c.DrawArc(center,12,-Mathf.Pi/2,-Mathf.Pi/2+Mathf.Tau*(1-Mathf.Clamp(cooldown/Mathf.Max(.001f,maximum),0,1)),32,color,2);
    }
}
