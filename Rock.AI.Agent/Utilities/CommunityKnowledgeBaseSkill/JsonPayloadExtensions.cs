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
    /// Converts a payload into plain objects that can be serialized by the tool
    /// result.
    /// </summary>
    /// <remarks>
    /// <para>
    /// 8/19/26 - CLAUDE
    ///
    /// A tool result is serialized with System.Text.Json, and these payloads are
    /// Newtonsoft tokens. Handing one over directly does not work: System.Text.Json
    /// sees a JObject's own surface rather than its contents, so the result fills with
    /// Type, HasValues and Path, and JToken.Parent points back at its container, which
    /// is a reference cycle the serializer refuses outright.
    ///
    /// Reason: The two JSON libraries meet here, and this is where the payload has to
    /// cross from one to the other.
    /// </para>
    /// <para>
    /// Integers come back as <see cref="long"/> and fractional numbers as
    /// <see cref="double"/>, which is what the underlying token already holds. Dates
    /// are deliberately left as their original strings rather than parsed, because the
    /// point of returning the payload unmapped is that nothing here decides what a
    /// value means.
    /// </para>
    /// </remarks>
    /// <param name="token">The token to convert, which may be null.</param>
    /// <returns>A dictionary, a list, a primitive, or <c>null</c>.</returns>
    public static object ToPlainObject( this JToken token )
    {
        if ( token == null )
        {
            return null;
        }

        switch ( token.Type )
        {
            case JTokenType.Object:
                var members = new Dictionary<string, object>();

                foreach ( var property in ( ( JObject ) token ).Properties() )
                {
                    members[property.Name] = property.Value.ToPlainObject();
                }

                return members;

            case JTokenType.Array:
                return ( ( JArray ) token ).Select( item => item.ToPlainObject() ).ToList();

            case JTokenType.Null:
            case JTokenType.Undefined:
                return null;

            case JTokenType.Integer:
                return token.ToObject<long>();

            case JTokenType.Float:
                return token.ToObject<double>();

            case JTokenType.Boolean:
                return token.ToObject<bool>();

            default:
                return token.ToString();
        }
    }

    /// <summary>
    /// Converts a payload's items into plain objects, keeping at most
    /// <paramref name="maximumItems"/> of them.
    /// </summary>
    /// <remarks>
    /// The array is found as the payload itself, or under a <c>results</c> or
    /// <c>matches</c> member. The routes differ on which they return, and a caller
    /// should not have to care which one it asked.
    /// </remarks>
    /// <param name="data">The <c>data</c> member of a response.</param>
    /// <param name="maximumItems">The most items to keep.</param>
    /// <returns>The items, empty when the payload carries none.</returns>
    public static List<object> ToPlainItems( this JToken data, int maximumItems )
    {
        var items = data as JArray
            ?? data?["results"] as JArray
            ?? data?["matches"] as JArray;

        if ( items == null )
        {
            return new List<object>();
        }

        return items
            .Take( maximumItems )
            .Select( item => item.ToPlainObject() )
            .ToList();
    }

    /// <summary>
    /// Converts a response's <c>meta</c> member into tool result metadata.
    /// </summary>
    /// <remarks>
    /// Metadata rather than content, because these describe the response rather than
    /// answer the question: paging positions, totals, and the flags saying whether
    /// more exists. Keeping them out of the payload means the payload stays exactly
    /// what the service returned.
    /// </remarks>
    /// <param name="meta">The <c>meta</c> member of a response.</param>
    /// <returns>The metadata, empty when there is none.</returns>
    public static Dictionary<string, object> ToPlainMetadata( this JToken meta )
    {
        return meta.ToPlainObject() as Dictionary<string, object>
            ?? new Dictionary<string, object>();
    }

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
