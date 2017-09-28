using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace NanYUtilityLib
{
    [Serializable]
    public class SimpleBinaryNode
    {
        public int id;
        public int lchd;
        public int rchd;
        public int parent;

        public SimpleBinaryNode(int id)
        {
            if (id < 0)
            {
                throw new Exception("Tree node id must be positive!");
            }
            this.id = id;
            lchd = -1;
            rchd = -1;
            parent = -1;
        }

        public void AddLChd(SimpleBinaryNode chdNode)
        {
            if (chdNode == null)
            {
                return;
            }

            lchd = chdNode.id;
            chdNode.parent = id;
        }

        public void AddRChd(SimpleBinaryNode chdNode)
        {
            if (chdNode == null)
            {
                return;
            }

            rchd = chdNode.id;
            chdNode.parent = id;
        }
    }

    [Serializable]
    public class SimpleBinaryTree
    {
        public int Capacity { get; private set; }

        public int Count { get; private set; }

        private SimpleBinaryNode[] pool;

        public SimpleBinaryNode this[int id]
        {
            get
            {
                if (id < 0 || id >= Count)
                {
                    throw new Exception("Index out of range!");
                }

                return pool[id];
            }
        }

        public SimpleBinaryNode Root
        {
            get
            {
                return pool[0];
            }
        }

        public int AllocateNewNode()
        {
            if (Count >= Capacity)
            {
                throw new Exception("Node pool is full!");
            }

            int id = Count;

            Count += 1;

            pool[id] = new SimpleBinaryNode(id);

            return id;
        }

        public SimpleBinaryTree(int Capacity)
        {
            if(Capacity <= 0)
            {
                throw new Exception("Capacity must be greater than 0!");
            }

            pool = new SimpleBinaryNode[Capacity];

            this.Capacity = Capacity;

            AllocateNewNode();
        }

        public void DumpToStream(BinaryWriter bw)
        {
            bw.Write(SIG);
            bw.Write(VER);
            bw.Write((Int32)Capacity);
            bw.Write((Int32)Count);

            for (int i = 0; i < Count; ++i)
            {
                var node = pool[i];
                bw.Write(node.id);
                bw.Write(node.lchd);
                bw.Write(node.rchd);
                bw.Write(node.parent);
            }
        }

        public static SimpleBinaryTree LoadFromStream(BinaryReader br)
        {
            ulong sig = br.ReadUInt64();
            if (sig != SIG)
            {
                throw new Exception("Signiture does not match!");
            }

            ulong ver = br.ReadUInt64();
            switch (ver)
            {
                case 0uL:
                    return LoadFromStreamV0(br);
                default:
                    throw new Exception("Ver number is not correct!");
            }
        }

        private static SimpleBinaryTree LoadFromStreamV0(BinaryReader br)
        {
            int cap = br.ReadInt32();
            int count = br.ReadInt32();

            var tree = new SimpleBinaryTree();

            tree.Capacity = cap;
            tree.Count = count;
            tree.pool = new SimpleBinaryNode[cap];
            for (int i = 0; i < count; ++i)
            {
                int id = br.ReadInt32();
                int lchd = br.ReadInt32();
                int rchd = br.ReadInt32();
                int parent = br.ReadInt32();
                var node = new SimpleBinaryNode(id);
                node.lchd = lchd;
                node.rchd = rchd;
                node.parent = parent;
                tree.pool[i] = node;
            }

            return tree;
        }

        private SimpleBinaryTree()
        {
        }

        private const ulong SIG = 0x0101010100000003UL;
        private const ulong VER = 0x0000000000000000UL;
    }
}
