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
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.Logging;

using Rock.Data;
using Rock.Enums.Core.Automation.Triggers;
using Rock.Logging;
using Rock.ViewModels.Core.Automation.Triggers;

namespace Rock.Core.Automation.Triggers
{
    /// <summary>
    /// A single criteria rule for the simple Entity Change trigger criteria.
    /// This handles the comparison of a single property on an entity that is
    /// being saved.
    /// </summary>
    internal class EntityChangeCriteriaRule
    {
        #region Fields

        /// <summary>
        /// The property for the property that will be accessed by this rule.
        /// </summary>
        private readonly PropertyInfo _propertyInfo;

        /// <summary>
        /// The name of the property that will be accessed by this rule.
        /// </summary>
        private readonly string _propertyName;

        /// <summary>
        /// The original value that will be checked for specific change types.
        /// </summary>
        private readonly object _originalValue;

        /// <summary>
        /// The updated value that will be checked for specific change types.
        /// </summary>
        private readonly object _updatedValue;

        /// <summary>
        /// The type of change that will be checked for this rule.
        /// </summary>
        private readonly EntityChangeSimpleChangeType _changeType;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityChangeCriteriaRule"/> class.
        /// </summary>
        /// <param name="type">The type of entity that will be matched by this rule.</param>
        /// <param name="rule">The definition of the rule that will be processed.</param>
        public EntityChangeCriteriaRule( Type type, EntityChangeSimpleCriteriaRuleBag rule )
        {
            _propertyInfo = type.GetProperty( rule.Property );
            _propertyName = rule.Property;
            _changeType = rule.ChangeType;
            _originalValue = ParseValue( type, _propertyName, rule.OriginalValue );
            _updatedValue = ParseValue( type, _propertyName, rule.UpdatedValue );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified entry matches this rule.
        /// </summary>
        /// <param name="entry">The save entry describing the entity being saved.</param>
        /// <returns><c>true</c> if the entry matches this rule; <c>false</c> if it did not match or the rule was not valid.</returns>
        public bool IsMatch( IEntitySaveEntry entry )
        {
            switch ( _changeType )
            {
                case EntityChangeSimpleChangeType.AnyChange:
                    {
                        return entry.ModifiedProperties.Contains( _propertyName );
                    }

                case EntityChangeSimpleChangeType.HasSpecificValue:
                    {
                        if ( _propertyInfo == null )
                        {
                            return false;
                        }

                        var currentValue = _propertyInfo.GetValue( entry.Entity );

                        if ( !entry.OriginalValues.TryGetValue( _propertyName, out var originalValue ) )
                        {
                            originalValue = null;
                        }

                        return DoesValueMatch( _updatedValue, currentValue )
                            || DoesValueMatch( _updatedValue, originalValue );
                    }

                case EntityChangeSimpleChangeType.ChangedFromValue:
                    {
                        if ( _propertyInfo == null )
                        {
                            return false;
                        }

                        if ( !entry.ModifiedProperties.Contains( _propertyName ) )
                        {
                            return false;
                        }

                        if ( !entry.OriginalValues.TryGetValue( _propertyName, out var originalValue ) )
                        {
                            originalValue = null;
                        }

                        return DoesValueMatch( _originalValue, originalValue );
                    }

                case EntityChangeSimpleChangeType.ChangedToValue:
                    {
                        if ( _propertyInfo == null )
                        {
                            return false;
                        }

                        if ( !entry.ModifiedProperties.Contains( _propertyName ) )
                        {
                            return false;
                        }

                        var currentValue = _propertyInfo.GetValue( entry.Entity );

                        return DoesValueMatch( _updatedValue, currentValue );
                    }

                case EntityChangeSimpleChangeType.ChangedFromValueToValue:
                    {
                        if ( _propertyInfo == null )
                        {
                            return false;
                        }

                        if ( !entry.ModifiedProperties.Contains( _propertyName ) )
                        {
                            return false;
                        }

                        var currentValue = _propertyInfo.GetValue( entry.Entity );

                        if ( !entry.OriginalValues.TryGetValue( _propertyName, out var originalValue ) )
                        {
                            return false;
                        }

                        return DoesValueMatch( _originalValue, originalValue )
                            && DoesValueMatch( _updatedValue, currentValue );
                    }

                default:
                    return false;
            }
        }

        /// <summary>
        /// Parses the value for the specified property on the type. This returns
        /// a native object that represents the value in a type that matches the
        /// property type. So an integer property will have the <paramref name="value"/>
        /// converted to an <c>int</c>.
        /// </summary>
        /// <param name="type">The object type that will be matched by the rule.</param>
        /// <param name="propertyName">The name of the property on the object.</param>
        /// <param name="value">The configured value to be parsed.</param>
        /// <returns>The value converted to the property type. This will be <c>null</c> if the value is an empty string or if the conversion is not possible.</returns>
        internal static object ParseValue( Type type, string propertyName, string value )
        {
            var propertyInfo = type.GetProperty( propertyName );

            if ( propertyInfo == null )
            {
                return null;
            }

            if ( value.IsNullOrWhiteSpace() )
            {
                return null;
            }

            try
            {
                var propertyType = Nullable.GetUnderlyingType( propertyInfo.PropertyType )
                    ?? propertyInfo.PropertyType;

                if ( propertyType.IsEnum )
                {
                    return Enum.Parse( propertyType, value );
                }
                else
                {
                    return Convert.ChangeType( value, propertyType );
                }
            }
            catch ( Exception ex )
            {
                var logger = RockLogger.LoggerFactory.CreateLogger<EntityChangeCriteriaRule>();

                logger.LogError( ex, "Error parsing criteria value {value}: {error}", value, ex.Message );

                return null;
            }
        }

        /// <summary>
        /// Determines whether the specified values match. This will check for
        /// the special case of <c>null</c> values and empty strings.
        /// </summary>
        /// <param name="compareValue">The value configured on the rule.</param>
        /// <param name="entityValue">The value from the entity.</param>
        /// <returns><c>true</c> if the values are considered equal; otherwise <c>false</c>.</returns>
        internal static bool DoesValueMatch( object compareValue, object entityValue )
        {
            if ( compareValue == null )
            {
                if ( entityValue is string stringValue )
                {
                    return stringValue == string.Empty;
                }
                else
                {
                    return entityValue == null;
                }
            }
            else
            {
                return compareValue.Equals( entityValue );
            }

        }

        #endregion
    }
}
