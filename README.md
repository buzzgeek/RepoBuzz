The source contains a number of projects written in various languages, that I have created over a number of years. I will eventually separate out active projects and create new relevant repositories.

- RepoBuzz\dotnet contains a c# dnn artificial implementation, that actually works. This project is still under constructions and contains a lot of brain farts. So sorry for the mess.

  - The Brain class contains an implementation of a classical dnn via the cluster member as well as an object oriented approach. via the brainCells member. Initially the dnn cluster is trained on a set of training images, inorder to pre/initialize the weights and biases with the help of forward- and backward propagation. Once the dnn has reached a satisfactory state, the object oriented brain will be initialized with the weights and biases. The intend is to use this brain to further increase the accuracy of the results, by using a completely different approach to classic dnn math. The object oriented approach has not been implemented yet, so this is still under construction. However, the classical dnn implementation seems to provide sufficient results. I also have provided a python implementation using keras and tensorflow, in order to validate my results. To me the comparism comes up with similar results, so I am satisfied with my dnn implementation.
  - see on YouTube 3Blue1Brown - YouTube - https://www.youtube.com/channel/UCYO_jab_esuFRV4b17AJtAw - very good videos on these type of dnns and linear algebra (vector matrix calculations)
  - see http://neuralnetworksanddeeplearning.com/chap1.html for very good introduction and tutorial in python
  - see https://sudeepraja.github.io/Neural/ on Backward propagation (excluding bias)
  - see https://en.wikipedia.org/wiki/Activation_function for a list and description of signal activation functions and their derivatives
  
- RepoBuzz\python contains a python keras/tensorflow implementation of the same dnn implemented in the dotnet version. This has been created to compare the two approaches. This keras/tensorflow approach is of course a LOT faster even when only running on the cpu

- RepoBuzz\data contains sample data that can be used to train and validate the two dnn implementations.

- RepoBuzz\hashcode_2019 and RepoBuzz\hashcode_practice contain source code related to Google's HashCode 2019 challenge. The sources have been created in colaboration with https://github.com/Sinitax (so a huge Thanks to sinitax). I have created a related github repository HashCode2019 which contains code, that only my poor litle brain came up with.

- RepoBuzz\cpp\project01 contain something really old in relation. A Win32 game of life presentation that uses the RGB color channels as competing entities/cells. It is quite prety. Has been implemented with VS 6 using very old win32 Windows programming...well I have even older code than this... 