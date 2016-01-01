# TicTacToe

## Builds

iOS: <https://dl.dropboxusercontent.com/s/870lp7mlfn4d1r7/index.html>

Mac App: <https://www.dropbox.com/sh/0umn74crjefvpa9/AACI4YU34RtUz0hnbcSICJMWa?dl=0>

## Development

####Screencast:
[![](http://img.youtube.com/vi/npk8uwKSReM/0.jpg)](https://www.youtube.com/watch?v=npk8uwKSReM "TTT")

####Timer: 05:17:51

![](https://photos-4.dropbox.com/t/2/AAAN8b-g59KypjTZcP96szQtNBSAxF88XELyRcr4AVZxuQ/12/92483404/png/32x32/3/1451635200/0/2/timer.png/EKDE60cYthogAigC/gwYS1hkvjVzgVYAOzQb8g9pQD_I2IDxP11mqufLU9CU?size_mode=3&size=1280x960)


##Approach:

####Game System:

1. Setup coordinate system 6x6
2. Be able to get a block from a block by passing a reference coordinate e.g. (-1,1)
3. Setup controller to determine who's turn is it
4. Check winning by looking at a list of horizontal, vertical, diagonal right, diagonal left

####AI:
1. Build an profile for each list of horizontal, vertical, diagonal right, diagonal left
2. Checking if there is a threat and assign the highest weight for it
3. Sorting available moves by weight
4. Determine game over sequence


##Test:
1. WIN - get 4 in a row and see will it display the correct game over sequence
2. LOSE - let the AI win and see will it display the correct game over sequence
3. DRAW - play until there is no more moves see will it display the correct game over sequence
4. AI sharpness - try to win and see if it will block you