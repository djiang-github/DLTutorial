using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NanYUtilityLib
{
    /*
     * A lightweight BitVector implementation.
     * I made it so I could efficiently do things like checking for null intersection
     * (without allocating anything)
     * 
     * WARNING!!! I do no boundary condition checks anywhere, so be careful, or add them yourself.
     * 
     * Has a few convenience methods to perform some operations without allocating a new bitvector
     */
    public class BitVector
    {
        uint[] bits;
        int length;

        // this will crash if you try to make a zero-length one (but why would you do that?)
        public BitVector(int length)
        {
#if DEBUG
            if (length == 0) throw new Exception("No zero length bit arrays allowed!");
#endif
            this.length = length;
            bits = new uint[((length - 1) >> 5) + 1];
            System.GC.SuppressFinalize(this);
        }

        // copy constructor
        public BitVector(BitVector orig)
        {
            length = orig.length;
            bits = new uint[orig.bits.Length];
            Array.Copy(orig.bits, bits, bits.Length);
            System.GC.SuppressFinalize(this);
        }

        public int Length { get { return length; } }

        public void Clear()
        {
            for (int i = 0; i < bits.Length; i++)
            {
                bits[i] = 0u;
            }
        }

        public bool this[int i]
        {
            get
            {
                int whichInt = i >> 5;
                int pos = i & 31;
                return (bits[whichInt] & (1u << pos)) > 0;
            }//end get

            set
            {
                int whichInt = i >> 5;
                if (whichInt > bits.Length - 1)
                {
                    Console.Error.WriteLine("Error: found a vector out of boundary.");
                    return;
                }

                int pos = i & 31;
                if (value)
                {
                    bits[whichInt] |= (1u << pos);
                }
                else
                {
                    bits[whichInt] &= ~(1u << pos);
                }
            }//end set

        }

        public bool IsNullOnSpan(int start, int end)
        {
            for (int i = start; i < end; i++)
            {
                if (this[i]) return false;
            }
            return true;
        }

        public bool IsAllTrue()
        {
            for (int i = 0; i < bits.Length - 1; i++)
            {
                if (bits[i] < 0xFFFFFFFFu) return false;
            }
            if (bits[bits.Length - 1] < (0xFFFFFFFFu >> (32 - length & 31))) return false;

            return true;
        }

        public bool IsSubsetOf(BitVector other)
        {
            for (int i = 0; i < bits.Length; i++)
            {
                if ((~other.bits[i] & bits[i]) != 0) return false;
            }
            return true;
        }

        public static BitVector operator &(BitVector a, BitVector b)
        {
            BitVector result = new BitVector(a.length);
            for (int i = 0; i < a.bits.Length; i++)
            {
                result.bits[i] = a.bits[i] & b.bits[i];
            }
            return result;
        }

        public static BitVector operator |(BitVector a, BitVector b)
        {
            BitVector result = new BitVector(a.length);
            for (int i = 0; i < a.bits.Length; i++)
            {
                result.bits[i] = a.bits[i] | b.bits[i];
            }
            return result;
        }

        public static BitVector operator ^(BitVector a, BitVector b)
        {
            BitVector result = new BitVector(a.length);
            for (int i = 0; i < a.bits.Length; i++)
            {
                result.bits[i] = a.bits[i] ^ b.bits[i];
            }
            return result;
        }

        public override bool Equals(object obj)
        {
            if (this == obj) return true;

            BitVector other = obj as BitVector;
            if (other == null) return false;

            if (length != other.length) return false;

            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i] != other.bits[i]) return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            uint val = (uint)(503 * length);
            foreach (uint i in bits) val ^= i;
            return (int)val;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                sb.Append(this[i] ? '1' : '0');
            }
            return sb.ToString();
        }
    }

    // the representation of an alignment matrix, implemented efficiently with a BitVector
    public class AlignmentMatrix //: InternedObject 
    {
        BitVector bits;
        private readonly ushort targetLength, sourceLength;
        //int[] sourceFert;
        //int[] targetFert;

        //int[] sourceAlign;
        //int[] targetAlign;

        public AlignmentMatrix(int targetLength, int sourceLength)
        {
            this.targetLength = (ushort)targetLength;
            this.sourceLength = (ushort)sourceLength;

            //sourceAlign = new int[sourceLength];
            //targetAlign = new int[targetLength];

            //for (int i = 0; i < sourceFert.Length; ++i)
            //{
            //    sourceFert[i] = 0;
            //    sourceAlign[i] = -1;
            //}

            //for (int i = 0; i < targetFert.Length; ++i)
            //{
            //    targetFert[i] = 0;
            //    targetAlign[i] = -1;
            //}

            bits = new BitVector(sourceLength * targetLength);
            System.GC.SuppressFinalize(this);
        }

        public AlignmentMatrix(int targetLength, int sourceLength, string alignLine)
            : this(targetLength, sourceLength)
        {
            string[] alignPairs = alignLine.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string[] sep = { "-" };
            foreach (string alignPair in alignPairs)
            {
                string[] parts = alignPair.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    int sid;
                    int tid;
                    if (int.TryParse(parts[0], out sid) && int.TryParse(parts[1], out tid))
                    {
                        if (sid < sourceLength && sid >= 0 && tid < targetLength && tid >= 0)
                        {
                            this[tid, sid] = true;
                        }
                    }
                }
            }
        }

        public AlignmentMatrix(int targetLength, int sourceLength, string alignLine, bool allowPossibleLinks)
            : this(targetLength, sourceLength)
        {
            string[] alignPairs = alignLine.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string[] sep = { "-" };
            foreach (string alignPair in alignPairs)
            {
                string[] parts = alignPair.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 || (parts.Length == 3 && allowPossibleLinks))
                {
                    int sid;
                    int tid;
                    if (int.TryParse(parts[0], out sid) && int.TryParse(parts[1], out tid))
                    {
                        if (sid < sourceLength && sid >= 0 && tid < targetLength && tid >= 0)
                        {
                            this[tid, sid] = true;
                        }
                    }
                }
            }
        }


        public static AlignmentMatrix FitToSize(int targetLength, int sourceLength, AlignmentMatrix A)
        {
            var r = new AlignmentMatrix(targetLength, sourceLength);

            int minS = Math.Min(sourceLength, A.sourceLength);
            int minT = Math.Min(targetLength, A.targetLength);

            for (int tid = 0; tid < minT; ++tid)
            {
                for (int sid = 0; sid < minS; ++sid)
                {
                    r[tid, sid] = A[tid, sid];
                }
            }

            return r;
        }

        public AlignmentMatrix(string alignLine)
        {
            string[] alignPairs = alignLine.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string[] sep = { "-" };

            List<int> srcPos = new List<int>();
            List<int> trgPos = new List<int>();
            int maxsid = -1;
            int maxtid = -1;
            foreach (string alignPair in alignPairs)
            {
                string[] parts = alignPair.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    int sid;
                    int tid;
                    if (int.TryParse(parts[0], out sid) && int.TryParse(parts[1], out tid))
                    {
                        if (sid >= 0 && tid >= 0)
                        {
                            srcPos.Add(sid);
                            trgPos.Add(tid);
                            maxsid = Math.Max(sid, maxsid);
                            maxtid = Math.Max(tid, maxtid);
                        }
                    }
                }
            }

            sourceLength = (ushort)(maxsid + 1);
            targetLength = (ushort)(maxtid + 1);

            if (srcPos.Count > 0)
            {
                //this.sourceFert = new int[sourceLength];
                //this.targetFert = new int[targetLength];

                //sourceAlign = new int[sourceLength];
                //targetAlign = new int[targetLength];

                //for (int i = 0; i < sourceFert.Length; ++i)
                //{
                //    sourceFert[i] = 0;
                //    sourceAlign[i] = -1;
                //}

                //for (int i = 0; i < targetFert.Length; ++i)
                //{
                //    targetFert[i] = 0;
                //    targetAlign[i] = -1;
                //}

                bits = new BitVector(sourceLength * targetLength);
                System.GC.SuppressFinalize(this);

                for (int id = 0; id < srcPos.Count; ++id)
                {
                    this[trgPos[id], srcPos[id]] = true;
                }
            }

        }

        public AlignmentMatrix(string alignLine, bool allowPossibleLinks)
        {
            string[] alignPairs = alignLine.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string[] sep = { "-" };

            List<int> srcPos = new List<int>();
            List<int> trgPos = new List<int>();
            int maxsid = -1;
            int maxtid = -1;
            foreach (string alignPair in alignPairs)
            {
                string[] parts = alignPair.Split(sep, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 || (parts.Length == 3 && allowPossibleLinks))
                {
                    int sid;
                    int tid;
                    if (int.TryParse(parts[0], out sid) && int.TryParse(parts[1], out tid))
                    {
                        if (sid >= 0 && tid >= 0)
                        {
                            srcPos.Add(sid);
                            trgPos.Add(tid);
                            maxsid = Math.Max(sid, maxsid);
                            maxtid = Math.Max(tid, maxtid);
                        }
                    }
                }
            }

            sourceLength = (ushort)(maxsid + 1);
            targetLength = (ushort)(maxtid + 1);

            if (srcPos.Count > 0)
            {
                //this.sourceFert = new int[sourceLength];
                //this.targetFert = new int[targetLength];

                //sourceAlign = new int[sourceLength];
                //targetAlign = new int[targetLength];

                //for (int i = 0; i < sourceFert.Length; ++i)
                //{
                //    sourceFert[i] = 0;
                //    sourceAlign[i] = -1;
                //}

                //for (int i = 0; i < targetFert.Length; ++i)
                //{
                //    targetFert[i] = 0;
                //    targetAlign[i] = -1;
                //}

                bits = new BitVector(sourceLength * targetLength);
                System.GC.SuppressFinalize(this);

                for (int id = 0; id < srcPos.Count; ++id)
                {
                    this[trgPos[id], srcPos[id]] = true;
                }
            }

        }

        public AlignmentMatrix Transpose()
        {
            AlignmentMatrix amx = new AlignmentMatrix(sourceLength, targetLength);

            for (int sid = 0; sid < sourceLength; ++sid)
            {
                for (int tid = 0; tid < targetLength; ++tid)
                {
                    amx[sid, tid] = this[tid, sid];
                }
            }

            return amx;
        }

        public AlignmentMatrix Multiply(AlignmentMatrix other)
        {
            if (targetLength != other.sourceLength)
            {
                throw new Exception("Two matrix is not valid for multiply operation!");
            }

            AlignmentMatrix amx = new AlignmentMatrix(other.targetLength, sourceLength);

            for (int sid = 0; sid < sourceLength; ++sid)
            {
                for (int tid = 0; tid < other.targetLength; ++tid)
                {
                    for (int k = 0; k < targetLength; ++k)
                    {
                        if (this[k, sid] && other[tid, k])
                        {
                            amx[tid, sid] = true;
                            break;
                        }
                    }
                }
            }

            return amx;
        }

        public string GetAlignmentString()
        {
            List<string> alist = new List<string>();

            for (int sid = 0; sid < sourceLength; ++sid)
            {
                for (int tid = 0; tid < targetLength; ++tid)
                {
                    if (this[tid, sid])
                    {
                        alist.Add(string.Format("{0}-{1}", sid, tid));
                    }
                }
            }

            return string.Join(" ", alist);
        }
        public int SourceLength { get { return sourceLength; } }
        public int TargetLength { get { return targetLength; } }

        public bool this[int targetIndex, int sourceIndex]
        {
            // check for bounds?
            get { return bits[IndexOf(targetIndex, sourceIndex)]; }
            set
            {
                bits[IndexOf(targetIndex, sourceIndex)] = value;
                //sourceFert[sourceIndex]++;
                //targetFert[targetIndex]++;
                //sourceAlign[sourceIndex] = targetIndex;
                //targetAlign[targetIndex] = sourceIndex;
            }
        }

        private int IndexOf(int targetIndex, int sourceIndex)
        {
            return targetIndex * sourceLength + sourceIndex;
        }

        public IEnumerable<int> SourceWordsAlignedTo(int targetIndex)
        {
            for (int s = 0; s < sourceLength; s++)
            {
                if (this[targetIndex, s]) yield return s;
            }
        }

        public IEnumerable<int> TargetWordsAlignedTo(int sourceIndex)
        {
            for (int t = 0; t < targetLength; t++)
            {
                if (this[t, sourceIndex]) yield return t;
            }
        }

        public IEnumerable<int> SourceWordsAlignedTo(int tb, int te)
        {
            for (int t = tb; t < te; t++)
            {
                for (int s = 0; s < sourceLength; s++)
                {
                    if (this[t, s])
                    {
                        yield return s;
                    }
                }
            }
        }

        public IEnumerable<int> TargetWordsAlignedTo(int sb, int se)
        {
            for (int s = sb; s < se; s++)
            {
                for (int t = 0; t < targetLength; t++)
                {
                    if (this[t, s])
                    {
                        yield return t;
                    }
                }
            }
        }

        public List<int> SourceWordListAlignedTo(int targetIndex)
        {
            List<int> r = new List<int>();
            for (int s = 0; s < sourceLength; s++)
            {
                if (this[targetIndex, s]) r.Add(s);
            }

            return r;
        }

        public List<int> TargetWordListAlignedTo(int sourceIndex)
        {
            List<int> r = new List<int>();
            for (int t = 0; t < targetLength; t++)
            {
                if (this[t, sourceIndex]) r.Add(t);
            }

            return r;
        }

        public List<int> SourceWordListAlignedTo(int tb, int te)
        {
            List<int> r = new List<int>();
            for (int t = tb; t < te; t++)
            {
                for (int s = 0; s < sourceLength; s++)
                {
                    if (this[t, s])
                    {
                        r.Add(s);
                    }
                }
            }

            return r;
        }

        public List<int> TargetWordListAlignedTo(int sb, int se)
        {
            List<int> r = new List<int>();
            for (int s = sb; s < se; s++)
            {
                for (int t = 0; t < targetLength; t++)
                {
                    if (this[t, s])
                    {
                        r.Add(t);
                    }
                }
            }

            return r;
        }

        /// <summary>
        /// Return index of word aligned to sourceIndex.
        /// Return -1 if unaligned
        /// Return index of an abitrary one if muliple alignment.
        /// </summary>
        /// <param name="targetIndex"></param>
        /// <returns></returns>
        //public int SourceWordAlignedTo(int targetIndex)
        //{
        //    return targetAlign[targetIndex];
        //}
        /// <summary>
        /// Return index of word aligned to targetIndex.
        /// Return -1 if unaligned
        /// Return index of an abitrary one if muliple alignment.
        /// </summary>
        /// <param name="sourceIndex"></param>
        /// <returns></returns>
        //public int TargetWordAlignedTo(int sourceIndex)
        //{
        //    return sourceAlign[sourceIndex];
        //}

        public bool TargetWordsAlignedTo(int sb, int se, out int tb, out int te)
        {
            tb = targetLength + 1;
            te = -1;
            if (sb >= se || sb < 0 || se > sourceLength)
            {
                return false;
            }
            bool flag = false;
            for (int s = sb; s < se; s++)
            {
                for (int t = 0; t < targetLength; t++)
                {
                    if (this[t, s])
                    {
                        flag = true;
                        tb = Math.Min(tb, t);
                        te = Math.Max(te, t + 1);
                    }
                }
            }
            return flag;
        }

        public bool TargetWordsAlignedTo(int s, out int tb, out int te)
        {
            return TargetWordsAlignedTo(s, s + 1, out tb, out te);
        }

        public bool SourceWordsAlignedTo(int tb, int te, out int sb, out int se)
        {
            sb = sourceLength + 1;
            se = -1;
            if (tb >= te || tb < 0 || te > targetLength)
            {
                return false;
            }
            bool flag = false;
            for (int t = tb; t < te; t++)
            {
                for (int s = 0; s < sourceLength; s++)
                {
                    if (this[t, s])
                    {
                        flag = true;
                        sb = Math.Min(sb, s);
                        se = Math.Max(se, s + 1);
                    }
                }
            }
            return flag;
        }

        public bool SourceWordsAlignedTo(int t, out int sb, out int se)
        {
            return SourceWordsAlignedTo(t, t + 1, out sb, out se);
        }

        public bool TargetWordUnaligned(int targetIndex)
        {
            for (int s = 0; s < sourceLength; s++)
            {
                if (this[targetIndex, s]) return false;
            }
            return true;
        }

        public bool SourceWordUnaligned(int sourceIndex)
        {
            for (int t = 0; t < targetLength; t++)
            {
                if (this[t, sourceIndex]) return false;
            }
            return true;
        }

        //public int SourceWordFertility(int sourceIndex)
        //{
        //    return sourceFert[sourceIndex];
        //}

        //public int TargetWordFertility(int targetIndex)
        //{
        //    return targetFert[targetIndex];
        //}

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(bits.Length * 2);
            sb.Append("  ");
            for (int i = 0; i < sourceLength; i++)
            {
                sb.Append(i % 10);
            }
            sb.Append('\n');
            for (int r = 0; r < targetLength; r++)
            {
                sb.Append(r % 10);
                sb.Append(' ');
                for (int c = 0; c < sourceLength; c++)
                {
                    sb.Append(this[r, c] ? '1' : '0');
                }
                sb.Append('\n');
            }
            return sb.ToString();
        }

        public bool NeitherJNorIHaveAlign(int targetIndex, int sourceIndex)
        {
            //to see whether the j_th word in source sentence has an alignment in A
            for (int k = 0; k < targetLength; k++)
            {
                if (this[k, sourceIndex] == true)
                    return false;
            }

            //to see whether the i_th word in target sentence has an alignment in A
            for (int k = 0; k < sourceLength; k++)
            {
                if (this[targetIndex, k] == true)
                    return false;
            }

            return true;
        }

        public bool AijHasHNeighbor(int targetIndex, int sourceIndex)
        {
            if ((sourceIndex != 0) && (this[targetIndex, sourceIndex - 1] == true))
            {
                return true;
            }

            if ((sourceIndex != sourceLength - 1) && (this[targetIndex, sourceIndex + 1] == true))
            {
                return true;
            }

            return false;
        }

        public bool AijHasVNeighbor(int targetIndex, int sourceIndex)
        {
            if ((targetIndex != 0) && (this[targetIndex - 1, sourceIndex] == true))
            {
                return true;
            }

            if ((targetIndex != targetLength - 1) && (this[targetIndex + 1, sourceIndex] == true))
            {
                return true;
            }

            return false;
        }

        public bool AijHasNeighbor(int targetIndex, int sourceIndex)
        {
            return (AijHasHNeighbor(targetIndex, sourceIndex) || AijHasVNeighbor(targetIndex, sourceIndex));
        }

        //Yajuan changed 051013. for the sencond condition in och's p432, the "neighbors" means not only immediate neighbors, otherwise follow error will occur
        public bool ExistAlignHavingHVNeighbors(int targetIndex, int sourceIndex)
        {

            if (AijHasWildHNeighbor(targetIndex, sourceIndex) && AijHasWildVNeighbor(targetIndex, sourceIndex))
            {
                return true;
            }

            //look for a alignment which has both horizontal and vertical neighbors
            for (int i = 0; i < targetLength; i++)
            {
                if (this[i, sourceIndex] == true && i != targetIndex && AijHasWildHNeighbor(i, sourceIndex))
                {
                    return true;
                }
            }

            for (int j = 0; j < sourceLength; j++)
            {
                if (this[targetIndex, j] == true && j != sourceIndex && AijHasWildVNeighbor(targetIndex, j))
                {
                    return true;
                }
            }

            //still can not find
            return false;
        }

        public bool AijHasWildHNeighbor(int targetIndex, int sourceIndex)
        {
            for (int j = 0; j < sourceLength; j++)
            {
                if (j != sourceIndex && this[targetIndex, j] == true)
                {
                    return true;
                }
            }

            return false;
        }

        public bool AijHasWildVNeighbor(int targetIndex, int sourceIndex)
        {
            for (int i = 0; i < targetLength; i++)
            {
                if (i != targetIndex && this[i, sourceIndex] == true)
                {
                    return true;
                }
            }

            return false;
        }

        public static AlignmentMatrix Intersection(AlignmentMatrix A, AlignmentMatrix B)
        {
            int sLen = Math.Max(A.SourceLength, B.SourceLength);

            int tLen = Math.Max(A.TargetLength, B.TargetLength);

            AlignmentMatrix intersect = new AlignmentMatrix(tLen, sLen);

            int minS = Math.Min(A.sourceLength, B.sourceLength);
            int minT = Math.Min(A.targetLength, B.targetLength);

            for (int tid = 0; tid < minT; ++tid)
            {
                for (int sid = 0; sid < minS; ++sid)
                {
                    intersect[tid, sid] = A[tid, sid] && B[tid, sid];
                }
            }

            return intersect;
        }

        public static AlignmentMatrix Union(AlignmentMatrix A, AlignmentMatrix B)
        {
            int sLen = Math.Max(A.SourceLength, B.SourceLength);

            int tLen = Math.Max(A.TargetLength, B.TargetLength);

            AlignmentMatrix unionMtx = new AlignmentMatrix(sLen, tLen);

            int minS = Math.Min(A.sourceLength, B.sourceLength);
            int minT = Math.Min(A.targetLength, B.targetLength);

            for (int tid = 0; tid < tLen; ++tid)
            {
                for (int sid = 0; sid < sLen; ++sid)
                {
                    if (tid < B.targetLength && sid < B.sourceLength
                        && B[tid, sid])
                    {
                        unionMtx[tid, sid] = true;
                    }
                    else if (tid < A.targetLength && sid < A.sourceLength
                        && A[tid, sid])
                    {
                        unionMtx[tid, sid] = true;
                    }
                }
            }

            return unionMtx;
        }

        public static AlignmentMatrix DiagGrow(AlignmentMatrix A, AlignmentMatrix B)
        {
            //var intersection = Intersection(A, B);

            int slen = Math.Max(A.sourceLength, B.sourceLength);
            int tlen = Math.Max(A.targetLength, B.targetLength);

            A = FitToSize(tlen, slen, A);

            B = FitToSize(tlen, slen, B);


            var result = Intersection(A, B);

            //second step,extend A
            bool changed = false;
            while (true)
            {
                changed = false;
                for (int tIndex = 0; tIndex < tlen; tIndex++)
                {
                    for (int sIndex = 0; sIndex < slen; sIndex++)
                    {
                        if (!result[tIndex, sIndex] && (A[tIndex, sIndex] || B[tIndex, sIndex]))
                        {
                            if ((result.NeitherJNorIHaveAlign(tIndex, sIndex) || ((result.AijHasNeighbor(tIndex, sIndex)) && (!result.ExistAlignHavingHVNeighbors(tIndex, sIndex)))))
                            {
                                //if (aCE.AijHasWildHNeighbor(eIndex, cIndex) ||
                                //    aCE.AijHasWildVNeighbor(eIndex, cIndex))
                                //{
                                result[tIndex, sIndex] = true;
                                changed = true;
                                //}
                            }
                        }
                    }
                }
                if (!changed) break;
            }

            return result;
        }
    }


}
