namespace Spillman.NodeConstraintSolver;

public interface INodeOption<TNodeKey, TNode, TNodeOption>
{
    public int Priority { get; }
    public IReadOnlyList<IConstraint<TNodeKey, TNode, TNodeOption>> AllConstraints { get; }
}