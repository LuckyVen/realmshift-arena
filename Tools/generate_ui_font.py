"""Compile the project's original 5x7 pixel alphabet into a stable, sharp TrueType font.
Run generate_font.py first when changing the alphabet. Requires fonttools + Pillow.
"""
from pathlib import Path
from PIL import Image
from fontTools.fontBuilder import FontBuilder
from fontTools.pens.ttGlyphPen import TTGlyphPen
import re
root=Path(__file__).resolve().parents[1]/'Assets/Art/UI'
im=Image.open(root/'realm_font.png').convert('RGBA')
glyphs={};metrics={};cmap={};order=['.notdef']
pen=TTGlyphPen(None);glyphs['.notdef']=pen.glyph();metrics['.notdef']=(600,0)
for row in (root/'realm_font.fnt').read_text().splitlines():
 if not row.startswith('char id='):continue
 v={k:int(val) for k,val in re.findall(r'(\w+)=(-?\d+)',row)};code=v['id'];name=f'uni{code:04X}';order.append(name);cmap[code]=name;pen=TTGlyphPen(None)
 for y in range(v['height']):
  for x in range(v['width']):
   if im.getpixel((v['x']+x,v['y']+y))[3]>100:
    xx=x*100;yy=(7-y)*100;pen.moveTo((xx,yy));pen.lineTo((xx+100,yy));pen.lineTo((xx+100,yy+100));pen.lineTo((xx,yy+100));pen.closePath()
 glyphs[name]=pen.glyph();metrics[name]=(v['xadvance']*100,0)
fb=FontBuilder(1000,isTTF=True);fb.setupGlyphOrder(order);fb.setupCharacterMap(cmap);fb.setupGlyf(glyphs);fb.setupHorizontalMetrics(metrics);fb.setupHorizontalHeader(ascent=800,descent=-200);fb.setupNameTable({'familyName':'Realm Pixel','styleName':'Regular','uniqueFontIdentifier':'RealmshiftArena.Pixel.1.1','fullName':'Realm Pixel','psName':'RealmPixel'});fb.setupOS2(sTypoAscender=800,sTypoDescender=-200,sTypoLineGap=0,usWinAscent=800,usWinDescent=200);fb.setupPost();fb.setupMaxp();fb.save(root/'realm_pixel.ttf');print('Original Realm Pixel TTF created')
