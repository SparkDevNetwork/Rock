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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Data;
using Rock.SystemGuid;

namespace Rock.Security
{
    /// <summary>
    /// The base for any security grant rule to inherit from. This defines the
    /// protocol that all rules must implement.
    /// </summary>
    [JsonConverter( typeof( SecurityGrantRuleConverter ) )]
    public abstract class SecurityGrantRule
    {
        #region Fields

        /// <summary>
        /// The cache of rule types that were found via reflection.
        /// </summary>
        private static Dictionary<Guid, Type> _ruleTypeCache;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the unique rule identifier.
        /// </summary>
        /// <remarks>This is calculated automatically from the <see cref="SecurityGrantRuleGuidAttribute"/>.</remarks>
        /// <value>The unique rule identifier.</value>
        [JsonProperty( "_id" )]
        public Guid RuleId
        {
            get
            {
                if ( !_ruleId.HasValue )
                {
                    var rockGuid = GetType().GetCustomAttribute<SecurityGrantRuleGuidAttribute>();

                    _ruleId = rockGuid != null ? rockGuid.Guid : Guid.Empty;
                }

                return _ruleId.Value;
            }
        }
        private Guid? _ruleId;

        /// <summary>
        /// Gets or sets the action this rule is authorizing.
        /// </summary>
        /// <value>The action this rule is authorizing.</value>
        [JsonProperty( "_a" )]
        public string Action { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityGrantRule"/> class.
        /// </summary>
        /// <param name="action">The action this rule is authorizing.</param>
        protected SecurityGrantRule( string action )
        {
            Action = action;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the CLR type for unique rule identifier.
        /// </summary>
        /// <param name="ruleId">The unique rule identifier.</param>
        /// <returns>A <see cref="Type"/> that is associated with the <paramref name="ruleId"/>; or <c>null</c> if not found.</returns>
        /// <exception cref="System.InvalidOperationException">Missing ${nameof( RockGuidAttribute )} on rule type.</exception>
        /// <exception cref="System.InvalidOperationException">Invalid identifier on rule type.</exception>
        public static Type GetRuleTypeForIdentifier( Guid ruleId )
        {
            // If this is the first time we have been called then initialize
            // our cache for performance.
            if ( _ruleTypeCache == null )
            {
                var cache = new Dictionary<Guid, Type>();

                // Get all the types that implement SecurityGrantRule.
                var ruleTypes = Rock.Reflection
                    .FindTypes( typeof( SecurityGrantRule ) )
                    .Select( t => t.Value );

                foreach ( var ruleType in ruleTypes )
                {
                    try
                    {
                        var guidAttribute = ruleType.GetCustomAttribute<RockGuidAttribute>();

                        // Ensure the rule type was given a RockGuid attribute.
                        if ( guidAttribute == null )
                        {
                            throw new InvalidOperationException( $"Missing ${nameof( RockGuidAttribute )} on rule type." );
                        }

                        // Ensure the guid is valid.
                        if ( guidAttribute.Guid == Guid.Empty )
                        {
                            throw new InvalidOperationException( $"Invalid identifier on rule type." );
                        }

                        cache.Add( guidAttribute.Guid, ruleType );
                    }
                    catch ( Exception ex )
                    {
                        System.Diagnostics.Debug.WriteLine( $"Failed to add grant rule type ${ruleType.FullName}: {ex.Message}" );
                        Model.ExceptionLogService.LogException( ex );
                    }
                }

                // Make sure we only set the cached value after it has been
                // fully initialized, otherwise two threads might try to update
                // the contents of the list at the same time.
                _ruleTypeCache = cache;
            }

            return _ruleTypeCache.TryGetValue( ruleId, out var type ) ? type : null;
        }

        /// <summary>
        /// Determines whether the object should be granted access.
        /// </summary>
        /// <param name="obj">The object to be checked for permission.</param>
        /// <param name="action">The security action being checked, such as <see cref="Authorization.VIEW"/> or <see cref="Authorization.EDIT"/>.</param>
        /// <returns><c>true</c> if access is explicitely granted to the object; otherwise, <c>false</c>.</returns>
        public abstract bool IsAccessGranted( object obj, string action );

        #endregion

        #region JSON Converter

        /// <summary>
        /// Handles JSON conversion for any class that inherits from
        /// the <see cref="SecurityGrantRule"/> base class.
        /// </summary>
        /// <seealso cref="Newtonsoft.Json.JsonConverter" />
        internal class SecurityGrantRuleConverter : JsonConverter
        {
            /// <inheritdoc/>
            public override bool CanConvert( Type objectType )
            {
                return typeof( SecurityGrantRule ) == objectType;
            }

            /// <inheritdoc/>
            public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
            {
                // Shouldn't really happen, but check for a null object.
                if ( reader.TokenType == JsonToken.Null )
                {
                    return null;
                }

                // Also shouldn't happen, but make sure we are starting an object.
                if ( reader.TokenType != JsonToken.StartObject )
                {
                    throw new InvalidOperationException( "Expected start of object." );
                }

                // Parse the object as a JObject so we can peek into it.
                var jObj = JObject.Load( reader );

                // Peek at the rule identifier value and find the rule type.
                var guid = jObj["_id"].ToString().AsGuid();
                var type = GetRuleTypeForIdentifier( guid );

                // Make sure we know what kind of rule it is.
                if ( type == null )
                {
                    throw new InvalidOperationException( "Security grant rule not found." );
                }

                // Initialize the rule and fill in it's properties.
                var rule = Activator.CreateInstance( type, true );
                serializer.Populate( jObj.CreateReader(), rule );

                return rule;
            }

            /// <inheritdoc/>
            public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
            {
                throw new NotSupportedException();
            }

            /// <inheritdoc/>
            public override bool CanWrite => false;
        }

        #endregion
    }
}