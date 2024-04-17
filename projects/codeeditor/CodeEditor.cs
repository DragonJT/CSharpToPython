using pygame;
using subprocess;

class CodeEditor{
    string text;
    int cursor;
    int fontsize;
    int linesize;
    string[] lines;
    Font font;

    CodeEditor(){
        text = "";
        lines = [];
        cursor = 0;
        fontsize = 18;
        linesize = fontsize * 1.5;
        font = pygame.font.Font("RedditMono-Medium.ttf", fontsize);
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

    void Draw(Surface surface){
        surface.fill((220, 220, 220))
        var y=fontsize*0.5;
        var textColor = (0,20,45);
        foreach(var l in lines){
            var img = font.render(l, true, textColor);
            surface.blit(img, (fontsize, y));
            y+=linesize;
        }

        var cursorYOffset = fontsize*0.1;
        y = fontsize*0.5;
        var numCharacters = 0;
        if(len(lines) == 0){
            pygame.draw.rect(surface, textColor, pygame.Rect(fontsize, y, 2, fontsize));
        }
        else{
            foreach(var l in lines){
                var charsInLine = len(l);
                if(cursor>=numCharacters && cursor<= numCharacters+charsInLine){
                    var cursorIsCharsFromStartOfLine = cursor - numCharacters;
                    var cursorX = font.size(l[0..cursorIsCharsFromStartOfLine])[0] + fontsize;
                    pygame.draw.rect(surface, textColor, pygame.Rect(cursorX, y+cursorYOffset, 2, fontsize));
                }
                numCharacters+=charsInLine+1;
                y+=linesize;
            }
        }
        
    }
}

class Program{

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

    static void Main(){
        pygame.init();
        var surface = pygame.display.set_mode((1280, 780));
        var codeEditor = new CodeEditor();
        codeEditor.SetText(ReadFile("projects/test/Test.cs"));
        var running = true;
        pygame.key.set_repeat(500,25);
        var globals={"rects"=[]};
        while(running){
            var keys = pygame.key.get_pressed();
            foreach(var @event in pygame.@event.@get()){
                if(@event.type == pygame.KEYDOWN){
                    if(keys[pygame.K_LCTRL]){
                        if(@event.key == pygame.K_r){
                            WriteFile("projects/test/Test.cs", codeEditor.text);
                            var result = subprocess.run(["compiler\\bin\\Debug\\net8.0\\CSharpToPython.exe", "projects/test/Test.cs--projects/test/Test.py"], 
                                shell:true, capture_output:true, text:true).stdout;
                            if(result == "ERROR"){
                                print(ReadFile("projects/test/Test.py"));
                            }
                            else{
                                exec(ReadFile("projects/test/Test.py"), globals);
                            }
                        }
                        else if(@event.key == pygame.K_s){
                            WriteFile("projects/test/Test.cs", codeEditor.text);
                        }
                    }else{
                        if(@event.key == pygame.K_RETURN){
                            codeEditor.Insert("\n");
                        }else if(@event.key == pygame.K_TAB){
                            codeEditor.Insert("    ");
                        }else if(@event.key == pygame.K_BACKSPACE){
                            codeEditor.Backspace();
                        }else if(@event.key == pygame.K_LEFT){
                            codeEditor.CursorLeft();
                        }else if(@event.key == pygame.K_RIGHT){
                            codeEditor.CursorRight();
                        }else{
                            codeEditor.Insert(@event.unicode);
                        }
                    }
                }
                
                if(@event.type == pygame.QUIT){
                    running = false;
                }
            }
            codeEditor.Draw(surface);
            foreach(var r in globals["rects"]){
                pygame.draw.rect(surface, (0,0,255), pygame.Rect(r[0],r[1],r[2],r[3]));
            }
            pygame.display.flip();
        }
        pygame.quit();
    }
}