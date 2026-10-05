using Godot;
using System.Collections.Generic;
namespace Realmshift;

public readonly record struct PlantPlacement(Vector2 Position,int Variant,int WindOffset);
/// <summary>Deterministic visual dressing; never registers an obstacle or touches the map RNG.</summary>
public partial class GroundDressing : Node2D
{
    public const int Capacity=2000;
    public IReadOnlyList<PlantPlacement> Plants=>_plants;
    private readonly List<PlantPlacement> _plants=new(Capacity);
    private readonly Texture2D[] _textures=new Texture2D[6];
    private WorldManager _world=null!;
    private float _time;
    private static int Hash(int x,int y)=>unchecked(x*73856093^y*19349663)&0x7fffffff;
    public void Build(WorldManager world,Node2D scenery)
    {
        _world=world;_plants.Clear();ZIndex=1;TextureFilter=TextureFilterEnum.Nearest;
        for(int i=0;i<6;i++)_textures[i]=Art.Get($"Foliage/realm_{world.Realm}_{i}.svg");
        int foreground=0;
        for(int y=2;y<(int)WorldManager.Size.Y/32-2;y++)for(int x=2;x<(int)WorldManager.Size.X/32-2;x++)
        {
            int hash=Hash(x+world.Realm*37,y+19),cluster=Hash(x/4+17,y/4+world.Realm*13);
            if(cluster%9>=4||hash%10>=6||_plants.Count>=Capacity)continue;
            Vector2 p=new(x*32+8+hash%17,y*32+4+(hash/31)%21);
            if(world.GroundKindAt(p)!=0||!world.CanStand(p,12)||p.DistanceTo(world.Spawn)<62)continue;
            int pick=(hash/11)%12;int variant=pick<4?0:pick<7?1:pick<9?2:pick==9?3:pick==10?4:5;
            _plants.Add(new(p,variant,hash%3));
            if(variant is 3 or 5&&hash%3==0&&foreground<80)
            {scenery.AddChild(new FoliageProp{Position=p,Texture=_textures[variant],WindOffset=hash%3});foreground++;}
        }
        QueueRedraw();
    }
    public override void _Process(double delta){if(_world==null)return;_time+=(float)delta;QueueRedraw();}
    public override void _Draw()
    {
        if(_world==null)return;var g=GameManager.Instance;Vector2 center=g.Camera?.GlobalPosition??_world.Spawn;
        float alpha=1-_world.Shift;Color patch=Palette.Realm(_world.Realm).Darkened(.45f);
        for(int i=0;i<_plants.Count;i++)
        {
            var plant=_plants[i];if(Mathf.Abs(plant.Position.X-center.X)>390||Mathf.Abs(plant.Position.Y-center.Y)>240)continue;
            var p=plant.Position;DrawRect(new Rect2(p+new Vector2(-9,-3),18,5),new Color(patch,.19f*alpha));
            DrawRect(new Rect2(p+new Vector2(-6,-5),12,2),new Color(patch,.14f*alpha));
            int frame=g.Settings.Particles==0?1:(int)(_time*2+plant.WindOffset)%3;
            DrawTextureRectRegion(_textures[plant.Variant],new Rect2(p+new Vector2(-12,-21),24,24),new Rect2(frame*24,0,24,24),new Color(1,1,1,.92f*alpha));
        }
    }
}

