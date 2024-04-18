#SUCCESS
class ColoredRect:
    def __init__(self,_color,_rect):
        self.color=_color
        self.rect=_rect
    def Draw(self):
        graphics.DrawRect(self.rect,self.color)
class ColoredText:
    def __init__(self,_color,_position,_text):
        self.color=_color
        self.position=_position
        self.text=_text
    def Draw(self):
        graphics.DrawText(self.position,self.text,self.color)
class Program:
    def Main():
        graphics.AddObject(ColoredRect((255,0,0),(100,100,100,100)))
        graphics.AddObject(ColoredRect((0,0,255),(120,120,60,60)))
        graphics.AddObject(ColoredRect((100,255,0),(200,200,50,50)))
        graphics.AddObject(ColoredText((0,50,100),(150,150),"HelloWorld"))
Program.Main()
