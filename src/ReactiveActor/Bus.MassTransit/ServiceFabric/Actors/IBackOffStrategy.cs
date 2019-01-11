using System;

namespace Bus.MassTransit.ServiceFabric.Actors
{
    public interface IBackOffStrategy
    {
        TimeSpan GetDue(int attempt);
    }
}