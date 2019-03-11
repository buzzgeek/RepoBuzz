## rules:
- pictures and slides have tags
- pictures oriented in slides:
    - 1 horizontal (slide tags = picture tags)
    - 2 vertical (slide tags = sum of picture tags)
- atleast 1 slide per slideshow
- photos used 1 or 0 times

## input data:
- first line num of pictures
- line of photo after first is its id
- data per picture:
    - H/V for orientation
    - number of tags
    - tags after

## submissions:
- first line num of slides
- data per slide:
    - id(s) of picture

## scoring:
- pictures of the same slide dont have to have tags in commmon
- interest factor is min of these:
    - number of common tags
    - number of tags in one but not the other
