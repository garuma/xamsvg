ANDROID_SDK_HOME ?= /Users/jeremie/mono/android-sdk-macosx
XBUILD ?= /usr/bin/xbuild
AVD_NAME = unit_test_avd
AVD_PATH = unit_test.avd
TEST_DIRECTORY = XamSvgTests
ANDROID = $(ANDROID_SDK_HOME)/tools/android
EMULATOR = $(ANDROID_SDK_HOME)/tools/emulator
ADB = $(ANDROID_SDK_HOME)/platform-tools/adb

all: build check

build:
	$(XBUILD) /p:Configuration=Debug XamSvg.sln

setup-emulator: $(TEST_DIRECTORY)/$(AVD_PATH)/config.ini

$(TEST_DIRECTORY)/$(AVD_PATH)/config.ini:
	mkdir -p $(TEST_DIRECTORY)/$(AVD_PATH)
	yes '' | $(ANDROID) create avd --name $(AVD_NAME) --target android-18 --abi x86 --skin 720x1280 --path $(abspath $(TEST_DIRECTORY)/$(AVD_PATH)) -f

check: setup-emulator
	$(EMULATOR) -avd $(AVD_NAME) -no-snapshot &
	$(ADB) wait-for-device
	@while test `$(ADB) shell getprop init.svc.bootanim | tr -d "\015"` = running; do sleep 5; done
	@sleep 5
	$(XBUILD) $(TEST_DIRECTORY)/XamSvgTests.csproj /t:"Install;_Run"
	@sleep 5
	$(ANDROID_SDK_HOME)/tools/monkeyrunner ./run-monkey-test.py
	echo "kill" | nc localhost 5554
	fg
