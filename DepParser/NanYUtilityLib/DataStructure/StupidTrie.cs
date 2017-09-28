using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanYUtilityLib.DataStructure
{
    /// <summary>
    /// A simple but inefficient trie, implemented with Dictionary;
    /// </summary>
    /// <typeparam name="K">Keys</typeparam>
    /// <typeparam name="T">Items</typeparam>
    public class StupidTrie<K, T>
    {
        private class Node
        {
            public bool match;
            public T item;
            public Dictionary<K, Node> next;
        }

        private Node Root = new Node();
        public int Count { get; private set; }

        public StupidTrie()
        {
        }

        public bool Insert(K[] Keys, int Start, int Length, T Item)
        {
            if (Keys == null || Start < 0 || Length <= 0 || Start + Length > Keys.Length)
            {
                return false;
            }

            var node = Root;

            for (int i = 0; i < Length; ++i)
            {
                var key = Keys[Start + i];
                if (node.next == null)
                {
                    node.next = new Dictionary<K, Node>();
                }

                Node nnode;
                if (!node.next.TryGetValue(key, out nnode))
                {
                    nnode = new Node();
                    node.next.Add(key, nnode);
                }

                node = nnode;
            }

            if (node.match == false)
            {
                Count += 1;
            }
            node.match = true;
            node.item = Item;

            return true;
        }

        public bool TryExactMatch(K[] Keys, int Start, int Length, out T Item)
        {
            Item = default(T);
            if (Keys == null || Start < 0 || Length <= 0 || Start + Length > Keys.Length)
            {
                return false;
            }

            var node = Root;

            for (int i = 0; i < Length; ++i)
            {
                var key = Keys[Start + i];
                if (node.next == null)
                {
                    return false;
                }

                Node nnode;
                if (!node.next.TryGetValue(key, out nnode))
                {
                    return false;
                }

                node = nnode;
            }

            if (!node.match)
            {
                return false;
            }

            Item = node.item;

            return true;
        }

        public bool TryLongestMatch(K[] Keys, int Start, int Length, out T Item)
        {
            Item = default(T);
            if (Keys == null || Start < 0 || Length <= 0 || Start >= Keys.Length)
            {
                return false;
            }

            Length = Math.Min(Length, Keys.Length - Start);

            bool found = false;
            var node = Root;

            for (int i = 0; i < Length; ++i)
            {
                if (node.match)
                {
                    found = true;
                    Item = node.item;
                }
                var key = Keys[Start + i];
                if (node.next == null)
                {
                    return found;
                }

                Node nnode;
                if (!node.next.TryGetValue(key, out nnode))
                {
                    return found;
                }

                node = nnode;
            }

            if (!node.match)
            {
                return found;
            }

            Item = node.item;

            return true;
        }

        public bool TryShortestMatch(K[] Keys, int Start, int Length, out T Item)
        {
            Item = default(T);
            if (Keys == null || Start < 0 || Length <= 0 || Start >= Keys.Length)
            {
                return false;
            }

            Length = Math.Min(Length, Keys.Length - Start);

            var node = Root;

            for (int i = 0; i < Length; ++i)
            {
                if (node.match)
                {
                    Item = node.item;
                    return true;
                }
                var key = Keys[Start + i];
                if (node.next == null)
                {
                    return false;
                }

                Node nnode;
                if (!node.next.TryGetValue(key, out nnode))
                {
                    return false;
                }

                node = nnode;
            }

            if (!node.match)
            {
                return false;
            }

            Item = node.item;

            return true;
        }

        public bool TryAllMatch(K[] Keys, int Start, int Length, out List<T> Items)
        {
            Items = null;
            if (Keys == null || Start < 0 || Length <= 0 || Start >= Keys.Length)
            {
                return false;
            }

            var node = Root;

            Length = Math.Min(Length, Keys.Length - Start);

            for (int i = 0; i < Length; ++i)
            {
                if (node.match)
                {
                    if (Items == null)
                    {
                        Items = new List<T>();
                    }
                    Items.Add(node.item);
                }
                var key = Keys[Start + i];
                if (node.next == null)
                {
                    return Items != null;
                }

                Node nnode;
                if (!node.next.TryGetValue(key, out nnode))
                {
                    return Items != null;
                }

                node = nnode;
            }

            if (node.match)
            {
                if (Items == null)
                {
                    Items = new List<T>();
                    Items.Add(node.item);
                }
            }

            return Items != null;
        }
    }
}
