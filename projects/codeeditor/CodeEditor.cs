using pygame;
using subprocess;

class CodeEditor {
    Rect rect;
    string text;
    int cursor;
    string[] lines;
    int scrollY;

    CodeEditor(Rect _rect, string _text){
        rect = _rect;
        SetText(_text);
        cursor = 0;
        scrollY = 0;
    }

    void SetText(string _text){
        text = _text;
        lines = text.split('\n');
    }

    void Insert(string value){
        SetText(text[..cursor] + value + text[cursor..]);
        cursor+=len(value);
    }

    void Backspace(){
        if(cursor>0){
            SetText(text[..(cursor-1)] + text[cursor..]);
            cursor--;
        }
    }

    void SetCursor(int _cursor){
        cursor = _cursor;
        var locationID = GetCursorLocationID();
        if(locationID[1]>scrollY+20){
            scrollY = locationID[1] - 20;
        }
        else if(locationID[1]<scrollY+10){
            scrollY = max(locationID[1] - 10, 0);
        }
    }

    void CursorLeft(){
        if(cursor>0){
            SetCursor(cursor-1);
        }
    }

    void CursorRight(){
        if(cursor<len(text)){
            SetCursor(cursor+1);
        }    
    }

    void Save(){
        File.WriteFile("projects/test/Test.cs", text);
    }

    void OnEvent(Graphics graphics, Event @event){
        if(@event.type == pygame.KEYDOWN){
            var keys = pygame.key.get_pressed();
            if(keys[pygame.K_LCTRL]){
                if(@event.key == pygame.K_TAB){
                    graphics.NextUI();
                }
            }else{
                if(@event.key == pygame.K_RETURN){
                    Insert("\n");
                }else if(@event.key == pygame.K_TAB){
                    Insert("    ");
                }else if(@event.key == pygame.K_BACKSPACE){
                    Backspace();
                }else if(@event.key == pygame.K_LEFT){
                    CursorLeft();
                }else if(@event.key == pygame.K_RIGHT){
                    CursorRight();
                }else{
                    Insert(@event.unicode);
                }
            }
        }
    }

    Vector2i GetCursorLocationID(){
        var lineID = 0;
        var numCharacters = 0;
        foreach(var line in lines){
            var charsInLine = len(line);
            if(cursor>=numCharacters && cursor<= numCharacters+charsInLine){
                var cursorIsCharsFromStartOfLine = cursor - numCharacters;
                return (cursorIsCharsFromStartOfLine, lineID);
            }
            numCharacters+=charsInLine+1;
            lineID++;
        }
    }

    void Draw(Graphics graphics, bool active){
        var y = rect[1];
        var x = rect[0];
        var bgcolor = active?(180,180,180):(200,200,200);
        graphics.DrawRect(rect, bgcolor);
        var textColor = (0,20,45);
        var numLines = len(lines);
        foreach(var i in range(scrollY, scrollY+30)){
            if(i>=0 && i<numLines){
                graphics.DrawText((x,y), lines[i], textColor);
                y+=graphics.linesize;
            }
        }

        if(active){
            var cursorYOffset = graphics.fontsize*0.1;
            y = rect[1] - scrollY * graphics.linesize;
            if(len(lines) == 0){
                graphics.DrawRect((x, y, 2, graphics.fontsize), textColor);
            }
            else{
                var locationID = GetCursorLocationID();
                var cursorX = graphics.font.size(lines[locationID[1]][0..locationID[0]])[0] + x;
                var cursorY = graphics.linesize * locationID[1] + y;
                graphics.DrawRect((cursorX, cursorY, 2, graphics.fontsize), textColor);
            }
        }
    }
}

class GameObject{
    string name;
    object[] components;

    GameObject(string _name){
        name = _name;
        components = [];
    }
}

class Button{
    Rect rect;
    string text;
    Action onclick;

    Button(Rect _rect, string _text, Action _onclick){
        rect = _rect;
        text = _text;
        onclick = _onclick;
    }

    void Save(){}

    void OnEvent(Graphics graphics, Event @event){
        if(@event.type == pygame.KEYDOWN){
            if(@event.key == pygame.K_RETURN){
                onclick();
            }else if(@event.key == pygame.K_TAB){
                graphics.NextUI();
            }
        }
    }

    void Draw(Graphics graphics, bool active){
        var bgcolor = active?(180,180,180):(200,200,200);
        graphics.DrawRect(rect, bgcolor);
        var textColor = (0,50,100);
        graphics.DrawText((rect[0],rect[1]), text, textColor);
    }
}

class TreeView{
    Rect rect;
    GameObject[] gameObjects;

    TreeView(Rect _rect){
        gameObjects = [];
        rect = _rect;
    }

    void Save(){}

    void Add(GameObject gameObject){
        gameObjects.append(gameObject);
    }

    void OnEvent(Graphics graphics, Event @event){
        if(@event.type == pygame.KEYDOWN){
            if(@event.key == pygame.K_TAB){
                graphics.NextUI();
            }
        }
    }

    void Draw(Graphics graphics, bool active){
        var bgcolor = active?(180,180,180):(200,200,200);
        graphics.DrawRect(rect, bgcolor);
        graphics.DrawText((rect[0],rect[1]), "TreeView", (0,50,100));
    }
}

class File{
    static string ReadFile(string path){
        var f = open(path, 'r');
        var content = f.read();
        f.close();
        return content;
    }

    static void WriteFile(string path, string content){
        var f = open(path, 'w');
        f.write(content);
        f.close();
    }
}

class Graphics{
    object[] objects;
    object[] uis;
    int activeUI;
    Surface surface;
    Vector2 screenSize;
    Font font;
    float fontsize;
    float linesize;

    Graphics(Vector2 _screenSize, string _font, int _fontsize){
        surface = pygame.display.set_mode((_screenSize[0], _screenSize[1]));
        font = pygame.font.Font(_font, _fontsize);
        fontsize = _fontsize;
        linesize = fontsize * 1.4;
        screenSize = _screenSize;
        uis = [];
        objects = [];
        activeUI = 0;
    }

    void Clear(Color color){
        surface.fill(color);
    }

    void Save(){
        foreach(var u in uis){
            u.Save();
        }
    }

    void DrawText(Vector2 position, string text, Color color){
        var img = font.render(text, true, color);
        surface.blit(img, position);
    }

    void DrawRect(Rect rect, Color color){
        pygame.draw.rect(surface, color, pygame.Rect(rect[0], rect[1], rect[2], rect[3]));
    }

    void AddObject(object obj){
        objects.append(obj);
    }

    void DrawObjects(){
        foreach(var obj in objects){
            obj.Draw();
        }
    }

    void ClearObjects(){
        objects.clear();
    }

    void AddUI(object ui){
        uis.append(ui);
    }

    void OnEvent(Event @event){
        uis[activeUI].OnEvent(this, @event);
    }

    void DrawUI(){
        foreach(var i in range(0, len(uis))){
            uis[i].Draw(this, i==activeUI);
        }
    }

    void NextUI(){
        activeUI++;
        if(activeUI >= len(uis)){
            activeUI = 0;
        }
    }
}

class Program {
    Graphics graphics;

    void Save(){
        graphics.Save();
    }

    void Run(){
        graphics.Save();
        var result = subprocess.run(["compiler\\bin\\Debug\\net8.0\\CSharpToPython.exe", "projects/test/Test.cs--projects/test/Test.py"], 
            shell:true, capture_output:true, text:true).stdout;
        if(result == "ERROR"){
            print(File.ReadFile("projects/test/Test.py"));
        }
        else{
            var globals = {"graphics"=graphics};
            exec(File.ReadFile("projects/test/Test.py"), globals);
        }
    }

    void Start(){
        pygame.init();
        graphics = new Graphics((1280,780), "RedditMono-Medium.ttf", 16);
        graphics.AddUI(new TreeView((20,80,280,680)));
        graphics.AddUI(new CodeEditor((320,80,900,680), File.ReadFile("projects/test/Test.cs")));
        graphics.AddUI(new Button((500,20,100,40), ">", Run));
        graphics.AddUI(new Button((780,20,100,40), "save", Save));

        var running = true;
        pygame.key.set_repeat(500,25);
        while(running){
            foreach(var @event in pygame.@event.@get()){
                if(@event.type == pygame.KEYDOWN && @event.key == pygame.K_ESCAPE){
                    graphics.ClearObjects();
                }else if(@event.type == pygame.QUIT){
                    running = false;
                }else{
                    graphics.OnEvent(@event);
                }
            }
            graphics.Clear((220, 220, 220));
            graphics.DrawUI();
            graphics.DrawObjects();
            pygame.display.flip();
        }
        pygame.quit();
    }

    static void Main(){
        var program = new Program();
        program.Start();
    }
}