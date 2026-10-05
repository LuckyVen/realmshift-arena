#!/usr/bin/env python3
"""Original code-native, grid-aligned SVG art. No external assets or imaging dependencies."""
from pathlib import Path
from random import Random
ROOT=Path(__file__).resolve().parents[1]/'Assets/Art'
INK='#152633';GOLD='#d5aa62';LIGHT='#f2ddb4';METAL='#7197a0';STEEL='#c5e5dc';VIOLET='#aa8ed7';TEAL='#8ad8c3'
def svg(path,body,width=32,height=32):
 p=ROOT/path;p.parent.mkdir(parents=True,exist_ok=True)
 p.write_text(f'<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}" shape-rendering="crispEdges">{body}</svg>')
def polygon(points,fill):return f'<polygon points="{points}" fill="{fill}"/>'
def rect(x,y,w,h,fill):return f'<rect x="{x}" y="{y}" width="{w}" height="{h}" fill="{fill}"/>'
def path(d,fill):return f'<path d="{d}" fill="{fill}"/>'
# Diagonal hand-held orientation: the tip faces north-east before the player rotates it.
weapons=[]
# Wand: wrapped walnut grip, brass collars, luminous faceted crystal, floating rune.
s=polygon('3,28 5,31 24,12 21,9',INK)+polygon('5,28 6,29 22,13 20,11','#78574d')
s+=polygon('7,25 9,27 11,25 9,23',GOLD)+polygon('13,19 15,21 17,19 15,17',LIGHT)+polygon('17,13 21,17 24,14 20,10',GOLD)
s+=polygon('18,7 22,2 29,3 30,10 25,14 20,12',INK)+polygon('20,7 23,4 27,4 28,9 25,12 22,10',TEAL)+polygon('23,4 27,4 24,9 22,10','#d7fff0')+polygon('25,8 28,9 25,12','#43978d')
s+=rect(16,4,2,2,GOLD)+rect(30,14,1,2,TEAL)+rect(12,11,1,1,LIGHT);weapons.append(s)
# Longbow: recurved yew limbs, taut string, silver arrowhead and fletching.
s=path('M6 3H12V5H19V8H23V13H26V20H24V25H19V28H11V30H7V27H12V25H17V22H20V18H21V13H19V10H15V7H9V5H6Z',INK)
s+=path('M8 4H11V6H18V9H21V14H24V20H22V24H18V26H11V28H9V27H14V24H18V20H21V15H19V11H15V8H10V6H8Z','#a27650')
s+=path('M10 5H13V7H18V9H20V12H18V10H15V8H11V6H10Z',LIGHT)+path('M21 19H23V22H20V25H16V26H13V25H18V22H20Z',GOLD)
s+=polygon('8,5 9,5 10,27 9,28','#d4ded0')+polygon('4,27 6,29 26,9 24,7',INK)+polygon('6,27 7,28 24,11 23,10',LIGHT)
s+=polygon('23,7 29,4 27,11 25,12',STEEL)+polygon('5,24 7,24 10,27 8,28',TEAL)+rect(22,6,2,2,GOLD);weapons.append(s)
# Rune blade: long faceted steel, central enchanted channel and flared crossguard.
s=polygon('3,28 5,31 10,26 9,24 13,23 16,25 19,22 15,18 29,6 30,1 25,2 12,16 8,12 5,15 8,19 7,23',INK)
s+=polygon('11,17 25,4 28,3 27,7 14,21',STEEL)+polygon('14,18 26,5 27,7 15,20',METAL)+polygon('12,18 24,6 25,6 13,20',VIOLET)
s+=polygon('6,15 8,14 18,23 16,24',GOLD)+polygon('8,15 15,21 14,22 7,16',LIGHT)+polygon('7,21 10,24 6,28 4,27','#77544b')+polygon('3,27 6,26 8,29 5,31',GOLD)+rect(18,12,1,2,'#f2e6ff');weapons.append(s)
# Staff: longer staff and branching prongs, an amethyst focus held between arms.
s=polygon('2,28 4,31 21,14 19,11',INK)+polygon('4,28 5,29 20,14 18,13','#72564c')+polygon('7,24 10,25 12,22 9,21',GOLD)
s+=path('M14 10V5H17V9L21 13H24L28 9V4H30V10L25 16H20Z',INK)+path('M16 6H17V10L21 14H24L28 10V6H29V10L25 15H20L16 10Z',GOLD)
s+=polygon('19,2 24,1 28,5 26,10 21,12 18,7',INK)+polygon('21,3 24,3 26,6 24,9 21,10 20,6',VIOLET)+polygon('21,3 24,3 23,7 21,8','#e5d8ff')
s+=rect(14,2,1,2,TEAL)+rect(28,15,2,2,TEAL)+polygon('12,18 15,21 17,19 14,16',LIGHT);weapons.append(s)
# Chakram: open ring, four swept knife fins and a hollow silhouette.
s=path('M9 4H22L26 8V12H30V18H27V22L23 27H19V30H13V27H9L4 23V19H1V13H5V9ZM12 10L9 14V19L13 23H19L23 19V13L19 9H14Z',INK)
s+=path('M10 6H21L24 9V14H28V17H25V21L21 25H17V28H14V25H10L6 21V17H3V14H7V10ZM12 10L9 14V19L13 23H19L23 19V13L19 9H14Z',METAL)
s+=path('M10 6H21L24 9V11H22L19 8H12L8 12H6V10Z',STEEL)+polygon('23,14 28,14 26,17 23,18',LIGHT)+polygon('14,25 17,24 17,28 14,28',GOLD)+polygon('3,14 7,13 8,16 3,17',GOLD)
s+=rect(10,7,3,1,TEAL)+rect(22,21,2,2,TEAL);weapons.append(s)
# Orbs: three different faceted crystals and a broken orbit path.
s=path('M8 4H18V5H8ZM22 6H24V9H22ZM27 15H28V23H27ZM15 27H23V28H15ZM3 17H4V24H3Z','#77719c')
for x,y,k in [(10,9,1),(22,20,0),(7,25,0)]:
 sz=6 if k else 4;s+=polygon(f'{x},{y-sz} {x+sz},{y-1} {x+sz-1},{y+sz-1} {x},{y+sz+1} {x-sz},{y+2} {x-sz},{y-2}',INK)
 s+=polygon(f'{x},{y-sz+2} {x+sz-2},{y-1} {x+sz-2},{y+sz-2} {x},{y+sz-1} {x-sz+2},{y+1} {x-sz+2},{y-1}',VIOLET if k else TEAL)
 s+=polygon(f'{x},{y-sz+2} {x+1},{y-1} {x-2},{y+2} {x-sz+2},{y-1}',LIGHT)+rect(x,y+2,2,1,'#6b78b4')
s+=rect(24,4,2,2,GOLD)+rect(18,14,1,2,LIGHT);weapons.append(s)
# Spellbook: deep indigo leather, visible leaves, brass clasps and glowing rune.
s=polygon('4,7 9,4 26,7 29,11 29,27 24,30 5,26 3,22',INK)+polygon('5,9 9,7 25,10 26,25 23,27 5,23','#d9cba6')
s+=polygon('6,11 23,14 23,25 6,22','#efe1b5')+path('M7 16L23 19V20L7 17ZM7 20L23 23V24L7 21Z','#9c9e84')
s+=polygon('7,5 26,8 26,24 7,21',VIOLET)+polygon('7,5 10,6 10,21 7,21','#655b87')+polygon('11,7 25,9 25,22 11,20','#766294')
s+=polygon('17,10 21,15 17,19 14,14',GOLD)+polygon('17,12 19,15 17,17 16,14',TEAL)+rect(24,12,4,3,GOLD)+rect(24,12,3,1,LIGHT)+rect(11,7,3,1,LIGHT)+rect(7,20,2,2,GOLD);weapons.append(s)
# Gauntlets: paired knuckle plates, overlapping scale cuffs and inset crystals.
s=''
for x,y in [(3,7),(17,15)]:
 s+=path(f'M{x} {y+5}V{y+1}H{x+3}V{y}H{x+10}V{y+2}H{x+13}V{y+10}H{x+10}V{y+14}H{x+3}V{y+12}H{x}Z',INK)
 s+=rect(x+2,y+2,9,8,METAL)+rect(x+3,y+10,7,3,GOLD)+rect(x+3,y+2,7,2,STEEL)+rect(x+2,y+5,2,4,STEEL)
 s+=rect(x+4,y+5,5,3,'#557880')+polygon(f'{x+6},{y+4} {x+9},{y+6} {x+6},{y+9} {x+4},{y+6}',TEAL)+rect(x+4,y+11,5,1,LIGHT)
s+=rect(16,4,1,3,GOLD)+rect(15,5,3,1,GOLD);weapons.append(s)
for i,s in enumerate(weapons):svg(Path(f'Weapons/Polished/weapon_{i}.svg'),s)
# Native HUD glyphs are authored on the same pixel grid.
svg(Path('UI/HUD/heart.svg'),polygon('3,3 8,3 10,5 12,3 17,3 20,6 20,11 12,19 10,19 2,11 2,6',INK)+polygon('4,5 8,5 11,8 14,5 17,5 18,7 18,10 11,17 4,10', '#d76663')+rect(5,6,3,2,'#ffb995')+polygon('14,12 18,8 18,10 11,17 10,15','#994b59'),22,22)
svg(Path('UI/HUD/dash.svg'),polygon('2,6 8,6 10,3 18,3 11,10 15,10 5,20 7,12 2,12',INK)+polygon('4,7 9,7 11,4 16,4 9,11 12,11 7,16 9,10 4,10',TEAL)+rect(1,15,3,1,LIGHT)+rect(0,18,3,1,TEAL),20,22)
svg(Path('UI/HUD/skill.svg'),polygon('16,3 19,11 27,16 19,20 16,29 12,20 4,16 12,12',LIGHT)+polygon('16,8 18,14 23,16 18,18 16,23 14,18 9,16 14,14',GOLD)+rect(4,5,2,2,TEAL)+rect(25,25,2,2,TEAL))
svg(Path('UI/HUD/hero.svg'),polygon('16,3 27,14 27,18 16,29 5,18 5,14',LIGHT)+polygon('16,7 23,14 23,18 16,25 9,18 9,14',INK)+polygon('16,10 21,16 16,22 12,16',TEAL)+rect(15,12,2,6,LIGHT))
colors=['#ed9b63','#91d9e9','#f2d779','#98d682','#b79eeb']
elements=[polygon('8,1 13,7 12,14 8,16 3,13 2,9 6,4 6,9 9,7',colors[0])+polygon('8,8 10,12 8,14 6,12','#fff1b7'),polygon('8,1 15,8 8,16 1,8',colors[1])+polygon('8,3 9,8 8,13 4,8',LIGHT),polygon('8,1 14,1 9,7 13,7 3,16 6,9 2,9',colors[2]),polygon('3,14 2,8 5,3 14,1 15,9 11,13 6,14',colors[3])+polygon('3,14 11,5 12,5 4,16',LIGHT),polygon('8,1 15,8 8,15 1,8',colors[4])+polygon('8,4 12,8 8,12 4,8',INK)+rect(7,7,2,2,LIGHT)]
for i,s in enumerate(elements):svg(Path(f'UI/HUD/element_{i}.svg'),s,16,16)
# Three wind poses x six plant silhouettes for each realm. Baseline stays fixed.
palettes=[('#325e49','#538563','#85aa72','#baca89','#e4d39e'),('#694642','#92694e','#b39163','#cca579','#e19a6c'),('#425c6d','#6f8b99','#9fb5b7','#ceddd2','#ebf0dc'),('#454464','#6b6489','#9382b0','#b2a1c8','#d4c4ed')]
for realm,pa in enumerate(palettes):
 for variant in range(6):
  sheet=''
  for frame in range(3):
   shift=(-1,0,1)[frame];seed=Random(508+realm*79+variant*43);s=''
   if variant in (0,1,2):
    s+=polygon('3,20 6,22 19,22 22,20 19,18 7,18',pa[0])
    for blade in range(8 if variant==1 else 5):
     x=5+blade*2+(seed.randrange(2));h=seed.randrange(5,13) if variant!=2 else seed.randrange(3,8)
     tip=x+shift+(blade%3-1)*2
     s+=polygon(f'{x},21 {x+2},21 {tip+1},{21-h} {tip-1},{19-h}',pa[1+blade%3])
     s+=rect(x,19,1,2,pa[0])
    if variant==2:
     for x,y in [(7,15),(14,13),(19,17)]:s+=rect(x+shift,y-3,1,5,pa[1])+rect(x+shift-1,y-4,3,2,pa[4])+rect(x+shift,y-5,1,1,pa[3])
   elif variant==3:
    s+=polygon('3,21 3,16 6,12 10,10 14,12 20,13 23,17 21,21',pa[0])
    s+=polygon('4,17 7,13 11,12 14,14 19,14 21,18 18,20 7,20',pa[1])
    s+=rect(7+shift,13,4,2,pa[2])+rect(15+shift,15,4,2,pa[2])+rect(10,18,3,1,pa[3])+rect(20,17,2,1,pa[3])
   elif variant==4:
    for x,y in [(8,20),(17,21)]:
     s+=rect(x,y-7,2,7,pa[3])+polygon(f'{x-4+shift},{y-5} {x-3+shift},{y-9} {x+shift},{y-11} {x+4+shift},{y-8} {x+5+shift},{y-5}',pa[0])
     s+=polygon(f'{x-3+shift},{y-6} {x-2+shift},{y-9} {x+shift},{y-10} {x+3+shift},{y-8} {x+4+shift},{y-6}',pa[2])+rect(x+shift-1,y-9,2,1,pa[4])
   else:
    for x,y in [(6,20),(12,22),(19,19)]:
     s+=rect(x,y-9,1,9,pa[0])+polygon(f'{x},{y-4} {x-5+shift},{y-8} {x-4+shift},{y-11} {x-1},{y-8} {x+1},{y-6}',pa[1])
     s+=polygon(f'{x},{y-6} {x+4+shift},{y-13} {x+6+shift},{y-12} {x+4},{y-7}',pa[2])+rect(x+1,y-2,2,2,pa[3])
   sheet+=f'<g transform="translate({frame*24},0)">{s}</g>'
  svg(Path(f'Foliage/realm_{realm}_{variant}.svg'),sheet,72,24)
print('8 weapon vectors, 9 HUD glyphs and 24 animated foliage sheets authored.')
