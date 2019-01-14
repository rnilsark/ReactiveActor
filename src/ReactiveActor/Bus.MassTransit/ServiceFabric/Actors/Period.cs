using System;

namespace Bus.MassTransit.ServiceFabric.Actors
{
    public class Period
    {
        public static TimeSpan Never => TimeSpan.FromMilliseconds(-1);
    }
}