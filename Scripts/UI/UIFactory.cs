using Godot;
using System;
namespace Realmshift;
public static class UIFactory
{
    public static Font Font=>GD.Load<Font>("res://Assets/Art/UI/realm_font.fnt");
    public static PixelPanel Panel(Control parent,float x,float y,float w,float h)
    {var n=new PixelPanel{Position=new Vector2(x,y),Size=new Vector2(w,h)};parent.AddChild(n);return n;}
    public static Label Text(Control p,string text,float x,float y,float w,float h,int size=10,Color? color=null,bool wrap=false)
    {
        var n=new Label{Text=text,Position=new Vector2(x,y),Size=new Vector2(w,h),AutowrapMode=wrap?TextServer.AutowrapMode.WordSmart:TextServer.AutowrapMode.Off,MouseFilter=Control.MouseFilterEnum.Ignore};
        n.AddThemeFontOverride("font",Font);n.AddThemeFontSizeOverride("font_size",size);n.AddThemeColorOverride("font_color",color??Palette.Paper);p.AddChild(n);return n;
    }
    public static Button Button(Control p,string text,float x,float y,float w,float h,Action click,bool selected=false)
    {
        var n=new Button{Text=text,Position=new Vector2(x,y),Size=new Vector2(w,h),FocusMode=Control.FocusModeEnum.All,MouseDefaultCursorShape=Control.CursorShape.PointingHand};n.AddThemeFontOverride("font",Font);n.AddThemeFontSizeOverride("font_size",10);
        n.AddThemeColorOverride("font_color",Palette.Paper);n.AddThemeColorOverride("font_hover_color",Palette.Gold);n.AddThemeColorOverride("font_disabled_color",new Color("617f7c"));
        foreach(string state in new[]{"normal","hover","pressed","focus","disabled"})
        {Color bg=state=="hover"?new("25493f"):state=="pressed"?new("4c6652"):selected?new("355648"):new("152e32");Color border=state=="hover"||state=="focus"||selected?Palette.Gold:new("466b62");var style=new StyleBoxFlat{BgColor=bg,BorderColor=border,BorderWidthTop=1,BorderWidthBottom=2,BorderWidthLeft=1,BorderWidthRight=1,ContentMarginLeft=6,ContentMarginRight=6,ContentMarginTop=2,ContentMarginBottom=2};n.AddThemeStyleboxOverride(state,style);}
        n.Pressed+=()=>{GameManager.Instance.Audio.Play("ui");click();};p.AddChild(n);if(p.GetViewport().GuiGetFocusOwner()==null)n.GrabFocus();return n;
    }
    public static TextureRect Icon(Control p,Texture2D tex,float x,float y,float size,Color? color=null)
    {var n=new TextureRect{Texture=tex,Position=new(x,y),Size=new(size,size),ExpandMode=TextureRect.ExpandModeEnum.IgnoreSize,StretchMode=TextureRect.StretchModeEnum.KeepAspectCentered,Modulate=color??Colors.White,MouseFilter=Control.MouseFilterEnum.Ignore,TextureFilter=CanvasItem.TextureFilterEnum.Nearest};p.AddChild(n);return n;}
}
