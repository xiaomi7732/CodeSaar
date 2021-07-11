using System;

namespace HelloOptions
{
    public class ApplicationOptions
    {
        public const string SectionName = "Application";
        public string Name { get; set; }
        public TimeSpan CacheLifetime { get; set; }
    }
}