#SUCCESS
import pygame
import subprocess
class CodeEditor:
    def __init__(self,_rect,_text):
        self.rect=_rect
        self.SetText(_text)
        self.cursor=0
        self.scrollY=0
        self.cursorX=0
    def SetText(self,_text):
        self.text=_text
        self.lines=self.text.split('\n')
    def Insert(self,value):
        self.SetText(self.text[:self.cursor]+value+self.text[self.cursor:])
        self.cursor+=len(value)
    def Backspace(self):
        if self.cursor>0:
            self.SetText(self.text[:(self.cursor-1)]+self.text[self.cursor:])
            self.cursor-=1
    def SetCursor(self,_cursor,updateCursorX=True):
        self.cursor=_cursor
        locationID=self.GetCursorLocationID()
        if updateCursorX:
            self.cursorX=locationID[0]
        if locationID[1]>self.scrollY+20:
            self.scrollY=locationID[1]-20
        elif locationID[1]<self.scrollY+10:
            self.scrollY=max(locationID[1]-10,0)
    def SetCursorLocationID(self,locationID):
        print(locationID)
        _cursor=0
        for i in range(0,locationID[1]):
            _cursor+=len(self.lines[i])+1
        _cursor+=min(locationID[0],len(self.lines[locationID[1]]))
        self.SetCursor(_cursor,False)
    def CursorUp(self):
        cursorLine=self.GetCursorLocationID()[1]
        if cursorLine>0:
            self.SetCursorLocationID((self.cursorX,cursorLine-1))
    def CursorDown(self):
        cursorLine=self.GetCursorLocationID()[1]
        if cursorLine<len(self.lines)-1:
            self.SetCursorLocationID((self.cursorX,cursorLine+1))
    def CursorLeft(self):
        if self.cursor>0:
            self.SetCursor(self.cursor-1)
    def CursorRight(self):
        if self.cursor<len(self.text):
            self.SetCursor(self.cursor+1)
    def Save(self):
        File.WriteFile("projects/test/Test.cs",self.text)
    def OnEvent(self,graphics,event):
        if event.type==pygame.KEYDOWN:
            keys=pygame.key.get_pressed()
            if keys[pygame.K_LCTRL]:
                if event.key==pygame.K_TAB:
                    graphics.NextUI()
            else:
                if event.key==pygame.K_RETURN:
                    self.Insert("\n")
                elif event.key==pygame.K_TAB:
                    self.Insert("    ")
                elif event.key==pygame.K_BACKSPACE:
                    self.Backspace()
                elif event.key==pygame.K_UP:
                    self.CursorUp()
                elif event.key==pygame.K_DOWN:
                    self.CursorDown()
                elif event.key==pygame.K_LEFT:
                    self.CursorLeft()
                elif event.key==pygame.K_RIGHT:
                    self.CursorRight()
                else:
                    self.Insert(event.unicode)
    def GetCursorLocationID(self):
        lineID=0
        numCharacters=0
        for line in self.lines:
            charsInLine=len(line)
            if self.cursor>=numCharacters and self.cursor<=numCharacters+charsInLine:
                cursorIsCharsFromStartOfLine=self.cursor-numCharacters
                return (cursorIsCharsFromStartOfLine,lineID)
            numCharacters+=charsInLine+1
            lineID+=1
    def Draw(self,graphics,active):
        y=self.rect[1]
        x=self.rect[0]
        bgcolor=(180,180,180) if active else (200,200,200)
        graphics.DrawRect(self.rect,bgcolor)
        textColor=(0,20,45)
        numLines=len(self.lines)
        for i in range(self.scrollY,self.scrollY+30):
            if i>=0 and i<numLines:
                graphics.DrawText((x,y),self.lines[i],textColor)
                y+=graphics.linesize
        if active:
            cursorYOffset=graphics.fontsize*0.1
            y=self.rect[1]-self.scrollY*graphics.linesize
            if len(self.lines)==0:
                graphics.DrawRect((x,y,2,graphics.fontsize),textColor)
            else:
                locationID=self.GetCursorLocationID()
                _cursorX=graphics.font.size(self.lines[locationID[1]][0:locationID[0]])[0]+x
                cursorY=graphics.linesize*locationID[1]+y
                graphics.DrawRect((_cursorX,cursorY,2,graphics.fontsize),textColor)
class GameObject:
    def __init__(self,_name):
        self.name=_name
        self.components=[]
class Button:
    def __init__(self,_rect,_text,_onclick):
        self.rect=_rect
        self.text=_text
        self.onclick=_onclick
    def Save(self):
        pass
    def OnEvent(self,graphics,event):
        if event.type==pygame.KEYDOWN:
            if event.key==pygame.K_RETURN:
                self.onclick()
            elif event.key==pygame.K_TAB:
                graphics.NextUI()
    def Draw(self,graphics,active):
        bgcolor=(180,180,180) if active else (200,200,200)
        graphics.DrawRect(self.rect,bgcolor)
        textColor=(0,50,100)
        graphics.DrawText((self.rect[0],self.rect[1]),self.text,textColor)
class TreeView:
    def __init__(self,_rect):
        self.gameObjects=[]
        self.rect=_rect
    def Save(self):
        pass
    def Add(self,gameObject):
        self.gameObjects.append(gameObject)
    def OnEvent(self,graphics,event):
        if event.type==pygame.KEYDOWN:
            if event.key==pygame.K_TAB:
                graphics.NextUI()
    def Draw(self,graphics,active):
        bgcolor=(180,180,180) if active else (200,200,200)
        graphics.DrawRect(self.rect,bgcolor)
        graphics.DrawText((self.rect[0],self.rect[1]),"TreeView",(0,50,100))
class File:
    def ReadFile(path):
        f=open(path,'r')
        content=f.read()
        f.close()
        return content
    def WriteFile(path,content):
        f=open(path,'w')
        f.write(content)
        f.close()
class Graphics:
    def __init__(self,_screenSize,_font,_fontsize):
        self.surface=pygame.display.set_mode((_screenSize[0],_screenSize[1]))
        self.font=pygame.font.Font(_font,_fontsize)
        self.fontsize=_fontsize
        self.linesize=self.fontsize*1.4
        self.screenSize=_screenSize
        self.uis=[]
        self.objects=[]
        self.activeUI=0
    def Clear(self,color):
        self.surface.fill(color)
    def Save(self):
        for u in self.uis:
            u.Save()
    def DrawText(self,position,text,color):
        img=self.font.render(text,True,color)
        self.surface.blit(img,position)
    def DrawRect(self,rect,color):
        pygame.draw.rect(self.surface,color,pygame.Rect(rect[0],rect[1],rect[2],rect[3]))
    def AddObject(self,obj):
        self.objects.append(obj)
    def DrawObjects(self):
        for obj in self.objects:
            obj.Draw()
    def ClearObjects(self):
        self.objects.clear()
    def AddUI(self,ui):
        self.uis.append(ui)
    def OnEvent(self,event):
        self.uis[self.activeUI].OnEvent(self,event)
    def DrawUI(self):
        for i in range(0,len(self.uis)):
            self.uis[i].Draw(self,i==self.activeUI)
    def NextUI(self):
        self.activeUI+=1
        if self.activeUI>=len(self.uis):
            self.activeUI=0
class Program:
    def Save(self):
        self.graphics.Save()
    def Run(self):
        self.graphics.Save()
        result=subprocess.run(["compiler\\bin\\Debug\\net8.0\\CSharpToPython.exe","projects/test/Test.cs--projects/test/Test.py"],shell=True,capture_output=True,text=True).stdout
        if result=="ERROR":
            print(File.ReadFile("projects/test/Test.py"))
        else:
            globals={"graphics":self.graphics}
            exec(File.ReadFile("projects/test/Test.py"),globals)
    def Start(self):
        pygame.init()
        self.graphics=Graphics((1280,780),"RedditMono-Medium.ttf",16)
        self.graphics.AddUI(TreeView((20,80,280,680)))
        self.graphics.AddUI(CodeEditor((320,80,900,680),File.ReadFile("projects/test/Test.cs")))
        self.graphics.AddUI(Button((500,20,100,40),">",self.Run))
        self.graphics.AddUI(Button((780,20,100,40),"save",self.Save))
        running=True
        pygame.key.set_repeat(500,25)
        while running:
            for event in pygame.event.get():
                if event.type==pygame.KEYDOWN and event.key==pygame.K_ESCAPE:
                    self.graphics.ClearObjects()
                elif event.type==pygame.QUIT:
                    running=False
                else:
                    self.graphics.OnEvent(event)
            self.graphics.Clear((220,220,220))
            self.graphics.DrawUI()
            self.graphics.DrawObjects()
            pygame.display.flip()
        pygame.quit()
    def Main():
        program=Program()
        program.Start()
Program.Main()
