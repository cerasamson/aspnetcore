// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.OutputCaching;

/// <summary>
/// Represent the current caching context for the request.
/// </summary>
public interface IOutputCachingContext
{
    /// <summary>
    /// Gets the cached entry age.
    /// </summary>
    TimeSpan? CachedEntryAge { get; }

    /// <summary>
    /// Gets the <see cref="HttpContext"/>.
    /// </summary>
    HttpContext HttpContext { get; }

    /// <summary>
    /// Gets the response time.
    /// </summary>
    DateTimeOffset? ResponseTime { get; }

    /// <summary>
    /// Gets the response date.
    /// </summary>
    DateTimeOffset? ResponseDate { get; }

    /// <summary>
    /// Gets the response expiration.
    /// </summary>
    DateTimeOffset? ResponseExpires { get; }

    /// <summary>
    /// Gets the response shared max age.
    /// </summary>
    TimeSpan? ResponseSharedMaxAge { get; }

    /// <summary>
    /// Gets the response max age.
    /// </summary>
    TimeSpan? ResponseMaxAge { get; }

    /// <summary>
    /// Gets the cached response headers.
    /// </summary>
    IHeaderDictionary CachedResponseHeaders { get; }

    /// <summary>
    /// Gets the <see cref="CachedVaryByRules"/> instance.
    /// </summary>
    CachedVaryByRules CachedVaryByRules { get; }

    /// <summary>
    /// Gets the tags of the cached response.
    /// </summary>
    HashSet<string> Tags { get; }

    /// <summary>
    /// Gets the logger.
    /// </summary>
    ILogger Logger { get; }

    /// <summary>
    /// Gets or sets the custom expiration timespan for the response
    /// </summary>
    public TimeSpan? ResponseExpirationTimeSpan { get; set; }

    /// <summary>
    /// Determines whether the output caching logic should be configured for the incoming HTTP request.
    /// </summary>
    bool EnableOutputCaching { get; set; }

    /// <summary>
    /// Determines whether the output caching logic should be attempted for the incoming HTTP request.
    /// </summary>
    bool AttemptOutputCaching { get; set; }

    /// <summary>
    /// Determines whether a cache lookup is allowed for the incoming HTTP request.
    /// </summary>
    bool AllowCacheLookup { get; set; }

    /// <summary>
    /// Determines whether storage of the response is allowed for the incoming HTTP request.
    /// </summary>
    bool AllowCacheStorage { get; set; }

    /// <summary>
    /// Determines whether the request should be locked.
    /// </summary>
    bool AllowLocking { get; set; }

    /// <summary>
    /// Determines whether the response received by the middleware can be cached for future requests.
    /// </summary>
    bool IsResponseCacheable { get; set; }

    /// <summary>
    /// Determines whether the response retrieved from the cache store is fresh and can be served.
    /// </summary>
    bool IsCacheEntryFresh { get; set; }
}
