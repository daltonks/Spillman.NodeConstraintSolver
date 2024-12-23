using System.Text;

namespace Spillman.NodeConstraintSolver.Example;

class TileService
{
    public readonly Tile[,] Tiles = new Tile[10, 10];

    public string CreateTilesString()
    {
        var stringBuilder = new StringBuilder();
        for (var y = 0; y < Tiles.GetLength(1); y++)
        {
            for (var x = 0; x < Tiles.GetLength(0); x++)
            {
                stringBuilder.Append(Tiles[x, y].SelectedOption!.Character);
            }
            stringBuilder.AppendLine();
        }
        return stringBuilder.ToString();
    }
}