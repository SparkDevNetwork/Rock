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
using System.Data.Entity;
using System.Linq;
#if WEBFORMS
using System.Web.UI;
using System.Web.UI.WebControls;

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
    /// Field Type used to display a dropdown list of reports
    /// Stored as Report.Guid
    /// </summary>
    [FieldTypeUsage( FieldTypeUsage.System )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.REPORT )]
    public class ReportFieldType : FieldType, IEntityFieldType, IEntityReferenceFieldType
    {
        #region Formatting

        private const string VALUES_PUBLIC_KEY = "values";

        /// <inheritdoc/>
        public override string GetTextValue( string value, Dictionary<string, string> privateConfigurationValues )
        {
            string formattedValue = string.Empty;

            Guid? guid = value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var report = new ReportService( rockContext ).GetNoTracking( guid.Value );
                    if ( report != null )
                    {
                        formattedValue = report.Name;
                    }
                }
            }
            return formattedValue;
        }

        #endregion

        #region Edit Control

        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return privateValue;
        }

        #endregion

        #region Configuration

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string privateValue )
        {
            var publicConfigurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, privateValue );

            if ( usage != ConfigurationValueUsage.View )
            {
                using ( var rockContext = new RockContext() )
                {
                    publicConfigurationValues[VALUES_PUBLIC_KEY] = new ReportService( rockContext )
                        .Queryable()
                        .AsNoTracking()
                        .OrderBy( r => r.Name )
                        .Select( r => new ListItemBag
                        {
                            Text = r.Name,
                            Value = r.Guid.ToString(),
                        } )
                        .ToList()
                        .ToCamelCaseJson( false, true );
                }
            }

            return publicConfigurationValues;
        }

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPrivateConfigurationValues( Dictionary<string, string> publicConfigurationValues )
        {
            var privateConfigurationValues = base.GetPrivateConfigurationValues( publicConfigurationValues );
            privateConfigurationValues.Remove( VALUES_PUBLIC_KEY );
            return privateConfigurationValues;
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
                return new ReportService( rockContext ).Get( guid.Value );
            }

            return null;
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
                var reportId = new ReportService( rockContext ).GetId( guid.Value );

                if ( !reportId.HasValue )
                {
                    return null;
                }

                return new List<ReferencedEntity>
                {
                    new ReferencedEntity( EntityTypeCache.GetId<Report>().Value, reportId.Value )
                };
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Name property of a Report and
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<Report>().Value, nameof( Report.Name ) )
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
            var editControl = new RockDropDownList { ID = id, EnhanceForLongLists = true };
            editControl.Items.Add( new ListItem() );

            var reports = new ReportService( new RockContext() ).Queryable().OrderBy( d => d.Name );
            if ( reports.Any() )
            {
                foreach ( var report in reports )
                {
                    editControl.Items.Add( new ListItem( report.Name, report.Guid.ToString() ) );
                }

                return editControl;
            }

            return null;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as DropDownList;
            if ( picker != null )
            {
                // picker has value as Report.Guid
                return picker.SelectedValue;
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var editControl = control as ListControl;
            if ( editControl != null )
            {
                editControl.SetValue( value );
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
            var item = new ReportService( new RockContext() ).Get( guid );
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
            var item = new ReportService( new RockContext() ).Get( id ?? 0 );
            string guidValue = item != null ? item.Guid.ToString() : string.Empty;
            SetEditValue( control, configurationValues, guidValue );
        }

#endif
        #endregion
    }
}
