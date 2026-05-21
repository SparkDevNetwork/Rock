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
namespace Rock.Net;

/// <summary>
/// Parses raw user-agent strings into a Rock-owned <see cref="UserAgentInfo"/>
/// result.
/// </summary>
public interface IUserAgentParser
{
    /// <summary>
    /// Parses the given user-agent string and returns the result. The
    /// implementation is required to return a non-null result (with empty
    /// fields) for null or whitespace input so callers can chain into
    /// fields without null-guarding.
    /// </summary>
    /// <param name="userAgent">The raw user-agent string.</param>
    /// <returns>The parsed user-agent details.</returns>
    UserAgentInfo Parse( string userAgent );
}
