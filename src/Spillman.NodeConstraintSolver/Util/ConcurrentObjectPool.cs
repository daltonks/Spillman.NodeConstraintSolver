using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Spillman.NodeConstraintSolver.Util;

internal class ConcurrentObjectPool<T>(
    Func<T> createNew, 
    Action<T> clear
) where T : class
{
    private readonly ConcurrentBag<T> _bag = [];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Get()
    {
        if (!_bag.TryTake(out var item))
        {
            item = createNew();
        }

        return item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Recycle(T item)
    {
        clear(item);
        _bag.Add(item);
    }
}