using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tgv_Serialisierung
{
    class Program
    {
        static Graph MyGraph()
        {
            Graph g = new Graph();
            Graphnode node = new Graphnode("4", new List<int> { 4 }, null);
            g.root = new Graphnode("0", null, new List<Graphnode>{
                new Graphnode("1", new List<int>{1}, new List<Graphnode>{node}),
                new Graphnode("2", new List<int>{2}, new List<Graphnode>{node}),
                new Graphnode("3", new List<int>{3}, null)
            });
            return g;
        }

        static Graph TestGraph()
        {
            Graph g = new Graph();
            Graphnode a, b, c, d, e;
            a = new Graphnode("A");
            b = new Graphnode("B");
            c = new Graphnode("C", new List<int>{1});
            d = new Graphnode("D", new List<int>{2});
            e = new Graphnode("E");

            a.addChild(b);
            a.addChild(c);
            a.addChild(e);
            b.addChild(a);
            b.addChild(d);
            b.addChild(e);
            c.addChild(b);
            c.addChild(d);
            c.addChild(e);
            d.addChild(a);
            d.addChild(e);
            e.addChild(b);
            g.root = a;
            return g;
        }
        static void Main(string[] args)
        {

            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("---------------------MyGraph----------------------");
            Console.WriteLine("--------------------------------------------------");
            Graph g = MyGraph();
            Pickletree pTree = new Pickletree(g);
            pTree.print();
            Console.WriteLine("\n\n");
            string gSer = pTree.Serialize();
            Console.WriteLine(gSer);
            Pickletree.clean();

            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("-------------------ReMyGraph----------------------");
            Console.WriteLine("--------------------------------------------------");

            Pickletree rG = new Pickletree(gSer);
            rG.print();
            Console.WriteLine("\n\n");

            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("--------------------TestGraph---------------------");
            Console.WriteLine("--------------------------------------------------");

            Graph g2 = TestGraph();
            Pickletree p2 = new Pickletree(g2);
            p2.print();
            Console.WriteLine("\n\n");
            string ser = p2.Serialize();
            Console.WriteLine(ser);
            Pickletree.clean();


            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("------------------ReTestGraph---------------------");
            Console.WriteLine("--------------------------------------------------");

            Pickletree reTree = new Pickletree(ser);
            Console.WriteLine("\n");
            reTree.print();
            Console.WriteLine("\n\n");

            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("------------------DennisGraph---------------------");
            Console.WriteLine("--------------------------------------------------");
            Pickletree dTree = new Pickletree("PROMISE;1;BLOCK;de.effing.model.Entity;0\nLOAD;1\nPROMISE;2;BLOCK;de.effing.model.Entity;1\nBLOCK;de.effing.model.Entity;1\nSTORE;4\nCHUNK;java.lang.Integer;1\nBLOCK;de.effing.model.Entity;3\nSTORE;3\nLOAD;4\nFULFIL;2;3\nLOAD;2\nLOAD;3\nLOAD;4\nCHUNK;java.lang.Integer;2\nBLOCK;de.effing.model.Entity;4\nLOAD;4\nFULFIL;1;3");
            dTree.print();
            Console.WriteLine("\n\n");

            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("--------------------TimGraph----------------------");
            Console.WriteLine("--------------------------------------------------");
            Pickletree tTree = new Pickletree("PROMISE;0;BLOCK;A;0\nLOAD;0\nPROMISE;3;BLOCK;E;0\nCHUNK;G;4711\nBLOCK;D;3\nSTORE;2\nLOAD;3\nBLOCK;B;3\nSTORE;1\nLOAD;1\nLOAD;2\nLOAD;3\nCHUNK;F;42\nBLOCK;C;4\nLOAD;1\nFULFIL;3;1\nFULFIL;0;3\n"
);
            tTree.print();
            Console.WriteLine("\n\n");


            Console.ReadLine();
        }
    }
}
