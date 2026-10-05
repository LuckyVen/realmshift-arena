using Godot;
namespace Realmshift;
public sealed class WorldShiftManager
{
    public float Progress {get;private set;}
    private int _from;
    private bool _final,_rebuilt;
    private float _particleTimer;
    public void Begin(int from,bool final)
    {
        var g=GameManager.Instance;_from=from;_final=final;_rebuilt=false;Progress=0;_particleTimer=0;g.State=RunState.Shift;
        g.World.NextRealm=(from+1)%4;g.Camera.Focus=g.Player!.Position;g.Audio.Play("shift");g.Camera.Shake(6);
        g.UI.Announce("THE REALM IS SHIFTING",Catalog.Realms[from].ShiftText,5.5f);g.Audio.Music((from+1)%4);
        g.Effects.Sigil(g.Player.Position,110,from==0?Element.Nature:from==1?Element.Fire:from==2?Element.Frost:Element.Arcane,1.1f);
    }
    public void Tick(float dt)
    {
        var g=GameManager.Instance;if(g.State!=RunState.Shift)return;Progress+=dt/5.5f;
        if(!_rebuilt)g.World.Shift=Mathf.Clamp(Progress*1.4f,0,1);
        _particleTimer-=dt;if(_particleTimer<=0)
        {_particleTimer=.1f;int next=(_from+1)%4;Element element=next==0?Element.Nature:next==1?Element.Fire:next==2?Element.Frost:Element.Arcane;var p=g.Player!.Position+Vector2.FromAngle(Progress*25)*Progress*300;g.Effects.Particles(p,element,4);g.Effects.Sigil(g.Player.Position,45+Progress*180,element,.22f);}
        if(Progress>.73f&&!_rebuilt)
        {
            _rebuilt=true;int next=(_from+1)%4;g.World.Build(next);g.World.NextRealm=next;g.Player!.Position=g.World.SafePoint(g.Player.Position);g.Camera.Shake(4);g.Effects.Ring(g.Player.Position,280,Palette.Realm(next),1.2f);g.Effects.Impact(g.Player.Position,next==0?Element.Nature:next==1?Element.Fire:next==2?Element.Frost:Element.Arcane,2.5f,null,false);
        }
        if(Progress>=1)
        {
            g.Camera.Focus=null;
            if(_final){g.Player!.Avatar.Victory=true;g.EndRun(true);}
            else{g.World.PortalActive=true;g.State=RunState.Portal;g.UI.Announce(Catalog.Realms[(_from+1)%4].DisplayName.ToUpper(),"Step through the awakened gate. Press "+InputBindings.Label("interact")+" nearby.",4);}
        }
    }
}
