# Introduction

This is an Universal Windows Platform (UWP) app that can run on all devices supporting UWP apps,
but is mainly targeting devices running the Windows 10 IoT Core. An example of such device
is the Raspberry Pi 2.

The app connects to the Azure IoT Hub to get cloud-to-device messages to control lamps.

# Running

Open the solution file (`IoTCoreApp.sln`) in Visual Studio 2015 and hit run. Requires
Windows 10 SDK to be installed. See below for more details about the project structure.

# Project structure

## LightController

The main logic of the app is written in this project. Initializes the connection to the cloud
and to the lamps found locally.

### org.allseen.LSF.LampState

Project auto-generated using the
[AllJoyn Studio](https://visualstudiogallery.msdn.microsoft.com/064e58a7-fb56-464b-bed5-f85914c89286)
and is used to control lamps.

## IoTHubClient

## IoTCoreApp

A headless app that can run as a background task on a Windows 10 IoT Core device. This type
of app can be set as a headless startup app on the device, which means that it will be started
after device boot and re-started if it shuts down or crashes.

## SimpleApp

A simple wrapper app for the main functionality that allows debugging via a simple UI and
running the app on a Windows 10 desktop machine.

