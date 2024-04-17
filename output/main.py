import pygame
def Main():
    pygame.init()
    screen=pygame.display.set_mode((1280,780))
    running=True
    text=""
    fontsize=25
    font=pygame.font.SysFont(None,fontsize)
    while running:
        for event in pygame.event.get():
            if event.type==pygame.KEYDOWN:
                if event.key==pygame.K_RETURN:
                    text+='\n'
                elif event.key==pygame.K_BACKSPACE:
                    if event.key==pygame.K_BACKSPACE:
                        text=text[:-1]
                    else:
                        text+=event.unicode
                else:
                    text+=event.unicode
            if event.type==pygame.QUIT:
                running=False
        screen.fill((220,220,220))
        y=fontsize
        for l in text.split('\n'):
            img=font.render(l,True,(0,20,45))
            screen.blit(img,(20,y))
            y+=fontsize
        pygame.display.flip()
    pygame.quit()
Main()
