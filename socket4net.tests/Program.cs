using System;

namespace socket4net.tests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var test = new ScheduleCase();
            test.Init();
            test.TestCoroutine();
            var msg = Console.ReadLine();
            test.Destroy();
        }
    }
}
