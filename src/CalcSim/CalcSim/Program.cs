using System;

namespace CalcSim
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Simulator ...");
            var sim = new Simulator(args[0]);
        }
    }
}
