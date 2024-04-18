#SUCCESS
class Rect:
    def __init__(self,rect):
        self.x=rect[0]
        self.y=rect[1]
        self.width=rect[2]
        self.height=rect[3]
    def ToTuple(self):
        return (self.x,self.y,self.width,self.height)
class ColoredRect:
    def __init__(self,_color,_rect,_speed):
        self.color=_color
        self.rect=Rect(_rect)
        self.speed=_speed
    def Draw(self):
        self.rect.y+=self.speed
        if self.rect.y>500:
            self.rect.y=0
        graphics.DrawRect(self.rect.ToTuple(),self.color)
class ColoredText:
    def __init__(self,_color,_position,_text):
        self.color=_color
        self.position=_position
        self.text=_text
    def Draw(self):
        graphics.DrawText2(self.position,self.text,self.color)
class Program:
    def Main():
        graphics.AddObject(ColoredRect((255,0,0),(100,100,100,100),0.5))
        graphics.AddObject(ColoredRect((0,0,255),(120,120,60,60),0.25))
        graphics.AddObject(ColoredRect((100,255,0),(200,200,50,50),0.3))
        graphics.AddObject(ColoredText((255,255,255),(150,150),"HelloWorld"))
        graphics.AddObject(ColoredText((255,255,255),(200,200),"MOOO"))
        graphics.AddObject(ColoredText((255,0,0),(100,200),"BOO"))
Program.Main()
