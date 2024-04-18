class ColoredRect{
    Color color;
    Rect rect;

    ColoredRect(Color _color, Rect _rect){
        color = _color;
        rect = _rect;
    }

    void Draw(){
        graphics.DrawRect(rect, color);
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
        graphics.DrawText(position, text, color);
    }
}

class Program{
    static void Main(){
        graphics.AddObject(new ColoredRect((255,0,0), (100,100,100,100)));
        graphics.AddObject(new ColoredRect((0,0,255), (120,120,60,60)));
        graphics.AddObject(new ColoredRect((100,255,0), (200,200,50,50));
        graphics.AddObject(new ColoredText((0,50,100), (150,150), "HelloWorld");
    }
}