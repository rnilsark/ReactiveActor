using System;

namespace Bus.MassTransit.ServiceFabric.Actors
{
    internal class FibonacciBackOffStrategy : IBackOffStrategy
    {
        public TimeSpan GetDue(int attempt)
        {
            return TimeSpan.FromMilliseconds(Fib(attempt) * 100);
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