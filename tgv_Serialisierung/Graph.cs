using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tgv_Serialisierung
{
    public class Graph
    {
        public Graphnode root;
    }

    public class Graphnode
    {
        public Graphnode(String name, List<int> values = null, List<Graphnode> children = null)
        {
            if (values == null)
                this.values = new List<int>();
            else
                this.values = values;
            if (children == null)
                nodes = new List<Graphnode>();
            else
                nodes = children;
            this.name = name;

        }
        public void addChild(Graphnode n)
        {
            this.nodes.Add(n);
        }
        public String name;
        public List<Graphnode> nodes;
        public List<int> values;
    }
}
