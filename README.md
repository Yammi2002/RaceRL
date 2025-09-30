# RaceRL

A simple Unity project demonstrating the **ML-Agents framework**, where a vehicle learns to drive autonomously on a track using Reinforcement Learning.

## Requirements

- [Unity Hub](https://unity.com/download)  
- **Unity Editor 6.2**  
- **Python 3.9**  
- **ML-Agents**

## Setup Instructions

### 1. Clone the repository
git clone https://github.com/Yammi2002/RaceRL.git  
cd RaceRL

### 2. Open the project in Unity
Launch **Unity Hub**  
Click **Add project from disk** and select the cloned folder  
Open the project using **Unity Editor 2022.3 LTS**  

### 3. Set up the ML-Agents Python environment
Open a command prompt or terminal in the project folder  
Create a virtual environment:  
python -m venv venv  
Activate the environment:  
- Windows: venv\Scripts\activate  
- Linux/Mac: source venv/bin/activate  
Upgrade pip if necessary: python -m pip install --upgrade pip  
Install ML-Agents: pip install mlagents  
Install PyTorch: pip install torch torchvision torchaudio  
Test the installation: mlagents-learn --help  
If any errors about missing packages appear, install them using pip  
Run mlagents-learn --help again to confirm it works  

### 4. Train the model
Start training with:  
mlagents-learn Assets/ML-Agents/Configs/car_config.yaml --run-id=<id-run>  
Press **Play** in the Unity Editor scene to see the vehicle moving autonomously
