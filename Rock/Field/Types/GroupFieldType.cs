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
#if WEBFORMS
using System.Web.UI;
#endif
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) Group
    /// Stored as Group.Guid
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.GROUP )]
    public class GroupFieldType : FieldType, IEntityFieldType, IEntityReferenceFieldType
    {
        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var guid = privateValue.AsGuidOrNull();

            if ( guid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var groupName = new GroupService( rockContext ).GetSelect( guid.Value, a => a.Name );

                    if ( groupName != null )
                    {
                        return groupName;
                    }
                }
            }

            return privateValue;
        }

        #endregion

        #region Edit Control

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            var groupValue = publicValue.FromJsonOrNull<ListItemBag>();

            if ( groupValue != null )
            {
                return groupValue.Value;
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( Guid.TryParse( privateValue, out Guid guid ) )
            {
                using ( var rockContext = new RockContext() )
                {
                    var group = new GroupService( rockContext ).GetNoTracking( guid );
                    if ( group != null )
                    {
                        return new ListItemBag()
                        {
                            Value = group.Guid.ToString(),
                            Text = group.Name,
                        }.ToCamelCaseJson( false, true );
                    }
                }
            }

            return string.Empty;
        }

        #endregion

        #region Entity Methods

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value )
        {
            return GetEntity( value, null );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value, RockContext rockContext )
        {
            Guid? guid = value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                rockContext = rockContext ?? new RockContext();
                return new GroupService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        #endregion

        #region Persistence

        /// <inheritdoc/>
        public override PersistedValues GetPersistedValues( string privateValue, Dictionary<string, string> privateConfigurationValues, IDictionary<string, object> cache )
        {
            if ( string.IsNullOrWhiteSpace( privateValue ) )
            {
                return new PersistedValues
                {
                    TextValue = string.Empty,
                    CondensedTextValue = string.Empty,
                    HtmlValue = string.Empty,
                    CondensedHtmlValue = string.Empty
                };
            }

            var textValue = GetTextValue( privateValue, privateConfigurationValues );
            var condensedTextValue = textValue.Truncate( CondensedTruncateLength );

            return new PersistedValues
            {
                TextValue = textValue,
                CondensedTextValue = condensedTextValue,
                HtmlValue = textValue,
                CondensedHtmlValue = condensedTextValue
            };
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var guid = privateValue.AsGuidOrNull();

            if ( !guid.HasValue )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var groupId = new GroupService( rockContext ).GetId( guid.Value );

                if ( !groupId.HasValue )
                {
                    return null;
                }

                return new List<ReferencedEntity>
                {
                    new ReferencedEntity( EntityTypeCache.GetId<Group>().Value, groupId.Value )
                };
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Name property of a Group and
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<Group>().Value, nameof( Group.Name ) )
            };
        }

        #endregion


        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            GroupPicker groupPicker = new GroupPicker { ID = id };
            return groupPicker;
        }

        /// <summary>
        /// Reads new values entered by the user for the field (as id)
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as GroupPicker;
            if ( picker != null )
            {
                int? itemId = picker.SelectedValue.AsIntegerOrNull();
                Guid? itemGuid = null;
                if ( itemId.HasValue && itemId > 0 )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        itemGuid = new GroupService( rockContext ).GetNoTracking( itemId.Value ).Guid;
                        return itemGuid?.ToString() ?? string.Empty;
                    }
                }

                return string.Empty;
            }

            return null;
        }

        /// <summary>
        /// Sets the value (as id)
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var picker = control as GroupPicker;
            if ( picker != null )
            {
                Guid? itemGuid = value.AsGuidOrNull();
                if ( itemGuid.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var group = new GroupService( rockContext ).Get( itemGuid.Value );
                        picker.SetValue( group );
                    }
                }
                else
                {
                    picker.SetValue( null );
                }
            }
        }

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Guid guid = GetEditValue( control, configurationValues ).AsGuid();
            var item = new GroupService( new RockContext() ).Get( guid );
            return item != null ? item.Id : ( int? ) null;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            var item = new GroupService( new RockContext() ).Get( id ?? 0 );
            string guidValue = item != null ? item.Guid.ToString() : string.Empty;
            SetEditValue( control, configurationValues, guidValue );
        }

#endif
        #endregion
    }
}