using System;

namespace ConsoleAppNet5.Configuration
{
    // 1) Make sure appsettings.json & relative environment files are copied to the application root folder during publishing (File propery > Content > Copy if newer/Copy always)
    // 2) Do not store config values directly in the root object, but instead use Options classes
    public class AppConfig
    {
        public DemoOptions DemoOptions { get; set; }
        public ExtraOptions ExtraOptions { get; set; }
    }

    public class DemoOptions
    {
        public bool Enabled { get; set; }
        public string ContentMessage { get; set; }
        public Uri BaseUrl { get; set; }
    }

    public class ExtraOptions
    {
        public string ConsoleId { get; set; }
    }
}
