using Godot;
using System.Collections.Generic;
using System.Linq;
namespace Realmshift;
public sealed class UpgradeSystem
{
    public readonly Dictionary<string,int> Ranks=new();
    public UpgradeData[] Offer()
    {
        var player=GameManager.Instance.Player!;
        var valid=Catalog.Upgrades.Where(x=>Ranks.GetValueOrDefault(x.Id)<x.MaxRank&&(x.Weapon<0||player.Weapons.Slots.Any(w=>(int)w.Data.Kind==x.Weapon))&&(x.Element<0||player.Weapons.Slots.Any(w=>(int)w.Element==x.Element))).ToList();
        var result=new List<UpgradeData>();while(valid.Count>0&&result.Count<3){int i=(int)(GD.Randi() % (uint)valid.Count);result.Add(valid[i]);valid.RemoveAt(i);}
        while(result.Count<3)result.Add(new UpgradeData{Id="field_remedy",DisplayName="FIELD REMEDY",Description="Restore 30 health immediately.",Effect="heal",Amount=30,MaxRank=999});return result.ToArray();
    }
    public void Apply(UpgradeData data)
    {Ranks[data.Id]=Ranks.GetValueOrDefault(data.Id)+1;var p=GameManager.Instance.Player!;p.Stats.Apply(data.Effect,data.Amount);if(data.Effect=="charges")p.Recharge();SaveManager.Data.Relics.Add(data.Id);GameManager.Instance.Audio.Play("level");}
}
