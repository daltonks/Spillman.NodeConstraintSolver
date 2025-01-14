namespace Spillman.NodeConstraintSolver;

public interface INodeOption<TNodeKey, TNode, TNodeOption, TContext>
{
    public int Priority { get; }
    public IReadOnlyList<IConstraint<TNodeKey, TNode, TNodeOption, TContext>> GetConstraints(
        ref TNodeKey nodeKey, 
        ref TNodeKey otherNodeKey
    );
}