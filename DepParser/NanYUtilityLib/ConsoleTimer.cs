using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NanYUtilityLib.Sweets
{
    public class ConsoleTimer
    {
        public ConsoleTimer()
            :this(DefaultBatchSize)
        {
        }

        public ConsoleTimer(int batchSize)
        {
            this.BatchSize = Math.Max(1, batchSize);
            startTime = DateTime.Now;
        }

        public void Up()
        {
            if (++counter % BatchSize == 0)
            {
                Console.Error.Write("#{0}\tTime: {1}...\r", counter, DateTime.Now - startTime);
            }
        }

        public void Message(string msg)
        {
            Console.Error.WriteLine("#{0}: {1}", counter, msg);
        }

        public void Finish()
        {
            Console.Error.WriteLine("#{0}\tTime: {1}...Done", counter, DateTime.Now - startTime);
        }

        public void Start()
        {
            startTime = DateTime.Now;
            counter = 0;
        }

        public void Reset()
        {
            startTime = DateTime.Now;
            counter = 0;
        }

        long counter = 0;
        DateTime startTime;

        const int DefaultBatchSize = 1000;
        public int BatchSize { get; private set; }
    }
}
