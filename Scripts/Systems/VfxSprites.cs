using Godot;
namespace Realmshift;

/// <summary>Nearest-filter frames shared by pooled shots, impacts, fields, melee and status effects.</summary>
public static class VfxSprites
{
    public static string Name(Element element)=>element.ToString().ToLowerInvariant();
    public static void Frame(CanvasItem canvas,string path,Vector2 position,int width,int height,int frames,float phase,float scale=1,float angle=0,Color? color=null)
    {
        var texture=Art.Get("VFX/"+path+".png");int frame=Mathf.Clamp((int)phase,0,frames-1);
        canvas.DrawSetTransform(position.Round(),angle,Vector2.One*scale);
        canvas.DrawTextureRectRegion(texture,new Rect2(-width/2f,-height/2f,width,height),new Rect2(frame*width,0,width,height),color??Colors.White);
        canvas.DrawSetTransform(Vector2.Zero);
    }
    public static void Glow(CanvasItem canvas,Vector2 position,Element element,float size,float alpha=.3f)
    {
        if(SaveManager.Data.Settings.Particles<2||SaveManager.Data.Settings.ReducedFlash)return;
        Frame(canvas,"Lights/pulse",position,32,32,1,0,size/32f,0,new Color(Palette.ElementColor(element),alpha));
    }
    public static void Bolt(CanvasItem canvas,Vector2 position,Vector2 velocity,Element element,int style,float age,bool enemy=false,float alpha=1)
    {
        float angle=velocity.LengthSquared()>.1f?velocity.Angle():0;Color tint=new(1,1,1,alpha);
        float scale=style==3?1.4f:1;
        if(enemy){canvas.DrawArc(position,7,0,Mathf.Tau,12,new Color("fc9fbe"),1);scale=.86f;}
        else Glow(canvas,position,element,style==3?38:24,.22f);
        if(style==1)
        {
            Frame(canvas,"Projectiles/arrow",position-Vector2.FromAngle(angle)*14,32,12,4,(age*20)%4,1,angle,tint);
            Frame(canvas,"Particles/"+Name(element),position,8,8,4,(age*20)%4,.65f,angle,tint);
        }
        else if(style==4)Frame(canvas,"Projectiles/chakram",position,24,24,8,(age*24)%8,1,age*4,tint);
        else if(style==5)Frame(canvas,"Projectiles/orb",position,20,20,8,(age*16)%8,.85f,age*.8f,new Color(Palette.ElementColor(element),alpha));
        else Frame(canvas,"Projectiles/"+Name(element),position,24,24,8,(age*22)%8,scale,angle,tint);
    }
    public static void Field(CanvasItem canvas,Vector2 position,float radius,Element element,float age,float alpha=.65f)
    {Frame(canvas,"Fields/"+Name(element),position,64,64,8,(age*12)%8,radius/28f,0,new Color(1,1,1,alpha));}
}
