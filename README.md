# UNetyEmu

> Documentation is available [here](https://hackmd.io/@goes-ariel/B1XbK46KJg)

**UNetyEmu** is a Unity and Mininet-WiFi-based framework for simulating multiple vehicles while emulating real-time networks. By combining Unity’s high-fidelity mobility simulation with Mininet-WiFi’s network emulation, **UNetyEmu** enables comprehensive analysis of future smart city applications, including 5G-enabled vehicular communication and edge computing.

## Requirements

In order to take advantage of all the network features of Mininet-Wifi on `Linux`, and also to take advantage of the compatibility and optimization of Unity on `Windows`, **UNetyEmu** uses both operating systems. For this, we use Unity on `Windows 11`, and Mininet-Wifi in a virtual machine with `Lubuntu 20.04`. 

In this first version of **UNetyEmu**, we present three different scenarios, in which the user can test all the features of **UNetyEmu**, such as different types of drones, different sensors, different algorithms, and above all, the integration with Mininet-Wifi.

The *First* and *Second scenarios* do not use network emulation, so you simply need to:

- [Unity account](https://unity.com/products) (Free accounts for personal or student use)
- [Unity Hub](https://unity.com/download)
- [Unity 2022.3.22f1](https://unity.com/releases/editor/archive) (Tested version)

Then, for the *Third scenario* we show the integration with Mininet-Wifi network emulator, so you also need the following:

- [VirtualBox 7.1.4](https://download.virtualbox.org/virtualbox/7.1.4/) (Only tested on Windows)
- [Mininet-WiFi](https://drive.google.com/file/d/1R8n4thPwV2krFa6WNP0Eh05ZHZEdhw4W/view?usp=sharing)(Pre-configured Virtual Machine)
- [.NET SDK 9.0](https://dotnet.microsoft.com/pt-br/download/dotnet/9.0) (Required for integration with VSCode and Unity libraries)
- [Python](https://www.python.org/downloads/) (tested on 3.13.1) or [Python from the Microsoft Store](https://apps.microsoft.com/detail/9pnrbtzxmb4z?hl=en-us&gl=US)
- [Pip 21.2.3 (or higher)](https://www.liquidweb.com/blog/install-pip-windows/) [(Make sure the PATH environment variable knows it)](https://stackoverflow.com/questions/62088784/how-to-install-pip-on-windows)
- [Scapy](https://scapy.readthedocs.io/en/latest/installation.html)
- [Npcap 1.8 (or higher)](https://npcap.com/#download)
- [WinPcap](https://www.winpcap.org/install/)
- [Pandas] (pip install pandas)
- [Seaborn] (pip install seaborn)
- [Matplotlib] (pip install matplotlib)

Finally, to help understand the algorithms and codes created in **UNetyEmu**, we recommend using Visual Studio Code:

- [VSCode](https://code.visualstudio.com/download)
    * Extensions to be installed in VSCode:
        * Unity
        * C# Dev Kit
        * C#
        * .NET Install Tool
        * Python
        * Pylance


## Unity Student License
Before anything, we need to make sure to have a Unity license. Unity has free [student plans](https://unity.com/products/unity-student) for secondary and post-secondary students.



## Documentation
For the full documentation, please access the Wiki in this repository.
---

_This setup allows drones to receive missions, pick up packages, and deliver them while communicating with Mininet-WiFi._

