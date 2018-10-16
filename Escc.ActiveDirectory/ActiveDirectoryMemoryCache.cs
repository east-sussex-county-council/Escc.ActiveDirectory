using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;

namespace Escc.ActiveDirectory
{
    /// <summary>
    /// Manages results of Active Directory queries in a memory cache shared between instances
    /// </summary>
    /// <seealso cref="Escc.ActiveDirectory.IActiveDirectoryCache" />
    public class ActiveDirectoryMemoryCache : IActiveDirectoryCache
    {
        private static readonly ObjectCache _cache = new MemoryCache("ActiveDirectoryMemoryCache");
        private readonly TimeSpan _cacheDuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveDirectoryMemoryCache"/> class with a 1 hour cache duration.
        /// </summary>
        public ActiveDirectoryMemoryCache()
        {
            _cacheDuration = TimeSpan.FromHours(1);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveDirectoryMemoryCache"/> class.
        /// </summary>
        /// <param name="cacheDuration">Duration of the cache.</param>
        public ActiveDirectoryMemoryCache(TimeSpan cacheDuration)
        {
            _cacheDuration = cacheDuration;
        }

        /// <summary>
        /// Checks for a set of results in the cache.
        /// </summary>
        /// <typeparam name="T">The expected type of the results</typeparam>
        /// <param name="cacheKey">The cache key.</param>
        /// <returns></returns>
        public T CheckForSavedValue<T>(string cacheKey) where T : class
        {
            return _cache.Get(cacheKey) as T;
        }

        /// <summary>
        /// Saves a set of results in the cache.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="results">The results.</param>
        public void SaveValue(string cacheKey, object results)
        {
            _cache.Add(cacheKey, results, DateTimeOffset.UtcNow.Add(_cacheDuration));
        }
    }
}
