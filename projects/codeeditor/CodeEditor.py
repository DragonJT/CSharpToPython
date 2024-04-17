#SUCCESS
import pygame
import subprocess
class CodeEditor:
    def __init__(self):
        self.text=""
        self.lines=[]
        self.cursor=0
        self.fontsize=18
        self.linesize=self.fontsize*1.5
        self.font=pygame.font.Font("RedditMono-Medium.ttf",self.fontsize)
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
    def Draw(self,surface):
        surface.fill((220,220,220))
        y=self.fontsize*0.5
        textColor=(0,20,45)
        for l in self.lines:
            img=self.font.render(l,True,textColor)
            surface.blit(img,(self.fontsize,y))
            y+=self.linesize
        cursorYOffset=self.fontsize*0.1
        y=self.fontsize*0.5
        numCharacters=0
        if len(self.lines)==0:
            pygame.draw.rect(surface,textColor,pygame.Rect(self.fontsize,y,2,self.fontsize))
        else:
            for l in self.lines:
                charsInLine=len(l)
                if self.cursor>=numCharacters and self.cursor<=numCharacters+charsInLine:
                    cursorIsCharsFromStartOfLine=self.cursor-numCharacters
                    cursorX=self.font.size(l[0:cursorIsCharsFromStartOfLine])[0]+self.fontsize
                    pygame.draw.rect(surface,textColor,pygame.Rect(cursorX,y+cursorYOffset,2,self.fontsize))
                numCharacters+=charsInLine+1
                y+=self.linesize
class Program:
    def ReadFile(path):
        f=open(path,'r')
        content=f.read()
        f.close()
        return content
    def WriteFile(path,content):
        f=open(path,'w')
        f.write(content)
        f.close()
    def Main():
        pygame.init()
        surface=pygame.display.set_mode((1280,780))
        codeEditor=CodeEditor()
        codeEditor.SetText(Program.ReadFile("projects/test/Test.cs"))
        running=True
        pygame.key.set_repeat(500,25)
        globals={"rects":[]}
        while running:
            keys=pygame.key.get_pressed()
            for event in pygame.event.get():
                if event.type==pygame.KEYDOWN:
                    if keys[pygame.K_LCTRL]:
                        if event.key==pygame.K_r:
                            Program.WriteFile("projects/test/Test.cs",codeEditor.text)
                            result=subprocess.run(["compiler\\bin\\Debug\\net8.0\\CSharpToPython.exe","projects/test/Test.cs--projects/test/Test.py"],shell=True,capture_output=True,text=True).stdout
                            if result=="ERROR":
                                print(Program.ReadFile("projects/test/Test.py"))
                            else:
                                exec(Program.ReadFile("projects/test/Test.py"),globals)
                        elif event.key==pygame.K_s:
                            Program.WriteFile("projects/test/Test.cs",codeEditor.text)
                    else:
                        if event.key==pygame.K_RETURN:
                            codeEditor.Insert("\n")
                        elif event.key==pygame.K_TAB:
                            codeEditor.Insert("    ")
                        elif event.key==pygame.K_BACKSPACE:
                            codeEditor.Backspace()
                        elif event.key==pygame.K_LEFT:
                            codeEditor.CursorLeft()
                        elif event.key==pygame.K_RIGHT:
                            codeEditor.CursorRight()
                        else:
                            codeEditor.Insert(event.unicode)
                if event.type==pygame.QUIT:
                    running=False
            codeEditor.Draw(surface)
            for r in globals["rects"]:
                pygame.draw.rect(surface,(0,0,255),pygame.Rect(r[0],r[1],r[2],r[3]))
            pygame.display.flip()
        pygame.quit()
Program.Main()
