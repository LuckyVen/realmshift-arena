using Godot;
namespace Realmshift;

/// <summary>North-up local radar. Terrain refreshes periodically and after movement; actors remain live.</summary>
public partial class CircularMinimap : Control
{
    public const float Range=720,Radius=34;
    public static readonly Vector2 Center=new(42,42);
    public static readonly Color EnemyColor=new("ff6573");
    private ImageTexture? _terrain;
    private Font _font=null!;
    private float _refresh;
    private Vector2 _origin;
    private int _realm=-1;
    public int VisibleEnemyMarkers {get;private set;}
    public static Vector2 Project(Vector2 position,Vector2 origin)=>Center+(position-origin)*(Radius/Range);
    public static Vector2 Edge(Vector2 point)=>Center+(point-Center).LimitLength(Radius-2);
    public static bool Inside(Vector2 point)=>(point-Center).LengthSquared()<=Mathf.Pow(Radius-2+.01f,2);
    public override void _Ready(){Size=new(84,84);MouseFilter=MouseFilterEnum.Ignore;_font=UIFactory.SmallFont;TextureFilter=TextureFilterEnum.Nearest;}
    public override void _Process(double delta)
    {
        if(!IsVisibleInTree())return;var g=GameManager.Instance;var p=g.Player;if(p==null)return;
        _refresh-=(float)delta;
        if(_terrain==null||_realm!=g.World.Realm||_refresh<=0||p.Position.DistanceSquaredTo(_origin)>144){_refresh=.25f;_origin=p.Position;_realm=g.World.Realm;RefreshTerrain(g.World);}
        QueueRedraw();
    }
    private void RefreshTerrain(WorldManager world)
    {
        using var image=Image.CreateEmpty(72,72,false,Image.Format.Rgba8);image.Fill(Colors.Transparent);
        Color soil=new[]{new Color("243e39"),new Color("46332e"),new Color("344956"),new Color("332d4b")}[world.Realm];
        for(int y=0;y<72;y+=2)for(int x=0;x<72;x+=2)
        {
            Vector2 offset=new(x-35.5f,y-35.5f);Vector2 pos=_origin+offset*(Range/Radius);
            int kind=world.GroundKindAt(pos);Color color=kind<0?new Color("13242d"):kind==1?soil.Lightened(.23f):kind==2?new Color("446877"):soil;
            for(int yy=0;yy<2;yy++)for(int xx=0;xx<2;xx++)if(new Vector2(x+xx-35.5f,y+yy-35.5f).LengthSquared()<Radius*Radius)image.SetPixel(x+xx,y+yy,color);
        }
        if(_terrain==null)_terrain=ImageTexture.CreateFromImage(image);else _terrain.Update(image);
    }
    private void Marker(Vector2 q,Color color,int size=3)
    {q=q.Round();DrawRect(new Rect2(q-new Vector2(size/2+1,size/2+1),size+2,size+2),Palette.Ink);DrawRect(new Rect2(q-new Vector2(size/2,size/2),size,size),color);}
    private void Objective(Vector2 position,Vector2 origin,Color color,bool important=false)
    {
        Vector2 q=Project(position,origin);bool outside=!Inside(q);if(outside&&!important)return;q=Edge(q).Round();
        HUDWidgets.Diamond(this,q,important?4:2,Palette.Ink);HUDWidgets.Diamond(this,q,important?3:1,color);
        if(outside)DrawLine(q,Center+(q-Center).Normalized()*(Radius-7),color,1);
    }
    public override void _Draw()
    {
        var g=GameManager.Instance;var p=g.Player;if(p==null)return;
        DrawCircle(Center,39,Palette.Ink);DrawCircle(Center,37,new Color("627d70"));DrawCircle(Center,35,new Color("152a33"));
        if(_terrain!=null)DrawTexture(_terrain,Center-new Vector2(36,36));
        DrawArc(Center,37,Mathf.Pi,Mathf.Tau,48,new Color("b2c29c"),1);
        DrawLine(Center+new Vector2(-38,0),Center+new Vector2(-35,0),Palette.Muted);DrawLine(Center+new Vector2(35,0),Center+new Vector2(38,0),Palette.Muted);
        foreach(var chest in g.World.Chests)Objective(chest,p.Position,Palette.Gold);
        foreach(var shrine in g.World.Shrines)Objective(shrine,p.Position,Palette.Teal);
        VisibleEnemyMarkers=0;bool nearDanger=false;
        foreach(var enemy in g.Enemies.Active)
        {
            if(!enemy.Active||enemy.IsBoss)continue;var q=Project(enemy.Position,p.Position);if(!Inside(q))continue;
            Marker(q,EnemyColor,2);VisibleEnemyMarkers++;if((q-Center).LengthSquared()<36)nearDanger=true;
        }
        if(g.Enemies.Boss is {Active:true} boss){var q=Edge(Project(boss.Position,p.Position));HUDWidgets.Diamond(this,q,5,Palette.Ink);HUDWidgets.Diamond(this,q,3.5f,new Color("ff485d"));VisibleEnemyMarkers++;}
        if(g.World.PortalActive)Objective(g.World.PortalPosition,p.Position,new Color("d4adff"),true);
        if(nearDanger)DrawArc(Center,6,0,Mathf.Tau,24,EnemyColor,1);
        Vector2 aim=p.Aim.Normalized();DrawCircle(Center,3,Palette.Ink);DrawCircle(Center,2,Palette.Paper);DrawLine(Center+aim*2,Center+aim*5,Palette.Teal,2);
        DrawRect(new Rect2(35,3,14,9),Palette.Ink);HUDWidgets.CenterText(this,_font,"N",new(42,10),7,Palette.Paper);
    }
    public override void _ExitTree(){_terrain?.Dispose();_terrain=null;}
}
