#!/bin/bash

# Ensure the script is run with sudo privileges
if [ "$EUID" -ne 0 ]; then
  echo "Please run this script with sudo privileges."
  exit 1
fi

# Stop all running containers
echo "Stopping all running containers..."
sudo docker stop $(sudo docker ps -q)

# Check if there are any containers to stop
if [ $? -eq 0 ]; then
  echo "All running containers have been stopped."
else
  echo "No running containers found."
fi

# Remove all containers (running and stopped)
echo "Removing all containers..."
sudo docker rm $(sudo docker ps -a -q)

# Check if there are any containers to remove
if [ $? -eq 0 ]; then
  echo "All containers have been removed."
else
  echo "No containers found to remove."
fi

echo "Script execution complete."