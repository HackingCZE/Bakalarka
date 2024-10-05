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
}
