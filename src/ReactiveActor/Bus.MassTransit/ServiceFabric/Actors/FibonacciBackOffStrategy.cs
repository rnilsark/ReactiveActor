using System;

namespace Bus.MassTransit.ServiceFabric.Actors
{
    internal class FibonacciBackOffStrategy : IBackOffStrategy
    {
        public TimeSpan GetDue(int attempt)
        {
            return TimeSpan.FromSeconds(Fib(attempt));
        }

        private static int Fib(int n)
        {
            if (n <= 1)  
            {  
                return n;  
            }

            return Fib(n - 1) + Fib(n - 2);
        }
    }
}