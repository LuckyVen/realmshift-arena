using Godot;
using System;
namespace Realmshift;
public static class UIFactory
{
    public const int Padding=12, Gap=8, ButtonHeight=28;
    private static Font? _font;
    private static Font? _smallFont;
    public static Font SmallFont=>_smallFont??=GD.Load<Font>("res://Assets/Art/UI/realm_font.fnt");
    public static Font Font
    {
        get
        {
            if(_font==null){var pixel=GD.Load<FontFile>("res://Assets/Art/UI/realm_pixel.ttf");pixel.Antialiasing=TextServer.FontAntialiasing.None;pixel.Hinting=TextServer.Hinting.None;pixel.SubpixelPositioning=TextServer.SubpixelPositioning.Disabled;pixel.Oversampling=1;_font=pixel;}
            return _font;
        }
    }
    public static void Release(){_font?.Dispose();_font=null;_smallFont?.Dispose();_smallFont=null;}
    public static PixelPanel Panel(Control parent,float x,float y,float w,float h)
    {var n=new PixelPanel{Position=new Vector2(x,y),Size=new Vector2(w,h)};parent.AddChild(n);return n;}
    public static Label Text(Control p,string text,float x,float y,float w,float h,int size=10,Color? color=null,bool wrap=false)
    {
        // A fixed layout slot prevents a label's initial theme minimum from widening its panel.
        var slot=new Control{Position=new(x,y),Size=new(w,h),MouseFilter=Control.MouseFilterEnum.Ignore};p.AddChild(slot);
        var n=new Label{AutowrapMode=wrap?TextServer.AutowrapMode.WordSmart:TextServer.AutowrapMode.Off,Text=text,MouseFilter=Control.MouseFilterEnum.Ignore,TextOverrunBehavior=TextServer.OverrunBehavior.TrimEllipsis};
        n.AddThemeFontOverride("font",Font);
        if(!wrap&&text.IndexOf('\n')<0)while(size>8&&Font.GetStringSize(text,HorizontalAlignment.Left,-1,size).X>w)size--;
        n.AddThemeFontSizeOverride("font_size",size);n.AddThemeColorOverride("font_color",color??Palette.Paper);slot.AddChild(n);n.Position=Vector2.Zero;n.Size=new(w,h);return n;
    }
    public static Button Button(Control p,string text,float x,float y,float w,float h,Action click,bool selected=false)
    {
        var n=new Button{Text=text,ClipText=true,TextOverrunBehavior=TextServer.OverrunBehavior.TrimEllipsis,FocusMode=Control.FocusModeEnum.All,MouseDefaultCursorShape=Control.CursorShape.PointingHand};n.AddThemeFontOverride("font",Font);n.AddThemeFontSizeOverride("font_size",10);
        n.AddThemeColorOverride("font_color",Palette.Paper);n.AddThemeColorOverride("font_hover_color",Palette.Gold);n.AddThemeColorOverride("font_disabled_color",new Color("617f7c"));
        foreach(string state in new[]{"normal","hover","pressed","focus","disabled"})
        {Color bg=state=="hover"?new("25493f"):state=="pressed"?new("4c6652"):selected?new("355648"):new("152e32");Color border=state=="hover"||state=="focus"||selected?Palette.Gold:new("466b62");var style=new StyleBoxFlat{BgColor=bg,BorderColor=border,BorderWidthTop=1,BorderWidthBottom=2,BorderWidthLeft=1,BorderWidthRight=1,ContentMarginLeft=6,ContentMarginRight=6,ContentMarginTop=2,ContentMarginBottom=2};n.AddThemeStyleboxOverride(state,style);}
        n.Pressed+=()=>{GameManager.Instance.Audio.Play("ui");click();};p.AddChild(n);n.Position=new(x,y);n.Size=new(w,h);if(p.GetViewport().GuiGetFocusOwner()==null)n.GrabFocus();return n;
    }
    public static VBoxContainer Flow(Control parent,float x,float y,float w,float h,bool scroll=false)
    {
        var margin=new MarginContainer{Position=new(x,y),Size=new(w,h),MouseFilter=Control.MouseFilterEnum.Ignore};parent.AddChild(margin);
        foreach(string side in new[]{"left","right","top","bottom"})margin.AddThemeConstantOverride("margin_"+side,Padding);
        var box=new VBoxContainer{SizeFlagsHorizontal=Control.SizeFlags.ExpandFill};box.AddThemeConstantOverride("separation",Gap);
        if(scroll){var view=new ScrollContainer{HorizontalScrollMode=ScrollContainer.ScrollMode.Disabled,SizeFlagsHorizontal=Control.SizeFlags.ExpandFill,SizeFlagsVertical=Control.SizeFlags.ExpandFill};margin.AddChild(view);view.AddChild(box);}else margin.AddChild(box);
        return box;
    }
    public static Label FlowText(Container parent,string text,int size=10,Color? color=null,float minHeight=0)
    {
        var n=new Label{AutowrapMode=TextServer.AutowrapMode.WordSmart,Text=text,SizeFlagsHorizontal=Control.SizeFlags.ExpandFill,CustomMinimumSize=new(0,minHeight),MouseFilter=Control.MouseFilterEnum.Ignore};n.AddThemeFontOverride("font",Font);n.AddThemeFontSizeOverride("font_size",size);n.AddThemeColorOverride("font_color",color??Palette.Paper);parent.AddChild(n);return n;
    }
    public static Button FlowButton(Container parent,string text,Action click,bool selected=false,float height=ButtonHeight)
    {var b=Button(parent,text,0,0,0,height,click,selected);b.CustomMinimumSize=new(0,height);b.SizeFlagsHorizontal=Control.SizeFlags.ExpandFill;return b;}
    public static TextureRect Icon(Control p,Texture2D tex,float x,float y,float size,Color? color=null)
    {var n=new TextureRect{Texture=tex,Position=new(x,y),Size=new(size,size),ExpandMode=TextureRect.ExpandModeEnum.IgnoreSize,StretchMode=TextureRect.StretchModeEnum.KeepAspectCentered,Modulate=color??Colors.White,MouseFilter=Control.MouseFilterEnum.Ignore,TextureFilter=CanvasItem.TextureFilterEnum.Nearest};p.AddChild(n);return n;}
}
