import pygame, os, sys

fname = "".join(sys.argv[1:])
if fname == "":
    sys.exit()
cwd = os.path.dirname(os.path.realpath(__file__))
f = open(cwd + fname)

# load data

f.close()

# pygame setup
pygame.init()
pygame.font.init()
screen = pygame.display.set_mode((window_w, window_h))
pygame.display.set_caption('Hashcode 2019')
font = pygame.font.SysFont("monospace", 18)
screen.fill((0,0,0))

plate = screen.subsurface((offset, offset, window_w - offset, window_h - offset))

# render
while not terminate:
    for event in pygame.event.get():
        if event.type == pygame.QUIT: terminate = True

    textsurface = font.render(str(pie), False, (0, 0, 0))
    screen.blit(textsurface,(0, 0))

    pygame.display.flip()
    pygame.time.wait(100)
