using pygame;
using subprocess;

class CodeEditor {
    Rect rect;
    string text;
    int cursor;
    string[] lines;
    Graphics graphics;

    CodeEditor(Rect _rect, string _text, Graphics _graphics){
        rect = _rect;
        SetText(_text);
        cursor = 0;
        graphics = _graphics;
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

    void CursorLeft(){
        if(cursor>0){
            cursor--;
        }
    }

    void CursorRight(){
        if(cursor<len(text)){
            cursor++;
        }    
    }

    void OnEvent(Event @event){
        if(@event.type == pygame.KEYDOWN){
            //var keys = pygame.key.get_pressed();
            /*if(keys[pygame.K_LCTRL]){
                if(@event.key == pygame.K_r){
                    File.WriteFile("projects/test/Test.cs", text);
                    var result = subprocess.run(["compiler\\bin\\Debug\\net8.0\\CSharpToPython.exe", "projects/test/Test.cs--projects/test/Test.py"], 
                        shell:true, capture_output:true, text:true).stdout;
                    if(result == "ERROR"){
                        print(File.ReadFile("projects/test/Test.py"));
                    }
                    else{
                        exec(File.ReadFile("projects/test/Test.py"), globals);
                    }
                }
                else if(@event.key == pygame.K_s){
                    File.WriteFile("projects/test/Test.cs", text);
                }
                else if(@event.key == pygame.K_TAB){
                    active++;
                    if(active >= len(uis)){
                        active = 0;
                    }
                }
            }else{*/
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
            //}
        }
    }

    void Draw(bool active){
        var y = rect[1];
        var x = rect[0];
        var bgcolor = active?(180,180,180):(200,200,200);
        graphics.DrawRect(rect, bgcolor);
        var textColor = (0,20,45);
        foreach(var l in lines){
            graphics.DrawText((x,y), l, textColor);
            y+=graphics.linesize;
        }

        if(active){
            var cursorYOffset = graphics.fontsize*0.1;
            y = rect[1];
            var numCharacters = 0;
            if(len(lines) == 0){
                graphics.DrawRect((x, y, 2, graphics.fontsize), textColor);
            }
            else{
                foreach(var l in lines){
                    var charsInLine = len(l);
                    if(cursor>=numCharacters && cursor<= numCharacters+charsInLine){
                        var cursorIsCharsFromStartOfLine = cursor - numCharacters;
                        var cursorX = graphics.font.size(l[0..cursorIsCharsFromStartOfLine])[0] + x;
                        graphics.DrawRect((cursorX, y+cursorYOffset, 2, graphics.fontsize), textColor);
                    }
                    numCharacters+=charsInLine+1;
                    y+=graphics.linesize;
                }
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

class TreeView{
    Rect rect;
    GameObject[] gameObjects;
    Graphics graphics;

    TreeView(Rect _rect, Graphics _graphics){
        gameObjects = [];
        rect = _rect;
        graphics = _graphics;
    }

    void Add(GameObject gameObject){
        gameObjects.append(gameObject);
    }

    void OnEvent(Event @event){
    }

    void Draw(bool active){
        var bgcolor = active?(180,180,180):(200,200,200);
        graphics.DrawRect(rect, bgcolor);
    }
}

class UIs{
    object[] uis;
    int active;

    UIs(){
        uis = [];
        active = 1;
    }

    void Add(object ui){
        uis.append(ui);
    }

    void OnEvent(Event @event){
        uis[active].OnEvent(@event);
    }

    void Draw(){
        foreach(var i in range(0, len(uis))){
            uis[i].Draw(i==active);
        }
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
    }

    void Clear(Color color){
        surface.fill(color);
    }

    void DrawText(Vector2 position, string text, Color color){
        var img = font.render(text, true, color);
        surface.blit(img, position);
    }

    void DrawRect(Rect rect, Color color){
        pygame.draw.rect(surface, color, pygame.Rect(rect[0], rect[1], rect[2], rect[3]));
    }
}

class Program {
    static void Main(){
        pygame.init();
        var graphics = new Graphics((1280,780), "RedditMono-Medium.ttf", 16);

        var uis = new UIs();
        uis.Add(new TreeView((20,20,280,740), graphics));
        uis.Add(new CodeEditor((320,20,900,740), File.ReadFile("projects/test/Test.cs"), graphics));
        var running = true;
        pygame.key.set_repeat(500,25);
        //var globals={"objects"=[], "pygame"=pygame};
        while(running){
            foreach(var @event in pygame.@event.@get()){
                /*if(@event.type == pygame.KEYDOWN && @event.key == pygame.K_ESCAPE){
                    globals={"objects"=[], "pygame"=pygame};
                }else*/
                if(@event.type == pygame.QUIT){
                    running = false;
                }else{
                    uis.OnEvent(@event);
                }
            }
            graphics.Clear((220, 220, 220));
            uis.Draw();
            /*foreach(var obj in globals["objects"]){
                obj.Update();
            }*/
            pygame.display.flip();
        }
        pygame.quit();
    }
}