using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NanYUtilityLib
{
    public class LargeArray<T>
    {
        private const long ELEM_PER_ARR = 64 * 1024 * 1024;

        private T[][] pools;

        public long Length { get; private set; }

        public T this[long index]
        {
            get
            {
                return pools[index / ELEM_PER_ARR][index % ELEM_PER_ARR];
            }
            set
            {
                pools[index / ELEM_PER_ARR][index % ELEM_PER_ARR] = value;
            }
        }

        public LargeArray(long Length)
        {
            if (Length < 0)
            {
                throw new Exception("Length must be at least 0.");
            }

            this.Length = Length;

            if (Length == 0)
            {
                return;
            }

            long poolCount = Length / ELEM_PER_ARR;
            long reminder = Length % ELEM_PER_ARR;

            if (reminder > 0)
            {
                pools = new T[poolCount + 1][];

                for (long i = 0; i < poolCount; ++i)
                {
                    pools[i] = new T[ELEM_PER_ARR];
                }

                pools[pools.Length - 1] = new T[reminder];
            }
            else
            {
                pools = new T[poolCount][];

                for (long i = 0; i < poolCount; ++i)
                {
                    pools[i] = new T[ELEM_PER_ARR];
                }
            }
        }
    }
}
