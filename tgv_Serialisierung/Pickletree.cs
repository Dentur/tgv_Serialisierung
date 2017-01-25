using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tgv_Serialisierung
{
    public enum SerializationState
    {
        nothing,
        promised,
        fullfilled
    }

    public class Pickletree
    {

        public Pickletree (Graph graph)
        {
            Dictionary<Graphnode, Pickletreenode> nodeDictionary = new Dictionary<Graphnode, Pickletreenode>();
            //Determin indexed nodes
            Dictionary<Graphnode, bool> indexedNodes = new Dictionary<Graphnode, bool>();
            Dictionary<Graphnode, int> referenceAmmount = new Dictionary<Graphnode, int>();
            Dictionary<Graphnode, bool> alreadyVisited = new Dictionary<Graphnode,bool>();
            CalculateIndexedNodes(graph.root, referenceAmmount, alreadyVisited);
            foreach(Graphnode node in referenceAmmount.Keys)
            {
                indexedNodes[node] = referenceAmmount[node] > 1;
            }
            root = this.GraphToPickleNode(graph.root, nodeDictionary, indexedNodes);
        }

        public Pickletree (String serialisation)
        {
            String[] lines = serialisation.Split('\n');
            Dictionary<int, Pickletreenode> nodes = new Dictionary<int, Pickletreenode>();//Reference Nodes or Index Nodes
            List<Pickletreenode> children = new List<Pickletreenode>();
            foreach (String line in lines)
            {
                string[] words = line.Split(';');
                int index;
                String name;
                int childrenCount;
                switch (words[0])
                {
                    case "PROMISE":
                        index = int.Parse(words[1]);
                        if(words[2]!="BLOCK")
                        {
                            throw new FormatException("Could not Parse " + line + ". Unrecognised word: " + words[2]+"!");
                        }
                        name = words[3];
                        childrenCount = int.Parse(words[4]);
                        PickletreeReferenceNode refNode = new PickletreeReferenceNode(index, childrenCount);
                        refNode.type = name;
                        children.Add(refNode);
                        nodes[index] = refNode;
                        break;
                    case "LOAD":
                        index = int.Parse(words[1]);
                        if(nodes[index]is PickletreeReferenceNode)
                        {
                            children.Add(nodes[index]);
                        }
                        else if(nodes[index]is PickletreeIndexNode)
                        {
                            PickletreeIndexNode iNode = nodes[index] as PickletreeIndexNode;
                            PickletreeReferenceNode refnode = new PickletreeReferenceNode(iNode.index, iNode.children.Count);
                            refnode.type = iNode.type;
                            children.Add(refnode);
                        }
                        else
                        {
                            throw new FormatException("Could not Parse line: " + line);
                        }
                        break;
                    case "CHUNK":
                        name = words[1];
                        int value = int.Parse(words[2]);
                        PickletreeValueNode valueNode = new PickletreeValueNode(value);
                        valueNode.type = name;
                        children.Add(valueNode);
                        break;
                    case "BLOCK":
                        name = words[1];
                        childrenCount = int.Parse(words[2]);
                        Pickletreenode n = new Pickletreenode();
                        n.type = name;

                        for (int lIndex = children.Count, end = children.Count-childrenCount; lIndex > end; lIndex--)
                        {
                            n.children.Add(children[end]);
                            children.RemoveAt(end);
                        }
                        children.Add(n);

                        break;
                    case "STORE":
                        Pickletreenode sNode = children[children.Count - 1];
                        children.RemoveAt(children.Count - 1);
                        PickletreeIndexNode indexNode = new PickletreeIndexNode();
                        indexNode.type = sNode.type;
                        indexNode.index = int.Parse(words[1]);
                        indexNode.children = sNode.children;
                        children.Add(indexNode);
                        nodes[indexNode.index] = indexNode;
                        break;
                    case "FULFIL":
                        index = int.Parse(words[1]);
                        childrenCount = int.Parse(words[2]);
                        PickletreeIndexNode inNode = new PickletreeIndexNode();
                        inNode.index = index;
                        inNode.type = nodes[index].type;
                        for (int lIndex = children.Count, end = children.Count - childrenCount; lIndex > end; lIndex--)
                        {
                            inNode.children.Add(children[end]);
                            children.RemoveAt(end);
                        }
                        children.Add(inNode);
                        nodes[index] = inNode;
                        break;
                }

            }
            this.root = children[0];

        }

        public String Serialize()
        {
            return root.serialize("", new Dictionary<int, SerializationState>());
        }

        public Pickletreenode root;

        public void print()
        {
            root.print("", false);
        }

        public static void clean()
        {
            PickletreeIndexNode.clean();
        }

        

        private void CalculateIndexedNodes(Graphnode node, Dictionary<Graphnode, int> referenceAmmount, Dictionary<Graphnode, bool> alredyVisited)
        {
            if (alredyVisited.ContainsKey(node))
                return;
            else
                alredyVisited[node] = true;
            if (!referenceAmmount.ContainsKey(node))
                referenceAmmount[node] = 0;

            foreach (Graphnode child in node.nodes)
            {
                if(referenceAmmount.ContainsKey(child))
                {
                    referenceAmmount[child]++;
                }
                else
                {
                    referenceAmmount[child] = 1;
                }
                CalculateIndexedNodes(child, referenceAmmount, alredyVisited);
            }
        }

        private Pickletreenode GraphToPickleNode(Graphnode node, Dictionary<Graphnode, Pickletreenode> nodeDictionary, Dictionary<Graphnode, bool> indexedNodes)
        {
            Pickletreenode pNode;
            Pickletreenode nNode;
            if(nodeDictionary.ContainsKey(node)) //Node already exists
            {
                nNode = nodeDictionary[node]; //Can only be a PickletreeIndexNode, as this node is referenced multiple times
                PickletreeIndexNode tempNode = nNode as PickletreeIndexNode;
                pNode = new PickletreeReferenceNode(tempNode.index, node.nodes.Count+node.values.Count);
                pNode.type = nNode.type;
                return pNode;
            }
            //Create this node
            if(indexedNodes[node])//Determin if this node needs to be a indexed node
            {
                nNode = new PickletreeIndexNode();
                nNode.type = node.name;
            }
            else
            {
                nNode = new Pickletreenode();
                nNode.type = node.name;
            }
            nodeDictionary.Add(node, nNode);
            //Create child value nodes
            foreach(int value in node.values)
            {
                PickletreeValueNode vNode = new PickletreeValueNode(value);
                vNode.type = "int";
                nNode.children.Add(vNode);
            }
            //Create child nodes
            foreach (Graphnode child in node.nodes)
            {
                nNode.children.Add(GraphToPickleNode(child, nodeDictionary, indexedNodes));
            }


            return nNode;
        }
    }

    public class Pickletreenode
    {

        public List<Pickletreenode> children;
        public String type;
        public Pickletreenode()
        {
            children = new List<Pickletreenode>();
        }

        public virtual string serializeChildren(string serialization, Dictionary<int, SerializationState> prommised)
        {
            foreach (Pickletreenode child in children)
            {
                serialization = child.serialize(serialization, prommised);
            }
            return serialization;
        }

        public virtual string serialize(string serialization, Dictionary<int, SerializationState> prommised)
        {
            serialization = serializeChildren(serialization, prommised);
            serialization += "BLOCK;" + this.type + ";" + this.children.Count+"\n";
            return serialization;
        }

        public virtual void print(string indent, bool last)
        {
            Console.Write(indent);
            if(last)
            {
                Console.Write("\\-");
                indent += "  ";
            }
            else
            {
                Console.Write("|-");
                indent += "| ";
            }
            Console.WriteLine("type: " +this.type);
            for(int index=0; index < this.children.Count; index++)
            {
                children[index].print(indent, index == children.Count - 1);
            }
        }
    }

    public class PickletreeReferenceNode : Pickletreenode
    {
        public int referenceID;
        public int childCount;

        public PickletreeReferenceNode(int referenceID, int childCount) : base()
        {
            this.referenceID = referenceID;
            this.childCount = childCount;
        }

        public override void print(string indent, bool last)
        {
            Console.Write(indent);
            if (last)
            {
                Console.Write("\\-");
                indent += "  ";
            }
            else
            {
                Console.Write("|-");
                indent += "| ";
            }
            Console.WriteLine("Reference: " + this.referenceID + " type: " + this.type);
            for (int index = 0; index < this.children.Count; index++)
            {
                children[index].print(indent, index == children.Count - 1);
            }
        }

        public override string serialize(string serialization, Dictionary<int, SerializationState> prommised)
        {
            serialization = serializeChildren(serialization, prommised);
            if(prommised.ContainsKey(referenceID) && (prommised[referenceID]==SerializationState.promised || prommised[referenceID] == SerializationState.fullfilled))
                serialization += "LOAD;" + this.referenceID + "\n";
            else
            {
                serialization += "PROMISE;" + this.referenceID+";BLOCK;"+this.type+";"+this.childCount+"\n";
                prommised[referenceID]=SerializationState.promised;
            }
            return serialization;
        }
    }

    class PickletreeValueNode : Pickletreenode
    {
        public int value;

        public PickletreeValueNode(int value) : base()
        {
            this.value = value;
        }

        public override string serialize(string serialization, Dictionary<int, SerializationState> prommised)
        {
            serialization = serializeChildren(serialization, prommised);
            serialization += "CHUNK;" + this.type + ";" + this.value + "\n";
            return serialization;
        }

        public override void print(string indent, bool last)
        {
            Console.Write(indent);
            if (last)
            {
                Console.Write("\\-");
                indent += "  ";
            }
            else
            {
                Console.Write("|-");
                indent += "| ";
            }
            Console.WriteLine("value: " + this.value + " type: " + this.type);
            for (int index = 0; index < this.children.Count; index++)
            {
                children[index].print(indent, index == children.Count - 1);
            }
        }
    }

    class PickletreeIndexNode : Pickletreenode
    {
        public int index;
        static int lastIndex = -1;

        public PickletreeIndexNode() : base()
        {
            this.index = ++lastIndex;
        }

        public override string serialize(string serialization, Dictionary<int, SerializationState> prommised)
        {
            serialization = serializeChildren(serialization, prommised);
            if (prommised.ContainsKey(index) && (prommised[this.index] == SerializationState.nothing || prommised[index] == SerializationState.fullfilled))
            {
                serialization += "BLOCK;" + this.type + ";" + this.children.Count + "\n";
                serialization += "STORE;" + this.index + "\n";
                prommised[index] = SerializationState.fullfilled;
            }
            else if(prommised.ContainsKey(index))
            {
                serialization += "FULFIL;" + this.index + ";" + this.children.Count + "\n";
                prommised[index] = SerializationState.fullfilled;
            }
            else
            {
                serialization += "BLOCK;" + this.type + ";" + this.children.Count + "\n";
                serialization += "STORE;" + this.index + "\n";
                prommised[index] = SerializationState.fullfilled;
            }
            return serialization;
        }

        public override void print(string indent, bool last)
        {
            Console.Write(indent);
            if (last)
            {
                Console.Write("\\-");
                indent += "  ";
            }
            else
            {
                Console.Write("|-");
                indent += "| ";
            }
            Console.WriteLine("index: " + this.index + " type: " +this.type);
            for (int index = 0; index < this.children.Count; index++)
            {
                children[index].print(indent, index == children.Count - 1);
            }
        }

        public static void clean()
        {
            lastIndex = -1;
        }
    }
}
