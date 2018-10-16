using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Escc.ActiveDirectory
{
    /// <summary>
    /// Manages results of Active Directory queries in a cache 
    /// </summary>
    public interface IActiveDirectoryCache
    {
        /// <summary>
        /// Checks for a value saved in the cache.
        /// </summary>
        /// <typeparam name="T">The expected type of the value.</typeparam>
        /// <param name="cacheKey">The cache key.</param>
        /// <returns></returns>
        T CheckForSavedValue<T>(string cacheKey) where T : class;

        /// <summary>
        /// Saves a set of results in the cache.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="value">The value to save.</param>
        void SaveValue(string cacheKey, object value);
    }
}
