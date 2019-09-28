using System;

namespace Calculation
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Starting up Calculation App");
            var sender = new MessageSender();
            var handler = new MessageHandler(sender);
        }
    }
}
