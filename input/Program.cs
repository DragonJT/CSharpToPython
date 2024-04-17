using pygame;

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

    void Insert(string value){
        text = text[..cursor] + value + text[cursor..];
        lines = text.split('\n');
        cursor+=len(value);
    }

    void Backspace(){
        if(cursor>0){
            text = text[..(cursor-1)] + text[cursor..];
            lines = text.split('\n');
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
    static void Main(){
        pygame.init();
        var surface = pygame.display.set_mode((1280, 780));
        var codeEditor = new CodeEditor();
        var running = true;
        pygame.key.set_repeat(500,25);
        
        while(running){
            foreach(var @event in pygame.@event.@get()){
                if(@event.type == pygame.KEYDOWN){
                    if(@event.key == pygame.K_RETURN){
                        codeEditor.Insert("\n");
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
                if(@event.type == pygame.QUIT){
                    running = false;
                }
            }
            codeEditor.Draw(surface);
            pygame.display.flip();
        }
        pygame.quit();
    }
}