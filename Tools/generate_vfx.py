"""Original hand-authored pixel clusters and frame choreography for Realmshift Arena.
No sampled reference artwork; deterministic, nearest-filter sprite sheets.
"""
from pathlib import Path
from PIL import Image,ImageDraw
import math,random
ROOT=Path(__file__).resolve().parents[1]/'Assets/Art/VFX'
PALS={
'fire':['#7a283f','#d94336','#f37935','#ffbc53','#fff3b0'],
'frost':['#294575','#4982b1','#6abfdc','#adf0ed','#f0ffff'],
'lightning':['#784961','#ae8653','#f2bd54','#ffe896','#ffffef'],
'nature':['#234845','#457754','#80b96b','#b4e5a1','#eaffc7'],
'arcane':['#443260','#7455a1','#b28ad7','#ddaff2','#fff2ff']}
TAU=math.tau

def pt(x,y):return round(x),round(y)
def poly(d,points,c):d.polygon([pt(x,y) for x,y in points],fill=c)
def disc(d,x,y,r,c,sy=1):
 for yy in range(-math.ceil(r*sy),math.ceil(r*sy)+1):
  span=math.floor(math.sqrt(max(0,r*r-(yy/sy)**2)))
  if span:d.line((round(x)-span,round(y)+yy,round(x)+span,round(y)+yy),fill=c)
def line(d,points,c,w=1):d.line([pt(x,y) for x,y in points],fill=c,width=w)
def leaf(d,x,y,c,angle=0,scale=1):
 pts=[(-4,0),(-1,-3),(3,-2),(5,0),(1,3),(-2,2)];co,si=math.cos(angle),math.sin(angle)
 poly(d,[(x+(xx*co-yy*si)*scale,y+(xx*si+yy*co)*scale) for xx,yy in pts],c)
def ring(d,x,y,r,c,width=1,phase=0,parts=40):
 pts=[(x+math.cos(i*TAU/parts+phase)*r,y+math.sin(i*TAU/parts+phase)*r*.85) for i in range(parts+1)];line(d,pts,c,width)
def sheet(folder,name,size,frames,draw):
 w,h=size;out=Image.new('RGBA',(w*frames,h));
 for f in range(frames):
  im=Image.new('RGBA',(w,h));draw(ImageDraw.Draw(im),f,im);out.alpha_composite(im,(f*w,0))
 p=ROOT/folder/(name+'.png');p.parent.mkdir(parents=True,exist_ok=True);out.save(p)
for name,c in PALS.items():
 def bolt(d,f,im,c=c,name=name):
  if name=='fire':
   # Flame tongues point upstream; bright directional head expands and contracts.
   poly(d,[(3,12),(1,7+f%3),(9,9),(7,4+f%2),(16,8),(21,12),(16,18),(6,20-f%3),(9,15)],c[1])
   poly(d,[(7,12),(5,9),(13,10),(12,7),(19,10),(21,12),(16,16),(8,17),(11,14)],c[2])
   poly(d,[(11,12),(16,10),(21,12),(16,15),(12,14)],c[3]);disc(d,17,12,2,c[4])
   d.point((3+f%4,6+f%2),fill=c[3]);d.point((4+f%3,19),fill=c[2])
  elif name=='frost':
   poly(d,[(3,12),(11,6),(22,12),(11,19)],c[0]);poly(d,[(5,12),(12,8),(21,12),(12,17)],c[2]);poly(d,[(7,12),(12,9),(21,12),(13,13)],c[4]);line(d,[(12,9),(13,15),(20,12)],c[3])
   line(d,[(2,7+f%3),(6,7+f%3)],c[3]);d.point((4,19-f%3),fill=c[2])
  elif name=='lightning':
   rng=random.Random(f+42);pts=[(1,12),(6,8+rng.randrange(5)),(9,15),(13,7),(15,14),(22,12)];line(d,pts,c[1],4);line(d,pts,c[3],2);line(d,pts,c[4])
   line(d,[(12,10),(9,4+f%3),(6,5)],c[2]);line(d,[(16,13),(13,20),(9,18)],c[3]);disc(d,19,12,2,c[4])
  elif name=='nature':
   line(d,[(1,15),(5,11),(9,15),(14,10),(21,12)],c[1],2);leaf(d,10,9,c[2],-.5);leaf(d,7,16,c[1],.4);poly(d,[(11,12),(17,8),(22,12),(17,16)],c[2]);line(d,[(13,12),(21,12)],c[4]);leaf(d,16,12,c[3],0,.55)
   d.point((3,f%4+6),fill=c[4])
  else:
   poly(d,[(6,12),(14,4),(23,12),(14,20)],c[0]);poly(d,[(8,12),(15,7),(21,12),(15,17)],c[2]);poly(d,[(12,12),(16,9),(20,12),(16,15)],c[4]);line(d,[(15,4),(20,7)],c[3]);line(d,[(9,17),(14,20)],c[3]);d.rectangle((3+f%3,7,4+f%3,8),fill=c[2]);d.point((2,17-f%3),fill=c[3])
 sheet('Projectiles',name,(24,24),8,bolt)
 def particle(d,f,im,c=c,name=name):
  if name=='fire':poly(d,[(2,7),(1,4),(3,4),(4,0+f%3),(7,5),(5,7)],c[2]);poly(d,[(3,6),(4,3+f%2),(6,6)],c[4])
  elif name=='frost':poly(d,[(4,0),(7,4),(4,7),(1,4)],c[2]);line(d,[(4,1),(4,6)],c[4])
  elif name=='nature':leaf(d,4,4,c[2],f*.6,.7);line(d,[(2,5),(6,3)],c[3])
  elif name=='lightning':line(d,[(3,0),(1,4),(5,3),(3,7)],c[3],2);line(d,[(3,0),(2,4),(5,3)],c[4])
  else:line(d,[(1,2),(5,2),(5,6),(2,6),(2,4)],c[2]);d.point((6,0+f%4),fill=c[4])
 sheet('Particles',name,(8,8),4,particle)
 def impact(d,f,im,c=c,name=name):
  t=f/7;r=3+t*18;fade=max(0,1-t);rng=random.Random(120)
  if f<4:disc(d,24,24,10-f,c[2]);disc(d,24,24,6-f,c[4])
  for i in range(9):
   a=i*TAU/9+(.25 if name=='nature' else 0);x=24+math.cos(a)*r;y=24+math.sin(a)*r
   if name=='fire':poly(d,[(x,y),(x-math.cos(a)*6,y-math.sin(a)*6),(x+math.sin(a)*2,y-math.cos(a)*2),(x+math.cos(a)*2,y+math.sin(a)*2)],c[2 if f<4 else 1]);d.point(pt(x,y),fill=c[4])
   elif name=='frost':poly(d,[(x,y-4+f//3),(x+2,y),(x,y+4-f//3),(x-2,y)],c[3 if i%2 else 2]);d.point(pt(x,y-2),fill=c[4])
   elif name=='lightning':line(d,[(24+math.cos(a)*8,24+math.sin(a)*8),(x+math.sin(a)*3,y-math.cos(a)*3),(x,y),(x+math.cos(a)*4,y+math.sin(a)*4)],c[3 if i%2 else 4])
   elif name=='nature':leaf(d,x,y,c[2 if i%2 else 3],a+f*.4,max(.25,1-f*.1));line(d,[(24,24),(x,y)],c[1]) if f<3 else None
   else:poly(d,[(x-2,y-2),(x+2,y-2),(x+2,y+2),(x-2,y+2)],c[2]);d.point(pt(x,y),fill=c[4])
  if name=='arcane':
   ring(d,24,24,r,c[3]);
   if f<6:poly(d,[(24,24-r*.7),(24+r*.7,24),(24,24+r*.7),(24-r*.7,24)],'#00000000');line(d,[(24,24-r*.7),(24+r*.7,24),(24,24+r*.7),(24-r*.7,24),(24,24-r*.7)],c[2])
  elif f in (1,2,3):ring(d,24,24,r,c[3])
  if f>3:
   # Late frames lose opacity and fragments without ending on an abrupt full-strength ring.
   alpha=im.getchannel('A').point(lambda a:int(a*fade*1.6));im.putalpha(alpha)
 sheet('Impacts',name,(48,48),8,impact)
 def trail(d,f,im,c=c,name=name):
  x=8;y=8
  if name=='fire':poly(d,[(4,10),(5,6),(8,3+f%2),(9,7),(12,9),(9,12),(6,11)],c[2]);disc(d,8,8,2,c[3])
  elif name=='frost':poly(d,[(8,2),(11,8),(8,12),(5,8)],c[2]);line(d,[(8,4),(8,10)],c[4])
  elif name=='lightning':line(d,[(3,8),(6,5+f%3),(8,11),(10,6),(13,8)],c[3]);d.point((8,8),fill=c[4])
  elif name=='nature':leaf(d,8,8,c[2],f*.5,.8)
  else:line(d,[(4,5),(10,5),(10,10),(6,10),(6,8)],c[2]);d.point((12,3+f%4),fill=c[4])
 sheet('Trails',name,(16,16),6,trail)
 def slash(d,f,im,c=c):
  # Original tapered crescent. It sweeps across the facing vector over six frames.
  cx,cy=48,48;radius=34+f*1.7;angle=-1.1+f*.12;span=1.5;outer=[];inner=[]
  for i in range(19):
   a=angle+i/18*span;thick=math.sin(i/18*math.pi)*(8 if f<3 else 5)
   outer.append((cx+math.cos(a)*radius,cy+math.sin(a)*radius));inner.append((cx+math.cos(a)*(radius-thick),cy+math.sin(a)*(radius-thick)))
  poly(d,outer+inner[::-1],c[2]);line(d,outer,c[4],2 if f<2 else 1)
  line(d,[(cx+math.cos(a)*(radius-10),cy+math.sin(a)*(radius-10)) for a in [-1+f*.12+i/10*1.3 for i in range(11)]],c[1])
  im.putalpha(im.getchannel('A').point(lambda a:int(a*(1-f*.12))))
 sheet('Slashes',name,(96,96),6,slash)
 def field(d,f,im,c=c,name=name):
  phase=f*TAU/8;ring(d,32,32,27,c[1]);ring(d,32,32,23,c[2],phase=phase/8)
  if name=='nature':
   for i in range(6):
    a=i*TAU/6;line(d,[(32,32),(32+math.cos(a+.4)*14,32+math.sin(a+.4)*14),(32+math.cos(a)*24,32+math.sin(a)*24)],c[1]);leaf(d,32+math.cos(a)*20,32+math.sin(a)*20,c[2],a+phase*.1,.8)
  elif name=='frost':
   for i in range(6):
    a=i*TAU/6;line(d,[(32,32),(32+math.cos(a)*20,32+math.sin(a)*20)],c[2]);x,y=32+math.cos(a)*14,32+math.sin(a)*14;line(d,[(x+math.cos(a+.9)*6,y+math.sin(a+.9)*6),(x,y),(x+math.cos(a-.9)*6,y+math.sin(a-.9)*6)],c[3])
  elif name=='fire':
   for i in range(9):
    a=i*TAU/9;x,y=32+math.cos(a)*24,32+math.sin(a)*20;poly(d,[(x-3,y+2),(x-2,y-3),(x,y-6-f%2),(x+3,y+1),(x,y+3)],c[2]);d.point(pt(x,y),fill=c[3])
  elif name=='lightning':
   for i in range(4):
    a=i*TAU/4+phase*.2;line(d,[(32,32),(32+math.cos(a+.2)*11,32+math.sin(a+.2)*11),(32+math.cos(a)*22,32+math.sin(a)*22)],c[3])
  else:
   for i in range(8):
    a=i*TAU/8;x,y=32+math.cos(a)*27,32+math.sin(a)*23;line(d,[(x-2,y-2),(x+2,y-2),(x+2,y+2)],c[3])
   pts=[(32+math.cos(i*TAU/4+phase*.05)*17,32+math.sin(i*TAU/4+phase*.05)*17) for i in range(5)];line(d,pts,c[2]);pts=[(32+math.cos(i*TAU/3-phase*.08)*13,32+math.sin(i*TAU/3-phase*.08)*13) for i in range(4)];line(d,pts,c[3])
  disc(d,32,32,2,c[4])
 sheet('Fields',name,(64,64),8,field)

def arrow(d,f,im):
 line(d,[(3,6),(25,6)],'#302e40',3);line(d,[(4,5),(26,5)],'#d5bc8a');line(d,[(5,6),(25,6)],'#987951');poly(d,[(23,2),(30,5),(23,9),(25,5)],'#759ca5');line(d,[(23,2),(30,5)],'#f0f4d5');poly(d,[(3,5),(0,1),(8,5),(3,6),(0,9),(8,6)],'#9ccbb4');d.point((27,5),fill='#ffffff')
sheet('Projectiles','arrow',(32,12),4,arrow)
def chakram(d,f,im):
 ring(d,12,12,9,'#2d3041',3,parts=24);ring(d,12,12,8,'#e8c985',2,parts=24)
 for i in range(4):
  a=f*TAU/8+i*TAU/4;poly(d,[(12+math.cos(a)*7,12+math.sin(a)*7),(12+math.cos(a+.4)*11,12+math.sin(a+.4)*11),(12+math.cos(a+.8)*7,12+math.sin(a+.8)*7)],'#f8ecba')
 line(d,[(5,11),(6,7),(10,5)],'#fff8cf')
sheet('Projectiles','chakram',(24,24),8,chakram)
def orb(d,f,im):
 disc(d,10,10,7,'#665189');disc(d,10,9,5,'#a68fc7');disc(d,9,8,3,'#f1eee6');ring(d,10,10,9,'#d5c1dc',phase=f*.2,parts=16)
sheet('Projectiles','orb',(20,20),8,orb)
def cast(d,f,im):
 ring(d,16,16,5+f*1.8,'#b1dccb');line(d,[(16,5),(20,16),(16,27),(12,16),(16,5)],'#e4edd0');disc(d,16,16,4 if f<2 else 1,'#fff4cf');im.putalpha(im.getchannel('A').point(lambda a:int(a*(1-f*.15))))
sheet('Casts','release',(32,32),6,cast)
def dash(d,f,im):
 for i in range(5):
  x=16-i*3-f*2;y=16+(i%2*2-1)*(i+2);line(d,[(max(0,x-9),y),(x+4,y)],'#a4d8c7',1 if i%2 else 2)
 for i in range(4):disc(d,24-f*2+i*5,18+(i%2)*4,2,'#789a81')
 im.putalpha(im.getchannel('A').point(lambda a:int(a*(1-f*.16))))
sheet('Dash','streak',(48,32),6,dash)
def glow(d,f,im):
 for r in range(15,0,-1):disc(d,16,16,r,(255,255,255,round((1-r/16)**2*65)))
sheet('Lights','pulse',(32,32),1,glow)
print('Created',len(list(ROOT.rglob('*.png'))),'original animated VFX sheets')
