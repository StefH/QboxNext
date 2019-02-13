using JetBrains.Annotations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QboxNext.Server.Common.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// See https://blogs.msdn.microsoft.com/pfxteam/2012/03/05/implementing-a-simple-foreachasync-part-2/
        /// </summary>
        /// <typeparam name="T">The generic type.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="function">The function to execute.</param>
        /// <param name="partitionCount">The number of partitions to create.</param>
        /// <returns></returns>
        public static Task ForEachAsync<T>([NotNull] this IEnumerable<T> source, [NotNull] Func<T, Task> function, int partitionCount = 5)
        {
            return Task.WhenAll(
                from partition in Partitioner.Create(source).GetPartitions(partitionCount)
                select Task.Run(async delegate
                {
                    using (partition)
                    {
                        while (partition.MoveNext())
                        {
                            await function(partition.Current);
                        }
                    }
                }));
        }
    }
}