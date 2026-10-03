"""Original pixel assets, layered avatars, and synthesized score. No third-party art."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import random, math, wave, struct
ROOT=Path(__file__).resolve().parents[1]; ART=ROOT/'Assets/Art'; AUDIO=ROOT/'Assets/Audio'
random.seed(7025)
PALS=[['#162e32','#245041','#3b7051','#588f5b','#85b86a','#c4d78b','#385259','#728b78','#d4cc9e','#32606d','#558998'],['#291f36','#413042','#664147','#91534c','#c07657','#e8b679','#504057','#8c7674','#e4cb9a','#9c413f','#ec864c'],['#1a2c45','#304561','#496783','#73929c','#a4c5c9','#e0ede5','#52627d','#7f99ad','#e7dbb5','#375c8c','#79bbcd'],['#221c3d','#3d3057','#5a426a','#83567e','#aa74a5','#e5c6c1','#595174','#9692a0','#dfcaae','#473579','#a57ad7']]
OUT='#101e2d'
def canvas(w,h):
 im=Image.new('RGBA',(w,h)); return im,ImageDraw.Draw(im)
def save(im,folder,name):
 p=ART/folder/name;p.parent.mkdir(parents=True,exist_ok=True);im.save(p)
def blob(d,cx,cy,rx,ry,c):
 # Stepped, deliberately pixel-shaped cluster, never a smoothed ellipse.
 for y in range(-ry,ry+1,2):
  width=int(rx*math.sqrt(max(0,1-(y/max(ry,1))**2)))//2*2
  if width:d.rectangle((cx-width,cy+y,cx+width,cy+y+1),fill=c)
for realm,p in enumerate(PALS):
 im,d=canvas(256,64)
 for tile in range(64):
  x=(tile%16)*16;y=(tile//16)*16;kind=tile//16
  base=p[2] if kind==0 else p[7] if kind==1 else p[9] if kind==2 else p[1]
  d.rectangle((x,y,x+15,y+15),fill=base)
  for _ in range(4 if kind==0 else 5):
   px=x+random.randrange(16);py=y+random.randrange(16)
   d.line((px,py,min(x+15,px+random.randrange(1,4)),py),fill=p[random.choice([2,2,3])] if kind==0 else p[6] if kind==1 else p[10] if kind==2 else p[2])
  if kind==1:
   d.line((x,y+15,x+15,y+15),fill=p[6]);d.line((x+15,y,x+15,y+14),fill=p[6])
  if kind==2:
   d.line((x+2,y+4,x+9,y+4),fill=p[10]);d.line((x+7,y+11,x+14,y+11),fill=p[10])
 save(im,'Tilesets',f'realm_{realm}.png')
 # Tree: asymmetric clusters, bark and canopy depth, rim light.
 im,d=canvas(80,96);blob(d,40,83,33,8,'#10202d88')
 d.polygon([(33,80),(37,50),(46,48),(50,80),(57,87),(43,84),(30,88)],fill=p[0])
 d.rectangle((39,50,45,81),fill=p[6]);d.line((40,55,40,80),fill=p[8],width=2)
 for cx,cy,rx,ry in [(24,49,19,16),(56,49,18,14),(38,32,24,22),(23,34,16,13),(54,27,16,13),(39,17,14,13)]:
  blob(d,cx,cy,rx+2,ry+2,OUT);blob(d,cx,cy,rx,ry,p[1]);blob(d,cx-2,cy-5,rx-2,ry-4,p[2]);blob(d,cx-4,cy-8,rx-6,ry-6,p[3])
  for _ in range(9):
   xx=cx+random.randint(-rx//2,rx//2);yy=cy+random.randint(-ry//2,0);d.line((xx,yy,xx+3,yy),fill=p[4])
 save(im,'Props',f'tree_{realm}.png')
 im,d=canvas(48,64);blob(d,24,55,21,6,'#101e2d88')
 d.polygon([(5,54),(9,43),(14,41),(14,21),(10,17),(12,13),(35,13),(38,18),(34,21),(34,41),(40,45),(43,54)],fill=OUT)
 d.rectangle((16,21,32,43),fill=p[6]);d.rectangle((18,21,22,42),fill=p[7]);d.rectangle((13,14,35,19),fill=p[7]);d.rectangle((10,45,38,51),fill=p[6]);d.line((10,44,38,44),fill=p[8],width=2)
 d.rectangle((22,25,26,36),fill=p[0]);d.rectangle((23,26,25,33),fill=p[10]);d.line((16,14,32,14),fill=p[8])
 save(im,'Props',f'shrine_{realm}.png')
 im,d=canvas(32,40);blob(d,16,34,14,4,'#101e2d88')
 for cx,cy,sz in [(10,27,9),(20,26,12),(26,30,6)]:
  d.polygon([(cx-sz//2,cy),(cx-2,cy-sz*2),(cx+sz//2,cy-sz),(cx+sz//2,cy+3)],fill=OUT)
  d.polygon([(cx-sz//2+1,cy),(cx-1,cy-sz*2+2),(cx+1,cy+1)],fill=p[10]);d.polygon([(cx,cy-sz*2+3),(cx+sz//2-1,cy-sz),(cx+sz//2-1,cy+2),(cx+1,cy+2)],fill=p[3])
 save(im,'Props',f'crystal_{realm}.png')
 im,d=canvas(40,28);blob(d,20,23,18,4,'#101e2d88')
 d.polygon([(3,20),(7,9),(16,5),(30,8),(37,17),(35,23),(7,24)],fill=OUT);d.polygon([(6,18),(9,11),(17,8),(29,10),(33,18),(27,21),(8,20)],fill=p[6]);d.polygon([(9,11),(17,8),(29,10),(25,14),(14,15)],fill=p[7]);d.line((13,10,20,9),fill=p[8],width=2)
 save(im,'Props',f'rock_{realm}.png')
 im,d=canvas(24,24)
 for cx,cy in [(8,16),(15,12),(17,18)]:
  blob(d,cx,cy,7,5,OUT);blob(d,cx-1,cy-1,5,4,p[2]);d.line((cx-3,cy-3,cx+1,cy-3),fill=p[4],width=2)
 d.rectangle((6,10,7,12),fill='#eadca0');d.rectangle((17,8,19,9),fill='#d79395')
 save(im,'Props',f'shrub_{realm}.png')
 im,d=canvas(16,32)
 d.rectangle((0,9,15,31),fill=OUT);d.rectangle((1,10,14,28),fill=p[6]);d.rectangle((0,6,15,13),fill=p[7]);d.line((0,6,15,6),fill=p[8]);d.line((1,16,14,16),fill=p[0]);d.line((8,17,8,24),fill=p[0]);d.line((1,25,14,25),fill=p[0])
 save(im,'Props',f'wall_{realm}.png')
 # 6 silhouettes x 4 realms, four frames each.
 for kind in range(6):
  sheet,_=canvas(128,32)
  for frame in range(4):
   im,d=canvas(32,32);t=frame%2;blob(d,16,28,10,3,'#101e2d88')
   c=p[3];hi=p[5];lo=p[1]
   if kind==0: # hop slime
    blob(d,16,21-t*2,10+t,8-t,OUT);blob(d,16,20-t*2,9+t,7-t,c);blob(d,13,17-t*2,5,3,p[4]);d.rectangle((11,20-t*2,13,22-t*2),fill=OUT);d.rectangle((19,20-t*2,21,22-t*2),fill=OUT)
   elif kind==1: # robed caster
    d.polygon([(16,4+t),(7,13+t),(9,17),(6,28),(26,28),(23,17),(25,13+t)],fill=OUT);d.polygon([(16,6+t),(9,13+t),(11,17),(8,26),(24,26),(21,16),(23,13+t)],fill=c);d.rectangle((12,12+t,20,17+t),fill=lo);d.rectangle((13,13+t,14,14+t),fill=hi);d.rectangle((18,13+t,19,14+t),fill=hi);d.line((25,9,25,27),fill=p[8],width=2);d.rectangle((24,6,27,10),fill=p[10])
   elif kind==2: # quadruped thorn dasher
    d.polygon([(4,24),(6,13),(12,9),(24,10),(28,18),(25,26),(22,28),(19,23),(10,23),(7,28)],fill=OUT);d.polygon([(7,22),(8,14),(13,12),(23,13),(25,19),(23,24),(20,20),(10,20)],fill=c)
    for x in [10,16,22]:d.polygon([(x,13),(x-2,6+t),(x+4,12)],fill=hi)
    d.rectangle((9,16,10,18),fill=hi);d.rectangle((21,16,22,18),fill=hi)
   elif kind==3: # winged swarm
    d.polygon([(16,12),(5,6+t*2),(2,15),(9,19),(16,25),(24,19),(30,15),(26,6+t*2)],fill=OUT);d.polygon([(15,13),(6,9+t*2),(5,15),(12,17),(16,23),(22,17),(27,14),(26,10+t*2)],fill=c);d.rectangle((12,13,20,20),fill=lo);d.rectangle((12,14,13,15),fill=hi);d.rectangle((19,14,20,15),fill=hi)
   elif kind==4: # plated golem
    d.rectangle((8,9,24,25),fill=OUT);d.rectangle((6,15,9,26),fill=OUT);d.rectangle((24,15,27,26),fill=OUT);d.rectangle((10,10,22,23),fill=p[6]);d.rectangle((10,10,13,20),fill=p[7]);d.rectangle((12,5,20,13),fill=OUT);d.rectangle((13,6,19,10),fill=c);d.rectangle((14,8,18,9),fill=hi);d.rectangle((9,24-t,13,28-t),fill=lo);d.rectangle((19,24+t,23,28+t),fill=lo);d.rectangle((14,16,18,19),fill=p[10])
   else: # antler summoner
    d.polygon([(16,9),(8,18),(7,28),(25,28),(24,18)],fill=OUT);d.polygon([(16,11),(10,19),(9,26),(23,26),(22,19)],fill=c);d.rectangle((12,12,20,17),fill=lo);d.rectangle((13,13,14,14),fill=hi);d.rectangle((18,13,19,14),fill=hi)
    for x,sg in [(10,-1),(22,1)]:
     d.line((x,14,x+sg*3,4),fill=p[8],width=2);d.line((x+sg*2,8,x+sg*6,7),fill=p[8],width=2)
   sheet.alpha_composite(im,(frame*32,0))
  save(sheet,'Enemies',f'enemy_{realm}_{kind}.png')
 # Boss sprite: separate handmade silhouette per realm.
 sheet,_=canvas(384,96)
 for f in range(4):
  im,d=canvas(96,96);bob=f%2;blob(d,48,85,40,8,'#101e2da0')
  if realm==0:
   for x,s in [(18,-1),(78,1)]:
    d.polygon([(x,64),(x+s*12,47),(x+s*8,31),(x-s*7,37),(x-s*9,65),(x,80)],fill=OUT);d.line((x,70,x-s*5,42),fill=p[7],width=6)
   d.polygon([(25,76),(29,34),(38,22),(60,23),(67,37),(73,79),(57,86),(48,72),(37,85)],fill=OUT);d.polygon([(29,73),(34,35),(41,27),(57,28),(62,38),(67,75),(56,79),(48,66),(38,78)],fill=p[6]);d.line((38,36,36,67),fill=p[7],width=4);d.rectangle((40,37,56,49),fill=p[0]);d.rectangle((40,40,44,43),fill='#f6da90');d.rectangle((52,40,56,43),fill='#f6da90')
   for x,y in [(29,28),(43,19),(61,26),(21,37),(73,37)]:blob(d,x,y+bob,15,11,OUT);blob(d,x,y-3+bob,13,9,p[3]);blob(d,x-3,y-5+bob,7,5,p[4])
  elif realm==1:
   for x in [12,72]:d.rectangle((x,37,x+14,74),fill=OUT);d.rectangle((x+2,39,x+12,70),fill=p[6]);d.rectangle((x+4,50,x+10,64),fill=p[10])
   d.rectangle((27,28,69,74),fill=OUT);d.rectangle((30,32,66,69),fill=p[6]);d.rectangle((34,39,62,64),fill=p[1]);d.rectangle((37,44,59,62),fill=p[10]);d.rectangle((42,48,54,58),fill=p[5]);d.rectangle((34,72,42,86),fill=p[0]);d.rectangle((56,72,64,86),fill=p[0]);d.rectangle((37,17,59,33),fill=OUT);d.rectangle((39,19,57,28),fill=p[7]);d.rectangle((40,25,56,27),fill=p[10]);d.rectangle((29,10-bob*4,34,30),fill=p[6]);d.rectangle((62,10-bob*4,67,30),fill=p[6])
  elif realm==2:
   d.polygon([(48,17),(33,27),(27,44),(18,82),(78,82),(69,43),(63,27)],fill=OUT);d.polygon([(48,20),(36,29),(31,45),(24,78),(72,78),(65,44),(60,29)],fill=p[2]);d.polygon([(48,36),(37,48),(33,74),(62,74),(57,48)],fill=p[4]);d.rectangle((40,29,56,37),fill=p[0]);d.rectangle((41,31,45,33),fill=p[10]);d.rectangle((51,31,55,33),fill=p[10])
   for x,y in [(36,20),(48,17),(60,20)]:d.polygon([(x-4,y+3),(x,y-12-bob),(x+4,y+3)],fill=p[5])
   for x in [15,79]:d.polygon([(x-8,44),(x,22+bob),(x+8,44),(x,63)],fill=OUT);d.polygon([(x-5,44),(x,26+bob),(x+5,44),(x,58)],fill=p[10]);d.line((x,30,x,52),fill=p[5])
  else:
   for x,s in [(23,-1),(73,1)]:d.polygon([(x,63),(x+s*15,25),(x-s*4,36),(x-s*10,17),(x-s*14,53)],fill=OUT);d.polygon([(x,59),(x+s*10,31),(x-s*5,44),(x-s*10,26),(x-s*11,53)],fill=p[3])
   d.polygon([(48,13),(32,30),(37,44),(29,71),(48,89),(67,71),(59,44),(64,30)],fill=OUT);d.polygon([(48,18),(36,31),(42,47),(34,70),(48,83),(62,70),(54,46),(60,31)],fill=p[2]);d.polygon([(48,36),(39,49),(48,65),(57,49)],fill=p[10]);d.polygon([(48,42),(44,49),(48,56),(52,49)],fill=p[5]);d.rectangle((40,30,44,32),fill=p[5]);d.rectangle((52,30,56,32),fill=p[5])
   for x,y in [(48,5),(18,14),(79,14)]:d.rectangle((x-2,y-bob,x+2,y+3-bob),fill=p[10])
  sheet.alpha_composite(im,(f*96,0))
 save(sheet,'Bosses',f'boss_{realm}.png')
# Layered player, 4 facing rows (down/up/left/right), six run frames.
for body in range(3):
 sheet,_=canvas(192,128)
 for facing in range(4):
  for f in range(6):
   im,d=canvas(32,32);bob=1 if f in (1,4) else 0;wide=[0,1,2][body];side=facing>=2
   d.rectangle((11-wide,11+bob,20+wide,22+bob),fill=OUT);d.rectangle((12-wide,12+bob,19+wide,21+bob),fill='#9aa6b0')
   d.rectangle((12,6+bob,20,13+bob),fill=OUT);d.rectangle((13,7+bob,19,12+bob),fill='#e4d6bb');d.line((13,12+bob,18,12+bob),fill='#a09486')
   if facing!=1:
    if not side:d.rectangle((14,9+bob,14,10+bob),fill=OUT);d.rectangle((18,9+bob,18,10+bob),fill=OUT)
    else:d.rectangle((13 if facing==2 else 19,9+bob,13 if facing==2 else 19,10+bob),fill=OUT)
   leg=1 if f<3 else -1
   d.rectangle((12-wide,22+bob,15,27+leg),fill=OUT);d.rectangle((17,22+bob,20+wide,27-leg),fill=OUT);d.rectangle((9-wide,15+bob,11-wide,21+bob),fill='#d1c1a5');d.rectangle((21+wide,15+bob,23+wide,21+bob),fill='#d1c1a5')
   sheet.alpha_composite(im,(f*32,facing*32))
 save(sheet,'Characters',f'body_{body}.png')
for outfit in range(8):
 sheet,_=canvas(192,128)
 for facing in range(4):
  for f in range(6):
   im,d=canvas(32,32);bob=1 if f in (1,4) else 0;long=outfit in (2,5,6,7)
   if outfit in (0,2,4,6,7):d.polygon([(11,12+bob),(21,12+bob),(25,25+bob),(8,25+bob)],fill=OUT);d.polygon([(12,13+bob),(20,13+bob),(22,23+bob),(10,23+bob)],fill='#74849b')
   d.rectangle((11,13+bob,21,22+bob+(3 if long else 0)),fill=OUT);d.rectangle((12,14+bob,20,21+bob+(3 if long else 0)),fill='#b4c2ca');d.rectangle((12,14+bob,14,20+bob),fill='#e5eee7');d.rectangle((16,14+bob,20,21+bob),fill='#8192a2')
   d.rectangle((11,21+bob,21,22+bob),fill='#574654');d.rectangle((16,21+bob,17,22+bob),fill='#e6cc87')
   if outfit==3:d.rectangle((9,13+bob,12,15+bob),fill='#eee5c9');d.rectangle((21,13+bob,24,15+bob),fill='#eee5c9');d.rectangle((14,15+bob,18,18+bob),fill='#e4e5de')
   if outfit==1:d.line((12,14+bob,20,21+bob),fill='#484149',width=2)
   if outfit==4:d.rectangle((12,13+bob,20,14+bob),fill='#514c71')
   if outfit in (6,7):d.rectangle((16,16+bob,17,19+bob),fill='#f5d89c')
   sheet.alpha_composite(im,(f*32,facing*32))
 save(sheet,'Characters',f'outfit_{outfit}.png')
for hair in range(11):
 sheet,_=canvas(192,128)
 for facing in range(4):
  for f in range(6):
   im,d=canvas(32,32);bob=1 if f in (1,4) else 0
   d.polygon([(11,9+bob),(12,5+bob),(15,3+bob),(20,4+bob),(22,7+bob),(21,11+bob),(19,8+bob),(13,8+bob),(12,11+bob)],fill=OUT)
   d.rectangle((13,5+bob,20,7+bob),fill='#bdcbd5');d.line((14,5+bob,18,5+bob),fill='#ecf0e0')
   if hair in (1,2,7,8):d.rectangle((11,8+bob,12,14+bob+(3 if hair==2 else 0)),fill='#8d9aac');d.rectangle((21,7+bob,22,14+bob+(3 if hair==2 else 0)),fill='#8d9aac')
   if hair in (3,4,9):
    for x in [12,16,20]:d.polygon([(x,7+bob),(x-1,1+bob+(x%3)),(x+3,6+bob)],fill='#bdcbd5')
   if hair in (5,6):d.rectangle((22,6+bob,24,16+bob+(f%2)),fill=OUT);d.rectangle((23,7+bob,24,15+bob+(f%2)),fill='#bdcbd5')
   if hair==7:
    for x in [11,15,19,22]:blob(d,x,6+bob,2,2,'#bdcbd5')
   if hair==10:d.polygon([(8,7+bob),(16,-1+bob),(24,7+bob)],fill=OUT);d.polygon([(11,6+bob),(16,1+bob),(21,6+bob)],fill='#bdcbd5');d.rectangle((8,7+bob,24,8+bob),fill='#899aac')
   if facing==1:d.rectangle((12,8+bob,21,12+bob),fill='#899aac');d.line((13,8+bob,13,11+bob),fill='#bdcbd5')
   sheet.alpha_composite(im,(f*32,facing*32))
 save(sheet,'Characters',f'hair_{hair}.png')
for acc in range(6):
 im,d=canvas(32,32)
 if acc==1:d.rectangle((10,12,23,14),fill='#e9d6ac');d.polygon([(19,14),(24,16),(22,21),(20,18)],fill='#aeabbd')
 if acc==2:d.polygon([(11,13),(8,26),(23,26),(21,13)],fill='#687591');d.rectangle((12,13,20,16),fill='#bdcbd5')
 if acc==3:d.rectangle((9,13,12,16),fill='#d3dfcf');d.rectangle((21,13,24,16),fill='#d3dfcf')
 if acc==4:d.rectangle((18,18,20,21),fill='#f3dfa5');d.rectangle((19,18,19,19),fill='#ffffff')
 if acc==5:d.rectangle((11,5,22,6),fill='#ead492');d.rectangle((15,3,18,5),fill='#fdedd2')
 save(im,'Characters',f'accessory_{acc}.png')
# Weapon icons. Small sprites with clearly distinct shapes.
for w in range(8):
 im,d=canvas(24,24);gold='#efd59a';teal='#89dbc2';dark=OUT
 if w==0:d.line((6,20,16,6),fill=dark,width=4);d.line((6,19,15,7),fill=gold,width=2);d.polygon([(16,2),(20,6),(16,10),(12,6)],fill=teal)
 if w==1:d.line((7,3,7,21),fill=gold);d.line([(7,3),(15,7),(17,12),(15,17),(7,21)],fill=gold,width=3);d.line((5,12,22,12),fill=teal,width=2);d.polygon([(20,9),(23,12),(20,15)],fill=teal)
 if w==2:d.polygon([(5,21),(3,19),(9,12),(8,9),(11,8),(19,1),(22,2),(21,6),(14,13),(13,16),(10,15)],fill=dark);d.line((6,19,18,5),fill=gold,width=3);d.line((11,12,20,3),fill=teal,width=3);d.line((7,9,15,16),fill=gold,width=2)
 if w==3:d.line((8,21,14,7),fill=gold,width=3);d.polygon([(13,1),(20,4),(18,9),(12,11),(8,6)],fill=dark);d.polygon([(13,3),(18,5),(16,8),(11,8)],fill=teal)
 if w==4:
  d.ellipse((3,3,21,21),outline=dark,width=4);d.ellipse((4,4,20,20),outline=gold,width=3);d.rectangle((11,2,13,5),fill=teal);d.rectangle((2,11,5,13),fill=teal)
 if w==5:
  for x,y in [(7,7),(18,8),(13,18)]:d.polygon([(x,y-4),(x+4,y),(x,y+4),(x-4,y)],fill=dark);d.polygon([(x,y-3),(x+3,y),(x,y+3),(x-3,y)],fill=teal)
 if w==6:d.rectangle((4,4,20,21),fill=dark);d.rectangle((5,4,19,18),fill='#7982bc');d.rectangle((5,19,19,20),fill=gold);d.line((7,5,7,17),fill=gold);d.polygon([(14,7),(17,11),(14,15),(11,11)],fill=teal)
 if w==7:
  for x,y in [(3,10),(12,4)]:d.rectangle((x,y,x+7,y+10),fill=dark);d.rectangle((x+1,y+1,x+6,y+8),fill=gold);d.rectangle((x+2,y+1,x+5,y+4),fill=teal)
 save(im,'Weapons',f'weapon_{w}.png')
im,d=canvas(32,32);blob(d,16,28,14,3,'#101e2d88');d.rectangle((3,10,28,27),fill=OUT);d.rectangle((5,12,26,24),fill='#9b634b');d.rectangle((5,12,26,17),fill='#c08a5d');d.rectangle((7,9,24,12),fill='#e6c581');d.rectangle((7,12,9,25),fill='#d2b575');d.rectangle((23,12,25,25),fill='#d2b575');d.rectangle((14,16,18,21),fill='#f4d99d');d.rectangle((15,18,16,20),fill=OUT);save(im,'Props','chest.png')
im,d=canvas(64,80)
for x in [10,47]:d.rectangle((x,20,x+7,68),fill=OUT);d.rectangle((x+1,21,x+5,66),fill='#728482');d.line((x+1,24,x+5,24),fill='#d1d5ac')
d.polygon([(10,25),(16,13),(26,7),(38,7),(48,13),(55,25),(47,24),(42,17),(36,14),(28,14),(21,17),(17,25)],fill='#768681');d.polygon([(19,30),(25,20),(39,20),(45,30),(45,65),(19,65)],fill='#283951');d.line((20,30,20,63),fill='#8cddd0',width=2);d.line((43,30,43,63),fill='#8cddd0',width=2);d.rectangle((10,68,55,72),fill='#a4aaa0');save(im,'Props','portal.png')
# Original bitmap font: use bundled PIL fixed-width bitmap, export BMFont metrics.
font=ImageFont.load_default(size=10);im,d=canvas(256,128);lines=['info face="Realm Pixel" size=10','common lineHeight=12 base=10 scaleW=256 scaleH=128 pages=1 packed=0','page id=0 file="realm_font.png"','chars count=95']
for i,c in enumerate(range(32,127)):
 x=(i%16)*16;y=(i//16)*16;d.text((x,y),chr(c),font=font,fill='white',stroke_width=0)
 lines.append(f'char id={c} x={x} y={y} width=12 height=13 xoffset=0 yoffset=0 xadvance=6 page=0 chnl=15')
save(im,'UI','realm_font.png');(ART/'UI/realm_font.fnt').write_text('\n'.join(lines))
# Pixel logo, matching outline/title treatment.
im,d=canvas(440,96);f=ImageFont.load_default(size=43)
d.text((17,4),'REALMSHIFT',font=f,fill='#162b38',stroke_width=3,stroke_fill='#162b38');d.text((17,0),'REALMSHIFT',font=f,fill='#f2ddb2',stroke_width=1,stroke_fill='#66836c')
f2=ImageFont.load_default(size=18);d.text((111,49),'A R E N A',font=f2,fill='#8ec8b5');d.line((35,60,91,60),fill='#7ba790');d.line((280,60,335,60),fill='#7ba790');save(im,'UI','logo.png')
# Synthesis: low-volume 4 realm loops, boss score, menu motif. Original note sequences.
SR=22050
def wav(path,samples):
 path.parent.mkdir(parents=True,exist_ok=True)
 temp=path.with_suffix('.tmp')
 with wave.open(str(temp),'w') as w:w.setnchannels(1);w.setsampwidth(2);w.setframerate(SR);w.writeframes(b''.join(struct.pack('<h',int(max(-1,min(1,v))*32767)) for v in samples))
 temp.replace(path)
def tone(freq,t):return math.sin(2*math.pi*freq*t)*.7 + (1 if math.sin(2*math.pi*freq*t)>0 else -1)*.3
for theme in range(6):
 dur=16; data=[0.]*(SR*dur);scale=[[0,4,7,11],[0,3,7,10],[0,3,7,12],[0,5,7,10],[0,3,6,10],[0,4,7,12]][theme];root=[48,45,50,47,40,48][theme]
 for beat in range(64):
  note=root+scale[(beat//4)%4]+[0,12,7,12][beat%4];freq=440*2**((note-69)/12);start=int(beat*.25*SR)
  for j in range(int(.23*SR)):
   t=j/SR;env=(1-math.exp(-t*60))*math.exp(-t*11);data[start+j]+=tone(freq,t)*env*.115
  if beat%4==0:
   freq=440*2**((root-12-69)/12)
   for j in range(int(.8*SR)):
    if start+j<len(data):data[start+j]+=math.sin(2*math.pi*freq*j/SR)*math.exp(-j/SR*3)*.09
  if beat%2==0:
   for j in range(int(.08*SR)):data[start+j]+=(random.random()*2-1)*math.exp(-j/SR*60)*.03
 wav(AUDIO/'Music'/f'theme_{theme}.wav',data)
for name,freq,length in [('shoot',780,.075),('bow',360,.14),('blade',150,.12),('cast',450,.25),('hit',170,.08),('dash',260,.16),('pickup',1100,.07),('level',620,.5),('ui',840,.045),('hurt',85,.18),('shift',120,2.4),('boss',70,1.2)]:
 data=[]
 for i in range(int(SR*length)):
  t=i/SR;f=freq*(1+.5*math.sin(t*8)) if name in ('level','shift') else freq*(1-t/length*.65);env=min(1,t*150)*math.exp(-t/length*4)
  data.append((tone(f,t)*.17+(random.random()*2-1)*(.09 if name in ('hit','blade','dash','hurt') else .01))*env)
 wav(AUDIO/'SFX'/f'{name}.wav',data)
print('Original assets generated:',len(list(ART.rglob('*.png'))),'PNG files; 18 WAV files')

# Keep UI glyphs and title in the original pixel alphabet.
import runpy
runpy.run_path(str(ROOT/"Tools/generate_font.py"))
