import pygame
class CodeEditor:
    def __init__(self):
        self.text=""
        self.lines=[]
        self.cursor=0
        self.fontsize=18
        self.linesize=self.fontsize*1.5
        self.font=pygame.font.Font("RedditMono-Medium.ttf",self.fontsize)
    def Insert(self,value):
        self.text=self.text[:self.cursor]+value+self.text[self.cursor:]
        self.lines=self.text.split('\n')
        self.cursor+=len(value)
    def Backspace(self):
        if self.cursor>0:
            self.text=self.text[:(self.cursor-1)]+self.text[self.cursor:]
            self.lines=self.text.split('\n')
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
    def Main():
        pygame.init()
        surface=pygame.display.set_mode((1280,780))
        codeEditor=CodeEditor()
        running=True
        pygame.key.set_repeat(500,25)
        while running:
            for event in pygame.event.get():
                if event.type==pygame.KEYDOWN:
                    if event.key==pygame.K_RETURN:
                        codeEditor.Insert("\n")
                    elif event.key==pygame.K_BACKSPACE:
                        if event.key==pygame.K_BACKSPACE:
                            codeEditor.Backspace()
                        elif event.key==pygame.K_LEFT:
                            if event.key==pygame.K_LEFT:
                                codeEditor.CursorLeft()
                            elif event.key==pygame.K_RIGHT:
                                if event.key==pygame.K_RIGHT:
                                    codeEditor.CursorRight()
                                else:
                                    codeEditor.Insert(event.unicode)
                            else:
                                codeEditor.Insert(event.unicode)
                        elif event.key==pygame.K_RIGHT:
                            if event.key==pygame.K_RIGHT:
                                codeEditor.CursorRight()
                            else:
                                codeEditor.Insert(event.unicode)
                        else:
                            codeEditor.Insert(event.unicode)
                    elif event.key==pygame.K_LEFT:
                        if event.key==pygame.K_LEFT:
                            codeEditor.CursorLeft()
                        elif event.key==pygame.K_RIGHT:
                            if event.key==pygame.K_RIGHT:
                                codeEditor.CursorRight()
                            else:
                                codeEditor.Insert(event.unicode)
                        else:
                            codeEditor.Insert(event.unicode)
                    elif event.key==pygame.K_RIGHT:
                        if event.key==pygame.K_RIGHT:
                            codeEditor.CursorRight()
                        else:
                            codeEditor.Insert(event.unicode)
                    else:
                        codeEditor.Insert(event.unicode)
                if event.type==pygame.QUIT:
                    running=False
            codeEditor.Draw(surface)
            pygame.display.flip()
        pygame.quit()
Program.Main()
