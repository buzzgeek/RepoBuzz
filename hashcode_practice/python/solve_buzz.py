"""
Hashcode Problem 1

0 -> M
1 -> T
"""

import os, pygame, argparse
from sys import stdin
import time

# you will need to install orderedset --> 'pip install orderedset'
from orderedset import OrderedSet

parser = argparse.ArgumentParser(description='Slice the pizza.')
parser.add_argument('--index', type=int, help='Select the pizza index', default=2)
parser.add_argument('--only_lines', type=bool, help='Reduce the shapes to lines', default=False)
parser.add_argument('--show_progress', type=bool, help='Reduce the shapes to lines', default=True)
parser.add_argument('--show_possible', type=bool, help='Reduce the shapes to lines', default=False)

args = parser.parse_args()

pizza_index = args.index

cwd = os.path.dirname(os.path.realpath(__file__))
files = ["a_example.in", "b_small.in", "c_medium.in", "d_big.in"]
filename = files[pizza_index]
ifile = cwd + "/data/" + filename

dirs = [[-1,0],[0,-1],[1,0],[0,1]]

bestscore = 0
bestslices = []

### read input

f = open(ifile, "r")

info = [int(v) for v in f.readline().split(" ")]
rows = info[0]
cols = info[1]
mintps = info[2]
maxtiles = info[3]

set_indicies = OrderedSet()
for r in range(rows):
    for c in range(cols):
        set_indicies.add('{0:04d} {1:04d}'.format(c, r))

print(set_indicies)

shapes = list()
if not args.only_lines:
    for l in range(2 * mintps, maxtiles+1):
        for i in range(1, l):
            if l % i == 0:
                shapes.append((i, int(l/i)))
                shapes.append((int(l/i), i))
else:
    for l in range(2 * mintps, maxtiles+1):
        shapes.append((1, l))
        shapes.append((l, 1))

shapes.reverse()

shape_max_vertical = (1, maxtiles)
shape_max_horizontal = (maxtiles, 1)
shape_max_square = (maxtiles//2, maxtiles//2)

print(shape_max_vertical)
print(shape_max_horizontal)
print(shape_max_square)

grid = []
for i in range(rows):
    grid.append([0 if c == "M" else 1 for c in f.readline().replace("\n","")])

grid_copy = [l[:] for l in grid]
result = [l[:] for l in grid]

possible = []
for l in grid:
    possible.append([None for idx in l])

f.close()


### solve

tomato = 1
mushroom = 0

slices = []
val_slices = []

print("max_score: " + str(rows * cols))
print("min toppings: " + str(mintps))
print("max tiles: " + str(maxtiles))
print('evalutating')

def visualize():
    maxwidth = 1000
    maxheight = 600
    margin = 5
    colsratio = (maxwidth - 2 * margin)/cols
    rowsratio = (maxheight - 2 * margin)/rows
    tilesize = int(colsratio if rowsratio > colsratio else rowsratio)
    tilesize = 2 if tilesize <= 1 else tilesize
    width = tilesize * cols + 2 * margin
    height = tilesize * rows + 2 * margin

    mushroom_color = (0, 0, 255)
    tomato_color = (255, 0, 0)
    empty_color = (120,120,120)

    pygame.init()
    pygame.display.set_caption('Google Hashcode - Pizza Slicer')

    screen = pygame.display.set_mode((width, height))
    screen.fill((0,0,0))
    plate = screen.subsurface((margin, margin, width - margin, height - margin))

    for y in range(rows):
        for x in range(cols):
            v = result_at(x,y)
            if v == None:
                tile_color = empty_color
            elif v == tomato:
                tile_color = tomato_color
            else:
                tile_color = mushroom_color
            pygame.draw.rect(plate, tile_color, (x * tilesize, y * tilesize, tilesize, tilesize), 0)
            if tilesize > 2:
                pygame.draw.rect(plate, (0,0,0), (x * tilesize, y * tilesize, tilesize, tilesize), 1)

    pygame.display.flip()
    
    while (pygame.QUIT not in [e.type for e in pygame.event.get()]):    
        continue

"""
def inslice(x,y):
    for s in slices:
        if x >= s[0][0] and x <= s[1][0] and y >= s[0][1] and y <= s[1][1]:
            return True
    return False
"""

def item_at(x,y):
    return None if (x < 0 or x >= cols or y < 0 or y >= rows) else grid[y][x]


def firstvalid_optimum():
    if len(set_indicies) == 0:
        return None
    try:
        sIndex = set_indicies.pop(0)
    except KeyError:
        return None
    index = sIndex.split()
    set_indicies.add(sIndex)
    p = [int(index[0]), int(index[1])]
    
    return p

def firstvalid_offset(cy):
    for y in range(cy, rows):
        for x in range(cols):
            if item_at(x,y) != None:
                return (x,y)
    return None


def firstvalid():
    for y in range(rows):
        for x in range(cols):
            if item_at(x,y) != None:
                return (x,y)
    return None

def shapevalid(x0,y0,shape):
    toppings = [0,0]
    for y in range(y0, y0+shape[1]):
        for x in range(x0, x0+shape[0]):
            v = item_at(x,y)
            if v == None:
                return False
            else:
                toppings[v] += 1
    return toppings[0] >= mintps and toppings[1] >= mintps

def getaround(x0,y0):
    toppings = [0,0]
    for d in dirs:
        x = x0+d[0]
        y = y0+d[1]
        v = item_at(x,y)
        if v != None:
            toppings[grid[y][x]] += 1
    return toppings

def isactive(x,y):
    ctop = grid[y][x]
    around = getaround(x,y)
    return around[ctop ^ 1] != 0
   
"""
def recsum(obj):
    sum = 0
    if type(obj) == list:
        for v in obj:
            sum += recsum(v)
    else:
        sum += obj
    return sum
"""

def getscore(slices):
    score = sum([(s[1][1]-s[0][1])*(s[1][0]-s[0][0]) for s in slices])    
    return score

def lossfunc(x0,y0,shape):
    active = 0
    for y in range(y0, y0+shape[1]):
        for x in range(x0, x0+shape[0]):
            if isactive(x,y) != None:
                active += 1
    return active / (shape[0] * shape[1]) # over mintps

def result_at(x,y):
    return None if (x < 0 or x >= cols or y < 0 or y >= rows) else result[y][x]

def useresult(x0,y0,shape):
    for y in range(y0, y0+shape[1]):
        for x in range(x0, x0+shape[0]):
            result[y][x] = None

def useshape(x0,y0,shape):
    for y in range(y0, y0+shape[1]):
        for x in range(x0, x0+shape[0]):
            grid[y][x] = None
            
            key = '{0:04d} {1:04d}'.format(x, y)
            if key in set_indicies:
                set_indicies.remove(key)
    

def resetshape(x0,y0,shape):
    for y in range(y0, y0+shape[1]):
        for x in range(x0, x0+shape[0]):
            grid[y][x] = grid_copy[y][x]
            set_indicies.add[('{0} {1}'.format(x, y))]

def greedy():
    global slices, bestscore, bestslices

    missedtiles = 0

    prev_cx, prev_cy = 0, 0
    prev_shape = None
    i = 0
    while(True):
        p = firstvalid()

        if p == None:
            bestslices = slices
            bestscore = getscore(slices)
            return True # all spaces filled  => success!

        cx, cy = p
        valids = [s for s in shapes if shapevalid(cx,cy,s)]
        valids = sorted(valids, key=lambda s: lossfunc(cx,cy,s))

        if i < len(valids):
            s = valids[i]
            slices.append(((cx,cy),(cx+s[0],cy+s[1])))
            useshape(cx,cy,s)
            useresult(cx,cy,s)
            prev_cx = cx
            prev_cy = cy
            prev_shape = s
        elif prev_shape != None:
            resetshape(prev_cx, prev_cy, prev_shape)
            i += 1
            slices.pop()
            prev_shape = None
        elif prev_shape == None: 
            missedtiles += 1
            useshape(cx,cy,(1,1))
            i = 0

        score = getscore(slices)
        print('shapes:{0}\tscore:{1}\tmissing:{2}\t'.format(len(slices), score, missedtiles), end='\r')

def greedy_visual():
    global slices, bestscore, bestslices

    missedtiles = 0

    prev_cx, prev_cy = 0, 0
    prev_shape = None
    i = 0
    maxwidth = 1000
    maxheight = 600
    margin = 5
    colsratio = (maxwidth - 2 * margin)/cols
    rowsratio = (maxheight - 2 * margin)/rows
    tilesize = int(colsratio if rowsratio > colsratio else rowsratio)
    tilesize = 2 if tilesize <= 1 else tilesize
    width = tilesize * cols + 2 * margin
    height = tilesize * rows + 2 * margin

    pygame.init()
    pygame.display.set_caption('Google Hashcode - Pizza Slicer')

    screen = pygame.display.set_mode((width, height))
    screen.fill((0,0,0))

    plate = screen.subsurface((margin, margin, width - margin, height - margin))
    mushroom_color = (0, 0, 255)
    tomato_color = (255, 0, 0)
    empty_color = (120,120,120)

    # draw the pizza once
    for y in range(rows):
        for x in range(cols):
            v = result_at(x,y)
            if v == None:
                tile_color = empty_color
            elif v == tomato:
                tile_color = tomato_color
            else:
                tile_color = mushroom_color
            pygame.draw.rect(plate, tile_color, (x * tilesize, y * tilesize, tilesize, tilesize), 0)
            if tilesize > 2:
                pygame.draw.rect(plate, (0,0,0), (x * tilesize, y * tilesize, tilesize, tilesize), 1)

    cy = 0
    cx = 0
    while(True):
        p = firstvalid()
        
        #let the user decide when to stop
        if p == None:
            bestslices = slices
            bestscore = getscore(slices)
        else:
            cx, cy = p
            valids = [s for s in shapes if shapevalid(cx,cy,s)]
            #valids = sorted(valids, key=lambda s: lossfunc(cx,cy,s))
            s = None
            if i < len(valids):
                s = valids[i]
                slices.append(((cx,cy),(cx+s[0],cy+s[1])))
                useshape(cx,cy,s)
                useresult(cx,cy,s)
                prev_cx = cx
                prev_cy = cy
                prev_shape = s
            elif prev_shape != None:
                resetshape(prev_cx, prev_cy, prev_shape)
                i += 1
                slices.pop()
                prev_shape = None
            elif prev_shape == None: 
                missedtiles += 1
                useshape(cx,cy,(1,1))
                i = 0

            score = getscore(slices)
                        
        if not s == None:
            for y in range(cy, cy+s[1]):
                for x in range(cx, cx+s[0]):
                    v = result_at(x,y)
                    if v == None:
                        tile_color = empty_color
                    elif v == tomato:
                        tile_color = tomato_color
                    else:
                        tile_color = mushroom_color
                    pygame.draw.rect(plate, tile_color, (x * tilesize, y * tilesize, tilesize, tilesize), 0)
                    if tilesize > 2:
                        pygame.draw.rect(plate, (0,0,0), (x * tilesize, y * tilesize, tilesize, tilesize), 1)

        pygame.display.flip()
    
        print('shapes:{0}\tscore:{1}\tmissing:{2}\t'.format(len(slices), score, missedtiles), end='\r')
        if pygame.QUIT in [e.type for e in pygame.event.get()]:    
            break

def nextshape():
    global slices, bestscore, bestslices
    p = firstvalid()
    if p == None:
        bestslices = slices
        bestscore = getscore(slices)
        return True # all spaces filled  => success!
    
    cx, cy = p
    valids = [s for s in shapes if shapevalid(cx,cy,s)]
    
    #optimize?
    #valids = sorted(valids, key=lambda s: lossfunc(cx,cy,s))
    
    for s in valids:
        slices.append(((cx,cy),(cx+s[0],cy+s[1])))
        useshape(cx,cy,s)

        if nextshape(): # dont modify slices, just return (preserve state)
            return True
        
        resetshape(cx,cy,s)
        slices.pop()

    score = getscore(slices)
    if score > bestscore:
        print(bestscore)
        bestslices = slices[:]
    return False

def possible_at(x,y):
    return None if (x < 0 or x >= cols or y < 0 or y >= rows) else possible[y][x]

def usepossible(x0,y0,shape):
    for y in range(y0, y0+shape[1]):
        for x in range(x0, x0+shape[0]):
            if x < cols and y < rows:
                possible[y][x] = 1

def locate_possible_regions(visualize):
    global slices, bestscore, bestslices

    if visualize:
        missedtiles = 0

        prev_cx, prev_cy = 0, 0
        prev_shape = None
        i = 0
        maxwidth = 1000
        maxheight = 600
        margin = 5
        colsratio = (maxwidth - 2 * margin)/cols
        rowsratio = (maxheight - 2 * margin)/rows
        tilesize = int(colsratio if rowsratio > colsratio else rowsratio)
        tilesize = 2 if tilesize <= 1 else tilesize
        width = tilesize * cols + 2 * margin
        height = tilesize * rows + 2 * margin

        pygame.init()
        pygame.display.set_caption('Google Hashcode - Pizza Slicer')

        screen = pygame.display.set_mode((width, height))
        screen.fill((0,0,0))

        plate = screen.subsurface((margin, margin, width - margin, height - margin))
        mushroom_color = (0, 0, 255)
        tomato_color = (255, 0, 0)
        empty_color = (120,120,120)

        # draw the pizza once
        for y in range(rows):
            for x in range(cols):
                v = result_at(x,y)
                if v == None:
                    tile_color = empty_color
                elif v == tomato:
                    tile_color = tomato_color
                else:
                    tile_color = mushroom_color
                pygame.draw.rect(plate, tile_color, (x * tilesize, y * tilesize, tilesize, tilesize), 0)
                if tilesize > 2:
                    pygame.draw.rect(plate, (0,0,0), (x * tilesize, y * tilesize, tilesize, tilesize), 1)

    numOfTiles = 0 

    terminate = False
    for cy in range(rows):
        if terminate:
            break
        for cx in range(cols):
            if terminate:
                break
          
            hasHorizontal = shapevalid(cx, cy, shape_max_horizontal) 
            if not hasHorizontal:
                hasHorizontal = shapevalid(cx - shape_max_horizontal[0], cy, shape_max_horizontal) 
            if not hasHorizontal:
                hasHorizontal = shapevalid(cx, cy - shape_max_horizontal[1] , shape_max_horizontal) 

            hasVertical = shapevalid(cx, cy, shape_max_vertical) 
            if not hasVertical:
                hasVertical = shapevalid(cx - shape_max_vertical[0], cy, shape_max_vertical) 
            if not hasVertical:
                hasVertical = shapevalid(cx, cy - shape_max_vertical[1] , shape_max_vertical) 
                   
            hasSquare = shapevalid(cx, cy, shape_max_square)
            if not hasSquare:
                hasSquare = shapevalid(cx - shape_max_square[0], cy, shape_max_square) 
            if not hasSquare:
                hasSquare = shapevalid(cx, cy - shape_max_square[1] , shape_max_square) 

            s = None
            if hasHorizontal:
                slices.append(((cx,cy),(cx+shape_max_horizontal[0],cy+shape_max_horizontal[1])))
                usepossible(cx,cy,shape_max_horizontal)
                s = shape_max_horizontal
            if visualize and not s == None:
                height = tilesize * s[0]
                width = tilesize * s[1]
                pygame.draw.rect(plate, (0,255,0), (cx * tilesize, cy * tilesize, width, height), 1)

            s = None
            if hasVertical:
                slices.append(((cx,cy),(cx+shape_max_vertical[0],cy+shape_max_vertical[1])))
                usepossible(cx,cy,shape_max_vertical)
                s = shape_max_vertical
            if visualize and not s == None:
                height = tilesize * s[0]
                width = tilesize * s[1]
                pygame.draw.rect(plate, (0,255,0), (cx * tilesize, cy * tilesize, width, height), 1)

            s = None
            if hasSquare:
                slices.append(((cx,cy),(cx+shape_max_square[0],cy+shape_max_square[1])))
                usepossible(cx,cy,shape_max_square)
                s = shape_max_square
            if visualize and not s == None:
                height = tilesize * s[0]
                width = tilesize * s[1]
                pygame.draw.rect(plate, (0,255,0), (cx * tilesize, cy * tilesize, width, height), 1)
                
            numOfTiles +=1    
            print('\tx:{0}\ty:{1}\ttiles:{2}\t'.format(cx, cy, numOfTiles), end = '\r')

            if visualize:        
                pygame.display.flip()

            if visualize and pygame.QUIT in [e.type for e in pygame.event.get()]:    
                terminate = True
    while visualize and not terminate:
        if pygame.QUIT in [e.type for e in pygame.event.get()]:    
            break

def greedy_look_ahead(visualize):
    global slices, bestscore, bestslices

    start = time.time()
    end_time = start
    missedtiles = 0

    prev_cx, prev_cy = 0, 0
    prev_shape = None
    i = 0
    maxwidth = 1000
    maxheight = 600
    margin = 5
    colsratio = (maxwidth - 2 * margin)/cols
    rowsratio = (maxheight - 2 * margin)/rows
    tilesize = int(colsratio if rowsratio > colsratio else rowsratio)
    tilesize = 2 if tilesize <= 1 else tilesize
    width = tilesize * cols + 2 * margin
    height = tilesize * rows + 2 * margin

    if visualize:
        pygame.init()
        pygame.display.set_caption('Google Hashcode - Pizza Slicer')

        screen = pygame.display.set_mode((width, height))
        screen.fill((0,0,0))

        plate = screen.subsurface((margin, margin, width - margin, height - margin))
        mushroom_color = (0, 0, 255)
        tomato_color = (255, 0, 0)
        empty_color = (120,120,120)

        # draw the pizza once
        for y in range(rows):
            for x in range(cols):
                v = result_at(x,y)
                if v == None:
                    tile_color = empty_color
                elif v == tomato:
                    tile_color = tomato_color
                else:
                    tile_color = mushroom_color
                pygame.draw.rect(plate, tile_color, (x * tilesize, y * tilesize, tilesize, tilesize), 0)
                if tilesize > 2:
                    pygame.draw.rect(plate, (0,0,0), (x * tilesize, y * tilesize, tilesize, tilesize), 1)

    terminate = False
    cx = 0
    cy = 0
    p = [cx, cy]
    while(True):
    
        p = firstvalid_optimum()
        if p == None:
            bestslices = slices
            bestscore = getscore(slices)
            break
    
        cx, cy = p
        valids = [s for s in shapes if shapevalid(cx,cy,s)]

        if len(valids) == 0:
            useshape(cx,cy,(1,1))
            continue

        is_left_good = False
        is_down_good = False
        
        for v in valids:
            s = v
            left_cx = cx + v[0]
            down_cy = cy + v[1]
            
            if left_cx >= cols:  
                is_left_good = True
            else: 
                is_left_good = does_any_shape_fit(left_cx, cy)

            if down_cy >= rows:  
                is_down_good = True
            else: 
                is_down_good = does_any_shape_fit(cx, down_cy)

            # use the first best fit
            if is_left_good and is_down_good:
                s = v
                break

        slices.append(((cx,cy),(cx+s[0],cy+s[1])))
        val_slices.append(((cy,cx),(s[1],s[0])))
        useshape(cx,cy,s)
        useresult(cx,cy,s)

        score = getscore(slices)
                            
        for y in range(cy, cy+s[1]):
            for x in range(cx, cx+s[0]):
                v = result_at(x,y)
                if v == None:
                    tile_color = empty_color
                elif v == tomato:
                    tile_color = tomato_color
                else:
                    tile_color = mushroom_color
                
                if visualize:
                    pygame.draw.rect(plate, tile_color, (x * tilesize, y * tilesize, tilesize, tilesize), 0)
                    if tilesize > 2:
                        pygame.draw.rect(plate, (0,0,0), (x * tilesize, y * tilesize, tilesize, tilesize), 1)

        if visualize:
            pygame.display.flip()
    
        missedtiles = rows * cols - score
        end_time = time.time() - start
        print('shapes:{0}\tscore:{1}\tmissing:{2}\tindecies:{3}\tsecs:{4:.2f}'.format(len(slices), score, missedtiles, len(set_indicies), end_time), end='\r')
        if pygame.QUIT in [e.type for e in pygame.event.get()]:    
            terminate = True
            break

    while visualize and not terminate:
        print('shapes:{0}\tscore:{1}\tmissing:{2}\tindecies:{3}\tsecs:{4:.2f}'.format(len(slices), score, missedtiles, len(set_indicies), end_time), end='\r')
        if pygame.QUIT in [e.type for e in pygame.event.get()]:    
            break

def does_any_shape_fit(cx, cy):
    for s in shapes:
        if shapevalid(cx, cy, s):
            return True
    
    return False

# approach 1 - bruteforce
#nextshape()

# approach 2 - greedy

if args.show_progress and not args.show_possible:
    #greedy_visual()
    greedy_look_ahead(True)
elif not args.show_progress and not args.show_possible:
    greedy()
    visualize()
elif args.show_possible:
    locate_possible_regions(args.show_progress)

print(val_slices)

#ofile = open(filename + ".res", "w+")
#ofile.write(str(bestslices))
#ofile.close()

print()
print("score of output:" + str(bestscore))
