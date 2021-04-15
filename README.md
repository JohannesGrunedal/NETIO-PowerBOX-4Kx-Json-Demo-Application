# NETIO-PowerBOX-4Kx-Json-Demo-Application
Simple C# 'NETIO PowerBOX 4Kx' Json demo application.

Features
> Uses Json for data read/write
> Get device info
> Get all output status
> Read output status 
> Monitor voltage, current and power data


NOTE!
This application is tested with NETIO PowerBOX 4KF, minor changes may be needed to be run on other devices.
To keep demo simple and short, data validation and error handling is reduced to a minimum.


Getting started                   
 1. Enable read/write in web config M2M API Protocols -> JSON API -> Enable JSON API -> Enable READ-WRITE
 2. Enter username and password (default is netio).
 3. Click Save Changes.
 4. Note current NETIO IP address.
 5. Build and run this application.
 6. Enter IP address, user and password.
 7. Click Connect button.
 8. If successful, Info field is populated with current NETIO data (if not, check above settings).
 9. Set selected outputs to ON/OFF using the Control buttons.
10. Click Status button to read current NETIO putput status.
11. Every second voltage, current and power values are read and updated.


Run standalone
 1. Create a new C#/WPF project.
 2. Add NetIoDriver.cs to your project.
 3. Include it: using NetIo;
 4. Depending on your settings, one or more namespaces may have to be included (see using's below).
 5. Init driver:  netIoDriver = new NetIoDriver("192.168.11.250", netio, netio); // init driver
 6. Test connection: var agent = netIoDriver.GetAgent(); // get current device info
 7. Read data:
    var model = agent.Model;            // reads out all agent info
    var root = netIoDriver.GetRoot();   // get all data
 8. Set output:
    var isOutput2Set = netIoDriver.SetState(NetIoDriver.OutputName.Output_2, NetIoDriver.Action.On); // turn on output 2
 9. Read output:
    var output4 = netIoDriver.GetOutput(NetIoDriver.OutputName.Output_4); // read output 4
10. Enjoy!


![Screenshot_1](https://user-images.githubusercontent.com/25680930/114856522-8045db00-9de7-11eb-9c6b-400b2b94f383.jpg)
![Screenshot_2](https://user-images.githubusercontent.com/25680930/114856536-82a83500-9de7-11eb-9fe8-b67658009d73.jpg)
