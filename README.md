This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments

# Proficy Historian Module for the Azure IoT Gateway SDK
This reference implementation demonstrates how the Azure IoT Gateway SDK can be used to connect to existing Proficy Historian servers and send JSON encoded telemetry data from these servers using the Proficy Historian SDK pub/sub format (using a JSON payload) to Azure IoT Hub. All transport protocols supported by the Gateway SDK can be used, i.e. HTTPS, AMQP and MQTT. The transport is selected in the transport setting in gateway_config.json.

This module uses the Proficy Historian API, which GE has approved being included in this reposiotry.
However, to use the API and this module you need to connect to a properly licensed Proficy Historian. 

# Azure IoT Gateway SDK compatibility
The current version of the Proxy module is targeted at the [Azure IoT Gateway SDK 2017-01-13 release](https://github.com/Azure/azure-iot-gateway-sdk/releases/tag/2017-01-13).

Use the following command line to clone the compatible version Azure IoT Gateway SDK, then follow the build instructions included:

```
git clone --recursive https://github.com/Azure/azure-iot-gateway-sdk.git
```
# Directory Structure

## /samples
This folder contains a sample configuration that instructs a vanilla gateway host load the module and IoT Hub proxy module and configures the module to create a 
subscription on a standard server which publishes the specified tags to Azure IoT Hub.

## /src
This folder contains the C# Proficy Historian module source file (GatewayModule.cs).

# Building the Module
To build the module, open up the solution in Visual Studio and run the build command. 

# Configuring the Module
Proficy Historian tags whose values should be published to Azure IoT Hub can be configured in the module JSON configuration.  A sample template configuration file can be found in ```samples/gateway_config.json```.  The configuration consists of a OPC-UA Application Configuration and Subscriptions section.  

## Application Configuration section
The ```args``` section should be confiugred as shown below  

``` JSON
"args": {
    "ServerName": "<HistorianServerName>", //Mandatory
    "UserName": "<HistorianUserName>", //Mandatory
    "Password": "<HistorianPassWord>", //Mandatory
    "PrintToConsole": false, // Optional - default is false
    "TagsToSubscribe": [ ... ]
    ]
}
```

## TagsToSubscribe section
The ```TagsToSubscribe``` section contains an array of Tagnames that should be subscribed to from the Proficy Historian server at startup.

``` JSON
    "TagsToSubscribe": [
        { "TagName": "<Tagname1>" }, // Tagname is mandatory, MinimumElapsedMilliSeconds is optional - default is 1000
        { "TagName": "<Tagname2>", "MinimumElapsedMilliSeconds": 1000 }
    ]

```

# Running the module

To run the module and have it publish to IoT Hub, configure the name of your Hub (JSON field ```"IoTHubName"```) and the IoT Hub device ID and shared access key to use (JSON fields ```"Id"``` and ```"SharedAccessKey"```) in your version of ```gateway_config.json```.  Ensure that the right native module is configured, based on your platform (i.e. iothub.dll for Windows, libiothub.so for Linux, etc.).
