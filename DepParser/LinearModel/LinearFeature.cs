using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinearFunction
{
    public class LinearModelFeature : IComparable<LinearModelFeature>
    {
        public int DictId {get; private set;}

        public int[] ElemArr { get; private set;}

        public LinearModelFeature(int DictId, int[] ElemArr)
        {
            this.ElemArr = new int[ElemArr.Length];
            ElemArr.CopyTo(this.ElemArr, 0);
            //(int[])ElemArr.Clone();
            this.DictId = DictId;
        }

        public LinearModelFeature(int DictId, int Length)
        {
            this.ElemArr = new int[Length];
            this.DictId = DictId;
        }

        public LinearModelFeature(string descript)
        {
            string[] parts = descript.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length <= 1)
            {
                throw new Exception();
            }

            DictId = int.Parse(parts[0]);

            ElemArr = new int[parts.Length - 1];

            for (int i = 0; i < ElemArr.Length; ++i)
            {
                ElemArr[i] = int.Parse(parts[i + 1]);
            }
        }

        public LinearModelFeature(LinearModelFeature f)
        {
            //f.CopyTo(this);
            this.ElemArr = new int[f.ElemArr.Length];
            f.ElemArr.CopyTo(this.ElemArr, 0);
            //(int[])ElemArr.Clone();
            this.DictId = f.DictId;
        }

        public int Length { get { return ElemArr.Length; } }

        public override string ToString()
        {
            return string.Format("{0}_{1}", DictId, string.Join("_", ElemArr));
        }

        public int CompareTo(LinearModelFeature other)
        {
            if (other == null)
            {
                return -1;
            }
            int r = DictId.CompareTo(other.DictId);
            if (r != 0)
            {
                return r;
            }

            for (int i = 0; i < ElemArr.Length; ++i)
            {
                r = ElemArr[i].CompareTo(other.ElemArr[i]);
                if (r != 0)
                {
                    return r;
                }
            }

            return r;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(LinearModelFeature))
            {
                return false;
            }

            if (obj == null)
            {
                return false;
            }

            LinearModelFeature other = (LinearModelFeature)obj;

            if (Length != other.Length || DictId != other.DictId)
            {
                return false;
            }

            for (int i = 0; i < Length; ++i)
            {
                if (ElemArr[i] != other.ElemArr[i])
                {
                    return false;
                }
            }

            return true;
        }

        public bool Equals(LinearModelFeature other)
        {
            if (other == null)
            {
                return false;
            }

            if (Length != other.Length || DictId != other.DictId)
            {
                return false;
            }

            for (int i = 0; i < Length; ++i)
            {
                if (ElemArr[i] != other.ElemArr[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // Fowler-Noll-Vo Hash
                // modified from byte version

                const uint p = 16777619;
                uint hash = 2166136261;
                //foreach (byte b in data)
                //    hash = (hash ^ b) * p;
                //hash = (hash ^ (uint)DictId) * p;
                foreach (int data in ElemArr)
                {
                    //uint b = (uint)data & 0xff;


                    hash = (hash ^ ((uint)data)) * p;

                    //hash = (hash ^ ((uint)data & 0xff)) * p;

                    //hash = (hash ^ (((uint)data >> 8) & 0xff)) * p;

                    //hash = (hash ^ (((uint)data >> 16) & 0xff)) * p;

                    //hash = (hash ^ (((uint)data >> 24) & 0xff)) * p;
                    
                }
                hash += hash << 13;
                hash ^= hash >> 7;
                hash += hash << 3;
                hash ^= hash >> 17;
                hash += hash << 5;
                return (int)hash;
            }
        }

        public bool IsValid
        {
            get
            {
                foreach (var x in ElemArr)
                {
                    if (x < 0)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
