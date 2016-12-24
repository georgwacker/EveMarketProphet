using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EveMarketProphet.Utils
{
    public class ParallelUtils
    {
        public static void ParallelWhileNotEmpty<T>(IEnumerable<T> initialValues, Action<T, Action<T>> body)
        {
            var from = new ConcurrentQueue<T>(initialValues);
            while (!from.IsEmpty)
            {
                var to = new ConcurrentQueue<T>();
                Action<T> addMethod = to.Enqueue;
                Parallel.ForEach(from, new ParallelOptions() { MaxDegreeOfParallelism = 20 }, v => body(v, addMethod));
                from = to;
            }
        }
    }
}
