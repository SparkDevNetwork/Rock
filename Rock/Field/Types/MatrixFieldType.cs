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
using System.Web.UI.WebControls;
#endif
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    ///  Matrix Field Type
    ///  Value stored as AttributeMatrix.Guid
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.MATRIX )]
    public class MatrixFieldType : FieldType, IEntityFieldType
    {
        #region Configuration

        /// <summary>
        /// The attribute matrix template (stored as AttributeMatrixTemplate.Id)
        /// </summary>
        public const string ATTRIBUTE_MATRIX_TEMPLATE = "attributematrixtemplate";

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            using ( var rockContext = new RockContext() )
            {
                var attributeMatrixService = new AttributeMatrixService( rockContext );
                AttributeMatrix attributeMatrix = null;
                Guid? attributeMatrixGuid = privateValue.AsGuidOrNull();
                if ( attributeMatrixGuid.HasValue )
                {
                    attributeMatrix = attributeMatrixService.GetNoTracking( attributeMatrixGuid.Value );
                }

                if ( attributeMatrix != null )
                {
                    if ( privateConfigurationValues.ContainsKey( ATTRIBUTE_MATRIX_TEMPLATE ) )
                    {
                        // set the AttributeMatrixTemplateId just in case it was changed since the last time the attributeMatrix was saved
                        int attributeMatrixTemplateId = privateConfigurationValues[ATTRIBUTE_MATRIX_TEMPLATE].AsInteger();
                        if ( attributeMatrix.AttributeMatrixTemplateId != attributeMatrixTemplateId )
                        {
                            attributeMatrix.AttributeMatrixTemplateId = attributeMatrixTemplateId;
                            attributeMatrix.AttributeMatrixTemplate = new AttributeMatrixTemplateService( rockContext ).GetNoTracking( attributeMatrix.AttributeMatrixTemplateId );

                            // If the AttributeMatrixTemplateId changed, all the values in the AttributeMatrixItems 
                            // are referring to attributes from the old template, so wipe them out. All of them.
                            attributeMatrix.AttributeMatrixItems.Clear();
                        }
                    }

                    // make a temp attributeMatrixItem to see what Attributes they have
                    AttributeMatrixItem tempAttributeMatrixItem = new AttributeMatrixItem();
                    tempAttributeMatrixItem.AttributeMatrix = attributeMatrix;
                    tempAttributeMatrixItem.LoadAttributes();

                    var lavaTemplate = attributeMatrix.AttributeMatrixTemplate.FormattedLava;
                    Dictionary<string, object> mergeFields = Lava.LavaHelper.GetCommonMergeFields( null, null, new Lava.CommonMergeFieldsOptions() );
                    mergeFields.Add( "AttributeMatrix", attributeMatrix );
                    mergeFields.Add( "ItemAttributes", tempAttributeMatrixItem.Attributes.Select( a => a.Value ).OrderBy( a => a.Order ).ThenBy( a => a.Name ) );
                    mergeFields.Add( "AttributeMatrixItems", attributeMatrix.AttributeMatrixItems.OrderBy( a => a.Order ) );
                    return lavaTemplate.ResolveMergeFields( mergeFields );
                }
            }

            return string.Empty;
        }

        #endregion

        /// <summary>
        /// Gets a value indicating whether this field has a control to configure the default value
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has default control; otherwise, <c>false</c>.
        /// </value>
        public override bool HasDefaultControl => false;

        /// <summary>
        /// Gets the copy value.
        /// </summary>
        /// <param name="originalValue">The original value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>string.Empty, which means we don't actually want to copy the linked matrix.</returns>
        public override string GetCopyValue( string originalValue, RockContext rockContext )
        {
            // Don't copy
            return string.Empty;
        }

        #region Filter Control

        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return false;
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
                return new AttributeMatrixService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        #endregion

        #region Persistence

        /// <inheritdoc/>
        public override bool IsPersistedValueSupported( Dictionary<string, string> privateConfigurationValues )
        {
            // Matrix fields are far too complex for persistence logic.
            return false;
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
            configKeys.Add( ATTRIBUTE_MATRIX_TEMPLATE );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // build a drop down list of attribute matrix templates
            var ddlMatrixTemplate = new RockDropDownList();
            ddlMatrixTemplate.Required = true;
            controls.Add( ddlMatrixTemplate );
            ddlMatrixTemplate.Label = "Attribute Matrix Template";
            ddlMatrixTemplate.Help = "The Attribute Matrix Template that defines this matrix attribute";

            var list = new AttributeMatrixTemplateService( new RockContext() ).Queryable().OrderBy( a => a.Name ).Select( a => new
            {
                a.Id,
                a.Name
            } ).ToList();

            ddlMatrixTemplate.Items.Clear();
            ddlMatrixTemplate.Items.Add( new ListItem() );

            foreach ( var item in list )
            {
                ddlMatrixTemplate.Items.Add( new ListItem( item.Name, item.Id.ToString() ) );
            }

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( ATTRIBUTE_MATRIX_TEMPLATE, new ConfigurationValue( "Attribute Matrix Type", "The Attribute Matrix Template that defines this matrix attribute", string.Empty ) );

            if ( controls != null )
            {
                if ( controls.Count > 0 )
                {
                    var ddlMatrixTemplate = controls[0] as RockDropDownList;
                    configurationValues[ATTRIBUTE_MATRIX_TEMPLATE].Value = ddlMatrixTemplate?.SelectedValue;
                }
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
            if ( controls != null && configurationValues != null )
            {
                if ( controls.Count > 0 )
                {
                    var ddlMatrixTemplate = controls[0] as RockDropDownList;
                    if ( ddlMatrixTemplate != null && configurationValues.ContainsKey( ATTRIBUTE_MATRIX_TEMPLATE ) )
                    {
                        ddlMatrixTemplate.SetValue( configurationValues[ATTRIBUTE_MATRIX_TEMPLATE]?.Value );
                    }
                }
            }
        }

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
            // Never return a condensed value.
            return GetHtmlValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The id.</param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            if ( !configurationValues.ContainsKey( ATTRIBUTE_MATRIX_TEMPLATE ) )
            {
                return null;
            }

            AttributeMatrixEditor attributeMatrixEditor = new AttributeMatrixEditor { ID = id };
            attributeMatrixEditor.AttributeMatrixTemplateId = configurationValues[ATTRIBUTE_MATRIX_TEMPLATE].Value.AsInteger();

            return attributeMatrixEditor;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            AttributeMatrixEditor attributeMatrixEditor = control as AttributeMatrixEditor;

            if ( attributeMatrixEditor != null && configurationValues.ContainsKey( ATTRIBUTE_MATRIX_TEMPLATE ) )
            {
                int? attributeMatrixTemplateId = configurationValues[ATTRIBUTE_MATRIX_TEMPLATE]?.Value.AsIntegerOrNull();
                if ( attributeMatrixTemplateId.HasValue )
                {
                    if ( attributeMatrixEditor.AttributeMatrixGuid.HasValue )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            var attributeMatrix = new AttributeMatrixService( rockContext ).GetNoTracking( attributeMatrixEditor.AttributeMatrixGuid.Value );
                            return attributeMatrix.Guid.ToString();
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            AttributeMatrixEditor attributeMatrixEditor = control as AttributeMatrixEditor;
            if ( attributeMatrixEditor != null )
            {
                var rockContext = new RockContext();
                AttributeMatrixTemplate attributeMatrixTemplate = null;
                if ( attributeMatrixEditor.AttributeMatrixTemplateId.HasValue )
                {
                    attributeMatrixTemplate = new AttributeMatrixTemplateService( rockContext ).Get( attributeMatrixEditor.AttributeMatrixTemplateId.Value );
                }

                if ( attributeMatrixTemplate != null )
                {
                    var attributeMatrixService = new AttributeMatrixService( rockContext );
                    AttributeMatrix attributeMatrix = null;
                    Guid? attributeMatrixGuid = value.AsGuidOrNull();
                    if ( attributeMatrixGuid.HasValue )
                    {
                        attributeMatrix = attributeMatrixService.Get( attributeMatrixGuid.Value );
                    }

                    if ( attributeMatrix == null )
                    {
                        // Create the AttributeMatrix now and save it even though they haven't hit save yet. We'll need the AttributeMatrix record to exist so that we can add AttributeMatrixItems to it
                        // If this ends up creating an orphan, we can clean up it up later
                        attributeMatrix = new AttributeMatrix { Guid = Guid.NewGuid() };
                        attributeMatrix.AttributeMatrixTemplateId = attributeMatrixEditor.AttributeMatrixTemplateId.Value;
                        attributeMatrix.AttributeMatrixItems = new List<AttributeMatrixItem>();
                        attributeMatrixService.Add( attributeMatrix );
                        rockContext.SaveChanges();
                    }

                    // If the AttributeMatrixTemplateId jwas changed since the last time the attributeMatrix was saved, change it and wipe out the items
                    if ( attributeMatrix.AttributeMatrixTemplateId != attributeMatrixEditor.AttributeMatrixTemplateId.Value )
                    {
                        attributeMatrix.AttributeMatrixTemplateId = attributeMatrixEditor.AttributeMatrixTemplateId.Value;

                        var attributeMatrixItemService = new AttributeMatrixItemService( rockContext );

                        // If the AttributeMatrixTemplateId changed, all the values in the AttributeMatrixItems 
                        // are referring to attributes from the old template, so wipe them out. All of them.
                        foreach ( var attributeMatrixItem in attributeMatrix.AttributeMatrixItems.ToList() )
                        {
                            attributeMatrixItemService.Delete( attributeMatrixItem );
                        }

                        attributeMatrix.AttributeMatrixItems.Clear();
                        rockContext.SaveChanges();
                    }

                    attributeMatrixEditor.AttributeMatrixGuid = attributeMatrix.Guid;
                }
            }
        }

        /// <summary>
        /// Creates the control needed to filter (query) values using this field type.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override System.Web.UI.Control FilterControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, Rock.Reporting.FilterMode filterMode )
        {
            // This field type does not support filtering
            return null;
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
            var item = new AttributeMatrixService( new RockContext() ).Get( guid );
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
            var item = new AttributeMatrixService( new RockContext() ).Get( id ?? 0 );
            string guidValue = item != null ? item.Guid.ToString() : string.Empty;
            SetEditValue( control, configurationValues, guidValue );
        }

#endif
        #endregion
    }
}