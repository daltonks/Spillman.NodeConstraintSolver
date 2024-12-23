namespace Spillman.NodeConstraintSolver.Example;

class TileConstraintSolver : ConstraintSolver<Vector2Int, Tile, TileOption>
{
    private readonly TileService _tileService;

    public TileConstraintSolver(TileService tileService)
    {
        _tileService = tileService;
    }

    public IReadOnlyList<TileOption> AllTileOptions { get; set; } = null!;
    
    protected override ref Tile GetNode(ref Vector2Int key)
    {
        return ref _tileService.Tiles[key.X, key.Y];
    }

    protected override IReadOnlyList<TileOption> GetAllOptions(ref Tile tile)
    {
        return AllTileOptions;
    }

    protected override void GetNeighborKeys(Vector2Int key, List<Vector2Int> neighborKeys)
    {
        AddIfExists(key with { X = key.X - 1 });
        AddIfExists(key with { X = key.X + 1 });
        AddIfExists(key with { Y = key.Y - 1 });
        AddIfExists(key with { Y = key.Y + 1 });
        return;

        void AddIfExists(Vector2Int neighborKey)
        {
            if (neighborKey.X >= 0 && neighborKey.X < _tileService.Tiles.GetLength(0) 
                && neighborKey.Y >= 0 && neighborKey.Y < _tileService.Tiles.GetLength(1))
            {
                neighborKeys.Add(neighborKey);
            }
        }
    }
}