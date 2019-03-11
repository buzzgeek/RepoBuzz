'''
This dnn implementation in keras is used to validate the buzz.net framework, 
in hopes to re-produce similar results. 
So far Keras is much much faster than the buzz.net framework (even without using the GPU)
'''

from __future__ import print_function

import os
import datetime
import time
import cv2
import numpy as np
import random
from matplotlib import pyplot as plt
import keras
from keras.datasets import mnist
from keras.models import Sequential
from keras import optimizers
from keras.layers import Dense, Dropout
from keras.optimizers import RMSprop
from keras.preprocessing.image import ImageDataGenerator, array_to_img, img_to_array, load_img
from keras.utils import normalize
from keras import initializers
from keras import metrics
from keras.models import load_model

image_training = 'D:\\dev\\source\\buzznet\\data\\training\\input\\buzz.png'
image_expecting = 'D:\\dev\\source\\buzznet\\data\\training\\expected\\buzz.png'
image_validating = 'D:\\dev\\source\\buzznet\\data\\training\\validation\\buzz.png'
output_path = 'D:\\dev\\source\\buzznet\\data\\training\\result\\'
img_size = 3
batch_size = 64
num_iterations = 1000000
epochs = 10
train_samples = 1024
positiveResult = 0.90
activationFunction = 'sigmoid'
numNeurons = img_size * img_size

class DynamicPlot():
	min_x = 10 # same as number of epochs
	max_x = 1000000 # num_iterations * epochs

	def on_launch(self):
		#Set up plot
		self.figure, self.ax = plt.subplots()
		self.losses, = self.ax.plot([],[],label = 'loss')
		self.metrics, = self.ax.plot([],[],label = 'metrics') 
		self.accuracies, = self.ax.plot([],[],label = 'custom accuracy') 
		#Autoscale on unknown axis and known lims on the other
		self.ax.set_autoscaley_on(True)
		self.ax.set_xlim(self.min_x, self.max_x)
		#Other stuff
		self.ax.grid()
		self.ax.set_xscale('log')

	def on_running(self, xdata, loss_data, metrics_data, acc_data):
		#Update data (with the new _and_ the old points)
		self.losses.set_xdata(xdata)
		self.losses.set_ydata(loss_data)
		self.metrics.set_xdata(xdata)
		self.metrics.set_ydata(metrics_data)
		self.accuracies.set_xdata(xdata)
		self.accuracies.set_ydata(acc_data)
		#Need both of these in order to rescale
		self.ax.relim()
		self.ax.autoscale_view()
		#We need to draw *and* flush
		self.figure.canvas.draw()
		self.figure.canvas.flush_events()

	#Example
	def __call__(self):
		self.on_launch()
		plt.legend()

	def drawPlot(self, xdata, loss_data, metrics_data, acc_data):
		self.on_running(xdata, loss_data, metrics_data, acc_data)

def prepareInput(a):
	return (a / 255)


def main():
	srcImg = cv2.imread(image_training,cv2.IMREAD_GRAYSCALE)
	tgtImg = cv2.imread(image_expecting,cv2.IMREAD_GRAYSCALE)
	valImg = cv2.imread(image_validating,cv2.IMREAD_GRAYSCALE)
	rows = int(srcImg.shape[0] / img_size)
	columns = int(srcImg.shape[1] / img_size)
	losses = []
	metric = []
	accuracies = []
	num_of_epochs = []
	setTrain = None
	setTarget = None

	# Preparing training data.... 
	print ("Preparing training data....")
	for i in range(0, train_samples):
		r = random.randint(0, rows - 1)
		c = random.randint(0, columns - 1)
		
		y = r * img_size
		x = c * img_size
		h = img_size
		w = img_size
		
		srcTile = srcImg[y:y+h, x:x+w]
		tgtTile = tgtImg[y:y+h, x:x+w]
		
		trainIn = img_to_array(srcTile)    
		trainIn = trainIn.reshape(1,numNeurons)
		trainIn = np.apply_along_axis(prepareInput, 1, trainIn)

		trainOut = img_to_array(tgtTile)
		trainOut = trainOut.reshape(1,numNeurons)
		trainOut = np.apply_along_axis(prepareInput, 1, trainOut)
		
		if setTrain is None:
			setTrain = trainIn
		else:
			setTrain = np.vstack((setTrain, trainIn))
		
		if setTarget is None:
			setTarget = trainOut
		else:
			setTarget = np.vstack((setTarget, trainOut))

	# setting up the dnn model (fully connected feed forward dnn)
	model = Sequential()
	model.add(Dense(numNeurons, activation=activationFunction, input_shape=(numNeurons,), use_bias=True, bias_initializer='zeros', kernel_initializer=initializers.RandomUniform(minval=-0.5, maxval=0.5, seed=42)))
	model.add(Dense(numNeurons, activation=activationFunction, input_shape=(int(numNeurons),), use_bias=True, bias_initializer='zeros', kernel_initializer=initializers.RandomUniform(minval=-0.5, maxval=0.5, seed=42))) 
	model.add(Dense(numNeurons, activation=activationFunction, input_shape=(int(numNeurons),), use_bias=True, bias_initializer='zeros', kernel_initializer=initializers.RandomUniform(minval=-0.5, maxval=0.5, seed=42))) 
	model.add(Dense(numNeurons, activation=activationFunction, input_shape=(numNeurons,), use_bias=True, bias_initializer='zeros', kernel_initializer=initializers.RandomUniform(minval=-0.5, maxval=0.5, seed=42)))
	model.summary()

	sgd = optimizers.SGD(lr=0.01, decay=1e-6, momentum=0.9, nesterov=True)
	model.compile(loss='mean_squared_error', optimizer=sgd, metrics=['accuracy', metrics.binary_accuracy])

	# initialization magic for the ui plot
	plt.ion()

	ls = DynamicPlot()
	ls()

	#let's train the model
	cnt = 0
	for i in range(0, num_iterations): 
		history = model.fit(setTrain, setTarget,
					batch_size=batch_size,
					epochs=epochs,
					verbose=0,
					validation_data=(setTrain, setTarget))

		score = model.evaluate(setTrain, setTarget, verbose=0)
		cnt = cnt + epochs
		
		customScore = 0
		p = model.predict_on_batch(setTrain)
		
		a = setTrain.flatten()
		b = p.flatten()
		
		for j in range(0, a.size):
			customScore = customScore + (1- abs(a[j] - b[j]))
		
		customAccuracy = float(customScore) / a.size
		
		num_of_epochs.append(cnt)
		losses.append(score[0])
		metric.append(score[2])
		accuracies.append(customAccuracy)
		ls.drawPlot(np.asarray(num_of_epochs), np.asarray(losses),  np.asarray(metric), np.asarray(accuracies))
		
		print('Loss:', score[0])
		print('Metrics:', score[2])
		print ('Accuracy', customAccuracy)
		print('evaluating next iteration: ', i)


	#let's run a final prediction on another image for validation purposes

	#  Preparing input data for validation prediction....
	print ("Preparing input data for validation prediction....")

	setResult = None
	rows = int(valImg.shape[0] / img_size)
	columns = int(valImg.shape[1] / img_size)

	print(rows, columns)
	 
	for r in range(0, rows) :
		for c in range(0, columns):
			y = r * img_size
			x = c * img_size
			h = img_size
			w = img_size
			
			srcTile = valImg[y:y+h, x:x+w]
			srcIn = img_to_array(srcTile)    
			srcIn = srcIn.reshape(1,numNeurons)
			srcIn = np.apply_along_axis(prepareInput, 1, srcIn)
			if setResult is None:
				setResult = srcIn
			else:
				setResult = np.vstack((setResult, srcIn))

	print('Predicting....')
	result = model.predict_on_batch(setResult)
	s = np.shape(result)
	print(s)

	# preparing image for display
	print ('Preparing image for display')
	i = 0
	for r in range(0, rows):
		print('proccesing row: ', r)
		for c in range(0, columns):
			resMat = np.asmatrix(result[i])
			resMat = resMat.reshape(img_size,img_size)
			for x in range(0, img_size):
				for y in range(0, img_size):
					valImg[x + r * img_size,y + c * img_size] = int(255 * resMat[x,y])
			i = i + 1
	print('Calculations complete! Result image might not be visible, see taskbar. Hit enter in image to terminate run.')
			
	cv2.imshow('Result',valImg)
	cv2.waitKey(0) & 0xFF # see https://docs.opencv.org/3.0-beta/doc/py_tutorials/py_gui/py_image_display/py_image_display.html

	st = datetime.datetime.now().strftime('%Y%m%d%H%M%S')
	directory = output_path + st

	# store the parameters of the trained network for later purposes
	if not os.path.exists(directory):
		os.makedirs(directory)

	# save the validation image
	resImage = directory + '\\result.png'
	cv2.imwrite(resImage, valImg)
	cv2.destroyAllWindows()

	modelFile = directory + '\\model.json'

	modelJson =  model.to_json()
	f = open(modelFile, 'w')
	f.write(modelJson)
	f.close()

	modelH5 = directory + '\\model.h5'
	model.save(modelH5)

if __name__ == "__main__":
	main()

