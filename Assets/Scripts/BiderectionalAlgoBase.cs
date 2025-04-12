using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public abstract class BiderectionalAlgoBase : AlgoBase
{
    public List<AlgoNode> forwardPath = new List<AlgoNode>();
    public List<AlgoNode> backwardPath = new List<AlgoNode>();

    protected Dictionary<AlgoNode, AlgoNode> _forwardParents;
    protected Dictionary<AlgoNode, AlgoNode> _backwardParents;

    public override Task<List<AlgoNode>> StartAlgo(AlgoNode startNode, AlgoNode endNode, List<AlgoNode> graph, IDrawingNode drawingNode)
    {
        throw new System.NotImplementedException();
    }

    public new async Task<List<AlgoNode>> GetResultPath(AlgoNode startNode, AlgoNode endNode)
    {
        forwardPath = new List<AlgoNode>();
        backwardPath = new List<AlgoNode>();

        // Find meeting point
        AlgoNode meetingPoint = null;
        foreach(var node in _forwardParents.Keys)
        {
            if(_backwardParents.ContainsKey(node))
            {
                meetingPoint = node;
                break;
            }
        }

        if(meetingPoint == null) throw new System.Exception("No meeting point found");

        // Forward path
        AlgoNode current = meetingPoint;
        while(current != null)
        {
            forwardPath.Add(current);
            current = _forwardParents[current];
        }
        forwardPath.Reverse();

        // Backward path
        current = _backwardParents[meetingPoint];
        while(current != null)
        {
            backwardPath.Add(current);
            current = _backwardParents[current];
        }

        List<AlgoNode> finalPath = new List<AlgoNode>();
        finalPath.AddRange(forwardPath);
        finalPath.AddRange(backwardPath);

        backwardPath.Reverse();
        backwardPath.Add(forwardPath.Last());

        return await Task.FromResult(finalPath);
    }
}
