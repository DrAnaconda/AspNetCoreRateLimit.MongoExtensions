using AspNetCoreRateLimit.MongoExtensions.MongoConfiguration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreRateLimit.MongoExtensions
{
    public class RateLimitConfigurationMongo : RateLimitConfiguration, IDisposable
    {
        private bool isDisposed = false;
        private readonly MongoClient mongoClient;

        private readonly string TargetDatabase;
        private readonly string TargetCollection;

        private readonly Task repeatableScanner;

        public RateLimitConfigurationMongo(
            MongoClient mongoClient, IHttpContextAccessor httpContextAccessor, 
            IOptions<IpRateLimitMongoConfiguration> ipOptions, IOptions<ClientRateLimitOptions> clientOptions) 
            : base(httpContextAccessor, ipOptions, clientOptions)
        {
            ValidateConstructor(ipOptions);

            this.TargetCollection = ipOptions.Value.TargetCollection;
            this.TargetDatabase = ipOptions.Value.TargetDatabase;
            this.mongoClient = mongoClient;
            LoadDataSync();
            if (ipOptions.Value.ReloadIntervalInSeconds > 0)
                repeatableScanner = LaunchRepeatableCollectionScanning(ipOptions.Value.ReloadIntervalInSeconds.Value);
        }

        private void ValidateConstructor(IOptions<IpRateLimitMongoConfiguration> ipOptions)
        {
            if (ipOptions.Value == null)
                throw new ArgumentNullException(nameof(ipOptions.Value));
            if (ipOptions.Value.TargetDatabase == null || ipOptions.Value.TargetDatabase.Length <= 0)
                throw new InvalidOperationException("`TargetDatabase` Paramether in `IpRateLimitMongoConfiguration` is required");
            if (ipOptions.Value.TargetDatabase == null || ipOptions.Value.TargetCollection.Length <= 0)
                throw new InvalidOperationException("`TargetCollection` Paramether in `IpRateLimitMongoConfiguration` is required");
        }

        private Task LaunchRepeatableCollectionScanning(int seconds)
        {
            return Task.Run(async () => 
            {
                await LoadDataAsync();
                await Task.Delay(TimeSpan.FromSeconds(seconds));
            }, 
            CancellationToken.None);
        }

        private async Task LoadDataAsync()
        {
            var filter = Builders<RateLimitRule>.Filter.Empty;
            var projection = Builders<RateLimitRule>.Projection
                .Exclude("_id").Include(x => x.Endpoint).Include(x => x.Limit).Include(x => x.Period).Include(x => x.PeriodTimespan);
            var collection = mongoClient.GetDatabase(TargetDatabase).GetCollection<RateLimitRule>(TargetCollection)
                .WithReadPreference(ReadPreference.SecondaryPreferred);
            var entireRules = await collection.Find(filter).Project<RateLimitRule>(projection).ToListAsync();
            base.IpRateLimitOptions.GeneralRules = entireRules;
        }

        private void LoadDataSync()
        {
            LoadDataAsync().Wait();
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }
        public virtual void Dispose(bool disposing)
        {
            if (!isDisposed && disposing)
            {
                repeatableScanner?.Dispose();
            }
            isDisposed = true;
        }
    }
}
