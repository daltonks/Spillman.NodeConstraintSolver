# Spillman.NodeConstraintSolver

A C# constraint solver to determine valid options for nodes based on constraints to their neighbors.

This is currently a greedy implementation with no backtracking in the case of contradictions.

Node options are solved using a priority-based system.

Node propagation can naturally stop without traversing all nodes, making this efficient for updating specific nodes.

An [example](src/Spillman.NodeConstraintSolver.Example/Program.cs) project can be found in this repository.

I'm personally using this project to solve tile textures. Click below to see an example video:
[![Watch the video](https://img.youtube.com/vi/EyDWT4BYBDU/maxresdefault.jpg)](https://youtu.be/EyDWT4BYBDU)
