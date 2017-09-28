using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanYUtilityLib
{
    public class HuffmanCode
    {
        static public void GenerateHuffmanTree(double[] w, out int[][] huffcode, out SimpleBinaryTree hufftree)
        {
            if(w == null || w.Length == 1)
            {
                throw new Exception("Must have at least two items!");
            }

            var heap = new HeapWithScore<int>(w.Length);

            hufftree = new SimpleBinaryTree(w.Length + w.Length - 1);

            int[] nodeIds = new int[w.Length];

            for (int i = 0; i < w.Length; ++i)
            {
                nodeIds[i] = hufftree.AllocateNewNode();

                heap.Insert(nodeIds[i], w[i]);
            }

            while (heap.Count > 2)
            {
                int lid = heap.Peek();
                double lw = heap.PeekScore();
                heap.RemoveTop();

                int rid = heap.Peek();
                double rw = heap.PeekScore();
                heap.RemoveTop();

                int pid = hufftree.AllocateNewNode();
                double pw = lw + rw;

                hufftree[pid].AddLChd(hufftree[lid]);
                hufftree[pid].AddRChd(hufftree[rid]);

                heap.Insert(pid, pw);
            }

            System.Diagnostics.Debug.Assert(heap.Count == 2);

            int xlid = heap.Peek();
            heap.RemoveTop();

            int xrid = heap.Peek();
            heap.RemoveTop();

            hufftree.Root.AddLChd(hufftree[xlid]);
            hufftree.Root.AddRChd(hufftree[xrid]);

            huffcode = new int[w.Length][];

            for (int i = 0; i < w.Length; ++i)
            {
                int nid = nodeIds[i];
                var node = hufftree[nid];

                List<int> code = new List<int>();
                while (node.parent >= 0)
                {
                    var parent = hufftree[node.parent];

                    if (parent.lchd == node.id)
                    {
                        code.Add(0);
                    }
                    else
                    {
                        code.Add(1);
                    }

                    node = parent;
                }

                code.Reverse();

                huffcode[i] = code.ToArray();
            }
        }
    }
}
