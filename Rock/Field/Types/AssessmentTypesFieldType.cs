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
    /// Field Type used to display Assessment type check boxes.
    /// Stored as Assessment type's Guid.
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian  )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.ASSESSMENT_TYPE )]
    public class AssessmentTypesFieldType : SelectFromListFieldType, IEntityReferenceFieldType
    {
        #region Configuration

        private const string INCLUDE_INACTIVE_KEY = "includeInactive";

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string value, Dictionary<string, string> privateConfigurationValues )
        {
            string formattedValue = string.Empty;

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                var guids = new List<Guid>();

                foreach ( string guidValue in value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                {
                    Guid? guid = guidValue.AsGuidOrNull();
                    if ( guid.HasValue )
                    {
                        guids.Add( guid.Value );
                    }
                }

                if ( guids.Any() )
                {
                    formattedValue =  guids.Select( a => AssessmentTypeCache.Get( a ) ).Select(a=>a.Title).JoinStrings( ", " );
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
            var assessmentTypeValues = publicValue.FromJsonOrNull<List<ListItemBag>>();

            if ( assessmentTypeValues != null && assessmentTypeValues.Any() )
            {
                return string.Join( ",", assessmentTypeValues.Select( s => s.Value ) );
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( !string.IsNullOrWhiteSpace( privateValue ) )
            {
                var assessmentTypeValues = new List<ListItemBag>();
                var guidList = privateValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries )
                    .Select( a => a.AsGuid() )
                    .Where( a => a != Guid.Empty )
                    .ToList();
                var assessmentTypes = AssessmentTypeCache.All().Where( a => guidList.Contains( a.Guid ) );
                assessmentTypeValues = assessmentTypes.ToListItemBagList();

                if ( assessmentTypeValues.Any() )
                {
                    return assessmentTypeValues.ToCamelCaseJson( false, true );
                }
            }

            return string.Empty;
        }


        #endregion

        #region Methods

        /// <summary>
        /// Gets the list source of Assessment types from the database
        /// </summary>
        /// <value>
        /// The list source.
        /// </value>
        internal override Dictionary<string, string> GetListSource( Dictionary<string, ConfigurationValue> configurationValues )
        {
            bool includeInactive = ( configurationValues != null && configurationValues.ContainsKey( INCLUDE_INACTIVE_KEY ) && configurationValues[INCLUDE_INACTIVE_KEY].Value.AsBoolean() );

            return new AssessmentTypeService( new RockContext() )
                .Queryable().AsNoTracking()
                .OrderBy( t => t.Title )
                .Where( t => t.IsActive || includeInactive )
                .Select( t => new
                {
                    t.Guid,
                    t.Title,
                } )
                .ToDictionary( t => t.Guid.ToString(), t => t.Title );
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( privateValue.IsNullOrWhiteSpace() )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var valueGuidList = privateValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList();

                var ids = new AssessmentTypeService( rockContext )
                    .Queryable()
                    .Where( at => valueGuidList.Contains( at.Guid ) )
                    .Select( at => at.Id )
                    .ToList();

                var assessmentTypeEntityTypeId = EntityTypeCache.GetId<AssessmentType>().Value;

                return ids
                    .Select( id => new ReferencedEntity( assessmentTypeEntityTypeId, id ) )
                    .ToList();
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Title property of a AssessmentType and
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<AssessmentType>().Value, nameof( AssessmentType.Title ) )
            };
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( INCLUDE_INACTIVE_KEY );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // Add CheckBox for deciding if the list should include inactive items
            var cb = new RockCheckBox();
            controls.Add( cb );
            cb.AutoPostBack = true;
            cb.CheckedChanged += OnQualifierUpdated;
            cb.Label = "Include Inactive";
            cb.Text = "Yes";
            cb.Help = "When set, inactive assessments will be included in the list.";

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = base.ConfigurationValues( controls );

            configurationValues.Add( INCLUDE_INACTIVE_KEY, new ConfigurationValue( "Assessment Type", "When set, inactive assessments will be included in the list.", string.Empty ) );

            if ( controls != null )
            {
                CheckBox cbIncludeInactive = controls.Count > 2 ? controls[2] as CheckBox : null;
                configurationValues[INCLUDE_INACTIVE_KEY].Value = cbIncludeInactive != null ? cbIncludeInactive.Checked.ToString() : null;
            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="configurationValues"></param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            base.SetConfigurationValues( controls, configurationValues );

            if ( controls != null && configurationValues != null )
            {
                CheckBox cbIncludeInactive = controls.Count > 2 ? controls[2] as CheckBox : null;

                if ( cbIncludeInactive != null )
                {
                    cbIncludeInactive.Checked = configurationValues.GetValueOrNull( INCLUDE_INACTIVE_KEY ).AsBooleanOrNull() ?? false;
                }
            }
        }

#endif
        #endregion
    }
}