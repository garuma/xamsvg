ANDROID_SDK_PATH ?= "/Users/jeremie/mono/android-sdk-macosx"
XBUILD ?= "/usr/bin/xbuild"

all: build check

build:
	$(XBUILD) /p:Configuration=Debug XamSvg.sln

check:
	$(XBUILD) XamSvgTests/XamSvgTests.csproj /t:"Install;_Run"
	@sleep 5
	$(ANDROID_SDK_PATH)/tools/monkeyrunner ./run-monkey-test.py
