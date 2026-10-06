from PIL import Image,ImageDraw,ImageFont,ImageFilter,ImageChops
from pathlib import Path
import numpy as np, math, random, json, hashlib, argparse
ROOT=Path(__file__).resolve().parent
W,H=2480,3508
RW,RH=1240,1754
INK=(43,43,40,255); PENCIL=(100,119,140,205)
FONTS={'hand':r'C:\Windows\Fonts\UDDigiKyokashoN-R.ttc','print':r'C:\Windows\Fonts\BIZ-UDMinchoM.ttc','sans':r'C:\Windows\Fonts\BIZ-UDGothicR.ttc'}
def font(size,kind='hand'):return ImageFont.truetype(FONTS[kind],size)
class Doc:
 def __init__(self,id,title,old=False):
  self.id=id;self.title=title;self.rng=random.Random(1000+id);self.paper=Image.new('RGBA',(W,H),(249,247,238,255) if not old else (225,209,170,255));self.keep=Image.new('RGBA',(W,H));self.erase=Image.new('RGBA',(W,H));self.texts=[]
  a=np.random.default_rng(id).integers(-3,4,(H,W,1),dtype=np.int16);rgb=np.asarray(self.paper).copy();rgb[:,:,:3]=np.clip(rgb[:,:,:3].astype(np.int16)+a,0,255).astype(np.uint8);self.paper=Image.fromarray(rgb)
  if old:
   aging=Image.new('RGBA',(W,H));p=ImageDraw.Draw(aging);p.line([(1235,0),(1250,H)],fill=(141,107,56,40),width=10);p.line([(1251,0),(1263,H)],fill=(255,248,216,100),width=5)
   for x,y,rr in [(260,540,100),(1960,2930,170),(2160,560,60)]:
    p.ellipse((x-rr,y-rr,x+rr,y+rr),fill=(153,108,50,26),outline=(133,97,57,45),width=7)
   self.paper=Image.alpha_composite(self.paper,aging.filter(ImageFilter.GaussianBlur(8)))
 def line(self,pts,target=False,width=5,rough=False,color=None):
  lay=self.erase if target else self.keep;d=ImageDraw.Draw(lay);color=color or (PENCIL if target else INK)
  if rough:
   for _ in range(2):d.line([(x+self.rng.uniform(-7,7),y+self.rng.uniform(-7,7)) for x,y in pts],fill=(*color[:3],90),width=max(1,width//2))
  d.line(pts,fill=color,width=width,joint='curve')
 def ellipse(self,box,target=False,width=5,color=None):ImageDraw.Draw(self.erase if target else self.keep).ellipse(box,outline=color or (PENCIL if target else INK),width=width)
 def text(self,txt,x,y,size=54,target=False,kind='hand',color=None,extra_index=None):
  self.texts.append(txt);f=font(size,kind);cur=x
  for i,c in enumerate(txt):
   dest=self.erase if target or i==extra_index else self.keep
   ImageDraw.Draw(dest).text((cur,y+self.rng.uniform(-1.2,1.2)),c,font=f,fill=color or (PENCIL if target or i==extra_index else INK),stroke_width=0)
   cur+=f.getlength(c)
  return cur
 def rules(self,ys,color=(170,185,186,90)):
  overlay=Image.new('RGBA',(W,H));p=ImageDraw.Draw(overlay)
  for y in ys:p.line([(235,y),(2245,y)],fill=color,width=2)
  self.paper=Image.alpha_composite(self.paper,overlay)
 def star(self,x,y,r=80,target=True):
  pts=[(x+math.cos(-math.pi/2+i*4*math.pi/5)*r,y+math.sin(-math.pi/2+i*4*math.pi/5)*r) for i in range(6)];self.line(pts,target,7,True)
 def export(self):
  original=ROOT/'Source'/f'TEST_{self.id:03d}';runtime=ROOT.parent.parent/'Assets/Keshiya/ExternalArt'/f'TEST_{self.id:03d}';original.mkdir(parents=True,exist_ok=True);runtime.mkdir(parents=True,exist_ok=True)
  layers={'Paper':self.paper,'Protected':self.keep,'Erasable':self.erase}
  layers['EraseMask']=self.erase.getchannel('A').convert('RGB');layers['ProtectMask']=self.keep.getchannel('A').filter(ImageFilter.MinFilter(3)).convert('RGB')
  layers['CompletePreview']=Image.alpha_composite(self.paper,self.keep);layers['InitialPreview']=Image.alpha_composite(Image.alpha_composite(self.paper,self.erase),self.keep)
  for name,img in layers.items():img.save(original/(name+'.png'));img.resize((RW,RH),Image.Resampling.LANCZOS).save(runtime/(name+'.png'))
  mask=np.asarray(layers['EraseMask'].resize((RW,RH),Image.Resampling.LANCZOS))[:,:,0]
  strokes=[]
  for y in range(1,RH,max(1,round(7*RW/1240))):
   xs=np.where(mask[y]>20)[0];
   if not len(xs):continue
   start=prev=int(xs[0])
   for x in list(xs[1:])+[RW+100]:
    x=int(x)
    if x-prev>5:
     if prev>=start:strokes.append({'points':[{'x':(start/RW-.5)*5,'y':(.5-y/RH)*7.07258},{'x':((prev+1)/RW-.5)*5,'y':(.5-y/RH)*7.07258}]})
     start=x
    prev=x
  meta={'id':f'TEST_{self.id:03d}','title':self.title,'source':[W,H],'runtime':[RW,RH],'worldSize':[5,7.07258],'texts':self.texts,'layers':list(layers),'strokes':strokes}
  (original/'manifest.json').write_text(json.dumps(meta,ensure_ascii=False,indent=2),encoding='utf-8');(runtime/'manifest.json').write_text(json.dumps(meta,ensure_ascii=False),encoding='utf-8')
  return meta

def child(d,offset=(0,0),scale=1):
 def p(pts):return [(offset[0]+x*scale,offset[1]+y*scale) for x,y in pts]
 d.line(p([(340,1470),(340,900),(750,540),(1150,920),(1150,1470),(340,1470)]),True,8,True);d.line(p([(270,920),(750,490),(1250,930)]),True,8,True)
 d.line(p([(670,1460),(670,1110),(845,1100),(860,1460)]),True,8,True);d.line(p([(430,1000),(570,990),(580,1170),(430,1180),(430,1000)]),True,7,True)
 d.ellipse((*p([(1590,370)])[0],*p([(1950,730)])[0]),True,8)
 for i in range(12):
  a=i*math.pi/6;d.line(p([(1770+220*math.cos(a),550+220*math.sin(a)),(1770+330*math.cos(a),550+330*math.sin(a))]),True,7,True)
 # person, cat and scribbles
 d.ellipse((*p([(520,1850)])[0],*p([(810,2160)])[0]),True,9);d.line(p([(660,2160),(670,2670),(440,2960)]),True,9,True);d.line(p([(670,2670),(850,2970)]),True,9,True);d.line(p([(340,2430),(665,2280),(1010,2390)]),True,9,True)
 d.ellipse((*p([(1450,2060)])[0],*p([(1920,2460)])[0]),True,9);d.line(p([(1480,2140),(1460,1870),(1640,2070),(1780,2040),(1940,1850),(1900,2160)]),True,8,True)
 d.line(p([(1630,2260),(1710,2260),(1670,2330),(1630,2260)]),True,7,True)
 for y in [2300,2370]:d.line(p([(1490,y),(1320,y-40)]),True,6,True);d.line(p([(1870,y),(2080,y-25)]),True,6,True)
 for x,y in [(1560,2170),(1760,2170)]:d.ellipse((*p([(x,y)])[0],*p([(x+40,y+50)])[0]),True,8)
 for i in range(3):d.star(offset[0]+(2100-i*130)*scale,offset[1]+(1270+i*210)*scale,65*scale)
 pts=[(offset[0]+(1550+(15+i*2)*math.cos(i*.28))*scale,offset[1]+(2800+(15+i*2)*math.sin(i*.28))*scale) for i in range(150)];d.line(pts,True,6,True)

def make001():
 d=Doc(1,'机に置いてあった紙');im=Image.open(ROOT/'child-original.png').convert('L').resize((W,H),Image.Resampling.LANCZOS);alpha=ImageChops.invert(im).point(lambda x:0 if x<12 else x);ink=Image.new('RGBA',(W,H),(87,98,110));ink.putalpha(alpha);d.erase=ink;return d

def make002():
 d=Doc(2,'一文字だけ、お願いします');lines=['佐伯さんへ','','この前は、引っ越しの準備を手伝ってくれて','ありがとうございました。','','こちらに来たばかりの頃、','何も分からなかった私に、','最初に声をかけてくれたのも佐伯さんでした。','','離れてしまうのは寂しいですが、','また会えるのを楽しみにしていまます。','','次に会うときは、','今度は私がおいしい店を案内します。','','どうかお元気で。','','美咲']
 d.rules([450+i*142 for i in range(19)])
 for i,line in enumerate(lines):d.text(line,285 if i!=17 else 1900,330+i*142,62,extra_index=line.index('まま')+1 if 'まま' in line else None)
 return d

def make003():
 d=Doc(3,'締切は昨日でした');im=Image.open(ROOT/'manga-ink-original.png').convert('L').resize((2250,3180),Image.Resampling.LANCZOS);alpha=ImageChops.invert(im).point(lambda x:0 if x<20 else x);ink=Image.new('RGBA',im.size,(18,21,27));ink.putalpha(alpha);d.keep.alpha_composite(ink,(115,165))
 # Draft construction follows the rendered portrait, fingers and street perspectives.
 for box in [(455,1170,1060,1710),(930,380,1380,730),(840,2000,1290,2420),(1890,2460,2210,2890)]:
  d.ellipse(box,True,6,(126,155,175,170));cx=(box[0]+box[2])/2;cy=(box[1]+box[3])/2;d.line([(cx,box[1]-50),(cx+20,box[3]+65)],True,5);d.line([(box[0]-30,cy),(box[2]+50,cy+20)],True,5)
 for i in range(8):d.line([(190+i*270,910),(1280,340)],True,4,True)
 for i in range(7):d.line([(400+i*215,3200),(1350,2190)],True,4,True)
 for i in range(6):d.line([(1670+i*38,1430),(1860+i*40,1580),(1870+i*35,1720)],True,5,True)
 for i in range(9):d.line([(440+i*60,1950),(620+i*80,2200),(520+i*70,2600)],True,5,True)
 for x,y in [(950,1440),(600,1470),(2050,2740),(1160,600)]:
  d.line([(x-100,y-80),(x-30,y+30),(x+80,y+55)],True,6,True)
 # Older balloon proposal and page construction outside the ink.
 d.ellipse((1350,1195,1520,1475),True,5)
 d.line([(104,195),(2340,195)],True,5,True)
 d.line([(102,195),(102,3325)],True,5,True)
 d.line([(153,1855),(2300,1855)],True,5,True)
 # Feedback 0.8.2: keep a few facial/finger construction marks, move most
 # provisional composition work into open paper (old balloon, sky and action arcs).
 old=d.erase.copy();d.erase=Image.new('RGBA',(W,H))
 for box in [(1350,1195,1520,1475),(1880,1910,2225,2130),(1480,230,1800,405)]:
  d.ellipse(box,True,7,(126,155,175,170))
 for pts in [[(1420,350),(1580,300),(1780,335)],[(1310,2330),(1470,2400),(1530,2610)],[(1410,2250),(1550,2380),(1590,2640)],[(1760,1960),(2020,1900),(2250,2020)],[(350,3300),(900,3350),(1770,3300)]]:
  d.line(pts,True,7,True)
 # Downweight the dense old structural marks. Deliberate close-up points remain.
 olda=np.asarray(old.getchannel('A')).copy();yy,xx=np.indices(olda.shape)
 detail=((xx>680)&(xx<1020)&(yy>1370)&(yy<1500))|((xx>1780)&(xx<1950)&(yy>1480)&(yy<1630))
 olda=np.where(detail,olda,olda*.16).astype(np.uint8);old.putalpha(Image.fromarray(olda));d.erase=Image.alpha_composite(old,d.erase)
 # Exclude a 24-pixel zone around ink. The same trimming is applied to visual and official mask.
 safety=d.keep.getchannel('A').point(lambda x:255 if x>95 else 0).filter(ImageFilter.MaxFilter(49));a=ImageChops.multiply(d.erase.getchannel('A'),ImageChops.invert(safety));d.erase.putalpha(a)
 return d

def make004():
 d=Doc(4,'母のレシピ',True);d.rules([570+i*150 for i in range(17)],(142,142,118,75));d.text('うちのハンバーグ',295,280,92)
 lines=['材料　（４人分）','合挽肉　　　　　300g','玉ねぎ　　　　　1/2個','パン粉　　　　　大さじ４','牛乳　　　　　　大さじ３','卵　　　　　　　１個','塩・こしょう　　少々','','玉ねぎを細かく切って、よく炒める。','冷めてから、お肉といっしょにこねる。','真ん中を少しへこませて焼く。','ふたをして、弱火でじっくり。','','ソースはケチャップと中濃ソース。','お父さんの分は、少し大きめに。']
 for i,line in enumerate(lines):d.text(line,315,530+i*151,57,color=(78,66,50,255))
 d.text('おいしい',1550,1010,87,True);d.star(2030,1610,110);d.ellipse((1660,1840,2090,2180),True,9);d.ellipse((1760,1920,1805,1975),True,8);d.ellipse((1930,1920,1975,1975),True,8);d.line([(1770,2060),(1850,2110),(1950,2060)],True,8,True)
 d.line([(240,2560),(880,2520),(960,2610),(1500,2550),(2240,2610)],True,8,True);d.line([(230,900),(280,870),(355,950),(380,870)],True,7,True)
 # Keep crossings visible behind the old writing, while only accessible graphite is an official target.
 safety=d.keep.getchannel('A').point(lambda x:255 if x>95 else 0).filter(ImageFilter.MaxFilter(37));d.erase.putalpha(ImageChops.multiply(d.erase.getchannel('A'),ImageChops.invert(safety)))
 return d

def make005():
 d=Doc(5,'誰にも見せない名前');d.rules([530+i*215 for i in range(13)])
 for row,line in enumerate(['春　夏　秋　冬','２ × ３ ＝ ６','ありがとう　また明日','山　川　空　海','１２＋８＝２０','えんぴつの練習','あいうえお　かきくけこ','ABC abc 123','今日もよくできました']):d.text(line,320,440+row*265,82,True)
 for x,y in [(2000,620),(2010,2420)]:d.star(x,y,95)
 return d

def make006():
 d=Doc(6,'100点じゃ困るんです');p=ImageDraw.Draw(d.paper);p.rectangle((170,155,2310,3330),outline=(180,182,177,255),width=3)
 d.text('２年　算数　たしかめテスト',260,230,69,kind='sans');d.text('２年１組　　氏名　森川 ひなた',275,390,49,kind='sans')
 # The first two digits are graphite corrections; last zero and suffix remain protected.
 x,y=1810,500;d.text('10',x,y,105,True);d.text('0',x+126,y,105);d.text('点',x+245,y+15,64,kind='print')
 d.text('よくがんばりました！',1600,650,40,color=(174,59,55,255))
 problems=[('１　つぎの計算をしましょう。','12 + 8 = 20','30 - 7 = 23'),('２　かけ算の答えを書きましょう。','6 × 4 = 24','8 × 7 = 56'),('３　式と答えを書きましょう。','えんぴつが５本ずつ、３つあります。','5 × 3 = 15　　答え 15本'),('４　長さを求めましょう。','25 cm + 35 cm = 60 cm','答え　60 cm')]
 for i,lines in enumerate(problems):
  yy=900+i*555;d.line([(250,yy-60),(2240,yy-60)],False,2,color=(130,141,146,200))
  for n,line in enumerate(lines):d.text(line,280 if n==0 else 445,yy+n*130,48 if n==0 else 58,kind='print' if n==0 else 'hand')
  d.ellipse((1940,yy+120,2100,yy+280),False,8,(180,58,53,230))
 d.text('計算の途中も、ていねいに書けています。',300,3200,45,color=(173,58,55,255))
 return d

def make007():
 d=Doc(7,'渡さなかった手紙');lines=['あなたへ','','何度も書き直しているうちに、','もう夜になってしまいました。','会って話せばいいことなのに、','顔を見ると、いつも別の話をしてしまいます。','','帰り道に並んで歩いたことや、','雨の日に半分だけ貸してくれた傘のことを、','私は思っていたよりよく覚えています。','','ずっと、あなたのことを、好きでした。','','言ったら今までと変わってしまう気がして、','何でもないふりをしていました。','でも、知らないままでいてもらうのも、','少しだけ寂しくなりました。','','返事を急がせたいわけではありません。','今度会ったら、いつものように話せたら嬉しいです。','','ここまで読んでくれて、ありがとう。','','遥']
 d.rules([375+i*119 for i in range(25)])
 for i,line in enumerate(lines):d.text(line,280 if i!=23 else 1910,280+i*119,57,True,color=(95,99,104,215))
 return d

def make008():
 d=Doc(8,'全部消して構いません');im=Image.open(ROOT/'rough-original.png').convert('L').resize((W,H),Image.Resampling.LANCZOS);alpha=ImageChops.invert(im).point(lambda x:0 if x<12 else x);ink=Image.new('RGBA',(W,H),(69,73,78));ink.putalpha(alpha);d.erase=ink;return d

def main():
 global RW,RH
 parser=argparse.ArgumentParser();parser.add_argument('--jobs',default='1,2,3,4,5,6,7,8');parser.add_argument('--runtime-width',type=int,default=1240);args=parser.parse_args();RW=args.runtime_width;RH=round(RW*H/W);allmeta=[]
 for n in map(int,args.jobs.split(',')):
  d=globals()[f'make{n:03d}']();meta=d.export();allmeta.append(meta);print(meta['id'],len(meta['strokes']),flush=True)
 # Fictional name label for the supplied school eraser.
 label=Image.new('RGBA',(512,160));ImageDraw.Draw(label).text((34,37),'蒼井 ハル',font=font(69),fill=(101,103,105,195));label.save(ROOT.parent.parent/'Assets/Keshiya/ExternalArt/NameLabel.png')
 allmeta=[json.loads(p.read_text(encoding='utf8')) for p in sorted((ROOT/'Source').glob('TEST_*/manifest.json'))]
 (ROOT/'generation-summary.json').write_text(json.dumps([{k:v for k,v in x.items() if k!='strokes'} for x in allmeta],ensure_ascii=False,indent=2),encoding='utf-8')
if __name__=='__main__':main()
