﻿// <copyright>
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
using Rock.Security.SecurityGrantRules;
using Rock.Security;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) Connection Request
    /// Stored as ConnectionRequest.Guid
    /// </summary>
    [FieldTypeUsage( FieldTypeUsage.System )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.CONNECTION_REQUEST )]
    public class ConnectionRequestFieldType : FieldType, IEntityFieldType, IEntityReferenceFieldType, ISecurityGrantFieldType
    {
        #region Formatting

        /// <inheritdoc />
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            ConnectionRequest connectionRequest = null;

            using ( var rockContext = new RockContext() )
            {
                Guid? guid = privateValue.AsGuidOrNull();
                if ( guid.HasValue )
                {
                    connectionRequest = new ConnectionRequestService( rockContext ).GetNoTracking( guid.Value );
                }

                if ( connectionRequest != null &&
                    connectionRequest.PersonAlias != null &&
                    connectionRequest.PersonAlias.Person != null )
                {
                    return connectionRequest.PersonAlias.Person.FullName;
                }
            }

            return string.Empty;
        }

        #endregion

        #region Edit Control

        /// <inheritdoc />
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc />
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var guid = privateValue.AsGuidOrNull();

            if ( guid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var connectionRequest = new ConnectionRequestService( rockContext ).GetSelect( guid.Value, cr => new
                    {
                        Person = cr.PersonAlias.Person,
                        Guid = cr.Guid.ToString()
                    } );

                    return new ListItemBag() { Text = connectionRequest.Person?.FullName, Value = connectionRequest.Guid }.ToCamelCaseJson( false, true );
                }
            }

            return base.GetPublicEditValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc />
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            var jsonValue = publicValue.FromJsonOrNull<ListItemBag>();

            if ( jsonValue != null )
            {
                return jsonValue.Value;
            }

            return base.GetPrivateEditValue( publicValue, privateConfigurationValues );
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
            rockContext = rockContext ?? new RockContext();
            Guid? guid = value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                return new ConnectionRequestService( rockContext ).Get( guid.Value );
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
            Guid? guid = privateValue.AsGuidOrNull();

            if ( !guid.HasValue )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var connectionRequest = new ConnectionRequestService( rockContext ).GetNoTracking( guid.Value );

                if ( connectionRequest == null )
                {
                    return null;
                }

                return new List<ReferencedEntity>()
                {
                    new ReferencedEntity( EntityTypeCache.GetId<ConnectionRequest>().Value, connectionRequest.Id ),
                    new ReferencedEntity( EntityTypeCache.GetId<PersonAlias>().Value, connectionRequest.PersonAliasId ),
                    new ReferencedEntity( EntityTypeCache.GetId<Person>().Value, connectionRequest.PersonAlias.PersonId )
                };
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<ConnectionRequest>().Value, nameof( ConnectionRequest.PersonAliasId ) ),
                new ReferencedProperty( EntityTypeCache.GetId<PersonAlias>().Value, nameof( PersonAlias.PersonId ) ),
                new ReferencedProperty( EntityTypeCache.GetId<Person>().Value, nameof( Person.NickName ) ),
                new ReferencedProperty( EntityTypeCache.GetId<Person>().Value, nameof( Person.LastName ) ),
            };
        }

        #endregion

        #region ISecurityGrantFieldType

        /// <inheritdoc/>
        public void AddRulesToSecurityGrant( SecurityGrant grant, Dictionary<string, string> privateConfigurationValues )
        {
            grant.AddRule( new ConnectionRequestPickerSecurityGrantRule() );
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
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            ConnectionRequestPicker connectionRequestPicker = new ConnectionRequestPicker { ID = id };
            return connectionRequestPicker;
        }

        /// <summary>
        /// Reads new values entered by the user for the field (as id)
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            ConnectionRequestPicker picker = control as ConnectionRequestPicker;
            if ( picker != null )
            {
                int? id = picker.ItemId.AsIntegerOrNull();

                if ( id.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var connectionRequestGuid = new ConnectionRequestService( rockContext ).GetGuid( id.Value );

                        if ( connectionRequestGuid.HasValue )
                        {
                            return connectionRequestGuid.ToString();
                        }
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
            var picker = control as ConnectionRequestPicker;

            if ( picker != null )
            {
                ConnectionRequest connectionRequest = null;

                Guid? guid = value.AsGuidOrNull();
                if ( guid.HasValue )
                {
                    connectionRequest = new ConnectionRequestService( new RockContext() ).Get( guid.Value );
                }

                picker.SetValue( connectionRequest );
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
            using ( var rockContext = new RockContext() )
            {
                return new ConnectionRequestService( rockContext ).GetId( guid );
            }
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            using ( var rockContext = new RockContext() )
            {
                var itemGuid = new ConnectionRequestService( rockContext ).GetGuid( id ?? 0 );
                string guidValue = itemGuid.HasValue ? itemGuid.ToString() : string.Empty;
                SetEditValue( control, configurationValues, guidValue );
            }
        }

#endif
        #endregion
    }
}