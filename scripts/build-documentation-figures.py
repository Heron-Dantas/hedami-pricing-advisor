"""Generate original technical diagrams locally; no model or network calls."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import math,textwrap
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'docs/deliverables/assets';OUT.mkdir(parents=True,exist_ok=True)
NAVY='#17324D';TEAL='#087F8C';BLUE='#E8F2FB';PALE='#E9F6F3';AMBER='#FFF1D6';GRAY='#F3F6F8';INK='#17324D'
fontroot=Path('/System/Library/Fonts/Supplemental')
def font(size,bold=False):
    paths=[fontroot/('Arial Bold.ttf' if bold else 'Arial.ttf'),Path('/usr/share/fonts/truetype/dejavu')/('DejaVuSans-Bold.ttf' if bold else 'DejaVuSans.ttf')]
    for p in paths:
        if p.exists():return ImageFont.truetype(str(p),size)
    return ImageFont.load_default()
class Figure:
    def __init__(self,title,subtitle,h=1100):
        self.im=Image.new('RGB',(1800,h),'white');self.d=ImageDraw.Draw(self.im)
        self.d.text((45,25),title,font=font(48,True),fill=NAVY)
        self.d.text((45,90),subtitle,font=font(30),fill='#506779')
    def text(self,xy,text,size=34,bold=False,fill=INK):
        self.d.text(xy,text,font=font(size,bold),fill=fill)
    def node(self,box,title,body='',color=BLUE):
        x1,y1,x2,y2=box
        self.d.rounded_rectangle(box,20,fill=color,outline=NAVY,width=3)
        f=font(36,True);bf=font(30)
        def lines(s,f,maxw):
            out=[]
            for part in s.split('\n'):
                line=''
                for w in part.split():
                    probe=(line+' '+w).strip()
                    if self.d.textlength(probe,font=f)>maxw and line:out.append(line);line=w
                    else:line=probe
                out.append(line)
            return out
        titlelines=lines(title,f,x2-x1-34)
        bodylines=lines(body,bf,x2-x1-34) if body else []
        height=len(titlelines)*44+len(bodylines)*37+(12 if bodylines else 0)
        y=y1+(y2-y1-height)/2
        for line in titlelines:
            self.d.text(((x1+x2-self.d.textlength(line,font=f))/2,y),line,font=f,fill=INK);y+=44
        y+=12 if bodylines else 0
        for line in bodylines:
            self.d.text(((x1+x2-self.d.textlength(line,font=bf))/2,y),line,font=bf,fill=INK);y+=37
        assert y<=y2+5,(title,box,y)
    def edge(self,points,label='',at=None,color=TEAL):
        self.d.line(points,fill=color,width=6,joint='curve')
        a,b=points[-2:];angle=math.atan2(b[1]-a[1],b[0]-a[0]);l=20
        self.d.polygon([b,(b[0]-l*math.cos(angle-.5),b[1]-l*math.sin(angle-.5)),(b[0]-l*math.cos(angle+.5),b[1]-l*math.sin(angle+.5))],fill=color)
        if label and at:
            f=font(28);bb=self.d.textbbox(at,label,font=f);self.d.rectangle((bb[0]-5,bb[1]-4,bb[2]+5,bb[3]+5),fill='white');self.d.text(at,label,font=f,fill=color)
    def save(self,name):self.im.save(OUT/name,dpi=(240,240))
f=Figure('Architecture: local controls, external model','Implemented console prototype | arrows show application calls and returned evidence')
f.d.rounded_rectangle((30,145,1230,1030),25,fill=GRAY,outline='#BBCBD6',width=3)
f.edge([(350,240),(460,240)]);f.edge([(650,310),(650,400)])
f.edge([(920,445),(1330,445)],'request',(1040,405))
f.edge([(1330,510),(920,510)],'result',(1040,530))
f.edge([(520,550),(520,600),(300,600),(300,650)])
f.edge([(480,730),(630,730)],'calculate',(510,680))
f.edge([(300,900),(300,810)],'read',(320,855))
f.edge([(920,520),(1160,520),(1160,940),(1070,940)],'events',(1170,750))
f.edge([(350,440),(440,440)])
f.node((70,185,350,310),'Human','Objective / feedback',PALE)
f.node((460,185,850,310),'Console','Compose and render')
f.node((440,400,920,550),'Harness','State, checks, loop and budgets',PALE)
f.node((1330,380,1760,570),'OpenAI model','Analyst or fresh reviewer context',AMBER)
f.node((65,370,350,520),'Runtime skills','Two trusted files')
f.node((80,650,480,810),'Typed tools','Read, calculate, draft')
f.node((630,650,1070,810),'C# domain','decimal arithmetic',PALE)
f.node((80,900,480,1000),'Local catalog','Synthetic JSON')
f.node((630,900,1070,1000),'Local traces','Observable JSON')
f.save('architecture.png')

f=Figure('Main loop: model chooses, application executes','PricingHarness.RunAsync | success and failure paths have explicit stop conditions')
f.edge([(400,240),(590,240)]);f.edge([(1010,240),(1230,240)])
f.edge([(1470,320),(1470,440)]);f.edge([(1230,520),(1050,520)])
f.edge([(590,520),(420,520)]);f.edge([(220,600),(220,810)])
f.edge([(420,890),(720,890)],'terminal',(590,850))
f.edge([(420,960),(560,960),(560,700),(1760,700),(1760,240),(1710,240)],'continue with tool output',(730,655))
f.node((45,160,400,320),'Objective','Human request',PALE)
f.node((590,160,1010,320),'Harness checks','Context, usage and cancellation',PALE)
f.node((1230,160,1710,320),'Model response','Exactly one function call',AMBER)
f.node((1230,440,1710,600),'Tool request','Name + JSON arguments')
f.node((590,440,1050,600),'Validate','Schema, permission and plan')
f.node((45,440,420,600),'Execute tool','Stored data / calculator')
f.node((45,810,420,990),'Record and append','Result uses same call_id')
f.node((720,810,1350,990),'Draft / clarification / bounded stop','No arbitrary price application',PALE)
f.save('main-loop.png')

f=Figure('Reflection and collaboration: separated contexts','One provider/model may serve both roles | no independent truth oracle')
f.edge([(480,300),(680,300)]);f.edge([(1100,300),(1290,300)],'fresh context',(1120,245))
f.edge([(1510,420),(1510,650)]);f.edge([(1290,760),(1050,760)],'revise',(1110,700))
f.edge([(680,760),(270,760),(270,420)],'bounded feedback',(320,700))
f.edge([(1510,870),(1510,970),(1020,970)],'approve',(1250,920))
f.node((50,180,480,420),'Analyst context','Objective, tool results and draft',BLUE)
f.node((680,180,1100,420),'Harness','Verify evidence and coordinate',PALE)
f.node((1290,180,1740,420),'Reviewer context','Objective + draft; no analyst history',AMBER)
f.node((1290,650,1740,870),'submit_review','Verdict + traceable findings',AMBER)
f.node((680,650,1050,870),'Revision budget','One revised draft by default')
f.node((480,910,1020,1030),'Reviewed draft or stop','Human approval still required',PALE)
f.save('review-loop.png')

f=Figure('Adaptive planning: the selected action gates execution','The planner is an analyst mode, not a third independent agent')
f.edge([(420,270),(600,270)]);f.edge([(1050,270),(1260,270)])
f.edge([(1490,380),(1490,600)]);f.edge([(1260,710),(1050,710)])
f.edge([(600,710),(225,710),(225,380)],'replan after results / errors',(50,525))
f.node((40,170,420,380),'Analyst selects','update_plan(nextTool, objective)',AMBER)
f.node((600,170,1050,380),'Dependency checks','Read before calculate; evidence before draft',PALE)
f.node((1260,170,1740,380),'Stored NextTool','Short observable objective')
f.node((1260,600,1740,820),'Execution gate','Requested tool must match; selection is consumed',PALE)
f.node((600,600,1050,820),'Tool result','Advance evidence or revise next action')
f.node((190,920,1600,1040),'Safe exit: request_clarification','Missing information can stop without an artificial tool sequence',PALE)
f.save('planning.png')

f=Figure('Human feedback: a new linked run with current data','Demonstrated real workflow | prior target replaced by exactly 25%')
f.edge([(480,270),(690,270)]);f.edge([(1120,270),(1330,270)])
f.edge([(1540,400),(1540,650)]);f.edge([(1330,760),(1110,760)])
f.edge([(690,760),(480,760)])
f.node((50,170,480,400),'Prior run','Draft or questions; immutable run ID')
f.node((690,170,1120,400),'Human feedback','New objective overrides conflicting prior targets',AMBER)
f.node((1330,170,1750,400),'Context projection','Compact prior evidence; parentRunId')
f.node((1330,650,1750,880),'New harness','Fresh run / reread catalog and rules',PALE)
f.node((690,650,1110,880),'Recalculate','Stored inputs; target 25%; discount 10%',PALE)
f.node((50,650,480,880),'New draft','BRL 41.03 / 36.93; margins 25.01% / 17.79%')
f.text((120,1000),'No catalog mutation and no price application are introduced by feedback.',34)
f.save('feedback.png')

f=Figure('Phase two: prioritize evidence before expansion','PROPOSED ONLY | each gate needs measurable acceptance and human decisions')
f.edge([(780,300),(1010,300)]);f.edge([(1350,450),(1350,650)])
f.edge([(1010,800),(780,800)]);f.edge([(450,950),(450,1050),(1350,1050)])
f.node((70,160,780,450),'Gate 1: quality and review','Outcome-based grading; actionable critiques; retain historical failures',PALE)
f.node((1010,160,1720,450),'Gate 2: business inputs','Per-field provenance; verified costs; actual settlement assumptions',BLUE)
f.node((1010,650,1720,950),'Gate 3: context and state','Compact evidence; durable reservations; tested recovery and concurrency',PALE)
f.node((70,650,780,950),'Gate 4: human workflow','Usable review UI; governed edits; identity-bound approval if needed',BLUE)
f.text((640,990),'Conditional later: Azure / MCP / authorized channel writes',29,True)
f.save('phase-two.png')
print('Generated six diagrams in',OUT)
