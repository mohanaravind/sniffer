using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Sniffer
{
    class Program
    {
        [MTAThread]
        static void Main(string[] args)
        {
            //Declarations   
            ConsoleKey ck;
            Thread thSniffer;

            //Create the sniffer thread
            thSniffer = new Thread(new ThreadStart(Sniffer.GetSniffer().Start));
           
            //Start sniffing
            thSniffer.Start();
                                    
            Console.WriteLine("Press Enter key to quit anytime...");
            Console.WriteLine();

            //Read the console
            ck = Console.ReadKey().Key;

            //Shutdown the sniffer if the user opted to
            if (ck == ConsoleKey.Enter)
            {
                Sniffer.GetSniffer().ShutDown();
                thSniffer.Abort();
                
            }
            
        }






    }
    
}
