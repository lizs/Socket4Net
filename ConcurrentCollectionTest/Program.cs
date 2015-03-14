
using System;
using System.Threading;

#if NET35
using Core.ConcurrentCollection;
#else
#endif

namespace ConcurrentCollectionTest
{
    class Program
    {
        private static void Main(string[] args)
        {
            //QueueTest();
            BlockingCollectionTest();

            while (true)
            {
                Console.ReadKey();
            }
        }

        private static void QueueTest()
        {
            var queue = new ConcurrentQueue<int>();
            ThreadPool.QueueUserWorkItem(state =>
                    {
                        var i = 0;
                        while (true)
                        {
                            queue.Enqueue(i++);
                        }
                    });

            ThreadPool.QueueUserWorkItem(state =>
            {
                while (true)
                {
                    int i;
                    if (queue.TryDequeue(out i))
                    {
                        Console.WriteLine(i);
                    }
                }
            });
        }

        private static void BlockingCollectionTest()
        {
            var bc = new BlockingCollection<int>();
            ThreadPool.QueueUserWorkItem(state =>
            {
                var i = 0;
                while (true)
                {
                    if (bc.Add(i))
                    {
                        ++i;
                    }
                    else
                    {
                        Console.WriteLine("add failed");
                    }
                }
            });

            ThreadPool.QueueUserWorkItem(state =>
            {
                while (true)
                {
                    int i;
                    if (bc.TryTake(out i, 10))
                    {
                        Console.WriteLine(i);
                    }
                }
            });
        }
    }
}
