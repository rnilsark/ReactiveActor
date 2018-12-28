using System;
using Microsoft.Extensions.Configuration;

namespace Bus.Tests
{
    public class AzureServiceBusTestBase
    {
        protected static string ConnectionString
        {
            get
            {
                {
                    var connectionString = TestHelper.GetIConfigurationRoot().GetConnectionString("AzureServiceBus");

                    if (string.IsNullOrWhiteSpace(connectionString))
                    {
                        throw new Exception("Add ConnectionString as environment variable");
                    }

                    return connectionString;
                }
            }
        }
    }
}