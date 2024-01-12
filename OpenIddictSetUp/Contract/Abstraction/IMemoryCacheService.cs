namespace OpenIddictSetUp.Contract.Abstraction
{
    public interface IMemoryCacheService
    {
        Task<string?> GetValueAsync(string key);
        Task<bool> SetStringValueAsync(string key, string value);
        Task<bool> SetValueAsync<T>(string key, T value);
        Task<T?> GetAsync<T>(string key) where T : class;
        Task<T?> GetOrAddAsync<T>(string key, Func<Task<T>> action) where T : class;
        T? GetOrAdd<T>(string key, Func<T> action) where T : class;
        Task Clear(string key);
        void ClearAll();
    }
}
