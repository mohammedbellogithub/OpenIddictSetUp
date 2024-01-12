using EasyCaching.Core;
using OpenIddictSetUp.Contract.Abstraction;
using System.Text.Json;

namespace OpenIddictSetUp.Contract.Implementation
{
    public class MemoryCacheService : IMemoryCacheService
    {
        private readonly IEasyCachingProvider _memoryCache;
        private readonly TimeSpan _expireTime;

        public MemoryCacheService(IEasyCachingProvider memoryCache, TimeSpan expireTime)
        {
            _memoryCache = memoryCache;
            _expireTime = expireTime;
        }

        public async Task Clear(string key)
        {
            await _memoryCache.RemoveAsync(key);
        }

        public void ClearAll()
        {
            _memoryCache.Flush();
        }

        public async Task<T> GetAsync<T>(string key) where T : class
        {
            var result = await _memoryCache.GetAsync<T>(key);
            return result.Value;
        }

        public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> action) where T : class
        {
            var isExist = await _memoryCache.ExistsAsync(key);
            if (!isExist)
            {
                var result = JsonSerializer.SerializeToUtf8Bytes(await action());
                await SetValueAsync(key, result);
            }

            return _memoryCache.Get<T>(key).Value;
        }

        public async Task<string> GetValueAsync(string key)
        {
            var result = await _memoryCache.GetAsync<string?>(key);
            return result.Value;
        }

        public async Task<bool> SetStringValueAsync(string key, string value)
        {
            return await _memoryCache.TrySetAsync(key, value, _expireTime);
        }

        public async Task<bool> SetValueAsync<T>(string key, T value)
        {
            //var stringValue = JsonSerializer.Serialize(value);
            return await _memoryCache.TrySetAsync(key, value, _expireTime);
        }

        public T GetOrAdd<T>(string key, Func<T> action) where T : class
        {
            var result = _memoryCache.Get<string>(key);

            if (result.IsNull)
            {
                var data = JsonSerializer.SerializeToUtf8Bytes(action());
                _memoryCache.Set(key, data, _expireTime);
            }
            return JsonSerializer.Deserialize<T>(result.Value);
        }
    }
}
