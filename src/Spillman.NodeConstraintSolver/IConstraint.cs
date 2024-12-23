﻿namespace Spillman.NodeConstraintSolver;

public interface IConstraint<TNodeKey, TNode, TNodeOption>
{
    bool IsMet(
        ref TNodeKey nodeKey,
        ref TNode node,
        ref TNodeKey otherNodeKey,
        ref TNode otherNode,
        ref TNodeOption otherNodeOption
    );
}