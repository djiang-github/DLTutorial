using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NanYUtilityLib
{
    public class SimpleHeap<T> where T : IComparable<T>
    {
        public SimpleHeap(int cap)
        {
            _cap = cap;
            heap = new T[cap];
        }

        public int Count
        {
            get { return next; }
        }

        public int Cap
        {
            get { return _cap; }
        }

        public bool IsFull
        {
            get { return Count == Cap; }
        }

        public bool IsEmpty
        {
            get { return Count == 0; }
        }

        public bool Insert(T Item)
        {
            //if (!IsAcceptableScore(pcw.Score))
            //{
            //    return false;
            //}

            if (IsFull)
            {
                if(Item.CompareTo(heap[0]) > 0)
                {
                    //configList.RemoveAt(0);
                    heap[0] = Item;
                    TrickleDown(0);
                    return true;
                }
                return false;
            }
            else
            {
                heap[next++] = Item;
                BubbleUp(next - 1);
                return true;
            }
        }

        public T Peek()
        {
            if (IsEmpty)
            {
                throw new Exception("Heap is Empty");
            }

            return heap[0];
        }

        public void RemoveTop()
        {
            if (IsEmpty)
            {
                return;
            }

            heap[0] = heap[next - 1];

            next -= 1;

            TrickleDown(0);
        }

        public T[] ToSortedArray()
        {
            if (IsEmpty)
            {
                return null;
            }
            T[] toSort = new T[Count];
            Array.Copy(heap, toSort, Count);
            Array.Sort(toSort);
            //toSort = toSort.Reverse<T>();
            // Reverse

            int l = 0;
            int h = toSort.Length - 1;

            while (l < h)
            {
                T tmp = toSort[l];
                toSort[l] = toSort[h];
                toSort[h] = tmp;
                --h;
                ++l;
            }

            return toSort;
        }

        public void Clear()
        {
            //configList.Clear();
            next = 0;
        }

        private void TrickleDown(int i)
        {
            T tmp = heap[i];
            while (i * 2 + 1 < Count)
            {
                int l = 2 * i + 1;
                int r = 2 * i + 2;
                int min = (r >= Count || heap[l].CompareTo(heap[r]) <= 0) ? l : r;

                if (tmp.CompareTo(heap[min]) > 0)
                {
                    //ParserConfigWrapper tmp = configHeap[i];
                    heap[i] = heap[min];
                    i = min;
                }
                else
                {
                    break;
                }
            }
            heap[i] = tmp;
        }

        private void BubbleUp(int i)
        {
            T tmp = heap[i];
            while (i > 0 && heap[(i - 1) / 2].CompareTo(tmp) > 0)
            {
                heap[i] = heap[(i - 1) / 2];
                i = (i - 1) / 2;
            }
            heap[i] = tmp;
        }

        private readonly int _cap;
        //private SortedList<float, ParserConfigWrapper> configList;


        private T[] heap;
        private int next;
    }

    public class KMaxHeap<T> where T : IComparable<T>
    {
        public int Cap { get; private set; }

        public bool IsEmpty { get { return next <= 0; } }

        public bool IsFull
        {
            get { return next >= Cap; }
        }

        public int Count { get { return next; } }

        public T Min
        {
            get
            {
                if (IsEmpty)
                {
                    throw new Exception("heap is empty!");
                }

                return minHeap[0];
            }
        }

        public T Max
        {
            get
            {
                if (IsEmpty)
                {
                    throw new Exception("heap is empty!");
                }

                return maxHeap[0];
            }
        }

        private T[] minHeap;
        private T[] maxHeap;
        private int[] min2max;
        private int[] max2min;
        private int next;

        public KMaxHeap(int cap)
        {
            this.Cap = cap;
            minHeap = new T[cap];
            maxHeap = new T[cap];
            min2max = new int[cap];
            max2min = new int[cap];
        }

        public bool Insert(T Item)
        {
            if (IsFull)
            {
                if (Item.CompareTo(minHeap[0]) > 0)
                {
                    //configList.RemoveAt(0);
                    minHeap[0] = Item;
                    maxHeap[min2max[0]] = Item;
                    int yid = min2max[0];
                    TrickleDownMinHeap(0);
                    BubbleUpMaxHeap(yid);
                    return true;
                }
                return false;
            }
            else
            {
                minHeap[next] = Item;
                maxHeap[next] = Item;
                min2max[next] = next;
                max2min[next] = next;
                next += 1;
                BubbleUpMinHeap(next - 1);
                BubbleUpMaxHeap(next - 1);
                return true;
            }
        }

        public void RemoveMax()
        {
            if (IsEmpty)
            {
                throw new Exception("heap is empty!");
            }

            next -= 1;

            if (next <= 0)
            {
                return;
            }

            maxHeap[0] = maxHeap[next];

            int deleteMinId = max2min[0];

            max2min[0] = max2min[next];
            min2max[max2min[0]] = 0;

            TrickleDownMaxHeap(0);

            if (deleteMinId < next)
            {
                minHeap[deleteMinId] = minHeap[next];
                min2max[deleteMinId] = min2max[next];
                max2min[min2max[deleteMinId]] = deleteMinId;
                TrickleDownMinHeap(deleteMinId);
                BubbleUpMinHeap(deleteMinId);
            }
        }

        //public void RemoveMin()
        //{
        //    if (IsEmpty)
        //    {
        //        throw new Exception("heap is empty!");
        //    }
        //}

        private void TrickleDownMinHeap(int i)
        {
            T tmp = minHeap[i];
            int yid = min2max[i];
            while (i * 2 + 1 < Count)
            {
                int l = 2 * i + 1;
                int r = 2 * i + 2;
                int min = (r >= Count || minHeap[l].CompareTo(minHeap[r]) <= 0) ? l : r;

                if (tmp.CompareTo(minHeap[min]) > 0)
                {
                    //ParserConfigWrapper tmp = configHeap[i];
                    minHeap[i] = minHeap[min];
                    min2max[i] = min2max[min];
                    max2min[min2max[i]] = i;
                    i = min;
                }
                else
                {
                    break;
                }
            }
            minHeap[i] = tmp;
            min2max[i] = yid;
            max2min[min2max[i]] = i;
        }

        private void TrickleDownMaxHeap(int i)
        {
            T tmp = maxHeap[i];
            int yid = max2min[i];
            while (i * 2 + 1 < Count)
            {
                int l = 2 * i + 1;
                int r = 2 * i + 2;
                int max = (r >= Count || maxHeap[l].CompareTo(maxHeap[r]) >= 0) ? l : r;

                if (tmp.CompareTo(maxHeap[max]) < 0)
                {
                    //ParserConfigWrapper tmp = configHeap[i];
                    maxHeap[i] = maxHeap[max];
                    max2min[i] = max2min[max];
                    min2max[max2min[i]] = i;
                    i = max;
                }
                else
                {
                    break;
                }
            }
            maxHeap[i] = tmp;
            max2min[i] = yid;
            min2max[max2min[i]] = i;
        }


        private void BubbleUpMinHeap(int i)
        {
            T tmp = minHeap[i];
            int yid = min2max[i];
            while (i > 0 && minHeap[(i - 1) / 2].CompareTo(tmp) > 0)
            {
                minHeap[i] = minHeap[(i - 1) / 2];
                min2max[i] = min2max[(i - 1) / 2];
                max2min[min2max[i]] = i;
                i = (i - 1) / 2;
            }
            minHeap[i] = tmp;
            min2max[i] = yid;
            max2min[min2max[i]] = i;
        }

        private void BubbleUpMaxHeap(int i)
        {
            T tmp = maxHeap[i];
            int yid = max2min[i];
            while (i > 0 && maxHeap[(i - 1) / 2].CompareTo(tmp) < 0)
            {
                maxHeap[i] = maxHeap[(i - 1) / 2];
                max2min[i] = max2min[(i - 1) / 2];
                min2max[max2min[i]] = i;
                i = (i - 1) / 2;
            }
            maxHeap[i] = tmp;
            max2min[i] = yid;
            min2max[max2min[i]] = i;
        }

        public void SanityCheck()
        {
            if (next < 0)
            {
                throw new Exception("next is negative!");
            }

            if (IsEmpty)
            {
                return;
            }

            for (int i = 0; i < next; ++i)
            {
                int l = 2 * i + 1;
                int r = l + 1;

                if (l < next)
                {
                    if (minHeap[i].CompareTo(minHeap[l]) > 0)
                    {
                        throw new Exception("min heap is in disorder!");
                    }

                    if (maxHeap[i].CompareTo(maxHeap[l]) < 0)
                    {
                        throw new Exception("max heap is in disorder");
                    }
                }

                if (r < next)
                {
                    if (minHeap[i].CompareTo(minHeap[r]) > 0)
                    {
                        throw new Exception("min heap is in disorder!");
                    }

                    if (maxHeap[i].CompareTo(maxHeap[r]) < 0)
                    {
                        throw new Exception("max heap is in disorder");
                    }
                }
            }

            for (int i = 0; i < next; ++i)
            {
                if (max2min[min2max[i]] != i || min2max[max2min[i]] != i)
                {
                    throw new Exception("inconsistent min max mapping");
                }

                if (minHeap[i].CompareTo(maxHeap[min2max[i]]) != 0)
                {
                    throw new Exception("inconsistent min max mapping");
                }

                if (maxHeap[i].CompareTo(minHeap[max2min[i]]) != 0)
                {
                    throw new Exception("inconsistent min max mapping");
                }
            }
        }
    }

    public class SimpleMaxHeap<T> where T : IComparable<T>
    {
        public SimpleMaxHeap(int cap)
        {
            _cap = cap;
            heap = new T[cap];
        }

        public int Count
        {
            get { return next; }
        }

        public int Cap
        {
            get { return _cap; }
        }

        public bool IsFull
        {
            get { return Count == Cap; }
        }

        public bool IsEmpty
        {
            get { return Count == 0; }
        }

        public bool Insert(T Item)
        {
            //if (!IsAcceptableScore(pcw.Score))
            //{
            //    return false;
            //}

            if (IsFull)
            {
                throw new Exception("Max heap is full!");
                //if (Item.CompareTo(heap[0]) > 0)
                //{
                //    //configList.RemoveAt(0);
                //    heap[0] = Item;
                //    TrickleDown(0);
                //    return true;
                //}
                //return false;
            }
            else
            {
                heap[next++] = Item;
                BubbleUp(next - 1);
                return true;
            }
        }

        public T Peek()
        {
            if (IsEmpty)
            {
                throw new Exception("Heap is Empty");
            }

            return heap[0];
        }

        public void RemoveTop()
        {
            if (IsEmpty)
            {
                return;
            }

            heap[0] = heap[next - 1];

            next -= 1;

            TrickleDown(0);
        }

        public T[] ToSortedArray()
        {
            if (IsEmpty)
            {
                return null;
            }
            T[] toSort = new T[Count];
            Array.Copy(heap, toSort, Count);
            Array.Sort(toSort);
            //toSort = toSort.Reverse<T>();
            // Reverse

            int l = 0;
            int h = toSort.Length - 1;

            while (l < h)
            {
                T tmp = toSort[l];
                toSort[l] = toSort[h];
                toSort[h] = tmp;
                --h;
                ++l;
            }

            return toSort;
        }

        public void Clear()
        {
            //configList.Clear();
            next = 0;
        }

        private void TrickleDown(int i)
        {
            T tmp = heap[i];
            while (i * 2 + 1 < Count)
            {
                int l = 2 * i + 1;
                int r = 2 * i + 2;
                int min = (r >= Count || heap[l].CompareTo(heap[r]) >= 0) ? l : r;

                if (tmp.CompareTo(heap[min]) < 0)
                {
                    //ParserConfigWrapper tmp = configHeap[i];
                    heap[i] = heap[min];
                    i = min;
                }
                else
                {
                    break;
                }
            }
            heap[i] = tmp;
        }

        private void BubbleUp(int i)
        {
            T tmp = heap[i];
            while (i > 0 && heap[(i - 1) / 2].CompareTo(tmp) < 0)
            {
                heap[i] = heap[(i - 1) / 2];
                i = (i - 1) / 2;
            }
            heap[i] = tmp;
        }

        private readonly int _cap;
        //private SortedList<float, ParserConfigWrapper> configList;


        private T[] heap;
        private int next;
    }

    public class HeapWithScore<T>
    {
        public HeapWithScore(int cap)
        {
            this.cap = cap;
            scoreHeap = new double[cap];
            itemHeap = new T[cap];
            next = 0;
        }

        public int Count { get { return next; } }
        public int Cap { get { return cap; } }
        public bool IsFull { get { return next >= cap; } }
        public bool IsEmpty { get { return next <= 0; } }
        public bool Insert(T item, double score)
        {
            if (IsFull)
            {
                if (score > scoreHeap[0])
                {
                    //configList.RemoveAt(0);
                    itemHeap[0] = item;
                    scoreHeap[0] = score;
                    TrickleDown(0);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                scoreHeap[next] = score;
                itemHeap[next] = item;
                // Q: Why not first BubbleUp(next) and then next++?
                // A: Because next is used to indicate the number of items.
                next++;
                BubbleUp(next - 1);
                return true;
            }
        }

        public T[] ToSortedArray()
        {
            if (IsEmpty)
            {
                return null;
            }
            else
            {
                // heap sort; should use qsort but i'm lazy.
                T[] SortedItems = new T[Count];
                double[] Scores = new double[Count];
                T[] tmpItems = new T[Count];
                double[] tmpScores = new double[Count];
                Array.Copy(itemHeap, tmpItems, Count);
                Array.Copy(scoreHeap, tmpScores, Count);

                int cnt = Count;

                while (cnt > 0)
                {
                    SortedItems[cnt - 1] = tmpItems[0];
                    Scores[cnt - 1] = tmpScores[0];
                    RemoveFirst(tmpItems, tmpScores, cnt);
                    cnt--;
                }

                return SortedItems;
            }
        }

        public void GetSortedArrayWithScores(out T[] SortedItems, out double[] Scores)
        {
            if (IsEmpty)
            {
                SortedItems = null;
                Scores = null;
            }
            else
            {
                SortedItems = new T[Count];
                Scores = new double[Count];
                Array.Copy(itemHeap, SortedItems, Count);
                Array.Copy(scoreHeap, Scores, Count);
                SortItems(SortedItems, Scores, Count);
            }
        }

        private void SortItems(T[] items, double[] score, int cnt)
        {
            //naive_sort(actions, score);
            //return;
            q_sort(items, score, 0, cnt - 1);
            return;
        }

        void q_sort(T[] items, double[] score, int left, int right)
        {
            double pivot;
            int l_hold, r_hold;

            l_hold = left;
            r_hold = right;
            pivot = score[left];
            T pivot_act = items[left];
            while (left < right)
            {
                while ((score[right] <= pivot) && (left < right))
                {
                    right--;
                }

                if (left != right)
                {
                    score[left] = score[right];
                    items[left] = items[right];
                    left++;
                }

                while ((score[left] >= pivot) && (left < right))
                {
                    left++;
                }

                if (left != right)
                {
                    score[right] = score[left];
                    items[right] = items[left];
                    right--;
                }
            }

            score[left] = pivot;
            items[left] = pivot_act;
            //pivot = left;
            //left = l_hold;
            //right = r_hold;

            if (l_hold < left)
            {
                q_sort(items, score, l_hold, left - 1);
            }

            if (r_hold > left)
            {
                q_sort(items, score, left + 1, r_hold);
            }
        }

        public void Clear()
        {
            next = 0;
            Array.Clear(itemHeap, 0, cap);
            Array.Clear(scoreHeap, 0, cap);
        }

        public T Peek()
        {
            if (IsEmpty)
            {
                throw new Exception("Heap is empty!");
            }

            return itemHeap[0];
        }

        public double PeekScore()
        {
            if (IsEmpty)
            {
                throw new Exception("Heap is empty!");
            }

            return scoreHeap[0];
        }

        public void RemoveTop()
        {
            if (IsEmpty)
            {
                return;
            }

            itemHeap[0] = itemHeap[next - 1];
            scoreHeap[0] = scoreHeap[next - 1];

            next -= 1;

            TrickleDown(0);
        }

        private void RemoveFirst(T[] Items, double[] Scores, int cnt)
        {
            int pos = 0;
            Items[pos] = default(T);
            while (pos * 2 + 1 < cnt)
            {
                int l = pos * 2 + 1;
                int r = pos * 2 + 2;
                int min = (r >= cnt || Scores[l] <= Scores[r]) ? l : r;
                Items[pos] = Items[min];
                Scores[pos] = Scores[min];
                Items[min] = default(T);
                pos = min;
            }
        }

        private void TrickleDown(int i)
        {
            T tmp = itemHeap[i];
            double tmpScore = scoreHeap[i];

            int min;
            while ((min = MinChdIdx(i)) >= 0)
            {
                if (tmpScore > scoreHeap[min])
                {
                    //ParserConfigWrapper tmp = configHeap[i];
                    itemHeap[i] = itemHeap[min];
                    scoreHeap[i] = scoreHeap[min];
                    i = min;
                }
                else
                {
                    break;
                }
            }
            itemHeap[i] = tmp;
            scoreHeap[i] = tmpScore;
        }

        private void BubbleUp(int i)
        {
            T tmp = itemHeap[i];
            double tmpScore = scoreHeap[i];

            while (i > 0 && scoreHeap[(i - 1) / 2] > tmpScore)
            {
                itemHeap[i] = itemHeap[(i - 1) / 2];
                scoreHeap[i] = scoreHeap[(i - 1) / 2];
                i = (i - 1) / 2;
            }
            itemHeap[i] = tmp;
            scoreHeap[i] = tmpScore;
        }

        protected int LeftChdIdx(int id)
        {
            return 2 * id + 1;
        }

        protected int RightChdIdx(int id)
        {
            return 2 * id + 2;
        }

        protected int MinChdIdx(int id)
        {
            int l = LeftChdIdx(id);
            int r = RightChdIdx(id);
            if (l >= Count)
            {
                return -1;
            }
            return (r >= Count || scoreHeap[l] <= scoreHeap[r]) ? l : r;
        }

        private int ParentIdx(int id)
        {
            return (id - 1) / 2;
        }

        private readonly int cap;

        private T[] itemHeap;
        private double[] scoreHeap;
        private int next;
    }

    public class ScoredHeapWithQueryKey<K, T>
    {
        public ScoredHeapWithQueryKey(int cap)
        {
            this.cap = cap;
            scoreHeap = new double[cap];
            itemHeap = new T[cap];
            keyHeap = new K[cap];
            next = 0;
            idDict = new Dictionary<K, int>();
            //keyDict = new Dictionary<T, K>();
        }

        public int Count { get { return next; } }
        public int Cap { get { return cap; } }
        public bool IsFull { get { return next >= cap; } }
        public bool IsEmpty { get { return next <= 0; } }

        public bool Insert(T item, K key, double score)
        {
            if (idDict.Keys.Contains(key))
            {
                return false;
            }
            if (IsFull)
            {
                if (score > scoreHeap[0])
                {
                    //configList.RemoveAt(0);
                    idDict.Remove(keyHeap[0]);
                    idDict.Add(key, 0);
                    itemHeap[0] = item;
                    scoreHeap[0] = score;
                    keyHeap[0] = key;
                    TrickleDown(0);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                scoreHeap[next] = score;
                itemHeap[next] = item;
                keyHeap[next] = key;
                idDict.Add(key, next);
                // Q: Why not first BubbleUp(next) and then next++?
                // A: Because next is used to indicate the number of items.
                next++;
                BubbleUp(next - 1);
                return true;
            }
        }

        public bool IsAcceptableScore(double score)
        {
            if (!IsFull)
            {
                return true;
            }
            else
            {
                return score > scoreHeap[0];
            }
        }

        public bool TryGetItem(K key, out T item)
        {
            int id;
            if (idDict.TryGetValue(key, out id))
            {
                item = itemHeap[id];
                return true;
            }
            else
            {
                item = default(T);
                return false;
            }
        }

        public void ReScoring(K key, double newScore)
        {
            int id;
            if (!idDict.TryGetValue(key, out id))
            {
                return;
            }

            if (scoreHeap[id] < newScore)
            {
                scoreHeap[id] = newScore;
                TrickleDown(id);
            }
            else if (scoreHeap[id] > newScore)
            {
                scoreHeap[id] = newScore;
                BubbleUp(id);
            }
        }

        //public T[] ToSortedArray()
        //{
        //    if (IsEmpty)
        //    {
        //        return null;
        //    }
        //    else
        //    {
        //        // heap sort; should use qsort but i'm lazy.
        //        T[] SortedItems = new T[Count];
        //        double[] Scores = new double[Count];
        //        T[] tmpItems = new T[Count];
        //        double[] tmpScores = new double[Count];
        //        Array.Copy(itemHeap, tmpItems, Count);
        //        Array.Copy(scoreHeap, tmpScores, Count);

        //        int cnt = Count;

        //        while (cnt > 0)
        //        {
        //            SortedItems[cnt - 1] = tmpItems[0];
        //            Scores[cnt - 1] = tmpScores[0];
        //            RemoveFirst(tmpItems, tmpScores, cnt);
        //            cnt--;
        //        }

        //        return SortedItems;
        //    }
        //}

        public void GetSortedArrayWithScores(out T[] SortedItems, out double[] Scores)
        {
            if (IsEmpty)
            {
                SortedItems = null;
                Scores = null;
            }
            else
            {
                SortedItems = new T[Count];
                Scores = new double[Count];
                Array.Copy(itemHeap, SortedItems, Count);
                Array.Copy(scoreHeap, Scores, Count);
                SortItems(SortedItems, Scores, Count);
            }
        }

        private void SortItems(T[] items, double[] score, int cnt)
        {
            //naive_sort(actions, score);
            //return;
            q_sort(items, score, 0, cnt - 1);
            return;
        }

        void q_sort(T[] items, double[] score, int left, int right)
        {
            double pivot;
            int l_hold, r_hold;

            l_hold = left;
            r_hold = right;
            pivot = score[left];
            T pivot_act = items[left];
            while (left < right)
            {
                while ((score[right] <= pivot) && (left < right))
                {
                    right--;
                }

                if (left != right)
                {
                    score[left] = score[right];
                    items[left] = items[right];
                    left++;
                }

                while ((score[left] >= pivot) && (left < right))
                {
                    left++;
                }

                if (left != right)
                {
                    score[right] = score[left];
                    items[right] = items[left];
                    right--;
                }
            }

            score[left] = pivot;
            items[left] = pivot_act;
            //pivot = left;
            //left = l_hold;
            //right = r_hold;

            if (l_hold < left)
            {
                q_sort(items, score, l_hold, left - 1);
            }

            if (r_hold > left)
            {
                q_sort(items, score, left + 1, r_hold);
            }
        }


        public void Clear()
        {
            next = 0;
            Array.Clear(itemHeap, 0, cap);
            Array.Clear(scoreHeap, 0, cap);
            Array.Clear(keyHeap, 0, cap);
            idDict.Clear();
        }

        //private void RemoveFirst(T[] Items, double[] Scores, int cnt)
        //{
        //    int pos = 0;
        //    Items[pos] = default(T);
        //    while (pos * 2 + 1 < cnt)
        //    {
        //        int l = pos * 2 + 1;
        //        int r = pos * 2 + 2;
        //        int min = (r >= cnt || Scores[l] <= Scores[r]) ? l : r;
        //        Items[pos] = Items[min];
        //        Scores[pos] = Scores[min];
        //        Items[min] = default(T);
        //        pos = min;
        //    }
        //}

        private void TrickleDown(int i)
        {
            T tmp = itemHeap[i];
            double tmpScore = scoreHeap[i];
            K tmpKey = keyHeap[i];

            int min;

            while ((min = MinChdIdx(i)) >= 0)
            {
                if (tmpScore > scoreHeap[min])
                {
                    //ParserConfigWrapper tmp = configHeap[i];
                    idDict[keyHeap[min]] = i;
                    itemHeap[i] = itemHeap[min];
                    scoreHeap[i] = scoreHeap[min];
                    keyHeap[i] = keyHeap[min];
                    i = min;
                }
                else
                {
                    break;
                }
            }
            idDict[tmpKey] = i;
            itemHeap[i] = tmp;
            scoreHeap[i] = tmpScore;
            keyHeap[i] = tmpKey;
        }

        private void BubbleUp(int i)
        {
            T tmp = itemHeap[i];
            double tmpScore = scoreHeap[i];
            K tmpKey = keyHeap[i];
            while (i > 0 && scoreHeap[(i - 1) / 2] > tmpScore)
            {
                idDict[keyHeap[(i - 1) / 2]] = i;
                itemHeap[i] = itemHeap[(i - 1) / 2];
                scoreHeap[i] = scoreHeap[(i - 1) / 2];
                keyHeap[i] = keyHeap[(i - 1) / 2];
                i = (i - 1) / 2;
            }

            idDict[tmpKey] = i;
            itemHeap[i] = tmp;
            scoreHeap[i] = tmpScore;
            keyHeap[i] = tmpKey;
        }

        protected int LeftChdIdx(int id)
        {
            return 2 * id + 1;
        }

        protected int RightChdIdx(int id)
        {
            return 2 * id + 2;
        }

        protected int MinChdIdx(int id)
        {
            int l = LeftChdIdx(id);
            int r = RightChdIdx(id);
            if (l >= Count)
            {
                return -1;
            }
            return (r >= Count || scoreHeap[l] <= scoreHeap[r]) ? l : r;
        }

        private int ParentIdx(int id)
        {
            return (id - 1) / 2;
        }

        private readonly int cap;

        private T[] itemHeap;
        private double[] scoreHeap;
        private K[] keyHeap;
        private int next;
        private Dictionary<K, int> idDict;
        //private Dictionary<T, K> keyDict;
    }

    public class SingleScoredHeapWithQueryKey<K, T>
    {
        public SingleScoredHeapWithQueryKey(int cap)
        {
            this.cap = cap;
            scoreHeap = new float[cap];
            itemHeap = new T[cap];
            keyHeap = new K[cap];
            next = 0;
            idDict = new Dictionary<K, int>();
            //keyDict = new Dictionary<T, K>();
        }

        public int Count { get { return next; } }
        public int Cap { get { return cap; } }
        public bool IsFull { get { return next >= cap; } }
        public bool IsEmpty { get { return next <= 0; } }

        public bool Insert(T item, K key, float score)
        {
            int itemID;
            if (idDict.TryGetValue(key, out itemID))
            {
                if (scoreHeap[itemID] >= score)
                {
                    return false;
                }
                else
                {
                    itemHeap[itemID] = item;
                    scoreHeap[itemID] = score;
                    TrickleDown(itemID);
                    return true;
                }
            }
            if (IsFull)
            {
                if (score > scoreHeap[0])
                {
                    //configList.RemoveAt(0);
                    idDict.Remove(keyHeap[0]);
                    idDict.Add(key, 0);
                    itemHeap[0] = item;
                    scoreHeap[0] = score;
                    keyHeap[0] = key;
                    TrickleDown(0);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                scoreHeap[next] = score;
                itemHeap[next] = item;
                keyHeap[next] = key;
                idDict.Add(key, next);
                // Q: Why not first BubbleUp(next) and then next++?
                // A: Because next is used to indicate the number of items.
                next++;
                BubbleUp(next - 1);
                return true;
            }
        }

        public bool IsAcceptableScore(float score)
        {
            if (!IsFull)
            {
                return true;
            }
            else
            {
                return score > scoreHeap[0];
            }
        }

        public bool TryGetItem(K key, out T item)
        {
            int id;
            if (idDict.TryGetValue(key, out id))
            {
                item = itemHeap[id];
                return true;
            }
            else
            {
                item = default(T);
                return false;
            }
        }

        public void ReScoring(K key, float newScore)
        {
            int id;
            if (!idDict.TryGetValue(key, out id))
            {
                return;
            }

            if (scoreHeap[id] < newScore)
            {
                scoreHeap[id] = newScore;
                TrickleDown(id);
            }
            else if (scoreHeap[id] > newScore)
            {
                scoreHeap[id] = newScore;
                BubbleUp(id);
            }
        }

        //public T[] ToSortedArray()
        //{
        //    if (IsEmpty)
        //    {
        //        return null;
        //    }
        //    else
        //    {
        //        // heap sort; should use qsort but i'm lazy.
        //        T[] SortedItems = new T[Count];
        //        double[] Scores = new double[Count];
        //        T[] tmpItems = new T[Count];
        //        double[] tmpScores = new double[Count];
        //        Array.Copy(itemHeap, tmpItems, Count);
        //        Array.Copy(scoreHeap, tmpScores, Count);

        //        int cnt = Count;

        //        while (cnt > 0)
        //        {
        //            SortedItems[cnt - 1] = tmpItems[0];
        //            Scores[cnt - 1] = tmpScores[0];
        //            RemoveFirst(tmpItems, tmpScores, cnt);
        //            cnt--;
        //        }

        //        return SortedItems;
        //    }
        //}

        public void GetSortedArrayWithScores(out T[] SortedItems, out float[] Scores)
        {
            if (IsEmpty)
            {
                SortedItems = null;
                Scores = null;
            }
            else
            {
                SortedItems = new T[Count];
                Scores = new float[Count];
                Array.Copy(itemHeap, SortedItems, Count);
                Array.Copy(scoreHeap, Scores, Count);
                SortItems(SortedItems, Scores, Count);
            }
        }

        public List<T> GetSortedList()
        {
            if (IsEmpty)
            {
                return null;
            }

            List<T> items = new List<T>();
            float[] Scores = new float[Count];

            Array.Copy(scoreHeap, Scores, Count);

            for (int i = 0; i < Count; ++i)
            {
                items.Add(itemHeap[i]);
            }

            SortItems(items, Scores, Count);

            return items;
        }

        private void SortItems(List<T> items, float[] score, int cnt)
        {
            q_sort(items, score, 0, cnt - 1);
        }

        private void SortItems(T[] items, float[] score, int cnt)
        {
            //naive_sort(actions, score);
            //return;
            q_sort(items, score, 0, cnt - 1);
            return;
        }

        void q_sort(T[] items, float[] score, int left, int right)
        {
            float pivot;
            int l_hold, r_hold;

            l_hold = left;
            r_hold = right;
            pivot = score[left];
            T pivot_act = items[left];
            while (left < right)
            {
                while ((score[right] <= pivot) && (left < right))
                {
                    right--;
                }

                if (left != right)
                {
                    score[left] = score[right];
                    items[left] = items[right];
                    left++;
                }

                while ((score[left] >= pivot) && (left < right))
                {
                    left++;
                }

                if (left != right)
                {
                    score[right] = score[left];
                    items[right] = items[left];
                    right--;
                }
            }

            score[left] = pivot;
            items[left] = pivot_act;
            //pivot = left;
            //left = l_hold;
            //right = r_hold;

            if (l_hold < left)
            {
                q_sort(items, score, l_hold, left - 1);
            }

            if (r_hold > left)
            {
                q_sort(items, score, left + 1, r_hold);
            }
        }

        void q_sort(List<T> items, float[] score, int left, int right)
        {
            float pivot;
            int l_hold, r_hold;

            l_hold = left;
            r_hold = right;
            pivot = score[left];
            T pivot_act = items[left];
            while (left < right)
            {
                while ((score[right] <= pivot) && (left < right))
                {
                    right--;
                }

                if (left != right)
                {
                    score[left] = score[right];
                    items[left] = items[right];
                    left++;
                }

                while ((score[left] >= pivot) && (left < right))
                {
                    left++;
                }

                if (left != right)
                {
                    score[right] = score[left];
                    items[right] = items[left];
                    right--;
                }
            }

            score[left] = pivot;
            items[left] = pivot_act;
            //pivot = left;
            //left = l_hold;
            //right = r_hold;

            if (l_hold < left)
            {
                q_sort(items, score, l_hold, left - 1);
            }

            if (r_hold > left)
            {
                q_sort(items, score, left + 1, r_hold);
            }
        }

        public void Clear()
        {
            next = 0;
            Array.Clear(itemHeap, 0, cap);
            Array.Clear(scoreHeap, 0, cap);
            Array.Clear(keyHeap, 0, cap);
            idDict.Clear();
        }

        //private void RemoveFirst(T[] Items, double[] Scores, int cnt)
        //{
        //    int pos = 0;
        //    Items[pos] = default(T);
        //    while (pos * 2 + 1 < cnt)
        //    {
        //        int l = pos * 2 + 1;
        //        int r = pos * 2 + 2;
        //        int min = (r >= cnt || Scores[l] <= Scores[r]) ? l : r;
        //        Items[pos] = Items[min];
        //        Scores[pos] = Scores[min];
        //        Items[min] = default(T);
        //        pos = min;
        //    }
        //}

        private void TrickleDown(int i)
        {
            T tmp = itemHeap[i];
            float tmpScore = scoreHeap[i];
            K tmpKey = keyHeap[i];

            int min;

            while ((min = MinChdIdx(i)) >= 0)
            {
                if (tmpScore > scoreHeap[min])
                {
                    //ParserConfigWrapper tmp = configHeap[i];
                    idDict[keyHeap[min]] = i;
                    itemHeap[i] = itemHeap[min];
                    scoreHeap[i] = scoreHeap[min];
                    keyHeap[i] = keyHeap[min];
                    i = min;
                }
                else
                {
                    break;
                }
            }
            idDict[tmpKey] = i;
            itemHeap[i] = tmp;
            scoreHeap[i] = tmpScore;
            keyHeap[i] = tmpKey;
        }

        private void BubbleUp(int i)
        {
            T tmp = itemHeap[i];
            float tmpScore = scoreHeap[i];
            K tmpKey = keyHeap[i];
            while (i > 0 && scoreHeap[(i - 1) / 2] > tmpScore)
            {
                idDict[keyHeap[(i - 1) / 2]] = i;
                itemHeap[i] = itemHeap[(i - 1) / 2];
                scoreHeap[i] = scoreHeap[(i - 1) / 2];
                keyHeap[i] = keyHeap[(i - 1) / 2];
                i = (i - 1) / 2;
            }

            idDict[tmpKey] = i;
            itemHeap[i] = tmp;
            scoreHeap[i] = tmpScore;
            keyHeap[i] = tmpKey;
        }

        protected int LeftChdIdx(int id)
        {
            return 2 * id + 1;
        }

        protected int RightChdIdx(int id)
        {
            return 2 * id + 2;
        }

        protected int MinChdIdx(int id)
        {
            int l = LeftChdIdx(id);
            int r = RightChdIdx(id);
            if (l >= Count)
            {
                return -1;
            }
            return (r >= Count || scoreHeap[l] <= scoreHeap[r]) ? l : r;
        }

        private int ParentIdx(int id)
        {
            return (id - 1) / 2;
        }

        private readonly int cap;

        private T[] itemHeap;
        private float[] scoreHeap;
        private K[] keyHeap;
        private int next;
        private Dictionary<K, int> idDict;
        //private Dictionary<T, K> keyDict;
    }

    public class HeapWithSingleScore<T>
    {
        public HeapWithSingleScore(int cap)
        {
            this.cap = cap;
            scoreHeap = new float[cap];
            itemHeap = new T[cap];
            next = 0;
        }

        public int Count { get { return next; } }
        public int Cap { get { return cap; } }
        public bool IsFull { get { return next >= cap; } }
        public bool IsEmpty { get { return next <= 0; } }
        public bool Insert(T item, float score)
        {
            if (IsFull)
            {
                if (score > scoreHeap[0])
                {
                    //configList.RemoveAt(0);
                    itemHeap[0] = item;
                    scoreHeap[0] = score;
                    TrickleDown(0);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                scoreHeap[next] = score;
                itemHeap[next] = item;
                // Q: Why not first BubbleUp(next) and then next++?
                // A: Because next is used to indicate the number of items.
                next++;
                BubbleUp(next - 1);
                return true;
            }
        }

        public T[] ToSortedArray()
        {
            if (IsEmpty)
            {
                return null;
            }
            else
            {
                // heap sort; should use qsort but i'm lazy.
                T[] SortedItems = new T[Count];
                float[] Scores = new float[Count];
                T[] tmpItems = new T[Count];
                float[] tmpScores = new float[Count];
                Array.Copy(itemHeap, tmpItems, Count);
                Array.Copy(scoreHeap, tmpScores, Count);

                int cnt = Count;

                while (cnt > 0)
                {
                    SortedItems[cnt - 1] = tmpItems[0];
                    Scores[cnt - 1] = tmpScores[0];
                    RemoveFirst(tmpItems, tmpScores, cnt);
                    cnt--;
                }

                return SortedItems;
            }
        }

        public void GetSortedArrayWithScores(out T[] SortedItems, out float[] Scores)
        {
            if (IsEmpty)
            {
                SortedItems = null;
                Scores = null;
            }
            else
            {
                SortedItems = new T[Count];
                Scores = new float[Count];
                Array.Copy(itemHeap, SortedItems, Count);
                Array.Copy(scoreHeap, Scores, Count);
                SortItems(SortedItems, Scores, Count);
            }
        }

        private void SortItems(T[] items, float[] score, int cnt)
        {
            //naive_sort(actions, score);
            //return;
            q_sort(items, score, 0, cnt - 1);
            return;
        }

        void q_sort(T[] items, float[] score, int left, int right)
        {
            float pivot;
            int l_hold, r_hold;

            l_hold = left;
            r_hold = right;
            pivot = score[left];
            T pivot_act = items[left];
            while (left < right)
            {
                while ((score[right] <= pivot) && (left < right))
                {
                    right--;
                }

                if (left != right)
                {
                    score[left] = score[right];
                    items[left] = items[right];
                    left++;
                }

                while ((score[left] >= pivot) && (left < right))
                {
                    left++;
                }

                if (left != right)
                {
                    score[right] = score[left];
                    items[right] = items[left];
                    right--;
                }
            }

            score[left] = pivot;
            items[left] = pivot_act;
            //pivot = left;
            //left = l_hold;
            //right = r_hold;

            if (l_hold < left)
            {
                q_sort(items, score, l_hold, left - 1);
            }

            if (r_hold > left)
            {
                q_sort(items, score, left + 1, r_hold);
            }
        }

        public void Clear()
        {
            next = 0;
            Array.Clear(itemHeap, 0, cap);
            Array.Clear(scoreHeap, 0, cap);
        }

        private void RemoveFirst(T[] Items, float[] Scores, int cnt)
        {
            int pos = 0;
            Items[pos] = default(T);
            while (pos * 2 + 1 < cnt)
            {
                int l = pos * 2 + 1;
                int r = pos * 2 + 2;
                int min = (r >= cnt || Scores[l] <= Scores[r]) ? l : r;
                Items[pos] = Items[min];
                Scores[pos] = Scores[min];
                Items[min] = default(T);
                pos = min;
            }
        }

        private void TrickleDown(int i)
        {
            T tmp = itemHeap[i];
            float tmpScore = scoreHeap[i];

            int min;
            while ((min = MinChdIdx(i)) >= 0)
            {
                if (tmpScore > scoreHeap[min])
                {
                    //ParserConfigWrapper tmp = configHeap[i];
                    itemHeap[i] = itemHeap[min];
                    scoreHeap[i] = scoreHeap[min];
                    i = min;
                }
                else
                {
                    break;
                }
            }
            itemHeap[i] = tmp;
            scoreHeap[i] = tmpScore;
        }

        private void BubbleUp(int i)
        {
            T tmp = itemHeap[i];
            float tmpScore = scoreHeap[i];

            while (i > 0 && scoreHeap[(i - 1) / 2] > tmpScore)
            {
                itemHeap[i] = itemHeap[(i - 1) / 2];
                scoreHeap[i] = scoreHeap[(i - 1) / 2];
                i = (i - 1) / 2;
            }
            itemHeap[i] = tmp;
            scoreHeap[i] = tmpScore;
        }

        protected int LeftChdIdx(int id)
        {
            return 2 * id + 1;
        }

        protected int RightChdIdx(int id)
        {
            return 2 * id + 2;
        }

        protected int MinChdIdx(int id)
        {
            int l = LeftChdIdx(id);
            int r = RightChdIdx(id);
            if (l >= Count)
            {
                return -1;
            }
            return (r >= Count || scoreHeap[l] <= scoreHeap[r]) ? l : r;
        }

        private int ParentIdx(int id)
        {
            return (id - 1) / 2;
        }

        private readonly int cap;

        private T[] itemHeap;
        private float[] scoreHeap;
        private int next;
    }

}
