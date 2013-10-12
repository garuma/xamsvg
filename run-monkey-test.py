from com.android.monkeyrunner import MonkeyRunner, MonkeyDevice, MonkeyImage
import os
import os.path
import time

APK = "XamSvgTests/bin/Debug/XamSvgTests.XamSvgTests-Signed.apk"
PACKAGE = "org.neteril.XamSvgTests"
ACTIVITY = "XamSvgTests.MainActivity"
SCREENSHOTS_FOLDER = "XamSvgTests/screenshots"
REJECT_FOLDER = "XamSvgTests/rejects"
RAW_FOLDER = "XamSvgTests/Resources/raw"

print "Waiting for an Android device to spin up to run tests"
device = MonkeyRunner.waitForConnection()
width = device.getProperty('display.width')
height = device.getProperty('display.height')
density = device.getProperty('display.density')

suffix = width + 'x' + height + '@' + density
SCREENSHOTS_FOLDER = os.path.join(SCREENSHOTS_FOLDER, suffix)
REJECT_FOLDER = os.path.join(REJECT_FOLDER, suffix)

if not os.path.exists(SCREENSHOTS_FOLDER):
	os.makedirs(SCREENSHOTS_FOLDER)
if not os.path.exists(REJECT_FOLDER):
	os.makedirs(REJECT_FOLDER)

images = os.listdir(RAW_FOLDER)
for i in images:
	imgPath = os.path.join(SCREENSHOTS_FOLDER, i + '.png')
	screenshot = device.takeSnapshot()
	if not os.path.exists(imgPath):
		print "Adding screenshot for " + i
		screenshot.writeToFile(imgPath)
	else:
		existingShot = MonkeyRunner.loadImageFromFile(path=imgPath)
		if not screenshot.sameAs(existingShot):
			rejectPath = os.path.join(REJECT_FOLDER, i + '-reject.png')
			screenshot.writeToFile(rejectPath)
			print "*** VISUAL TEST FAIL: " + i + ". Offending image put in " + rejectPath

	# Go to next image
	device.touch (100, 100, 'DOWN_AND_UP')
	# Let the display refresh
	time.sleep (1)
