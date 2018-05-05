using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class BDDNode
{
    public int Id { get; set; }
    //index is variable index
    public int Index { get; set; }
    public int RefCount { get; set; }
    public BDDNode Low { get; set; }
    public BDDNode High { get; set; }
    public bool? Value { get; set; }

    public void SetLow(BDDNode low) { Low = low; RefCount++; }
    public void SetHigh(BDDNode high) { High = high; RefCount++; }


    public IEnumerable<BDDNode> Nodes
    {
        get
        {
            if (Low == null && High == null)
            {
                return new[] { this };
            }
            else
            {
                return new[] { this }.Union(Low.Nodes.Union(High.Nodes));
            }
        }
    }

    public Tuple2 Key
    {
        get
        {
            if (IsZero) return new Tuple2(-1, -1);
            if (IsOne) return new Tuple2(-1, 0);
            return new Tuple2(Low.Id, High.Id);
        }
    }

    public bool IsOne
    {
        get
        {
            return Value != null && ((bool)Value) == true;
        }
    }
    public bool IsZero
    {
        get { return Value != null && ((bool)Value) == false; }
    }

    public BDDNode()
    {
    }

    public BDDNode(int index, BDDNode high, BDDNode low) : this()
    {
        this.Index = index;
        this.High = high;
        this.Low = low;
    }

    public BDDNode(int index, bool value) : this()
    {
        this.Value = value;
        this.Index = index;
    }

    public override string ToString()
    {
        return string.Format("[Node: Identifier={0}, Value={1}, Index={2}, Low={3}, High={4}, RefCount={5}]",
            Id, Value, Index,
            Low != null ? Low.Id.ToString() : "null",
            High != null ? High.Id.ToString() : "null",
            RefCount);
    }

    public override bool Equals(object obj)
    {
        if (obj is BDDNode)
        {
            var node = (BDDNode)obj;
            return node.Id == Id && node.Low == Low && node.High == High;
        }
        return false;
    }

    public override int GetHashCode()
    {
        if (Value != null) return (bool)Value ? 1 : 0;
        return 17 * Index + 23 * (Low.Id + 23 * High.Id);
    }
}
