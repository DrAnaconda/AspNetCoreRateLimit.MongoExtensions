namespace AspNetCoreRateLimit.MongoExtensions.MongoConfiguration
{
    public class IpRateLimitMongoConfiguration : IpRateLimitOptions
    {
        public string TargetDatabase { get; set; }
        public string TargetCollection { get; set; }

        public int? ReloadIntervalInSeconds { get; set; }

        public IpRateLimitMongoConfiguration() { }
    }
}
