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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

using Rock.Attribute;
using Rock.Enums.Security;

namespace Rock.Security
{
    /// <summary>
    /// Performs validation of string values based on a set of rules. This is
    /// intended to be used to validate whole objects, but individual values
    /// can also be validated if the effective rules are known.
    /// </summary>
    [RockInternal( "17.8", keepInternalForever: true )]
    public static class StringValueValidator
    {
        #region Constants

        /// <summary>
        /// Holds the cached lookup of string properties to validate for each type.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, List<ValidationProperty>> StringPropertyLookup = new ConcurrentDictionary<Type, List<ValidationProperty>>();

        /// <summary>
        /// Provides the profile-to-rule mapping that defines the effective
        /// rules for each <see cref="StringValidationProfile"/>. This is the
        /// core of the profile system: changing the rules associated with a
        /// profile automatically changes the effective rules for every
        /// property that declares that profile.
        /// </summary>
        private static readonly IReadOnlyDictionary<StringValidationProfile, StringValidationRule> ProfileRules =
            new Dictionary<StringValidationProfile, StringValidationRule>
            {
                [StringValidationProfile.Unrestricted] = StringValidationRule.None,

                [StringValidationProfile.BasicHtml] =
                    StringValidationRule.LavaFormatting |
                    StringValidationRule.LavaCommands |
                    StringValidationRule.ScriptTags |
                    StringValidationRule.JavascriptProtocol |
                    StringValidationRule.EventHandlerAttributes,

                [StringValidationProfile.PlainText] =
                    StringValidationRule.LavaFormatting |
                    StringValidationRule.LavaCommands |
                    StringValidationRule.AnyHtmlTags |
                    StringValidationRule.ControlCharacters,

                [StringValidationProfile.Name] =
                    StringValidationRule.LavaFormatting |
                    StringValidationRule.LavaCommands |
                    StringValidationRule.AnyHtmlTags |
                    StringValidationRule.ControlCharacters |
                    StringValidationRule.BidiOverrides,

                // Identifier is intentionally just the allowlist. Every
                // other blocklist rule (HTML, Lava, control chars,
                // bidi overrides) is subsumed by the slug allowlist,
                // because none of those characters are URL-safe.
                [StringValidationProfile.Identifier] = StringValidationRule.NonUrlSlugCharacters,
            };

        private const RegexOptions DefaultOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant;

        private const RegexOptions IgnoreCase = DefaultOptions | RegexOptions.IgnoreCase;

        private static readonly Regex ScriptTagPattern = new Regex( @"<script\b", IgnoreCase );

        private static readonly Regex JavascriptProtocolPattern = new Regex( @"[=""'(]\s*javascript\s*:", IgnoreCase );

        // Build from the HTML Living Standard's enumerated event-handler
        // attribute names. Adjust as needed.
        private static readonly Regex EventHandlerPattern =
            new Regex(
                @"\bon(?:" +
                @"abort|auxclick|beforeinput|beforematch|beforetoggle|blur|" +
                @"cancel|canplay|canplaythrough|change|click|close|" +
                @"contextlost|contextmenu|contextrestored|copy|cuechange|cut|" +
                @"dblclick|drag|dragend|dragenter|dragleave|dragover|" +
                @"dragstart|drop|durationchange|emptied|ended|error|" +
                @"focus|formdata|input|invalid|keydown|keypress|keyup|" +
                @"load|loadeddata|loadedmetadata|loadstart|mousedown|" +
                @"mouseenter|mouseleave|mousemove|mouseout|mouseover|" +
                @"mouseup|paste|pause|play|playing|progress|ratechange|" +
                @"reset|resize|scroll|securitypolicyviolation|seeked|" +
                @"seeking|select|slotchange|stalled|submit|suspend|" +
                @"timeupdate|toggle|volumechange|waiting|wheel|" +
                @"afterprint|beforeprint|beforeunload|hashchange|" +
                @"languagechange|message|messageerror|offline|online|" +
                @"pagehide|pageshow|popstate|rejectionhandled|storage|" +
                @"unhandledrejection|unload" +
                @")\s*=",
                IgnoreCase );

        private static readonly Regex AnyHtmlTagPattern = new Regex( @"<[a-zA-Z!/]", DefaultOptions );

        private static readonly Regex ControlCharacterPattern = new Regex( "[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", DefaultOptions );

        private static readonly Regex BidiOverridePattern = new Regex( "[\u202A-\u202E\u2066-\u2069]", DefaultOptions );

        private static readonly Regex NonUrlSlugPattern = new Regex( "[^A-Za-z0-9_-]", DefaultOptions );

        #endregion

        #region Properties

        /// <summary>
        /// Used to temporarily disable string validation enforcement when it
        /// has been disabled in the system settings.
        /// </summary>
        public static bool DisableEnforcement { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Resolves a property to the effective rule bitmask that should be
        /// enforced when its value is saved. Honors the property's
        /// <see cref="StringValidationAttribute"/> if present, otherwise
        /// falls back to <see cref="StringValidationProfile.PlainText"/>.
        /// </summary>
        /// <param name="property">The property being saved.</param>
        /// <returns>The effective rule bitmask for the property.</returns>
        public static StringValidationRule GetEffectiveRules( PropertyInfo property )
        {
            var attr = property.GetCustomAttribute<StringValidationAttribute>();

            return attr == null
                ? ProfileRules[StringValidationProfile.PlainText]
                : GetEffectiveRules( attr );
        }

        /// <summary>
        /// Resolves an attribute's declared profile and per-property overrides
        /// to its effective rule bitmask.
        /// </summary>
        /// <param name="attribute">The attribute being evaluated.</param>
        /// <returns>The effective rule bitmask for the attribute.</returns>
        public static StringValidationRule GetEffectiveRules( StringValidationAttribute attribute )
        {
            var profileRules = ProfileRules[attribute.Profile];

            return ( profileRules & ~attribute.ExcludedRules ) | attribute.AdditionalRules;
        }

        /// <summary>
        /// Validates all string properties of the given source object. This
        /// will include only properties that are decorated with
        /// <see cref="DataMemberAttribute"/> and not decorated with
        /// <see cref="NotMappedAttribute"/>. Throws
        /// <see cref="PropertyValidationException"/> on the first rule
        /// that fails.
        /// </summary>
        /// <param name="source">The source object whose string properties should be validated.</param>
        public static void ValidateAllStrings( object source )
        {
            var sourceType = source.GetType();

            var properties = StringPropertyLookup.GetOrAdd( sourceType, GetValidationProperties );

            foreach ( var property in properties )
            {
                var value = property.Getter( source );

                if ( value != null )
                {
                    Validate( value, property.Rules, sourceType, property.Name );
                }
            }
        }

        /// <summary>
        /// Validates a single string value against the supplied rule bitmask.
        /// Throws <see cref="PropertyValidationException"/> on the first rule
        /// that fails.
        /// </summary>
        /// <param name="value">The string value being saved.</param>
        /// <param name="rules">The effective rule bitmask for the property.</param>
        /// <param name="type">The CLR type of the class that is being validated.</param>
        /// <param name="propertyName">The property name.</param>
        public static void Validate( string value, StringValidationRule rules, Type type, string propertyName )
        {
            // Do not check for whitespace as that would break identifier rule checking.
            if ( string.IsNullOrEmpty( value ) || rules == StringValidationRule.None )
            {
                return;
            }

            // TODO: Benchmark and see if bitwise operations are enough of a speed increase to be worth using.
            if ( rules.HasFlag( StringValidationRule.LavaFormatting ) && ContainsLavaFormatting( value ) )
            {
                Fail( type, propertyName, "may not contain Lava formatting" );
            }

            if ( rules.HasFlag( StringValidationRule.LavaCommands ) && ContainsLavaCommands( value ) )
            {
                Fail( type, propertyName, "may not contain Lava commands" );
            }

            if ( rules.HasFlag( StringValidationRule.ScriptTags ) && ScriptTagPattern.IsMatch( value ) )
            {
                Fail( type, propertyName, "may not contain script tags" );
            }

            if ( rules.HasFlag( StringValidationRule.JavascriptProtocol ) && JavascriptProtocolPattern.IsMatch( value ) )
            {
                Fail( type, propertyName, "may not contain JavaScript actions" );
            }

            if ( rules.HasFlag( StringValidationRule.EventHandlerAttributes ) && EventHandlerPattern.IsMatch( value ) )
            {
                Fail( type, propertyName, "may not contain JavaScript event handler attributes" );
            }

            if ( rules.HasFlag( StringValidationRule.AnyHtmlTags ) && AnyHtmlTagPattern.IsMatch( value ) )
            {
                Fail( type, propertyName, "may not contain HTML tags" );
            }

            if ( rules.HasFlag( StringValidationRule.ControlCharacters ) && ControlCharacterPattern.IsMatch( value ) )
            {
                Fail( type, propertyName, "may not contain control characters" );
            }

            if ( rules.HasFlag( StringValidationRule.BidiOverrides ) && BidiOverridePattern.IsMatch( value ) )
            {
                Fail( type, propertyName, "may not contain direction override characters" );

            }

            if ( rules.HasFlag( StringValidationRule.NonUrlSlugCharacters ) && NonUrlSlugPattern.IsMatch( value ) )
            {
                Fail( type, propertyName, "may not contain non-URL slug characters" );
            }
        }

        private static void Fail( Type type, string propertyName, string message )
        {
            var ex = new PropertyValidationException( type, propertyName, message );

            if ( DisableEnforcement )
            {
                Model.ExceptionLogService.LogException( ex );
                return;
            }

            throw ex;
        }

        /// <summary>
        /// Determines whether the specified value contains Lava formatting syntax.
        /// </summary>
        /// <param name="value">The string value to check.</param>
        /// <returns><c>true</c> if the value contains Lava formatting; otherwise, <c>false</c>.</returns>
        private static bool ContainsLavaFormatting( string value )
        {
            return value.IndexOf( "{{", StringComparison.Ordinal ) >= 0;
        }

        /// <summary>
        /// Determines whether the specified string contains Lava command delimiters.
        /// </summary>
        /// <param name="value">The string value to check.</param>
        /// <returns><c>true</c> if the value contains Lava commands; otherwise, <c>false</c>.</returns>
        private static bool ContainsLavaCommands( string value )
        {
            var length = value.Length - 1;

            for ( int i = 0; i < length; i++ )
            {
                if ( value[i] == '{' )
                {
                    var next = value[i + 1];

                    if ( next == '%' || next == '[' )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Get the properties of the specified type that should be validated,
        /// along with their effective rule bitmask.
        /// </summary>
        /// <param name="type">The type whose properties are being validated.</param>
        /// <returns>A list of validation properties for the specified type.</returns>
        private static List<ValidationProperty> GetValidationProperties( Type type )
        {
            var realType = type.IsDynamicProxyType() ? type.BaseType : type;

            // For now, skip plugins. They may be included in the future.
            if ( realType.Assembly != typeof( Data.RockContext ).Assembly )
            {
                return new List<ValidationProperty>();
            }

            return type.GetProperties()
                .Where( p => p.PropertyType == typeof( string )
                    && p.GetIndexParameters().Length == 0
                    && p.GetCustomAttribute<DataMemberAttribute>() != null
                    && p.GetCustomAttribute<NotMappedAttribute>() == null
                    && p.GetSetMethod() != null )
                .Select( p => new ValidationProperty( p ) )
                .ToList();
        }

        #endregion

        #region Support Classes

        private class ValidationProperty
        {
            public string Name { get; }

            public Func<object, string> Getter { get; }

            public StringValidationRule Rules { get; }

            public ValidationProperty( PropertyInfo property )
            {
                Name = property.Name;
                Rules = GetEffectiveRules( property );

                var parameter = Expression.Parameter( typeof( object ), "instance" );
                var castInstance = Expression.Convert( parameter, property.ReflectedType );
                var propertyMember = Expression.Property( castInstance, property );
                var lambda = Expression.Lambda<Func<object, string>>( propertyMember, parameter );

                Getter = lambda.Compile();
            }
        }

        #endregion
    }
}
