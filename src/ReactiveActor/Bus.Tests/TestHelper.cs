using Microsoft.Extensions.Configuration;

namespace Bus.Tests
{
    public class TestHelper
    {
        public static IConfigurationRoot GetIConfigurationRoot()
        {            
            return new ConfigurationBuilder()
                .AddJsonFile("secrets.json")
                .Build();
        }
    }
}