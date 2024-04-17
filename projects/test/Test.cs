class Object{
    Color color;
    Rect rect;

    Object(Color _color, Rect _rect){
        color = _color;
        rect = _rect;
    }

    void Update(Surface surface){
        pygame.draw.rect(surface,color,pygame.Rect(rect[0],rect[1],rect[2],rect[3]));
    }
}

class Program{
    static void Main(){
        objects.append(new Object((255,0,0), (100,100,100,100)));
        objects.append(new Object((0,0,255), (120,120,60,60)));
        objects.append(new Object((100,255,0), (200,200,50,50));
    }
}