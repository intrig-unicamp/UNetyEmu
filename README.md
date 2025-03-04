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



## Unity Basic Setup

Once you have downloaded `UNetyEmuProject.zip` or cloned the repository, you should have a folder `UNetyEmuProject`. Inside this folder, you will find the Unity project named `UNetyEmu`. To open it correctly: 

1. Open `Unity Hub` and log in with your Unity account. Make sure you have `Unity 2022.3.22f1` version previously installed;
![UnityVersion](https://hackmd.io/_uploads/SJJM_YMiyl.png)

2. If you don't find the correct version, please go to the Archive, where you will find Long-Term Support versions, or follow this [link].(https://unity.com/releases/editor/whats-new/2022.3.22)
![2](https://hackmd.io/_uploads/SJLwpamj1l.jpg)

3. Go to `Projects -> Add -> Add project from disk`;
![OpenUnityHub](https://hackmd.io/_uploads/r1PeBtfskl.png)

4. Find the `UNetyEmu` folder, inside of the `UNetyEmuProject` folder. Then, select Open;
![UNetyEmuFolder](https://hackmd.io/_uploads/rJTcHFzokg.png)

5. Once the project has been loaded in Unity Hub, select it to open;
![UNetyEmu_Unity](https://hackmd.io/_uploads/H1pcPFfsye.png)

6. Wait for Unity to open the project;
![OpenUNetyEmu](https://hackmd.io/_uploads/Bky6KYGokl.png)

7. If this is the first time you open the project, you will notice that Unity contains important toolbars and windows. On the left side is the `Hierarchy`, in the center of the screen is the `Scene` and `Game` window, while on the right side is the `Inspector`. Also, at the bottom left are the `Project` folders and to the right is the `Console`, where messages will be displayed during the simulation of the scenarios. If you find any messages about rebuilding libraries or `.meta` files, don't worry, you can simply disregard them and click on the `Clear` button in the `Console`;
![FirstView](https://hackmd.io/_uploads/r1AZqYfsJl.png)

8. In the `Projects` window, find the `Assets / Scenes` folder. Here you will find the three scenarios available in **UNetyEmu**.
![OpenScenarios](https://hackmd.io/_uploads/ry8hnKMoyl.png)

## First Scenario Demo

Open the scene of the `First Scenario`, by double clicking on `Assets / Scenes / First Scenario` in the `Projects` window:

![OpenScenarios1](https://hackmd.io/_uploads/B1J1vcMokl.png)

In the `Scene` window you can view and edit all objects belonging to the scene. If you cannot see them, you can double-click on any object in the `Hierarchy` window, which will zoom in on that object. On the other hand, the `Game` window shows the final result that will be displayed when starting the simulation, according to the main camera view of the scene.

![Scenario_1_1](https://hackmd.io/_uploads/Hy43O9Mokg.png)

As you can see, this scenario shows a small environment with buildings, trees, and static objects. A blue DronePad marks the drone's starting position and orientation, while five red DronePads indicate landing targets. 

This scenario showcases a 360-degree LiDAR sensor, and a depth camera sensor. In addition, we demonstrate the operation of a PID controller that allows the Drone to follow a small white sphere representing its target position and orientation.

### Start the Simulation for the First Scenario

![Play1](https://hackmd.io/_uploads/r1I6ncMi1x.png)

Users can adjust the target's position with the UJHK keys and its altitude/rotation with arrow keys. 


LiDAR detection is shown with red rays, while green rays indicate no obstacles.



The goal is to understand sensor functionality, PID control, and drone maneuvering by landing on red DronePads while avoiding obstacles.



## Second Scenario Demo


Open the scene of the `Second Scenario`, by double clicking on `Assets / Scenes / Second Scenario` in the `Projects` window;

![OpenScenarios2](https://hackmd.io/_uploads/B1-gw9Moyl.png)



### Start the Simulation for the Second Scenario



## Third Scenario Demo

Open the scene of the `Third Scenario`, by double clicking on `Assets / Scenes / Third Scenario` in the `Projects` window;

![OpenScenarios3](https://hackmd.io/_uploads/Byv-PqGiyg.png)


Before we start the simulation connecting both Unity and Mininet-WiFi, we need to make sure some steps were made upfront:

### Setting up Unity + Mininet-Wifi

#### Import the Mininet-WiFi VM

The Mininet-WiFi VM is around 7.4GB and is currently named as `mn-wifi-vm.ova`. To import it, do the following:

1. Open the Oracle VirtualBox Manager;

2. Go to `File -> Import Appliance`;
![{27B5C18D-4CFD-4CE0-85C2-90044EEB12E2}](https://hackmd.io/_uploads/B1WeqICFyx.png)

3. Find and select `mn-wifi-vm.ova` file
![{0EA50F96-2D7D-4BF5-BB4C-5BA2A417D027}](https://hackmd.io/_uploads/BJfmc80Y1g.png)

#### Configuring the Network Adapters

Before we start, some users reported there might be a bug in VirtualBox regarding the number of virtual adapters being limited to 1. To fix this: follow the quoted steps below -- extracted from the following [link](https://superuser.com/questions/732999/virtualbox-cant-add-2nd-adapter-to-network-for-vm).

> _If the VM is not running and is saved, you will need to start it and then in VirtualBox Manager, right click and select `Close > Shut Down`. You cannot add a network adapter in the saved state_.

After we import the OVA:
1. Go to `Settings -> Network` in VirtualBox;
2. Set `Adapter 1` to **Bridged Mode** and **Promiscous Mode: Allow All**:
![{A93DC451-7A6B-426A-9FD3-1E7C73785DCD}](https://hackmd.io/_uploads/r1l5q80Fyx.png)

3. Add `Adapter 2` as **Host-Only Mode** and **Promiscuous Mode: Allow All**:
![{182195D7-CF17-4D90-9AE2-E6FEDD64328E}](https://hackmd.io/_uploads/BkKxj8RKJx.png)

4. Check which network adapter is the Host-Only Network. For instance, in the image below it is `192.168.56.1/24`
![{8EC13C59-A35D-4329-84BB-D9ACC6971A1C}](https://hackmd.io/_uploads/SyqwR_Ct1g.png)

5. Given the Host-Only Network's IP (step 4, Adapter 2), open PowerShell (Unity-side) and add a persistent rule to forward traffic directly through Adapter 1:

```
route -p add 10.0.0.0 mask 255.0.0.0 192.168.56.101 metric 1
```

6. Do the same to allow traffic through Docker built-in interface at the VM:

```
route -p add 172.17.0.0 mask 255.0.0.0 192.168.56.101 metric 1
```

7. Make sure your `route print` looks like this for IPv4 rules:

![image](https://hackmd.io/_uploads/HkBixDfsyl.png)

> NOTE: `192.168.56.101` is the IP of the Host-Only Adapter. Make sure you replace the rules with the correct IP if it differs from the guidelines.

8. Start the VM, open a terminal, and run:
```
ifconfig
```

9. Identify which interface is *Bridged* and *Host-Only*.

![{CABD970F-C785-4F77-852F-A0C622E88519}](https://hackmd.io/_uploads/rJqcktRYJl.png)


> We already identified the Host-Only Network (Step 4) as `192.168.56.1/24`. Consequently, `enp0s8` is the Host-Only virtual interface (`Adapter 2`) where most of the traffic will go through after we install the routing rules on the Host-side - i.e., where Unity is running -, while `enp0s3` is the Bridge Adapter (`Adapter 1`)


#### Set the correct adapter to dynamically retrieve the Bridge IP
Since Adapter 1 is the Bridge and Adapter 2 is the Host-Only. We need to go to `Drone/Assets/Scripts/Network/BaseStation.cs` and set the variable `adapterIndex` as index 1 (Adapter 2), because the iteractive messages to Mininet follow a specific route through this adapter.

> Be sure `vmName` is also correct

![{7DAAE454-5FA2-4C24-A9DB-E0F1EFDFE175}](https://hackmd.io/_uploads/Bym6ak65kg.png)



#### Configure Windows Firewall
Besides the routing configuration at the host (Unity-side) , we have to EITHER `a.` disable the firewall or `b.` create inbound/outbound rules as follows:

a. Disable the Windows Firewall for private networks, or;

b. Create inbound/outbound rules, just like in the example below:

_Example:_
    
- b.1. Open `Windows Defender Firewall -> Advanced Settings`
    ![Screenshot 2025-02-15 183537](https://hackmd.io/_uploads/S1TSKt0F1x.png)

- Add Inbound and Outbound rules for TCP, allowing traffic on all ports.

- b.2. Rule Type as Port:

    ![{7E83C5ED-5FC6-4528-A54C-15B157F9C767}](https://hackmd.io/_uploads/r1kVXRXsJe.png)
    
- b.3. For simplicity, make all ports available:

    ![{BD94E0E0-B2AA-4C99-A803-34BDCEFCDF92}](https://hackmd.io/_uploads/r1VsmR7s1x.png)

- b.4. Mark to `Allow the connection`:
 
![{06029773-E7AA-43D0-9857-6094AF814920}](https://hackmd.io/_uploads/SyKZERmsJe.png)

- b.5. Uncheck Public and keep Domain and Private checked (for security reasons):
![{73ADF637-E5C2-4080-B2B8-9484DE94AFF9}](https://hackmd.io/_uploads/SkT640Xikl.png)

- b.6. Name the rule as `Allow All TCP From VM`. Repeat the procedure for Outbound rules, and name it as `Allow All TCP to VM`.

![{23543D78-BCD6-4978-8228-87F323FADDF0}](https://hackmd.io/_uploads/S1HFr07ikg.png)



_Consider all the steps were made sucessfully in order for the simulation to work!_






### Start the Simulation for the Third Scenario



#### In Unity


The following image summarizes the scenario in which the experiment is performed - that is, Scenario 3. In it, we see a base station represented by a cylinder and different drone pads around it.

> NOTE: If you intend to run more than 3 drones, consider increasing the VM specificatitons such as CPU core numbers and available Memory RAM, because multiple container images might freeze the VM with the built-in hardware configuration -- i.e., 1 Core, 2 GB RAM.

Make sure you selected the correct scene (`ThirdScenario`) at `Assets/Scenes`. At the beginning, two drones start at a starting point (blue pads), and receive a mission to pick up a package (orange pads) and deliver it to their respective destinations (red pads):


> It is worth remembering that the work is still in progress and implementation details in both the graphical, physical and connection areas will be improved.





#### In Mininet-WiFi

1. Copy all scripts from `Drone/Assets/Scripts/Network/mininet` to a custom folder (you name it) in the the VM.
> It does not matter the name of the folder we create and we can place it anywhere inside the VM as desired (e.g., /home/\<user\>/Desktop).

2. Make sure the script `remove_running_containers.sh`, which is among the copied files, has permission to execute:
```
sudo chmod +x remove_running_containers.sh
```

3. (Optional): Sometimes the substring '\r' ends up being copied along with the script, preventing it from being correctly recognized. To avoid this, you can apply a filter with `sed` - e.g., `sed -i 's/\r$//' remove_running_containers.sh`

![{71451E52-1987-40C5-A2A6-A3B2995F3291}](https://hackmd.io/_uploads/HJgyJCmiyl.png)


4. Run:
```
sudo python mininet_topo.py
```

The script will create the topology and wait for Unity messages.
![{3D02CABA-3E1B-4AEB-8A01-4BDF509334D4}](https://hackmd.io/_uploads/Hy8HWeTcJl.png)

> NOTE: After executing the following step on the first execution attempt, an error may occur because the image `ramonfontes/socket_position:python35` is not found and, consequently, downloaded automatically. If you want to avoid this, please download the docker images manually from DockerHub with `docker pull ramonfontes/socket_position:python35`. Otherwise, repeat step 3 above after the automatic download procedure.


#### In Unity

1. Press **Play** in Unity:
![image](https://hackmd.io/_uploads/H17iGe6ckg.png)


#### In Mininet-WiFi

2. Observe virtual drones appearing in the VM, with its positioning being dinamically modified as they move at Unity-side
![{7A5FB944-6A1D-49D8-9F96-567E3A2D0172}](https://hackmd.io/_uploads/HyTMh5yqyg.png)

---

_This setup allows drones to receive missions, pick up packages, and deliver them while communicating with Mininet-WiFi._

