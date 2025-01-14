namespace Spillman.NodeConstraintSolver;

public interface INodeOption<TNodeKey, TNode, TNodeOption, TContext>
{
    public int Priority { get; }
    public IReadOnlyList<IConstraint<TNodeKey, TNode, TNodeOption, TContext>> AllConstraints { get; }
}