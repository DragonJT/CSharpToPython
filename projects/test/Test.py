#SUCCESS
class Object:
    def __init__(self,_color,_rect):
        self.color=_color
        self.rect=_rect
    def Update(self,surface):
        pygame.draw.rect(surface,self.color,pygame.Rect(self.rect[0],self.rect[1],self.rect[2],self.rect[3]))
class Program:
    def Main():
        objects.append(Object((255,0,0),(100,100,100,100)))
        objects.append(Object((0,0,255),(120,120,60,60)))
        objects.append(Object((100,255,0),(200,200,50,50)))
Program.Main()
