using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NanYUtilityLib
{
    public class LRTreeNode<T>
    {
        public LRTreeNode(T Content)
        {
            this.Content = Content;
        }

        public LRTreeNode<T> parent { get; private set; }
        public LRTreeNode<T> nearlchild { get; private set; }
        public LRTreeNode<T> farlchild { get; private set; }
        public LRTreeNode<T> nearrchild { get; private set; }
        public LRTreeNode<T> farrchild { get; private set; }
        public LRTreeNode<T> lsib { get; private set; }
        public LRTreeNode<T> rsib { get; private set; }

        public int LeftChildCount { get; private set; }
        public int RightChildCount { get; private set; }
        public int ChildCount { get { return LeftChildCount + RightChildCount; } }

        public bool IsLeftChild { get; private set; }
        public bool IsRightChild { get; private set; }

        public T Content { get; private set; }

        public bool HaveParent { get { return parent != null; } }
        
        public bool HaveLeftChild { get { return nearlchild != null; } }
        
        public bool HaveRightChild { get { return nearrchild != null; } }
        
        public bool HaveChild { get { return HaveLeftChild || HaveRightChild; } }

        public bool HaveLeftSibling { get { return lsib != null; } }
        
        public bool HaveRightSibling { get { return rsib != null; } }

        public void InsertLeftChildFar(LRTreeNode<T> node)
        {
            node.parent = null;
            node.lsib = null;
            node.rsib = null;

            if (HaveLeftChild)
            {

                farlchild.lsib = node;
                node.rsib = farlchild;
                farlchild = node;
            }
            else
            {
                farlchild = node;
                nearlchild = node;
                node.rsib = null;
            }

            

            node.IsLeftChild = true;
            node.IsRightChild = false;
            
            node.lsib = null;
            node.parent = this;
            LeftChildCount++;
        }

        public void InsertLeftChildNear(LRTreeNode<T> node)
        {
            node.parent = null;
            node.lsib = null;
            node.rsib = null;

            if (HaveLeftChild)
            {
                nearlchild.rsib = node;
                node.lsib = nearlchild;
                nearlchild = node;
            }
            else
            {
                farlchild = node;
                nearlchild = node;
                node.lsib = null;
            }
            
            node.IsLeftChild = true;
            node.IsRightChild = false;
            
            node.rsib = null;
            node.parent = this;
            LeftChildCount++;
        }

        public void InsertRightChildFar(LRTreeNode<T> node)
        {
            node.parent = null;
            node.lsib = null;
            node.rsib = null;

            if (HaveRightChild)
            {
                farrchild.rsib = node;
                node.lsib = farrchild;
                farrchild = node;

            }
            else
            {
                farrchild = node;
                nearrchild = node;
                node.lsib = null;
            }

            
            node.IsLeftChild = false;
            node.IsRightChild = true;
            
            node.rsib = null;
            node.parent = this;
            RightChildCount++;
        }

        public void InsertRightChildNear(LRTreeNode<T> node)
        {
            node.parent = null;
            node.lsib = null;
            node.rsib = null;

            if (HaveRightChild)
            {
                
                nearrchild.lsib = node;
                node.rsib = nearrchild;
                nearrchild = node;
            }
            else
            {
                farrchild = node;
                nearrchild = node;
                node.rsib = null;
            }

            
            node.IsLeftChild = false;
            node.IsRightChild = true;
            
            node.lsib = null;
            node.parent = this;
            RightChildCount++;
        }

        public void InsertAsLeftSibling(LRTreeNode<T> node)
        {


            if (!HaveParent)
            {
                throw new Exception("Node cannot have sibling if it has node parent!");
            }

            node.parent = null;
            node.lsib = null;
            node.rsib = null;

            if (HaveLeftSibling)
            {
                lsib.rsib = node;
            }

            node.lsib = lsib;

            lsib = node;

            node.rsib = this;
            
            node.parent = parent;


            node.IsRightChild = IsRightChild;
            node.IsLeftChild = IsLeftChild;

            if (IsLeftChild)
            {
                parent.LeftChildCount++;

                if (this == parent.farlchild)
                {
                    parent.farlchild = node;
                }
            }
            else if (IsRightChild)
            {
                parent.RightChildCount++;

                if (this == parent.nearrchild)
                {
                    parent.nearrchild = node;
                }
            }
        }

        public void InsertAsRightSibling(LRTreeNode<T> node)
        {
            if (!HaveParent)
            {
                throw new Exception("Node cannot have sibling if it has node parent!");
            }

            node.parent = null;
            node.lsib = null;
            node.rsib = null;

            if (HaveRightSibling)
            {
                rsib.lsib = node;
            }

            

            node.rsib = rsib;

            rsib = node;

            node.lsib = this;

            node.parent = parent;

            node.IsRightChild = IsRightChild;
            node.IsLeftChild = IsLeftChild;

            if (IsLeftChild)
            {
                parent.LeftChildCount++;

                if (this == parent.nearlchild)
                {
                    parent.nearlchild = node;
                }
            }
            else if (IsRightChild)
            {
                parent.RightChildCount++;

                if (this == parent.farrchild)
                {
                    parent.farrchild = node;
                }
            }

        }

        public void RemoveWithTrace()
        {
            // Cut from left sibling
            if (HaveLeftSibling)
            {
                lsib.rsib = rsib;
            }

            // Cut from right sibling
            if (HaveRightSibling)
            {
                rsib.lsib = lsib;
            }

            // Cut from parent
            if (HaveParent)
            {
                if (IsLeftChild)
                {
                    parent.LeftChildCount--;
                    if (parent.farlchild == this)
                    {
                        parent.farlchild = rsib;
                    }
                    if (parent.nearlchild == this)
                    {
                        parent.nearlchild = lsib;
                    }
                }
                else if (IsRightChild)
                {
                    parent.RightChildCount--;
                    if (parent.farrchild == this)
                    {
                        parent.farrchild = lsib;
                    }
                    if (parent.nearrchild == this)
                    {
                        parent.nearrchild = rsib;
                    }
                }
            }
        }

        public IEnumerable<LRTreeNode<T>> Children
        {
            get
            {
                LRTreeNode<T> chd = farlchild;
                while (chd != null)
                {
                    yield return chd;
                    chd = chd.rsib;
                }

                chd = nearrchild;

                while (chd != null)
                {
                    yield return chd;

                    chd = chd.rsib;
                }
            }
        }

        public IEnumerable<LRTreeNode<T>> LeftChildren
        {
            get
            {
                LRTreeNode<T> chd = farlchild;
                while (chd != null)
                {
                    yield return chd;
                    chd = chd.rsib;
                }
            }
        }

        public List<LRTreeNode<T>> LeftChildrenList()
        {
            List<LRTreeNode<T>> r = new List<LRTreeNode<T>>();

            if (HaveLeftChild)
            {
                foreach (LRTreeNode<T> c in LeftChildren)
                {
                    r.Add(c);
                }
            }

            return r;
        }

        public IEnumerable<LRTreeNode<T>> RightChildren
        {
            get
            {

                LRTreeNode<T> chd = nearrchild;

                while (chd != null)
                {
                    yield return chd;

                    chd = chd.rsib;
                }
            }
        }

        public List<LRTreeNode<T>> RightChildrenList()
        {
            List<LRTreeNode<T>> r = new List<LRTreeNode<T>>();

            if (HaveRightChild)
            {
                foreach (LRTreeNode<T> c in RightChildren)
                {
                    r.Add(c);
                }
            }

            return r;
        }
    }

    public class LRTree<T>
    {
        public LRTree()
            :this(default(T))
        {

        }

        public LRTree(T Content)
        {
            Root = new LRTreeNode<T>(Content);
        }

        public LRTree(T[] Content, int[] HeadId)
        {
            LRTreeNode<T>[] Nodes = new LRTreeNode<T>[Content.Length];
            for (int i = 0; i < Nodes.Length; ++i)
            {
                Nodes[i] = new LRTreeNode<T>(Content[i]);
            }

            Root = Nodes[0];

            for (int i = 1; i < Nodes.Length; ++i)
            {
                int hid = HeadId[i];
                if (hid < i)
                {
                    // right child
                    Nodes[hid].InsertRightChildFar(Nodes[i]);
                }
                else if (hid > i)
                {
                    // left child
                    Nodes[hid].InsertLeftChildNear(Nodes[i]);
                }
                else
                {
                    throw new Exception("Tree nodes cannot have itself as head!");
                }
            }
        }

        public LRTreeNode<T> Root { get; set; }
    }
}
