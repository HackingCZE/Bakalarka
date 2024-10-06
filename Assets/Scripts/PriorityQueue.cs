using System;
using System.Collections.Generic;
using System.Linq;

public class PriorityQueue<T>
{
    private SortedDictionary<float, Queue<T>> _elements = new SortedDictionary<float, Queue<T>>();

    public int Count { get; private set; }

    public void Enqueue(T item, float priority)
    {
        if (!_elements.ContainsKey(priority))
        {
            _elements[priority] = new Queue<T>();
        }
        _elements[priority].Enqueue(item);
        Count++;
    }

    public T Dequeue()
    {
        if (Count == 0)
        {
            return default(T);
        }

        var firstPair = _elements.First();
        var item = firstPair.Value.Dequeue();
        if (firstPair.Value.Count == 0)
        {
            _elements.Remove(firstPair.Key);
        }

        Count--;
        return item;
    }

    public bool Contains(T item)
    {
        // Iterate through all the queues in the priority dictionary
        foreach (var queue in _elements.Values)
        {
            if (queue.Contains(item))
            {
                return true;  // Item found
            }
        }
        return false;  // Item not found
    }

}
