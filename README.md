## Frozen lake :snowflake:

Frozen lake is a Unity Environment created for training reinforcement learning agents.
The goal of the agent (cube) is to reach its target (sphere) and not to drown while doing it. 
The implementation was inspired by OpenAI's gym.

### Features

:white_check_mark: Includes a trained model for experiments\
:white_check_mark: Can be used with both Unity ML-Agents Trainer and its python low level API directly\
:white_check_mark: A prefab was created from the environment which allows it to be duplicated for faster training\
:white_check_mark: Random positions of the agent, its target and holes in the ice to prevent the agent from learning wrong patterns 

![](frozen_lake.png)

### Environment description
The environment is a for `4 by 4` grid where the agent uses continuous actions to move.
Positions of the agent and its target are set randomly at the beginning of each episode.
Holes in the ice are set randomly across the whole grid except positions occupied by the agent and its target.

Agent observations:
- `X` and `Z` coordinates of the agent itself and the target
- "one-hot" vector of 16 values with ones for indices of absent blocks (holes)

The agent uses discrete actions to navigate itself. The environment can be tested manually in `Heuristic`
mode by using standard WASD key combination. 

The agent will receive a `reward` of 1 once reached the target and a reward of -1 when falling in a hole or from the edge of the play area.
It will also continuously get a bit of negative reward to prevent it from doing nothing.

### Installation
1. Install Unity editor. Compatible version is 2021.3.25f1
2. Install ML-agents package version 2.2.1-exp.1 using Package Manager
3. Install requirements
```
    python -m venv env
    source env/bin/activate
    pip install --upgrade pip
    pip install -r requirements.txt
```

### Usage
Run the following command to start training an agent:

`mlagents-learn config/MoveToGoal.yaml --run-id=test_run`

### TO DO
- Add agent's velocity (X, Z) to observations to prevent the agent from not being able to decide in which direction to go
