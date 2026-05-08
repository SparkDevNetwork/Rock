// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.Concurrent;

using UAParser;

namespace Rock.Net;

/// <summary>
/// Default implementation of <see cref="IUserAgentParser"/>. Holds a
/// process-wide cache keyed by the raw user-agent string. Registered as a
/// singleton in the DI container so the cache is shared across every
/// consumer in the process.
/// </summary>
internal sealed class UserAgentParser : IUserAgentParser
{
    #region Constants

    /// <summary>
    /// The maximum number of entries to hold in the cache before clearing it.
    /// This is a simple defense against a botnet or fuzzed UA header blowing
    /// up the working set.
    /// </summary>
    private const int CacheCap = 10_000;

    #endregion

    #region Fields

    /// <summary>
    /// The parser to use when a cache miss occurs.
    /// </summary>
    private readonly Parser _parser = Parser.GetDefault();

    /// <summary>
    /// The cached user agent information.
    /// </summary>
    private readonly ConcurrentDictionary<string, UserAgentInfo> _cache = new();

    #endregion

    #region Methods

    /// <inheritdoc/>
    public UserAgentInfo Parse( string userAgent )
    {
        // A botnet or fuzzed UA header could blow up the working set; bound
        // it by clearing the cache when it exceeds the cap. Cache misses
        // fall back to the parser, so a clear is correct, just slower.
        if ( _cache.Count > CacheCap )
        {
            _cache.Clear();
        }

        return _cache.GetOrAdd( userAgent, ua => new UserAgentInfo( ua, _parser.Parse( ua ) ) );
    }

    #endregion
}
