using System;

namespace OtodomTests.Support
{
    public class TestConfig
    {
        public string Email { get; }
        public string Password { get; }
        public string StartUrl { get; }

        public TestConfig()
        {
            Email = Environment.GetEnvironmentVariable("OTODOM_TEST_EMAIL") ?? string.Empty;
            Password = Environment.GetEnvironmentVariable("OTODOM_TEST_PASSWORD") ?? string.Empty;
            StartUrl = Environment.GetEnvironmentVariable("OTODOM_START_URL") ?? "https://www.otodom.pl/";
        }
    }
}