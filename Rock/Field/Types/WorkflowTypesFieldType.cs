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
#endif
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a workflow type picker with option to select multiple. Stored as a comma-delimited list of WorkflowType Guids
    /// </summary>
    [Serializable]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.WORKFLOW_TYPES )]
    public class WorkflowTypesFieldType : FieldType, IEntityReferenceFieldType
    {
        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            string formattedValue = string.Empty;

            if ( !string.IsNullOrWhiteSpace( privateValue ) )
            {
                var guids = new List<Guid>();

                foreach ( string guidValue in privateValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                {
                    Guid? guid = guidValue.AsGuidOrNull();
                    if ( guid.HasValue )
                    {
                        guids.Add( guid.Value );
                    }
                }

                if ( guids.Any() )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var workflowTypes = new WorkflowTypeService( rockContext )
                            .Queryable()
                            .AsNoTracking()
                            .Where( a => guids.Contains( a.Guid ) );
                        if ( workflowTypes.Any() )
                        {
                            formattedValue = string.Join( ", ", ( from workflowType in workflowTypes select workflowType.Name ) );
                        }
                    }
                }
            }

            return formattedValue;
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
            if ( string.IsNullOrWhiteSpace( publicValue ) )
            {
                return string.Empty;
            }

            var jsonValues = publicValue.FromJsonOrNull<List<ListItemBag>>();

            if ( jsonValues != null )
            {
                return jsonValues.ConvertAll( c => c.Value ).AsDelimited( "," );
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( !string.IsNullOrWhiteSpace( privateValue ) )
            {
                var guids = privateValue.SplitDelimitedValues().AsGuidList();

                if ( guids.Count > 0 )
                {
                    var workflowTypes = guids.ConvertAll( c => WorkflowTypeCache.Get( c ) ).ToListItemBagList();
                    return workflowTypes.ToCamelCaseJson( false, true );
                }
            }

            return string.Empty;
        }

        #endregion

        #region FilterControl

        /// <summary>
        /// Gets the type of the filter comparison.
        /// </summary>
        /// <value>
        /// The type of the filter comparison.
        /// </value>
        public override ComparisonType FilterComparisonType
        {
            get
            {
                return ComparisonHelper.ContainsFilterComparisonTypes;
            }
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( string.IsNullOrWhiteSpace( privateValue ) )
            {
                return null;
            }

            var guids = new List<Guid>();

            foreach ( string guidValue in privateValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
            {
                Guid? guid = guidValue.AsGuidOrNull();
                if ( guid.HasValue )
                {
                    guids.Add( guid.Value );
                }
            }

            if ( guids.Any() )
            {
                using ( var rockContext = new RockContext() )
                {
                    var workflowTypeIds = new WorkflowTypeService( rockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Where( w => guids.Contains( w.Guid ) )
                        .Select( w => w.Id )
                        .ToList();
                    if ( workflowTypeIds.Any() )
                    {
                        return workflowTypeIds.Select( w => new ReferencedEntity( EntityTypeCache.GetId<WorkflowType>().Value, w ) )
                        .ToList();
                    }
                }
            }

            return null;
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Name property of a WorkflowTypes and
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<WorkflowType>().Value, nameof( WorkflowType.Name ) )
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
            return new WorkflowTypePicker { ID = id, AllowMultiSelect = true };
        }

        /// <summary>
        /// Reads new values entered by the user for the field ( as Guid )
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as WorkflowTypePicker;
            string result = string.Empty;

            if ( picker != null )
            {
                var ids = picker.SelectedValuesAsInt().ToList();
                using ( var rockContext = new RockContext() )
                {
                    var items = new WorkflowTypeService( rockContext ).GetByIds( ids ).ToList();

                    if ( items.Any() )
                    {
                        result = items.Select( s => s.Guid.ToString() ).ToList().AsDelimited( "," );
                    }
                }

                return result;
            }

            return null;
        }

        /// <summary>
        /// Sets the value. ( as Guid )
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var picker = control as WorkflowTypePicker;

            if ( picker != null )
            {
                var guids = value?.SplitDelimitedValues().AsGuidList() ?? new List<Guid>();
                var workflowTypes = new List<WorkflowType>();

                if ( guids.Any() )
                {
                    var rockContext = new RockContext();
                    workflowTypes = new WorkflowTypeService( rockContext ).GetByGuids( guids ).AsNoTracking().ToList();
                }

                picker.SetValues( workflowTypes );
            }
        }

        /// <summary>
        /// Gets the filter value control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            var control = base.FilterValueControl( configurationValues, id, required, filterMode );
            WorkflowTypePicker workflowTypePicker = ( WorkflowTypePicker ) control;
            workflowTypePicker.Required = required;
            workflowTypePicker.AllowMultiSelect = false;
            return control;
        }

#endif
        #endregion
    }
}