using System.Text;

namespace Spillman.NodeConstraintSolver.Example;

class Program
{
    static void Main(string[] args)
    {
        var tileService = new TileService();
        var solver = new TileConstraintSolver(tileService);
        Solve("Checkerboard:", TileOptions.Checkerboard);
        Solve("Dashes Every Other Row:", TileOptions.DashesEveryOtherRow);
        
        return;

        void Solve(string description, IReadOnlyList<TileOption> allTileOptions)
        {
            solver.AllTileOptions = allTileOptions;
            solver.Update(new Vector2Int(0, 0));

            Console.WriteLine(description);
            Console.WriteLine();
            Console.WriteLine(tileService.CreateTilesString());
            Console.WriteLine();
        }
    }
}