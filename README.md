# Introduction

This is an Universal Windows Platform (UWP) app that can run on all devices supporting UWP apps,
but is mainly targeting devices running the Windows 10 IoT Core. An example of such device
is the Raspberry Pi 2.

The app connects to the Azure IoT Hub to get cloud-to-device messages to control lamps.

# Running

The IoT Hub connection information is looked up from a file called IotHubSettings.txt.
The file is not present in the source repository (and is ignored by git to avoid accidentally
pushing secret information).

To make the connection work, create the file under `SimpleApp/Settings` or `IoTCoreApp/Settings`
depending which you want to run (see below for more details about the project structure).

The content of the file should be something like this:

```
{
  "Host": "<some-host>.azure-devices.net", // called HostName in a connection string
  "Port": 5671,
  "DeviceId": "<id-of-your-device>", // called DeviceId in a connection string
  "DeviceKey": "<key-of-your-device>" // called SharedAccessKey in a connection string
}
```

See below more about getting a device id and a key.

Open the solution file (`IoTCoreApp.sln`) in Visual Studio 2015 and hit run.

Requires Windows 10 SDK to be installed.

## Getting device id and key

To get a device id and key, you first need to create an Azure IoT Hub.
See [here](https://azure.microsoft.com/en-us/services/iot-hub/) to get started.

Next, you need to create a device in the hub. One of the easiest way to do that
is to use a tool called [iothub-explorer](https://www.npmjs.com/package/iothub-explorer),
which can be installed like this:

```
npm install -g iothub-explorer
```

See the tool instructions from [here](https://www.npmjs.com/package/iothub-explorer),
but the main command needed is `iothub-explorer create <name-of-you-device> --connection-string`,
which creates a device for you and prints out the connection string from which you can get
the values needed to configure this project.

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

# Troubleshooting

* If you get a big amount of build errors with messages like
`The type or namespace name 'System' could not be found` try to clean the solution first
and then rebuild the solution.
* In case the first build error is about a file called `IotHubSettings.txt` see the Running-section
above about how to construct the required file.
