using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public struct Tuple3
{
    public int Item1;
    public int Item2;
    public int Item3;

    public Tuple3(int i1,int i2,int i3)
    {
        Item1 = i1;
        Item2 = i2;
        Item3 = i3;
    }

    public class EqualityComparer : IEqualityComparer<Tuple3>
    {

        public bool Equals(Tuple3 x, Tuple3 y)
        {
            return x.Item1 == y.Item1 && x.Item2 == y.Item2 && x.Item3 == y.Item3;
        }

        public int GetHashCode(Tuple3 obj)
        {
            return 17*obj.Item1+23*(obj.Item2+29*obj.Item3);
        }
    }
}

public struct Tuple2
{
    public int Item1;
    public int Item2;

    public Tuple2(int i1, int i2)
    {
        Item1 = i1;
        Item2 = i2;
    }

    public class EqualityComparer : IEqualityComparer<Tuple2>
    {

        public bool Equals(Tuple2 x, Tuple2 y)
        {
            return x.Item1 == y.Item1 && x.Item2 == y.Item2 ;
        }

        public int GetHashCode(Tuple2 obj)
        {
            return 17 * obj.Item1 + 23 * obj.Item2  ;
        }
    }
}

public class BDD : MonoBehaviour {

    

    public int nextId = 0;
    List<int> _variable_order;

    public BDDNode Zero { get; private set; }
    public BDDNode One { get; private set; }

    int _n;
    public int N
    {
        get { return _n; }
        set
        {
            _n = value;
            Zero.Index = _n;
            One.Index = _n;
        }
    }

    Dictionary<Tuple3,BDDNode> unique_table ;

    public BDDNode Create(int index, bool value)
    {
        return new BDDNode(index, value) { Id = nextId++ };
    }


    public BDD(int n)
    {
        this.Zero = Create(n,false);
        this.One = Create(n,true);

        _variable_order = new List<int>(Enumerable.Range(0, n));
        _n = n;
        unique_table = new Dictionary<Tuple3, BDDNode>();
    }
    

    public BDDNode Create(int index,BDDNode low,BDDNode high)
    {
        BDDNode unique;
        Tuple3 tuple = new Tuple3(index, low.Id, high.Id);
        if (unique_table.TryGetValue(tuple, out unique))
        {
            Debug.Log("new ID : " + unique.Id);

            return unique;
        }
        unique = new BDDNode(index, low, high) { Id = nextId++ };
        high.RefCount++;
        low.RefCount++;
        unique_table.Add(tuple,unique);
        Debug.Log("new ID : "+unique.Id);
        return unique;
    }

    public BDDNode Reduce(BDDNode root)
    {
        var nodes = root.Nodes.ToArray();
        var size = nodes.Length;

        var subgraph = new BDDNode[size];
        var vlist = new List<BDDNode>[N + 1];

        for (int i = 0; i < size; i++)
        {
            if (vlist[nodes[i].Index] == null)
                vlist[nodes[i].Index] = new List<BDDNode>();

            vlist[nodes[i].Index].Add(nodes[i]);
        }

        int nextid = -1;
        for (int k = N; k >= 0; k--)
        {
            int i = (k == N) ? N : _variable_order[k];
            var Q = new List<BDDNode>();
            if (vlist[i] == null)
                continue;

            foreach (var u in vlist[i])
            {
                if (u.Index == N)
                {
                    Q.Add(u);
                }
                else
                {
                    if (u.Low.Id == u.High.Id)
                    {
                        u.Id = u.Low.Id;
                    }
                    else
                    {
                        Q.Add(u);
                    }
                }
            }

            Q.Sort((x, y) =>
            {
                var xlk = x.Key.Item1;
                var xhk = x.Key.Item2;
                var ylk = y.Key.Item1;
                var yhk = y.Key.Item2;
                int res = xlk.CompareTo(ylk);
                return res == 0 ? xhk.CompareTo(yhk) : res;
            });

            var oldKey = new Tuple2(-2, -2);
            foreach (var u in Q)
            {
                if (u.Key.Equals(oldKey))
                {
                    u.Id = nextid;
                }
                else
                {
                    nextid++;
                    u.Id = nextid;
                    subgraph[nextid] = u;
                    ////Console.WriteLine(u.Low?.Id.ToString() ?? "null" );
                    u.Low = u.Low == null ? null : subgraph[u.Low.Id];
                    u.High = u.High == null ? null : subgraph[u.High.Id];
                    oldKey = u.Key;
                }
            }
        }
        return subgraph[root.Id];
    }


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

    


    // Use this for initialization
    void Start () {

        var manager = new BDD(3);
        var n3 = manager.Create(2, manager.One, manager.One);
        var n4 = manager.Create(1, n3, manager.Zero);
        var n2 = manager.Create(1, n3, manager.Zero);
        var root = manager.Create(0, n2, n4);

        //manager.Reduce(root);
        foreach (var item in root.Nodes)
        {
            Debug.Log(item.ToString());
        }

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
