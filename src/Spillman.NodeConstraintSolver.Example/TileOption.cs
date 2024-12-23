namespace Spillman.NodeConstraintSolver.Example;

class TileOption : INodeOption<Vector2Int, Tile, TileOption>
{
    public TileOption(
        char character, 
        int priority, 
        IReadOnlyList<IConstraint<Vector2Int, Tile, TileOption>>? allConstraints = null)
    {
        Character = character;
        Priority = priority;
        AllConstraints = allConstraints ?? Array.Empty<IConstraint<Vector2Int, Tile, TileOption>>();
    }

    public char Character { get; }
    public int Priority { get; }
    public IReadOnlyList<IConstraint<Vector2Int, Tile, TileOption>> AllConstraints { get; }
}