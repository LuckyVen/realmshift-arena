namespace Realmshift;
public sealed class PlayerStats
{
    public float MaxHealth=100,Health=100,Shield,MoveSpeed=112,Damage=1,AttackRate=1,Range=1,Armor,Crit=.08f,Regen,Magnet=65;
    public float DashCooldown=1.6f,DashLength=1,HeroCooldown=18,HeroPower=1,Duration=1,Area=1,ElementPower=1;
    public int DashCharges=1,ExtraProjectiles,Pierce,Chain,OrbCount=3;
    public bool SecondSlot=true,Leech,Thorns,Explosive,ReturnShots;
    public void Apply(string key,float value)
    {
        switch(key)
        {
            case "damage":Damage+=value;break;case "rate":AttackRate+=value;break;case "range":Range+=value;break;
            case "health":MaxHealth+=value;Health=System.Math.Min(MaxHealth,Health+value);break;
            case "armor":Armor+=value;break;case "regen":Regen+=value;break;case "crit":Crit+=value;break;case "magnet":Magnet+=value;break;
            case "speed":MoveSpeed+=value;break;case "dash":DashCooldown=System.Math.Max(.45f,DashCooldown-value);break;case "charges":DashCharges+=(int)value;break;
            case "dashlength":DashLength+=value;break;case "multi":ExtraProjectiles+=(int)value;break;case "pierce":Pierce+=(int)value;break;
            case "chain":Chain+=(int)value;break;case "orbs":OrbCount+=(int)value;break;case "area":Area+=value;break;case "element":ElementPower+=value;break;
            case "hero":HeroCooldown=System.Math.Max(5,HeroCooldown-value);break;case "heropower":HeroPower+=value;break;
            case "leech":Leech=true;break;case "thorns":Thorns=true;break;case "explode":Explosive=true;break;case "return":ReturnShots=true;break;
            case "duration":Duration+=value;break;case "heal":Health=System.Math.Min(MaxHealth,Health+value);break;case "shield":Shield+=value;break;
        }
    }
}
