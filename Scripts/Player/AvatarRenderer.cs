using Godot;
namespace Realmshift;
public partial class AvatarRenderer : Node2D
{
    public AvatarConfig Config {get;set;}=new();
    public int Facing {get;set;}
    public bool Running {get;set;}
    public float Attack {get;set;}
    public float Hit {get;set;}
    public float Dash {get;set;}
    public bool Defeated {get;set;}
    public bool Victory {get;set;}
    public float Time {get;private set;}
    public override void _Process(double delta){Time+=(float)delta;Attack=Mathf.Max(0,Attack-(float)delta*5);Hit=Mathf.Max(0,Hit-(float)delta*4);QueueRedraw();}
    public override void _Draw()
    {
        int f=Running?(int)(Time*12)%6:0;var src=new Rect2(f*32,Facing*32,32,32);float bob=Running?0:Mathf.Sin(Time*2)*.5f;
        if(Defeated){DrawSetTransform(new Vector2(0,-5),Mathf.Pi/2,new Vector2(1,.7f));}
        else{DrawSetTransform(new Vector2(0,Victory?-Mathf.Abs(Mathf.Sin(Time*4))*4:bob),0,new Vector2(1+Attack*.12f,1-Attack*.08f));}
        var rect=new Rect2(-16,-29,32,32);Color flash=Hit>0?new Color(1.5f,1.5f,1.5f):Colors.White;
        DrawTextureRectRegion(Art.Get($"Characters/body_{Config.Body}.png"),rect,src,new Color(Config.Skin)*flash);
        if(Config.Accessory==2)DrawTexture(Art.Get("Characters/accessory_2.png"),rect.Position,new Color(Config.OutfitTint));
        DrawTextureRectRegion(Art.Get($"Characters/outfit_{Config.Outfit}.png"),rect,src,new Color(Config.OutfitTint)*flash);
        DrawTextureRectRegion(Art.Get($"Characters/hair_{Config.Hair}.png"),rect,src,new Color(Config.HairTint)*flash);
        if(Config.Accessory!=2)DrawTexture(Art.Get($"Characters/accessory_{Config.Accessory}.png"),rect.Position+new Vector2(0,f is 1 or 4?1:0),new Color(Config.OutfitTint)*flash);
        if(Facing==0)
        {Color eyes=new(Config.EyeTint);float y=-20+(f is 1 or 4?1:0);DrawRect(new Rect2(-2,y,1,Config.Eyes==1?1:2),eyes);DrawRect(new Rect2(2,y,1,Config.Eyes==1?1:2),eyes);if(Config.Eyes==2){DrawLine(new(-3,y),new(-1,y+1),eyes);DrawLine(new(1,y+1),new(3,y),eyes);}if(Config.Expression==1)DrawLine(new(-3,y-2),new(0,y-1),eyes);if(Config.Expression==2)DrawLine(new(-1,y+4),new(2,y+4),Palette.Paper);}
        DrawSetTransform(Vector2.Zero);
        if(Config.Aura>0)
        {Color c=Config.Aura<=5?Palette.ElementColor((Element)(Config.Aura-1)):Config.Aura==6?Palette.Gold:new Color("7865a6");for(int i=0;i<5;i++){var p=new Vector2(Mathf.Cos(Time*1.4f+i*1.256f)*16,Mathf.Sin(Time*1.4f+i*1.256f)*7-7);DrawRect(new Rect2(p,2,2),new Color(c,.6f));}}
    }
}
