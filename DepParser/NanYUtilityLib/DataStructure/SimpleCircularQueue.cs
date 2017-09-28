using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NanYUtilityLib
{
    public class SimpleCircularQueue<T>
    {
        public SimpleCircularQueue(int Capacity)
        {
            cap = Capacity;
            ItemArr = new T[Capacity];
            baseId = 0;
            nextId = 0;
            count = 0;
        }

        public bool Enqueue(T item)
        {
            if (IsFull)
            {
                return false;
            }

            ItemArr[nextId++] = item;
            if (nextId == Capacity)
            {
                nextId = 0;
            }
            count++;
            return true;
        }

        public bool Dequeue(out T item)
        {
            if (IsEmpty)
            {
                item = default(T);
                return false;
            }

            item = ItemArr[baseId++];
            if (baseId == Capacity)
            {
                baseId = 0;
            }
            count--;
            return true;
        }

        public int Count { get { return count; } }

        public int Capacity { get { return cap;}}

        public bool IsEmpty { get { return Count == 0; } }

        public bool IsFull { get { return Count == Capacity; } }

        T[] ItemArr;

        int baseId;
        int nextId;
        int count;
        readonly int cap;
    }

    public class HashedLinkList<T>
    {
        public HashedLinkList(int cap)
        {
            this.cap = cap;
            Count = 0;
            Head = new LinkNode();
            Head.ID = -1;
            
            Tail = new LinkNode();
            Tail.ID = -1;
            Tail.prev = Head;
            Head.next = Tail;
            hashDict = new Dictionary<T, LinkNode>();
        }

        class LinkNode
        {
            public long ID;
            public LinkNode prev;
            public LinkNode next;
            public T item;
        }

        public void InsertHead(T item)
        {
            LinkNode node;
            if (hashDict.TryGetValue(item, out node))
            {
                if (node == Head.next)
                {
                    return;
                }
                LinkNode prevNode = node.prev;
                while (prevNode != Head)
                {
                    prevNode.ID--;
                    prevNode = prevNode.prev;
                }
                node.ID = Head.next.ID + 1;
                node.prev.next = node.next;
                node.next.prev = node.prev;

                node.next = Head.next;
                node.prev = Head;

                Head.next.prev = node;

                Head.next = node;
            }
            else
            {
                node = new LinkNode();
                node.item = item;
                node.ID = Head.next.ID + 1;
                hashDict[item] = node;

                node.prev = Head;
                node.next = Head.next;

                Head.next.prev = node;
                Head.next = node;

                Count++;

                if (Count > cap)
                {
                    hashDict.Remove(Tail.prev.item);
                    Tail.prev.prev.next = Tail;
                    Tail.prev = Tail.prev.prev;
                    Count--;
                }

                //if ((Tail.prev == null || Head.next == null) && Count > 1)
                //{
                //    throw new Exception("sb");
                //}
            }
          //  Check();
        }

        public int GetDistToHead(T item)
        {
            LinkNode node;
            if (!hashDict.TryGetValue(item, out node))
            {
                return -1;
            }

            return (int)(Head.next.ID - node.ID);

        }

        private void Check()
        {
            LinkNode next = Head.next;
            if (Head.next == Tail)
            {
                if (Count != 0)
                {
                    throw new Exception("sb");
                }
            }
            else
            {
                long id = next.ID;
                int xcount = 1;
                while (next.next != Tail)
                {
                    if (next.next.ID != id - 1)
                    {
                        throw new Exception("sb");
                    }
                    id--;
                    xcount++;
                    next = next.next;
                }

                if (xcount != Count)
                {
                    throw new Exception("sb");
                }
            }

            LinkNode prev = Tail.prev;
            if (Tail.prev == Head)
            {
                if (Count != 0)
                {
                    throw new Exception("sb");
                }
            }
            else
            {
                long id = prev.ID;
                int xcount = 1;
                while (prev.prev != Head)
                {
                    if (prev.prev.ID != id + 1)
                    {
                        throw new Exception("sb");
                    }
                    id++;
                    xcount++;
                    prev = prev.prev;
                }

                if (xcount != Count)
                {
                    throw new Exception("sb");
                }
            }
        }

        Dictionary<T, LinkNode> hashDict;

        LinkNode Head;
        LinkNode Tail;

        int cap;
        public int Count { get; private set; }
    }
}
