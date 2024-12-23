namespace Spillman.NodeConstraintSolver.Example;

struct Tile : INode<TileOption>
{
    public TileOption? SelectedOption { get; set; }
}