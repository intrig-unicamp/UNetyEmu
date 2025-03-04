# Libraries
import sys
from scapy.all import send, TCP, IP, Raw, Ether

# -----------------------------------------------------------------------------------------------------

# Function to send a TCP packet
def send_tcp_packet(dst_ip, dst_port, message):
    
    try:
        
        # Construct TCP packet with RAW payload
        packet = (
            IP(dst=dst_ip) /
            TCP(sport=55555, dport=int(dst_port)) /
            Raw(load=message.encode())
        )
        
        # Send the packet
        print(f"Sending TCP packet to {dst_ip}:{dst_port} with message: {message}")
        send(packet, verbose=True)
        packet.show()
                
    except Exception as e:
        print(f"Error sending TCP packet: {e}")

# -----------------------------------------------------------------------------------------------------

# Main function
if __name__ == "__main__":
    
    # Check if the number of arguments is correct
    if len(sys.argv) < 4:
        print("Usage: python broker.py <dst_ip> <dst_port> <message>")
        sys.exit(1)
    
    # Extract arguments
    dst_ip = sys.argv[1]
    dst_port = int(sys.argv[2])
    message = sys.argv[3]

    # Print the received message
    print(f"(BROKER-TCP-SENDER) Received message from Unity: {message}")

    # Send the TCP packet
    send_tcp_packet(dst_ip, dst_port, message)
