namespace Spillman.NodeConstraintSolver.Example;

static class TileOptions
{
    public static readonly IReadOnlyList<TileOption> Checkerboard =
    [
        new(
            'X',
            priority: 1,
            [
                new LambdaConstraint(
                    (ref Vector2Int nodeKey, ref Tile node,
                            ref Vector2Int otherNodeKey, ref Tile otherNode, ref TileOption otherNodeOption) 
                        => otherNodeOption.Character == 'O')
            ]
        ),
        new(
            'O',
            priority: 0
        )
    ];
    
    public static readonly IReadOnlyList<TileOption> DashesEveryOtherRow =
    [
        new(
            '-',
            priority: 1,
            [
                new LambdaConstraint(
                    (ref Vector2Int nodeKey, ref Tile node,
                            ref Vector2Int otherNodeKey, ref Tile otherNode, ref TileOption otherNodeOption) 
                        => nodeKey.Y % 2 == 0)
            ]
        ),
        new(
            ' ',
            priority: 0
        )
    ];
}