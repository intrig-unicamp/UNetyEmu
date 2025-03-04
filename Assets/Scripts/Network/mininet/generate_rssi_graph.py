# Libraries
import os
import csv
import matplotlib.pyplot as plt

# -----------------------------------------------------------------------------------------------------

def read_rssi_files(directory):
    """Reads all CSV files in the directory that start with 'DRO' and ends with '.csv'."""
    rssi_data = {}
    for filename in os.listdir(directory):
        if filename.startswith('DRO') and filename.endswith('.csv'):
            file_path = os.path.join(directory, filename)
            with open(file_path, newline='') as csvfile:
                reader = csv.reader(csvfile)
                next(reader)  # Skip the header
                rssi_values = [int(row[1]) for row in reader if row]
                rssi_data[filename.split('.')[0]] = rssi_values  # Use the filename (without .csv) as the key
    return rssi_data

# -----------------------------------------------------------------------------------------------------

def plot_rssi_data(rssi_data, output_image_path):
    """Plot RSSI data for all drones and save the image."""
    plt.figure(figsize=(10, 6))  # Set the figure size

    # Plot a line for each drone
    for drone_name, rssi_values in rssi_data.items():
        plt.plot(range(len(rssi_values)), rssi_values, label=drone_name)

    # Customize plot
    plt.title('RSSI Data for Drones')
    plt.xlabel('Time (s)')
    plt.ylabel('RSSI (dBm)')
    plt.legend()
    plt.grid(True)

    # Save the plot as an image
    plt.savefig(output_image_path)
    plt.close()  # Close the plot to avoid memory issues

    print(f"Plot saved as {output_image_path}")

# -----------------------------------------------------------------------------------------------------

def main():
    input_directory = './'  # Set the directory containing the CSV files
    output_image_path = 'merged_rssi_plot.png'  # Output image path

    # Step 1: Read all RSSI CSV files in the directory
    rssi_data = read_rssi_files(input_directory)

    # Step 2: Plot the RSSI data and save as an image
    plot_rssi_data(rssi_data, output_image_path)

# -----------------------------------------------------------------------------------------------------

if __name__ == "__main__":
    main()
