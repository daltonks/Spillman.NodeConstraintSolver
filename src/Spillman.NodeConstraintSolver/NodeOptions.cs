namespace Spillman.NodeConstraintSolver;

public class NodeOptions<TNodeKey, TNode, TNodeOption, TContext> where TNodeOption : INodeOption<TNodeKey, TNode, TNodeOption, TContext>
{
    public NodeOptions(params TNodeOption[] options)
    {
        if (options.Length == 0)
        {
            throw new Exception($"At least 1 option must be provided for NodeOptions");
        }
        
        Options = options
            .OrderByDescending(c => c.Priority)
            .ToList();
    }
    
    public readonly IReadOnlyList<TNodeOption> Options;
}