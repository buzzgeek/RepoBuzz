import os, sys

cwd = os.path.dirname(os.path.realpath(__file__))

files = ["a_example.txt", "b_lovely_landscapes.txt", "c_memorable_moments.txt", "d_pet_pictures.txt", "e_shiny_selfies.txt"]

# functions
def loaddata(fname):
    cwd = os.path.dirname(os.path.realpath(__file__))
    f = open(cwd + "/" + fname)
    piccount = int(f.readline())
    picdata = list()
    for i in range(piccount):
        line = f.readline().split()
        picdata.append([line[0], int(line[1]), line[2:]])
    f.close()
    return picdata

def genslides(picdata, part):
    slides = list()
    horpics = None
    verpics = None
    if part == None: # use all pictures
        horpics = [i for i,p in enumerate(picdata) if p[0] == "H"]
        verpics = [i for i,p in enumerate(picdata) if p[0] == "V"]
    else:
        horpics = [i for i in part if picdata[i][0] == "H"]
        verpics = [i for i in part if picdata[i][0] == "V"]

    # adding most possible pictures as slides (ids)
    for i in range(int(len(verpics)/2)):
        slides.append([verpics[i*2], verpics[i*2+1]])
    for i in range(int(len(horpics))):
        slides.append([horpics[i]])
    return slides

def gettags(picdata, slide):
    tags = list()
    for s in slide:
        tags += picdata[s][2]
    return tags

def getscore(picdata, slidedata):
    score = 0
    for si in range(0, len(slidedata)-1):
        ctags = gettags(picdata, slidedata[si])
        ntags = gettags(picdata, slidedata[si+1])
        common = [t for t in ctags if t in ntags]
        score += min(len(ctags) - len(common), len(common), len(ntags) - len(common))
    return score

def writeslides(slidedata, fname):
    f = open(cwd + "/" + fname, "w+")
    f.write(str(len(slidedata)) + "\n")
    for s in slidedata:
        f.write(" ".join([str(v) for v in s]) + "\n")
    f.close()

"""
def partitiondata(picdata):
    eturn partdata

def attempt1(picdata): # similar amount of tags
    partdata = partitiondata(picdata)
    slides = list()
    for part in partdata:
        slides += genslides(picdata, part)
    return slides
"""

def gentagdict(picdata, ids):
    tagdict = dict()
    for i in ids:
        for t in picdata[i][2]:
            if t not in tagdict:
                tagdict[t] = [i]
            else:
                tagdict[t].append(i)
    return tagdict

def slidetags(picdata, slide):
    tags = list()
    for i in slide:
        tags += picdata[i][2]
    return tags

def solve(picdata):
    horpics = [i for i,p in enumerate(picdata) if p[0] == "H" and p[1] > 1]
    verpics = [i for i,p in enumerate(picdata) if p[0] == "V"]

    slides = list() # unordered slides

    # create slides
    ## horizontals
    slides += [[p] for p in horpics]

    """
    ## verticals
    verpics = sorted(verpics, key = lambda x : picdata[x][1])
    while len(verpics) > 1:
        minscore = None
        minind = None
        cslide = list()
        cslide.append(verpics[0])
        tags = picdata[verpics[0]][2]
        for i in range(1, len(verpics)):
            ctags = picdata[verpics[i]][2]
            common = [t for t in tags if t in ctags]
            score = len(common)

            if minscore == None or score < minscore:
                minind = i
                minscore = score
                if score == 0:
                    break

        cslide.append(verpics[minind])
        verpics.pop(minind)
        slides.append(cslide)
    """

    slides += genslides(picdata, sorted(verpics, key = lambda x : picdata[x][1])) # todo improve (most difference?)

    slides = sorted(slides, key=lambda x : slidetags(picdata, x))

    return attempt2(picdata, slides)

def attempt2(picdata, slides):
    slideshow = list()
    slidecount = len(slides)

    slideshow.append(slides[0])
    slides.pop(0)

    print("sorting slides..")

    while len(slides) > 0:
        tags = slidetags(picdata, slideshow[-1])

        umaxscore = int(len(tags)/2)
        maxscore = 0
        maxind = None
        for i, s in enumerate(slides):
            ctags = slidetags(picdata, s)
            common = [t for t in tags if t in ctags]
            score = min(len(tags) - len(common), len(common), len(ctags) - len(common))

            if maxscore == 0 or score > maxscore:
                maxind = i
                maxscore = score
                if maxscore == umaxscore:
                    break
            else:
                break

        slideshow.append(slides[maxind])
        slides.pop(maxind)

    return slideshow

def attempt1(picdata, slides):
    slideshow = list()

    slideshow.append(slides[0])
    slides.pop(0)

    # create dictionary of tags per slide
    tagdict = dict()
    for s in range(len(slides)):
        for i in slides[s]:
            for t in picdata[i][2]:
                if t not in tagdict:
                    tagdict[t] = [s]
                else:
                    tagdict[t].append(s)

        # sort dictionary of tags per slide
    stags = sorted(tagdict, key = lambda x : len(tagdict[x]), reverse = True)

    while len(slides) > 0:
        tags = slidetags(picdata, slideshow[-1])
        umaxscore = int(len(tags)/2)
        maxscore = 0
        maxind = None
        for i, s in enumerate(slides):
            ctags = slidetags(picdata, s)
            common = [t for t in tags if t in ctags]
            score = min(len(tags) - len(common), len(common), len(ctags) - len(common))

            if maxscore == 0 or score > maxscore:
                maxind = i
                maxscore = score
                if maxscore == umaxscore:
                    break
            else:
                break

        if maxind == None:
            print("??")
            break

        slideshow.append(slides[maxind])
        slides.pop(maxind)

    return slideshow

# load data
cfile = files[2]
picdata = loaddata("../data/" + cfile)
print("picdata: " + str(picdata))

slidedata = solve(picdata)
if not slidedata:
    sys.exit()

print("score: " + str(getscore(picdata, slidedata)))

# write output
writeslides(slidedata, cfile + ".out")

