namespace Realmshift;
public sealed class ExperienceSystem
{
    public int Rank {get;private set;}=1;
    public int Experience {get;private set;}
    public int Required=>18+Rank*9;
    public int Pending {get;private set;}
    public void Add(int value)
    {Experience+=value;while(Experience>=Required){Experience-=Required;Rank++;Pending++;}TryOffer();}
    public void TryOffer()
    {var g=GameManager.Instance;if(Pending>0&&(g.State==RunState.Combat||g.State==RunState.Rest)){Pending--;g.UI.ShowUpgrades();}}
    public void ConsumeAll()=>Pending=0;
}
