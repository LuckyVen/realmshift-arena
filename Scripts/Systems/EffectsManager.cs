using Godot;
using System.Collections.Generic;
namespace Realmshift;
public sealed class VisualEffect
{
    public Vector2 Position,End,Velocity;public Color Color;public float Life,MaxLife,Radius,Angle,Span;public int Kind,Number;public AvatarConfig? Avatar;public int Facing;
}
public partial class EffectsManager : Node2D
{
    private readonly List<VisualEffect> _items=new();
    private Font _font=null!;
    public override void _Ready(){ZIndex=25;_font=GD.Load<Font>("res://Assets/Art/UI/realm_font.fnt");}
    private void Add(VisualEffect e){if(_items.Count<450){e.MaxLife=e.Life;_items.Add(e);}}
    public void Burst(Vector2 p,Color c,int count)
    {
        count=SaveManager.Data.Settings.Particles==0?count/3:SaveManager.Data.Settings.Particles==2?count*2:count;
        for(int i=0;i<count;i++)Add(new(){Position=p,Velocity=Vector2.FromAngle(GD.Randf()*Mathf.Tau)*GD.RandRange(12,65),Life=(float)GD.RandRange(.18,.45),Color=c,Kind=0});
    }
    public void Ring(Vector2 p,float radius,Color c,float life)=>Add(new(){Position=p,Radius=radius,Color=c,Life=life,Kind=1});
    public void Line(Vector2 p,Vector2 end,Color c,float life)=>Add(new(){Position=p,End=end,Color=c,Life=life,Kind=2});
    public void Arc(Vector2 p,float radius,float angle,float span,Color c)=>Add(new(){Position=p,Radius=radius,Angle=angle,Span=span,Life=.2f,Color=c,Kind=3});
    public void Number(Vector2 p,int n,Color c){if(SaveManager.Data.Settings.Numbers)Add(new(){Position=p,Velocity=new Vector2(GD.Randf()*12-6,-24),Number=n,Life=.65f,Color=c,Kind=4});}
    public void Ghost(Vector2 p,AvatarConfig config,int facing)=>Add(new(){Position=p,Avatar=config,Facing=facing,Life=.18f,Color=Palette.Teal,Kind=5});
    public override void _Process(double delta)
    {
        if(GameManager.Instance.State==RunState.Pause||GameManager.Instance.State==RunState.Upgrade)return;float dt=(float)delta;
        for(int i=_items.Count-1;i>=0;i--){var e=_items[i];e.Life-=dt;e.Position+=e.Velocity*dt;if(e.Life<=0)_items.RemoveAt(i);}QueueRedraw();
    }
    public void Clear(){_items.Clear();QueueRedraw();}
    public override void _Draw()
    {
        foreach(var e in _items)
        {
            float a=e.Life/e.MaxLife;Color c=new(e.Color,a);
            switch(e.Kind)
            {case 0:DrawRect(new Rect2(e.Position,2,2),c);break;
             case 1:DrawArc(e.Position,e.Radius*(1-a*.3f),0,Mathf.Tau,32,c,1);break;
             case 2:DrawLine(e.Position,e.End,c,1);break;
             case 3:DrawArc(e.Position,e.Radius,e.Angle-e.Span/2,e.Angle+e.Span/2,18,c,3);break;
             case 4:DrawString(_font,e.Position,e.Number.ToString(),HorizontalAlignment.Center,-1,10,c);break;
             case 5:DrawTextureRectRegion(Art.Get($"Characters/outfit_{e.Avatar!.Outfit}.png"),new Rect2(e.Position-new Vector2(16,29),32,32),new Rect2(0,e.Facing*32,32,32),c);break;}
        }
    }
}
