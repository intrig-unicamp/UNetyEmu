# Libraries
import sys
import json
import socket
import threading
import time
import os
from mininet.node import Controller
from containernet.net import Containernet
from containernet.node import DockerSta
from containernet.term import makeTerm
from mn_wifi.link import wmediumd
from mininet.log import setLogLevel, info, debug
from mn_wifi.wmediumdConnector import interference
from mn_wifi.telemetry import telemetry
from containernet.cli import CLI
import subprocess
import math

# -----------------------------------------------------------------------------------------------------

# Class to convert Unity coordinates to Mininet-WiFi coordinates
def ConvertUnityPositionToMininetWIFI(unity_position):
    """Convert Unity coordinates (x, y, z) to Mininet-WiFi coordinates (x, z, y)."""
    x, y, z = unity_position
    return (x, z, y)

# -----------------------------------------------------------------------------------------------------

# Calculate the maximum distance between the ap and the drones to set the coverage radius dinamically
def calculate_max_distance(ap_position, drone_positions):
    """Calculate the maximum distance from AP to any drone."""
    max_distance = 0
    for drone in drone_positions:
        drone_pos = drone["position"]
        distance = math.sqrt(
            (ap_position[0] - drone_pos[0]) ** 2 +
            (ap_position[1] - drone_pos[1]) ** 2 + 
            (ap_position[2] - drone_pos[2]) ** 2
        )
        max_distance = max(max_distance, distance)
    return max_distance

# -----------------------------------------------------------------------------------------------------

# Send the coverage range to Unity
def send_coverage_range(radius, client_socket):
    """Send the calculated coverage range to Unity."""
    try:
        range_message = json.dumps({"coverageRadius": radius})
        print(f"Message to be sent to Unity: {range_message}")
        client_socket.sendall(range_message.encode("utf-8"))
        info(f"Coverage radius {radius} sent to Unity\n")
    except Exception as e:
        info(f"Error sending coverage range: {e}\n")

# -----------------------------------------------------------------------------------------------------

# Process the initial positions received from Unity
def process_positions_once(host, port):
    """Receive initial positions from Unity."""
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_socket.bind((host, port))
    server_socket.listen(5)
    info(f"Listening for position updates from Unity on {host}:{port}\n")

    positions = {}
    
    try:
        
        # Accept connection from Unity
        client_socket, addr = server_socket.accept()
        info(f"Connection from {addr}\n")

        data = client_socket.recv(4096)
        info(f"Data length received from Unity: {len(data)}\n")
        
        print("(... _once) data", data.decode())

        json_data = json.loads(data.decode())

        # Extract the base station position
        if "baseStation" in json_data:
            base_station = json_data["baseStation"]
            base_station_id = base_station["id"]
            base_station_position = base_station["position"]
            positions["ap_position"] = ConvertUnityPositionToMininetWIFI(
                (
                    base_station_position["x"],
                    base_station_position["y"],
                    base_station_position["z"],
                )
            )
            positions["ap_id"] = base_station_id
            info(f"(... _once) BaseStation ID: {base_station_id}, Position: {positions['ap_position']}\n")

        # Extract the drone positions
        if "drones" in json_data:
            positions["drone_positions"] = []
            for drone in json_data["drones"]:
                drone_id = drone["id"]
                drone_position = drone["position"]
                positions["drone_positions"].append(
                    {
                        "id": drone_id,
                        "position": ConvertUnityPositionToMininetWIFI(
                            (
                                drone_position["x"],
                                drone_position["y"],
                                drone_position["z"],
                            )
                        ),
                    }
                )
            info(f"(... _once) Drone positions parsed: {positions['drone_positions']}\n")

        info("(... _once) Positions received and converted\n")

    except json.JSONDecodeError as json_error:
        info(f"Error decoding JSON data: {json_error}\n")
    except Exception as e:
        info(f"Error receiving position: {e}\n")

    return client_socket, positions

# -----------------------------------------------------------------------------------------------------

# Get the bridge IP address
def get_bridge_ip():
    try:
        # Get the default route interface name
        route_output = subprocess.check_output("ip route | grep default", shell=True, text=True)
        interface = route_output.split("dev")[1].split()[0]

        # Get the IP address assigned to this interface
        ip_output = subprocess.check_output(f"ip -4 addr show {interface} | grep 'inet '", shell=True, text=True)
        host_ip = ip_output.split()[1].split('/')[0]  # Extract the IP address part

        return host_ip
    except Exception as e:
        print(f"Error retrieving bridge IP: {e}")
        return None

# -----------------------------------------------------------------------------------------------------

# Main function to create the topology
def minimal_topology():
    
    # Get the path of the current file
    os.system('iptables -A FORWARD -d 172.17.0.0/16 -j ACCEPT')
    path = os.path.dirname(os.path.abspath(__file__))

    # Set the host_ip variable dynamically
    host_ip = get_bridge_ip()
    print(f"Bridge IP: {host_ip}")

    # Create mininet object that supports containernet
    net = Containernet(link=wmediumd, wmediumd_mode=interference,
                       noise_th=-91, fading_cof=3)

    info("* Receiving initial positions for AP and drones\n")
    client_socket, positions = process_positions_once(host="0.0.0.0", port=12345)
    ap_position = positions.get("ap_position")
    ap_id = positions.get("ap_id")
    drone_positions = positions.get("drone_positions", [])

    info(f"Base station ID: {ap_id}, Position: {ap_position}\n")

    # Create the access point with the received position and ID
    ap1 = net.addAccessPoint(
        f"{ap_id}",
        mac="00:00:00:00:00:02",
        ssid="handover",
        failMode="standalone",
        mode="g",
        channel="1",
        position=ap_position
    )

    drone_last_ip_octect_fixed = 101 # last octect is fixed. For instance, "10.0.0." + 101 = "10.0.0.101" | "10.0.0." + 101 + 1 = "10.0.0.102"
    
    # Create the drones with the received positions
    for i, drone_data in enumerate(drone_positions):
        drone_id = drone_data["id"]
        pos = drone_data["position"]
        info(f"Initial position for drone {drone_id}: {pos}\n")
        drone_name = f"{drone_id}"

        net.addStation(drone_name, position=",".join(map(str,pos)), ip=f"10.0.0.{drone_last_ip_octect_fixed + i}",
                        cls=DockerSta, volumes=[f"{path}:/root"], dimage="ramonfontes/socket_position:python35",
                        cpu_shares=20, mac=f"00:02:00:00:00:{int(i)+10:02}")

        print("IP FIX:", drone_last_ip_octect_fixed + i)
        info(f"Created drone {drone_name} at position {pos}\n")


    info('*** Configuring WiFi nodes\n')
    net.configureWifiNodes()

    info('*** Starting network\n')
    net.build()
    net.addNAT().configDefault()
    ap1.start([])

    # Compute max distance from AP to drones
    max_distance = calculate_max_distance(ap_position, drone_positions)
    
    # Set power dynamically based on max distance
    power_dbm = 10 + (10 * 1.5 * math.log10(max_distance + 10))  # Adjusted power
    ap1.wintfs[0].txpower = min(power_dbm, 30)  # Cap at 30 dBm (max WiFi power)

    info(f"Updated AP power to {ap1.wintfs[0].txpower} dBm for range {max_distance + 50}m\n")

    # Send coverage radius back to Unity
    send_coverage_range(max_distance + 10, client_socket)

    info("*** Drone names ***\n")
    for drone in drone_positions:
        info(f"Drone {drone['id']} created\n")

    net.socketServer(ip=host_ip, port=12346)

    nodes = net.stations + net.aps
    net.telemetry(nodes=nodes, data_type="position", min_x=-100, max_x=700, min_y=-100, max_y=700)

    # Receiver call
    for drone in net.stations:
        makeTerm(drone, title=f'dr+{drone} sniffer', cmd=f"bash -c 'python3.5 /root/sniffer_container.py {host_ip};'")

    info('*** Running CLI\n')
    CLI(net)


    # If the experiment does not finalizes as it should, containers might keep running. So, we manually ensure they stop running
    info('*** Executing post-CLI process\n')
    subprocess.run(["sudo", "./remove_running_containers.sh"])  # Replace with your script

    info('*** Stopping network\n')
    net.stop()

# -----------------------------------------------------------------------------------------------------

# Main function
if __name__ == "__main__":
    setLogLevel('info')
    minimal_topology()
