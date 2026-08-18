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

using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;

namespace Rock.AI.Agent.Utilities.CommunityKnowledgeBaseSkill;

/// <summary>
/// Reads named members out of a knowledge base payload without throwing when they
/// are absent.
/// </summary>
/// <remarks>
/// <para>
/// These exist because the response shapes are not under Rock's control and are known
/// to drift from their published documentation. A reader that throws on an unexpected
/// payload turns a missing optional field into a failed tool call, which is a much
/// worse outcome than a null.
/// </para>
/// <para>
/// They also avoid <see cref="JToken.Value{T}(object)"/>, whose parameterless
/// appearance is misleading: the instance method takes a key, so calling it on a
/// token that has already been indexed does not compile.
/// </para>
/// </remarks>
internal static class JsonPayloadExtensions
{
    /// <summary>
    /// Reads a named string, or <c>null</c> when it is absent.
    /// </summary>
    /// <param name="token">The object to read from, which may be null.</param>
    /// <param name="name">The member name.</param>
    /// <returns>The value, or <c>null</c>.</returns>
    public static string GetString( this JToken token, string name )
    {
        var value = token?[name];

        if ( value == null || value.Type == JTokenType.Null )
        {
            return null;
        }

        return value.ToString();
    }

    /// <summary>
    /// Reads a named integer.
    /// </summary>
    /// <param name="token">The object to read from, which may be null.</param>
    /// <param name="name">The member name.</param>
    /// <param name="defaultValue">The value to use when the member is absent or unreadable.</param>
    /// <returns>The value, or <paramref name="defaultValue"/>.</returns>
    public static int GetInt( this JToken token, string name, int defaultValue = 0 )
    {
        return token.GetValueOrDefault( name, defaultValue );
    }

    /// <summary>
    /// Reads a named integer, or <c>null</c> when it is absent.
    /// </summary>
    /// <param name="token">The object to read from, which may be null.</param>
    /// <param name="name">The member name.</param>
    /// <returns>The value, or <c>null</c>.</returns>
    public static int? GetNullableInt( this JToken token, string name )
    {
        var value = token?[name];

        if ( value == null || value.Type == JTokenType.Null )
        {
            return null;
        }

        try
        {
            return value.ToObject<int>();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Reads a named boolean.
    /// </summary>
    /// <param name="token">The object to read from, which may be null.</param>
    /// <param name="name">The member name.</param>
    /// <param name="defaultValue">The value to use when the member is absent or unreadable.</param>
    /// <returns>The value, or <paramref name="defaultValue"/>.</returns>
    public static bool GetBool( this JToken token, string name, bool defaultValue = false )
    {
        return token.GetValueOrDefault( name, defaultValue );
    }

    /// <summary>
    /// Reads a named number.
    /// </summary>
    /// <param name="token">The object to read from, which may be null.</param>
    /// <param name="name">The member name.</param>
    /// <param name="defaultValue">The value to use when the member is absent or unreadable.</param>
    /// <returns>The value, or <paramref name="defaultValue"/>.</returns>
    public static double GetDouble( this JToken token, string name, double defaultValue = 0 )
    {
        return token.GetValueOrDefault( name, defaultValue );
    }

    /// <summary>
    /// Reads a named date, or <c>null</c> when it is absent.
    /// </summary>
    /// <remarks>
    /// Nullable rather than defaulted on purpose. A publish date that is genuinely
    /// absent must not become a very old date, because the service treats an absent
    /// date as unaffected by recency rather than as stale.
    /// </remarks>
    /// <param name="token">The object to read from, which may be null.</param>
    /// <param name="name">The member name.</param>
    /// <returns>The value, or <c>null</c>.</returns>
    public static DateTime? GetDateTime( this JToken token, string name )
    {
        var value = token?[name];

        if ( value == null || value.Type == JTokenType.Null )
        {
            return null;
        }

        try
        {
            return value.ToObject<DateTime?>();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Reads a named array of strings.
    /// </summary>
    /// <param name="token">The object to read from, which may be null.</param>
    /// <param name="name">The member name.</param>
    /// <returns>The values, empty when the member is absent.</returns>
    public static List<string> GetStringList( this JToken token, string name )
    {
        if ( !( token?[name] is JArray array ) )
        {
            return new List<string>();
        }

        return array
            .Where( a => a.Type != JTokenType.Null )
            .Select( a => a.ToString() )
            .ToList();
    }

    /// <summary>
    /// Reads a named array, or <c>null</c> when the member is absent or is not an
    /// array.
    /// </summary>
    /// <param name="token">The object to read from, which may be null.</param>
    /// <param name="name">The member name.</param>
    /// <returns>The array, or <c>null</c>.</returns>
    public static JArray GetArray( this JToken token, string name )
    {
        return token?[name] as JArray;
    }

    /// <summary>
    /// Reads a named value, falling back when it is absent or cannot be converted.
    /// </summary>
    /// <typeparam name="T">The type to convert to.</typeparam>
    /// <param name="token">The object to read from, which may be null.</param>
    /// <param name="name">The member name.</param>
    /// <param name="defaultValue">The value to use when the member is absent or unreadable.</param>
    /// <returns>The converted value, or <paramref name="defaultValue"/>.</returns>
    private static T GetValueOrDefault<T>( this JToken token, string name, T defaultValue )
    {
        var value = token?[name];

        if ( value == null || value.Type == JTokenType.Null )
        {
            return defaultValue;
        }

        try
        {
            return value.ToObject<T>();
        }
        catch
        {
            return defaultValue;
        }
    }
}
