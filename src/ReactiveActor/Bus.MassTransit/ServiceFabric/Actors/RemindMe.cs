using System;

namespace Bus.MassTransit.ServiceFabric.Actors
{
    public class RemindMe
    {
        public static TimeSpan Never = TimeSpan.FromMilliseconds(-1);
    }
}