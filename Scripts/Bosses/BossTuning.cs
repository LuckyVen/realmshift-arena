using Godot;
namespace Realmshift;

public readonly record struct BossProfile(float Health,float DamageMultiplier,float DamageTaken,float Interval,float Speed);
/// <summary>Encounter-only tuning. Warning windows and player/weapon resources remain independent.</summary>
public static class BossTuning
{
    private static readonly BossProfile[] Profiles={
        new(1850,1.12f,.94f,2.60f,28),new(3250,1.15f,.92f,2.55f,21),
        new(5100,1.18f,.90f,2.55f,25),new(7400,1.20f,.88f,2.05f,29)};
    public static BossProfile ForRealm(int realm)=>Profiles[Mathf.Clamp(realm,0,3)];
    public static float Health(int realm,int level)=>ForRealm(realm).Health+Mathf.Max(0,level-40)*140;
    public static float Interval(int realm,int phase)=>ForRealm(realm).Interval/(1+(Mathf.Clamp(phase,1,3)-1)*.22f);
}
