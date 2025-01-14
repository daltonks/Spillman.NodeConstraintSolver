using System.Diagnostics;
using System.Text;
using Spillman.NodeConstraintSolver.Util;

namespace Spillman.NodeConstraintSolver;

public abstract class ConstraintSolver<TNodeKey, TNode, TNodeOption, TContext> 
    where TNodeKey : notnull 
    where TNode : INode<TNodeOption>
    where TNodeOption : INodeOption<TNodeKey, TNode, TNodeOption, TContext>
{
    private static readonly ConcurrentObjectPool<List<TNodeOption>> NodeOptionsPool = new(
        () => [],
        list => list.Clear()
    );
    
    private static readonly ConcurrentObjectPool<List<TNodeKey>> NeighborKeyPool = new(
        () => [],
        list => list.Clear()
    );
    
    private readonly Queue<TNodeKey> _queue = new();
    private HashSet<TNodeKey> _setOfItemsInQueue = null!;
    private Dictionary<TNodeKey, List<TNodeOption>> _allPossibilities = null!;
    private Dictionary<TNodeKey, List<TNodeKey>> _solvedByExistingNodeOption = null!;
    private HashSet<TNodeKey> _solved = null!;
    private TContext _context = default!;
    
    #if DEBUG
    private readonly StringBuilder _debugStringBuilder = new();
    #endif
    
    public bool DebugLogging { get; set; }
    
    protected abstract ref TNode GetNode(ref TNodeKey key);
    protected abstract IReadOnlyList<TNodeOption> GetAllOptions(ref TNode node);
    protected abstract void GetNeighborKeys(TNodeKey key, List<TNodeKey> neighborKeys);
    
    public void Update(TNodeKey key, TContext context)
    {
        AppendDebugLine("####################################################################################");
        _context = context;
        _setOfItemsInQueue = [];
        _allPossibilities = new Dictionary<TNodeKey, List<TNodeOption>>();
        _solvedByExistingNodeOption = new Dictionary<TNodeKey, List<TNodeKey>>();
        _solved = [];
        
        TryQueue(key);
        ProcessQueue();

        foreach (var list in _allPossibilities.Values)
        {
            NodeOptionsPool.Recycle(list);
        }

        foreach (var list in _solvedByExistingNodeOption.Values)
        {
            NeighborKeyPool.Recycle(list);
        }
        
        #if DEBUG
        if (DebugLogging)
        {
            Debug.WriteLine(_debugStringBuilder.ToString());
            _debugStringBuilder.Clear();
        }
        #endif

        _context = default;
    }
    
    private readonly List<TNodeOption> _possibilitiesToRemove = [];
    private readonly List<TNodeKey> _neighborKeys = [];
    private readonly List<Connection> _connections = [];
    private void NarrowPossibilities(ref TNodeKey nodeKey)
    {
        AppendDebugLine("-----------------------------------------------------------------------------------");
        AppendDebugLine($"{nodeKey} narrowing possibilities");
        
        _possibilitiesToRemove.Clear();
        _neighborKeys.Clear();
        _connections.Clear();
        
        ref var node = ref GetNode(ref nodeKey);
        GetNeighborKeys(nodeKey, _neighborKeys);
        for (var i = 0; i < _neighborKeys.Count; i++)
        {
            _connections.Add(new Connection(_neighborKeys[i]));
        }
        
        var possibilitiesChanged = false;
        
        if (!_allPossibilities.TryGetValue(nodeKey, out var possibilities))
        {
            AppendDebugLine($"{nodeKey} adding all possibilities");
            possibilities = _allPossibilities[nodeKey] = NodeOptionsPool.Get();
            possibilities.AddRange(GetAllOptions(ref node));
            possibilitiesChanged = true;
        }

        AppendDebugLine("");
        
        int? highestPossibilityPriority = null;
        
        for (var i = 0; i < possibilities.Count; i++)
        {
            var option = possibilities[i];
            for (var j = 0; j < _connections.Count; j++)
            {
                var connection = _connections[j];
                ref var otherNode = ref GetNode(ref connection.OtherNodeKey);
                connection.ConnectionResult = GetConnectionResult(
                    ref nodeKey, ref node, ref option,
                    ref connection.OtherNodeKey, ref otherNode);
            }

            var connectsToNeighbors = true;
            var possiblyConnectsToNeighbors = true;
            for (var j = 0; j < _connections.Count; j++)
            {
                var connection = _connections[j];
                switch (connection.ConnectionResult)
                {
                    case ConnectionResult.NoConnection:
                        connectsToNeighbors = false;
                        possiblyConnectsToNeighbors = false;
                        break;
                    case ConnectionResult.ConnectionPossible:
                        connectsToNeighbors = false;
                        break;
                }
            }

            #if DEBUG
            if (DebugLogging)
            {
                AppendDebugLine($"Option {option}:");
                for (var j = 0; j < _connections.Count; j++)
                {
                    var connection = _connections[j];
                    AppendDebugLine($"  {connection.OtherNodeKey}: {connection.ConnectionResult}");
                }
                AppendDebugLine("");
            }
            #endif
            
            if (connectsToNeighbors && (highestPossibilityPriority is null || option.Priority >= highestPossibilityPriority))
            {
                AppendDebugLine($"{nodeKey} solved");
                _solved.Add(nodeKey);
                for (var j = 0; j < _connections.Count; j++)
                {
                    var connection = _connections[j];
                    if (connection.ConnectionResult == ConnectionResult.ConnectedThroughExistingNodeOption)
                    {
                        if (!_solvedByExistingNodeOption.TryGetValue(connection.OtherNodeKey, out var solvedByExistingNodeOptionList))
                        {
                            solvedByExistingNodeOptionList = _solvedByExistingNodeOption[connection.OtherNodeKey] 
                                = NeighborKeyPool.Get();
                        }
                        solvedByExistingNodeOptionList.Add(nodeKey);
                    }
                }
                NodeOptionsPool.Recycle(possibilities);
                _allPossibilities.Remove(nodeKey);
                if (!option.Equals(node.SelectedOption))
                {
                    node.SelectedOption = option;
                    if (_solvedByExistingNodeOption.TryGetValue(nodeKey, out var solvedByExistingNodeOptionList))
                    {
                        for (var j = 0; j < solvedByExistingNodeOptionList.Count; j++)
                        {
                            var otherNodeKey = solvedByExistingNodeOptionList[j];
                            _solved.Remove(otherNodeKey);
                        }

                        solvedByExistingNodeOptionList.Remove(nodeKey);
                        if (solvedByExistingNodeOptionList.Count == 0)
                        {
                            _solvedByExistingNodeOption.Remove(nodeKey);
                        }
                    }
                    
                    QueueNeighbors();
                }
                else
                {
                    QueueNeighborsWithPossibilities();
                }

                return;
            }
            
            if (possiblyConnectsToNeighbors)
            {
                highestPossibilityPriority ??= option.Priority;
            }
            else
            {
                _possibilitiesToRemove.Add(option);
            }
        }
        
        if (_possibilitiesToRemove.Count > 0)
        {
            for (var i = 0; i < _possibilitiesToRemove.Count; i++)
            {
                var possibilityToRemove = _possibilitiesToRemove[i];
                possibilities.Remove(possibilityToRemove);
                AppendDebugLine($"Removing possibility {possibilityToRemove}");
            }

            possibilitiesChanged = true;
        }
        
        if (possibilitiesChanged && possibilities.Count > 0)
        {
            QueueNeighbors();
        }

        return;

        void QueueNeighbors()
        {
            for (var j = 0; j < _connections.Count; j++)
            {
                var connection = _connections[j];
                TryQueue(connection.OtherNodeKey);
            }
        }

        void QueueNeighborsWithPossibilities()
        {
            for (var j = 0; j < _connections.Count; j++)
            {
                var connection = _connections[j];
                if (_allPossibilities.ContainsKey(connection.OtherNodeKey))
                {
                    TryQueue(connection.OtherNodeKey);
                }
            }
        }
    }
    
    private ConnectionResult GetConnectionResult(
        ref TNodeKey nodeKey,
        ref TNode node,
        ref TNodeOption nodeOption,
        ref TNodeKey otherNodeKey,
        ref TNode otherNode)
    {
        if (_solved.Contains(otherNodeKey))
        {
            return AreConstraintsSatisfied(
                ref nodeKey, ref node, nodeOption, 
                ref otherNodeKey, ref otherNode, otherNode.SelectedOption!
            ) 
                ? ConnectionResult.ConnectedThroughSolvedNode 
                : ConnectionResult.NoConnection;
        }
        
        if (_allPossibilities.TryGetValue(otherNodeKey, out var otherOptionsList))
        {
            for (var i = 0; i < otherOptionsList.Count; i++)
            {
                var otherOption = otherOptionsList[i];
                if (AreConstraintsSatisfied(
                    ref nodeKey, ref node, nodeOption, 
                    ref otherNodeKey, ref otherNode, otherOption))
                {
                    return ConnectionResult.ConnectedThroughAllPossibilities;
                }
            }
        }
        else
        {
            if (otherNode.SelectedOption is not null)
            {
                if (AreConstraintsSatisfied(
                    ref nodeKey, ref node, nodeOption, 
                    ref otherNodeKey, ref otherNode, otherNode.SelectedOption
                ) )
                {
                    return ConnectionResult.ConnectedThroughExistingNodeOption;
                }
            }
            
            var otherOptions = GetAllOptions(ref otherNode);
            for (var i = 0; i < otherOptions.Count; i++)
            {
                var otherOption = otherOptions[i];
                if (AreConstraintsSatisfied(
                    ref nodeKey, ref node, nodeOption, 
                    ref otherNodeKey, ref otherNode, otherOption))
                {
                    return ConnectionResult.ConnectionPossible;
                }
            }
        }
        
        return ConnectionResult.NoConnection;
    }
    
    private bool AreConstraintsSatisfied(
        ref TNodeKey nodeKey,
        ref TNode node,
        TNodeOption nodeOption,
        ref TNodeKey otherNodeKey,
        ref TNode otherNode,
        TNodeOption otherNodeOption)
    {
        var constraints = nodeOption.GetConstraints(ref nodeKey, ref otherNodeKey);
        for (var i = 0; i < constraints.Count; i++)
        {
            var constraint = constraints[i];
            if (!constraint.IsMet(ref nodeKey, ref node, ref otherNodeKey, ref otherNode, ref otherNodeOption, _context))
            {
                return false;
            }
        }

        var otherConstraints = otherNodeOption.GetConstraints(ref otherNodeKey, ref nodeKey);
        for (var i = 0; i < otherConstraints.Count; i++)
        {
            var otherConstraint = otherConstraints[i];
            if (!otherConstraint.IsMet(ref otherNodeKey, ref otherNode, ref nodeKey, ref node, ref nodeOption, _context))
            {
                return false;
            }
        }

        return true;
    }

    private void TryQueue(TNodeKey key)
    {
        if (_solved.Contains(key))
        {
            AppendDebugLine($"{key} already solved, not queueing");
            return;
        }
        
        if(!_setOfItemsInQueue.Contains(key))
        {
            _queue.Enqueue(key);
            _setOfItemsInQueue.Add(key);
            AppendDebugLine($"{key} queued");
        }
        else
        {
            AppendDebugLine($"{key} already queued, not queueing");
        }
    }
    
    private void ProcessQueue()
    {
        while (_queue.TryDequeue(out var nodeKey))
        {
            _setOfItemsInQueue.Remove(nodeKey);
            NarrowPossibilities(ref nodeKey);
        }
    }
    
    [Conditional("DEBUG")]
    private void AppendDebugLine(string str)
    {
        #if DEBUG
        if (DebugLogging)
        {
            _debugStringBuilder.AppendLine(str);
        }
        #endif
    }

    public class Connection
    {
        public Connection(TNodeKey otherNodeKey)
        {
            OtherNodeKey = otherNodeKey;
        }
        
        public TNodeKey OtherNodeKey;
        public ConnectionResult ConnectionResult = ConnectionResult.NotCalculated;
    }
}

public enum ConnectionResult
{
    NotCalculated,
    ConnectedThroughSolvedNode,
    ConnectedThroughAllPossibilities,
    ConnectedThroughExistingNodeOption,
    ConnectionPossible,
    NoConnection
}