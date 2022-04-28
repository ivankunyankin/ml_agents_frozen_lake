## Frozen lake :snowflake:

Frozen lake is a Unity Environment created for training reinforcement learning agents.
The goal of the agent (cube) is to reach its target (sphere) and not to drown while doing it. 
The implementation was inspired by OpenAI's gym.

### Features
- Can be used with both Unity ML-Agents Trainer and its python low level API directly
- A prefab was created from the environment which allows it to be duplicated for faster training
- Random positions of the agent, its target and holes in the ice to prevent the agent from learning wrong patterns
- Number of holes is a parameter. It allows to start training an agent in simple conditions and gradually increase difficulty 

![](frozen_lake.png)

### Environment description
The environment is a for 4 by 4 grid where the agent uses continuous actions to move.
Positions of the agent and its target are set randomly inside the first and the last row of the grid respectively at the begging of each episode.
Holes in the ice are set randomly across the hole grid except positions occupied by the agent and its target.

*Actions*\
The action space represents an array of 2 floats. One for each direction (X and Z axis)

*Observations*\
As observations the agent receives and array of 14 floats. The first four are X and Z coordinates of the agent itself and the target.
The last ten are the X and Y coordinates of ice holes.\
*Important*. If the number of holes was set less than 5, then instead of the absent holes coordinates the observation vector will be padded by zeros.

*Rewards*\
The agent will receive a reward of 1 once reached the target and a reward of -1 when falling in a hole or from the edge of the grid.

### Installation
1. Install Unity editor. Compatible version is 2020.3.32f1
2. Install ML-agents package version 2.2.1-exp.1 with Package Manager
3. Install ML agents python package version 0.28.0
```
    python -m venv env
    source env/bin/activate
    pip install --upgrade pip
    pip install mlagents==0.28.0
```
