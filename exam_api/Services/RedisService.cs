using System.Security.Principal;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace exam_api.Services;

public class RedisService
{
    private readonly IDatabase db;
    private readonly ILogger logger;
    private TimeSpan default_expiration = TimeSpan.FromMinutes(5);

    public RedisService(ILogger<RedisService> logger)
    {
        Lazy<ConnectionMultiplexer> lazy_connection = 
            new (() => ConnectionMultiplexer.Connect("localhost"));
        ConnectionMultiplexer Connection = lazy_connection.Value;
        db = Connection.GetDatabase();
        
        this.logger = logger;
    }
    
    public async Task SetValueAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var settings = new JsonSerializerSettings() {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        await db.StringSetAsync(key, JsonConvert.SerializeObject(value, settings), expiration ?? default_expiration);
        logger.LogInformation($"Set data at key {key} with expiration date {expiration ?? default_expiration}");
    }

    public async Task<T> GetValueAsync<T>(string key)
    {
        var settings = new JsonSerializerSettings() {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        string json = await db.StringGetAsync(key);
        if (string.IsNullOrEmpty(json))
        {
            logger.LogInformation($"Getting data at key {key} - returned null");
            return default;
        }
        logger.LogInformation($"Getting data at key {key} - data found");
        return JsonConvert.DeserializeObject<T>(json, settings);
    }

    public async Task RemoveCacheAsync(string key)
    {
        await db.KeyDeleteAsync(key);
        logger.LogInformation($"CachedDataController.RemoveCacheAsync - Removed key {key}");
    }

    public async Task RemoveAllKeysAsync(string pattern)
    {
        RedisValue[] keys = await db.SetMembersAsync(pattern);
        if (keys.Any())
        {
            foreach (var key in keys)
                await RemoveCacheAsync(key);
        }
    }

    // public static async Task ScheduleFileDeletionAsync(string file_id, TimeSpan delay)
    // {
    //     string cache_key = $"deleted_files";
    //
    //     long deletion_time = DateTimeOffset.UtcNow.Add(delay).ToUnixTimeSeconds();
    //     await RedisConnectionHelper.db.SortedSetAddAsync(cache_key, file_id, deletion_time);
    //     LoggingController.LogInfo($"Scheduled file {file_id} for deletion at {deletion_time}");
    // }
}
