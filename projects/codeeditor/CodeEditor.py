#SUCCESS
import pygame
import subprocess
class CodeEditor:
    def __init__(self,_rect,_text,_graphics):
        self.rect=_rect
        self.SetText(_text)
        self.cursor=0
        self.graphics=_graphics
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
    def CursorLeft(self):
        if self.cursor>0:
            self.cursor-=1
    def CursorRight(self):
        if self.cursor<len(self.text):
            self.cursor+=1
    def OnEvent(self,event):
        if event.type==pygame.KEYDOWN:
            if event.key==pygame.K_RETURN:
                self.Insert("\n")
            elif event.key==pygame.K_TAB:
                self.Insert("    ")
            elif event.key==pygame.K_BACKSPACE:
                self.Backspace()
            elif event.key==pygame.K_LEFT:
                self.CursorLeft()
            elif event.key==pygame.K_RIGHT:
                self.CursorRight()
            else:
                self.Insert(event.unicode)
    def Draw(self,active):
        y=self.rect[1]
        x=self.rect[0]
        bgcolor=(180,180,180) if active else (200,200,200)
        self.graphics.DrawRect(self.rect,bgcolor)
        textColor=(0,20,45)
        for l in self.lines:
            self.graphics.DrawText((x,y),l,textColor)
            y+=self.graphics.linesize
        if active:
            cursorYOffset=self.graphics.fontsize*0.1
            y=self.rect[1]
            numCharacters=0
            if len(self.lines)==0:
                self.graphics.DrawRect((x,y,2,self.graphics.fontsize),textColor)
            else:
                for l in self.lines:
                    charsInLine=len(l)
                    if self.cursor>=numCharacters and self.cursor<=numCharacters+charsInLine:
                        cursorIsCharsFromStartOfLine=self.cursor-numCharacters
                        cursorX=self.graphics.font.size(l[0:cursorIsCharsFromStartOfLine])[0]+x
                        self.graphics.DrawRect((cursorX,y+cursorYOffset,2,self.graphics.fontsize),textColor)
                    numCharacters+=charsInLine+1
                    y+=self.graphics.linesize
class GameObject:
    def __init__(self,_name):
        self.name=_name
        self.components=[]
class TreeView:
    def __init__(self,_rect,_graphics):
        self.gameObjects=[]
        self.rect=_rect
        self.graphics=_graphics
    def Add(self,gameObject):
        self.gameObjects.append(gameObject)
    def OnEvent(self,event):
        pass
    def Draw(self,active):
        bgcolor=(180,180,180) if active else (200,200,200)
        self.graphics.DrawRect(self.rect,bgcolor)
class UIs:
    def __init__(self):
        self.uis=[]
        self.active=1
    def Add(self,ui):
        self.uis.append(ui)
    def OnEvent(self,event):
        self.uis[self.active].OnEvent(event)
    def Draw(self):
        for i in range(0,len(self.uis)):
            self.uis[i].Draw(i==self.active)
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
    def Clear(self,color):
        self.surface.fill(color)
    def DrawText(self,position,text,color):
        img=self.font.render(text,True,color)
        self.surface.blit(img,position)
    def DrawRect(self,rect,color):
        pygame.draw.rect(self.surface,color,pygame.Rect(rect[0],rect[1],rect[2],rect[3]))
class Program:
    def Main():
        pygame.init()
        graphics=Graphics((1280,780),"RedditMono-Medium.ttf",16)
        uis=UIs()
        uis.Add(TreeView((20,20,280,740),graphics))
        uis.Add(CodeEditor((320,20,900,740),File.ReadFile("projects/test/Test.cs"),graphics))
        running=True
        pygame.key.set_repeat(500,25)
        while running:
            for event in pygame.event.get():
                if event.type==pygame.QUIT:
                    running=False
                else:
                    uis.OnEvent(event)
            graphics.Clear((220,220,220))
            uis.Draw()
            pygame.display.flip()
        pygame.quit()
Program.Main()
