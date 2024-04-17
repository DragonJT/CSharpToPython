using pygame;

class Program{
    static void Main(){
        pygame.init();
        var screen = pygame.display.set_mode((1280, 780));

        var running = true;
        var text = "";
        var fontsize = 18;
        var linesize = fontsize*1.5;
        var font = pygame.font.Font("RedditMono-Medium.ttf", fontsize)
        pygame.key.set_repeat(500,25);

        while(running){
            foreach(var @event in pygame.@event.@get()){
                if(@event.type == pygame.KEYDOWN){
                    if(@event.key == pygame.K_RETURN){
                        text+='\n';
                    }else if(@event.key == pygame.K_BACKSPACE){
                        text = text[..^1];
                    }else{
                        text += @event.unicode;
                    }
                }
                if(@event.type == pygame.QUIT){
                    running = false;
                }
            }
            screen.fill((220, 220, 220))
            var y=fontsize*0.5;
            foreach(var l in text.split('\n')){
                var img = font.render(l, true, (0,20,45));
                screen.blit(img, (20, y));
                y+=linesize;
            }
            pygame.display.flip();
        }
        pygame.quit();
    }
}