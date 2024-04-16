using pygame;

class Program{
    static void Main(){
        pygame.init();
        var screen = pygame.display.set_mode((1280, 780));
        var clock = pygame.time.Clock()
        var running = true;
        var player_pos = pygame.Vector2(screen.get_width() / 2, screen.get_height() / 2);
        var dt = 0;
        while(running){
            foreach(var @event in pygame.@event.@get()){
                if(@event.type == pygame.QUIT){
                    running = false;
                }
            }
            screen.fill("purple");
            pygame.draw.circle(screen, "red", player_pos, 40)

            keys = pygame.key.get_pressed()
            if (keys[pygame.K_w])
                player_pos.y -= 300 * dt
            if (keys[pygame.K_s])
                player_pos.y += 300 * dt
            if (keys[pygame.K_a])
                player_pos.x -= 300 * dt
            if (keys[pygame.K_d])
                player_pos.x += 300 * dt

            pygame.display.flip();
            dt = clock.tick(60) / 1000
        }
        pygame.quit();
    }
}