namespace Spillman.NodeConstraintSolver.Example;

class LambdaConstraint : IConstraint<Vector2Int, Tile, TileOption>
{
    public delegate bool IsMetDelegate(
        ref Vector2Int nodeKey, ref Tile node,
        ref Vector2Int otherNodeKey, ref Tile otherNode, ref TileOption otherNodeOption);
    
    private readonly IsMetDelegate _isMet;
    
    public LambdaConstraint(IsMetDelegate isMet)
    {
        _isMet = isMet;
    }
    
    public bool IsMet(
        ref Vector2Int nodeKey, ref Tile node, 
        ref Vector2Int otherNodeKey, ref Tile otherNode, ref TileOption otherNodeOption)
    {
        return _isMet(ref nodeKey, ref node, ref otherNodeKey, ref otherNode, ref otherNodeOption);
    }
}