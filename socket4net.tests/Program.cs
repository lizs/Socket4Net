using System;
using CustomLog;

namespace socket4net.tests
{
    class Program
    {
        static void Main(string[] args)
        {
            GlobalVarPool.Instance.Set(GlobalVarPool.NameOfLogger, new Log4Net());

            new ObjCase().Do();
            new PropertiedObjCase().Do();
            new ComponentedObjCase().Do();

            Console.ReadLine();
        }
    }
}
