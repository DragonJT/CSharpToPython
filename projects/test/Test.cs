class Rect{
    float x;
    float y;
    float width;
    float height;

    Rect(object rect){
        x=rect[0];
        y=rect[1];
        width=rect[2];
        height=rect[3];
    }
    
    object ToTuple(){
        return (x,y,width,height);
    }
}

class ColoredRect{
    Color color;
    float speed;
    Rect rect;

    ColoredRect(Color _color, Rect _rect, float _speed){
        color = _color;
        rect = new Rect(_rect);
        speed = _speed;
    }

    void Draw(){
        rect.y += speed;
        if(rect.y > 500){
            rect.y = 0;
        }
        graphics.DrawRect(rect.ToTuple(), color);
    }
}

class ColoredText{
    Color color;
    Vector2 position;
    string text;

    ColoredText(Color _color, Vector2 _position, string _text){
        color = _color;
        position = _position;
        text = _text;
    }

    void Draw(){
        graphics.DrawText2(position, text, color);
    }
}

class Program{
    static void Main(){
        graphics.AddObject(new ColoredRect((255,0,0), (100,100,100,100), 0.5));
        graphics.AddObject(new ColoredRect((0,0,255), (120,120,60,60), 0.25));
        graphics.AddObject(new ColoredRect((100,255,0), (200,200,50,50), 0.3));
        graphics.AddObject(new ColoredText((255,255,255), (150,150), "HelloWorld"));
        graphics.AddObject(new ColoredText((255,255,255), (200,200), "MOOO"));
        graphics.AddObject(new ColoredText((255,0,0), (100,200), "BOO"));
    }
}