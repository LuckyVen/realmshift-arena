from pathlib import Path
import json
ROOT=Path(__file__).resolve().parents[1]
def resource(folder,name,cls,props):
 p=ROOT/'Resources'/folder/(name+'.tres')
 s=f'[gd_resource type="Resource" script_class="{cls}" load_steps=2 format=3]\n[ext_resource type="Script" path="res://Scripts/Data/{cls}.cs" id="1"]\n[resource]\nscript=ExtResource("1")\n'
 for k,v in props.items():
  s+=k+'='+ (json.dumps(v) if isinstance(v,str) else 'true' if v is True else 'false' if v is False else str(v))+'\n'
 p.write_text(s)
weapons=[('ARCANE WAND','Rapid elemental bolts. Secondary: a ring of spells.',14,.24,280,340,6,-1),('LONGBOW','Hold to charge; release to fire piercing arrows. Secondary: arrow rain.',32,.32,420,450,8,-1),('RUNE BLADE','Wide close-range slashes. Secondary: a full-circle rune strike.',27,.34,0,60,5,-1),('ARCANE STAFF','Heavy detonating spells. Secondary: a persistent elemental zone.',42,.72,190,330,9,0),('CHAKRAM','A returning ring that cuts through a crowd. Secondary: six returning rings.',18,.6,250,300,8,0),('CRYSTAL ORBS','Orbiting crystals deal contact damage and launch together. Secondary: shield burst.',15,.45,270,320,8,1),('SPELLBOOK','A pulse field plus a missile fan. Secondary: a protective rune sanctuary.',20,.65,220,300,10,1),('ELEMENTAL GAUNTLETS','Quick elemental strikes. Secondary: a long forward impact with invulnerability.',17,.18,0,45,5,2)]
for i,(name,desc,dmg,interval,speed,rng,cd,unlock) in enumerate(weapons):resource('Weapons',f'weapon_{i}','WeaponData',dict(DisplayName=name,Description=desc,Kind=i,Damage=dmg,Interval=interval,Speed=speed,Range=rng,AbilityCooldown=cd,UnlockBoss=unlock))
names=[['Mossling','Willow Wisp','Thorn Hound','Petalwing','Bark Sentinel','Antler Sage'],['Cinder Slime','Ash Oracle','Glass Stalker','Ember Bat','Furnace Husk','Coal Conjurer'],['Rime Slime','Frost Adept','Snow Prowler','Shardwing','Frozen Armor','Winter Caller'],['Void Droplet','Rift Speaker','Phase Beast','Star Moth','Astral Golem','Echo Summoner']]
for realm in range(4):
 for k in range(6):resource('Enemies',f'enemy_{realm*6+k}','EnemyData',dict(DisplayName=names[realm][k],Kind=k,Realm=realm,Health=[22,28,32,12,90,55][k],Speed=[47,38,43,68,24,29][k],Damage=[9,10,12,6,16,10][k],Experience=[4,5,6,3,9,8][k]))
heroes=[('ASTER','Realm Wanderer','REJUVENATION','Restore health and wrap yourself in a protective shield.',1,0,1),('LYRA','Verdant Ranger','ROOTWAKE','Root nearby foes and move faster for five seconds.',5,1,4),('KAEL','Ember Scholar','CINDERFIELD','Create a lasting field of burning magic.',10,2,4),('BRANN','Rune Warden','AEGIS','A strong shield and a damaging defensive pulse.',9,3,3),('EIRA','Frost Traveler','WINTER STILL','Freeze nearby enemies and gain brief immunity.',2,7,2),('SOREN','Storm Artisan','SKYFALL','Release lightning in all directions.',4,5,5),('MIRA','Astral Archivist','STAR SURGE','Amplify attacks for eight seconds.',8,6,4),('VEIL','Rift Runner','PHASE STEP','Gain speed and temporary invulnerability.',3,4,1)]
for i,(name,title,ability,desc,hair,outfit,acc) in enumerate(heroes):
 resource('Characters',f'hero_{i}','HeroData',dict(DisplayName=name,Title=title,Ability=ability,Description=desc,Hair=hair,Outfit=outfit,Accessory=acc))
realms=[('GREENWARD BASIN','The old forest remembers.','The Rootbound Colossus','The first engine was buried under living roots. Every leaf still carries a trace of its song.','Roots split. Ancient runes awaken beneath the earth.'),('EMBERGLASS WASTES','A furnace beneath the world.','The Forge Tyrant','Glass deserts were once the workshops of the realmwrights. Their furnaces still burn without fuel.','The furnace falls silent. Steam rises; the realm freezes.'),('FROSTVEIL CITADEL','Time sleeps beneath the ice.','The Frostbound Sovereign','A queen of living crystal holds the final hour of the old world inside a frozen crown.','The ice fractures. Violet light lifts the ruins into the sky.'),('ASTRAL RUPTURE','Four worlds. One final fracture.','The Realmbreaker','The catastrophe never ended. At its heart, an unfinished engine is trying to assemble a world that cannot exist.','The broken engine yields. The four realms breathe again.')]
for i,(name,sub,boss,lore,shift) in enumerate(realms):resource('Battlegrounds',f'realm_{i}','RealmData',dict(DisplayName=name,Subtitle=sub,BossName=boss,Lore=lore,ShiftText=shift))
upgrades=[]
def add(name,desc,effect,value,weapon=-1,element=-1,rank=3):upgrades.append(dict(Id=f'relic_{len(upgrades):02}',DisplayName=name,Description=desc,Effect=effect,Amount=value,Weapon=weapon,Element=element,MaxRank=rank))
base=[('SWIFT STEP','Dash recharges 0.25 seconds sooner.','dash',.25),('SECOND WIND','Gain another dash charge.','charges',1),('LONG STRIDE','Dash travels 25% farther.','dashlength',.25),('RANGER SOUL','Collect shards from 35 pixels farther away.','magnet',35),('LIVING BARK','Gain 25 max health and restore that much health.','health',25),('RUNESTEEL','Reduce incoming damage by 3.','armor',3),('SPRINGHEART','Regenerate 1 health every second.','regen',1),('STAR HUNTER','Critical chance increases by 12%.','crit',.12),('TRAVELER BOOTS','Move 14 pixels per second faster.','speed',14),('BRIGHT CORE','Attacks deal 20% more base damage.','damage',.2),('QUICKENING','Attack 20% faster.','rate',.2),('DISTANT ECHO','Attack range increases by 25%.','range',.25),('EXPANDING RUNE','Fields and area attacks become 25% larger.','area',.25),('TIMEWEAVER','Persistent fields last 35% longer.','duration',.35),('AWAKENED SOUL','Hero ability recharges 3 seconds sooner.','hero',3),('HEROIC MEMORY','Hero ability power increases by 35%.','heropower',.35),('RESTORATION','Restore 45 health immediately.','heal',45),('CRYSTAL VEIL','Gain 35 temporary shield.','shield',35)]
for row in base:add(*row,rank=999 if row[2] in ('heal','shield') else 3)
add('BLOOM SIPHON','Each enemy defeat restores 1 health.','leech',1,rank=1);add('THORN GUARD','Taking a hit releases a thorn pulse.','thorns',1,rank=1)
mods=[('FORKED','Launch one additional bolt.','multi',1),('PENETRATING','Projectiles pierce one additional enemy.','pierce',1),('WIDE','Area attacks and spell impact become 30% larger.','area',.3),('FAR','Gain 30% attack range.','range',.3)]
for w,(name,*_) in enumerate(weapons):
 for suffix,desc,effect,amt in mods:
  if w in (2,7) and effect in ('multi','pierce'):effect,amt,desc=('rate',.25,'Strike 25% faster.') if effect=='multi' else ('damage',.3,'Strikes deal 30% more base damage.')
  if w==5 and effect=='multi':effect,amt,desc='orbs',1,'Add one orbiting crystal.'
  add(f'{suffix} {name}',desc,effect,amt,weapon=w)
for e,label in enumerate(['FIRE','FROST','LIGHTNING','NATURE','ARCANE']):
 add(label+' ATTUNEMENT','Strengthen elemental effects by 35%.','element',.35,element=e)
 add(label+' EXPANSION','Elemental spell fields grow 30% larger.','area',.3,element=e)
 if e==2:add('CHAIN REACTION','Lightning jumps to one more nearby target.','chain',1,element=e)
 elif e==0:add('CINDER IMPACT','Every projectile detonates on impact.','explode',1,element=e,rank=1)
 elif e==4:add('RETURNING ECHO','Ranged bolts return toward you after flight.','return',1,element=e,rank=1)
 elif e==3:add('EVERGREEN','Regenerate 1.5 health each second.','regen',1.5,element=e)
 else:add('LONG WINTER','Frost fields persist 50% longer.','duration',.5,element=e)
for i,u in enumerate(upgrades):resource('Upgrades',f'upgrade_{i:02}','UpgradeData',u)
print('Catalog:',len(weapons),'weapons;',24,'enemies;',8,'heroes;',4,'realms;',len(upgrades),'upgrade resources')

for realm, name in enumerate(["Grove Tender", "Smelt Mender", "Rime Keeper", "Rift Suture"]):
 resource("Enemies",f"enemy_{24+realm}","EnemyData",dict(DisplayName=name,Kind=6,Realm=realm,Health=45,Speed=35,Damage=9,Experience=7))
