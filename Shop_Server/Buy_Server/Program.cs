using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_2312
{
    class Program
    {
        static void Main(string[] args)
        {
            NetManager.GetInstance().Start();


            Console.ReadKey();
        }
    }
}
