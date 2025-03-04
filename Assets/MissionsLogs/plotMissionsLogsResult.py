# Libraries
import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
import os
import seaborn as sns

# -----------------------------------------------------------------------------------------------------

# CSV file name
csv_filename = "MissionsLogs_20250303_222334.csv"

# CSV file path
file_path = os.getcwd() + "/"
csv_file = file_path + csv_filename

# Read the CSV file into a DataFrame
df = pd.read_csv(csv_file)

# Display the first few rows of the DataFrame
print(df.head())

# -----------------------------------------------------------------------------------------------------

# Replace NaN values in MissionId with "NoMission"
df['MissionId'] = df['MissionId'].fillna('NoMission')

# Replace NaN values in MissionStatus with "NoMissionStatus"
df['MissionStatus'] = df['MissionStatus'].fillna('NoMissionStatus')

# Remove rows with MissionId 'NoMission'
df = df[df['MissionId'] != 'NoMission']

# Categorize MissionId in the order they appear in the CSV
df['MissionId'] = pd.Categorical(df['MissionId'], categories=df['MissionId'].unique(), ordered=True)

# Convert CurrentTime to datetime for better plotting
df['CurrentTime'] = pd.to_datetime(df['CurrentTime'], format='%H:%M:%S', errors='coerce')

# Sort data by MissionId, PlayerName, and CurrentTime
df.sort_values(by=['MissionId', 'PlayerName', 'CurrentTime'], inplace=True)

# Calculate battery consumption for each state by taking the absolute difference in BatteryLevel
df['BatteryConsumption'] = df.groupby(['MissionId', 'PlayerName'], observed=False)['BatteryLevel'].diff().abs()

# Fill NaN values (which occur in the first row of each group) with 0
df['BatteryConsumption'] = df['BatteryConsumption'].fillna(0)

# Group by MissionId, PlayerName, and CurrentState to get total battery consumption per state
battery_usage = df.groupby(['MissionId', 'PlayerName', 'CurrentState'], observed=False)['BatteryConsumption'].sum().reset_index()

# Pivot the table for easier plotting
pivot_table = battery_usage.pivot_table(index=['PlayerName'], columns='CurrentState', values='BatteryConsumption', fill_value=0, observed=False)

# -----------------------------------------------------------------------------------------------------

# Predefined colors for each state
classic_colors = ['red', 'blue', 'green', 'brown', 'orange', 'purple', 'black', 'pink', 'magenta', 'yellow', 'cyan']

# Assign colors to each state
state_colors = {
    'StandBy': classic_colors[0],
    'TakeOff': classic_colors[1],
    'MoveToPickupPackage': classic_colors[2],
    'MoveToCheckPoint': classic_colors[3],
    'MoveToDelivery': classic_colors[4],
    'Land': classic_colors[5],
    'PickUpPackage': classic_colors[6],
    'DeliverPackage': classic_colors[7],
    'ReturnToHub': classic_colors[8]
}

# -----------------------------------------------------------------------------------------------------

# Filter by player name, current time, and altitude
df = df[['PlayerName', 'CurrentTime', 'Altitude']]

# Create a new column for the relative time
df['RelativeTime'] = (pd.to_datetime(df['CurrentTime'], format='%H:%M:%S') - pd.to_datetime(df['CurrentTime'].iloc[0], format='%H:%M:%S')).dt.total_seconds()

# Plot the altitude of each drone over time
plt.figure(figsize=(10, 6))
for drone in df['PlayerName'].unique():
    drone_data = df[df['PlayerName'] == drone]
    plt.plot(drone_data['RelativeTime'], drone_data['Altitude'], linewidth=1.5, label=drone)

# Add labels and legend
plt.title('Altitudes during drone flight')
plt.xlabel('Time (s)')
plt.ylabel('Altitude (m)')
plt.legend()
plt.grid(True, linestyle='-', color='gray', alpha=0.5)

# -----------------------------------------------------------------------------------------------------

# Create a stacked bar plot for battery consumption
fig, ax = plt.subplots(figsize=(12, 6))

# Plot the stacked bar plot
pivot_table.plot(kind='bar', stacked=False, color=[state_colors.get(state, 'gray') for state in pivot_table.columns], ax=ax)

# Set labels and title
ax.set_ylabel('Battery Consumption (%)')
ax.set_xlabel('')
ax.set_title('Battery Consumption by Drone and Mission Status')
ax.legend(bbox_to_anchor=(1.05, 1), loc='upper left')
ax.set_xticklabels(pivot_table.index, rotation=0, ha='right')
ax.grid(True, linestyle='-', color='gray', alpha=0.5)

# Adjust layout for the bar plot
plt.tight_layout()

# -----------------------------------------------------------------------------------------------------

# Display all plots
plt.show()
