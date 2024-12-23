namespace Spillman.NodeConstraintSolver;

public interface INode<TNodeOption>
{
    public TNodeOption? SelectedOption { get; set; }
}