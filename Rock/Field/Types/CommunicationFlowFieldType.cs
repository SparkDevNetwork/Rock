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
using Rock.Enums.Communication;
using Rock.Model;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a <see cref="CommunicationFlow" />. Stored as the CommunicationFlow's Guid.
    /// </summary>
    [FieldTypeUsage( FieldTypeUsage.Administrative )]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.COMMUNICATION_FLOW )]
    public class CommunicationFlowFieldType : FieldType, IEntityFieldType, IEntityReferenceFieldType
    {
        /// <summary>
        /// The communication flows that can be selected.
        /// </summary>
        private const string VALUES = "values";
        private const string FILTER_TRIGGER_TYPES = "filterTriggerTypes";

        #region Edit Control

        /// <inheritdoc />
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc />
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            // The default implementation GetPublicEditValue calls GetTextValue which in this case has been overridden to return the
            // name of the Communication Flow, but what we actually need when editing is the saved Guid for the dropdown clientside,
            // so the private value (guid) is returned.
            return privateValue;
        }

        /// <inheritdoc />
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string value )
        {
            var publicConfigurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, value );

            using ( var rockContext = new RockContext() )
            {
                var filterTriggerTypes = publicConfigurationValues.ContainsKey( FILTER_TRIGGER_TYPES )
                    ? publicConfigurationValues[FILTER_TRIGGER_TYPES].SplitDelimitedValues().AsIntegerList()
                    : new List<int>();

                var noTriggerTypeFilter = !filterTriggerTypes.Any();
                
                var communicationFlows = new CommunicationFlowService( rockContext )
                    .Queryable()
                    .Where( cf =>
                        cf.IsActive
                        && ( noTriggerTypeFilter || filterTriggerTypes.Contains( ( int ) cf.TriggerType ) )
                    )
                    .Select( cf => new
                    {
                        cf.Guid,
                        cf.PublicName,
                        cf.Name
                    } )
                    .ToList()
                    .Select( a => new ListItemBag()
                    {
                        Value = a.Guid.ToString(),
                        Text = a.PublicName.IsNotNullOrWhiteSpace() ? a.PublicName : a.Name
                    } )
                    .OrderBy( t => t.Text )
                    .ToList();

                if ( communicationFlows.Any() )
                {
                    publicConfigurationValues[VALUES] = communicationFlows.ToCamelCaseJson( false, true );
                }
            }

            return publicConfigurationValues;
        }

        /// <inheritdoc />
        public override Dictionary<string, string> GetPrivateConfigurationValues( Dictionary<string, string> publicConfigurationValues )
        {
            var configuration = base.GetPrivateConfigurationValues( publicConfigurationValues );

            // Do not store the selectable communication flows.
            configuration.Remove( VALUES );

            return configuration;
        }

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var guid = privateValue.AsGuidOrNull();

            if ( guid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var communicationFlowInfo = new CommunicationFlowService( rockContext )
                        .Queryable()
                        .Where( cf => cf.Guid == guid.Value )
                        .Select( cf => new
                        {
                            cf.PublicName,
                            cf.Name
                        } )
                        .FirstOrDefault();

                    if ( communicationFlowInfo != null )
                    {
                        // Return the public name if set; otherwise, return the name.
                        if ( communicationFlowInfo.PublicName.IsNotNullOrWhiteSpace() )
                        {
                            return communicationFlowInfo.PublicName;
                        }
                        else
                        {
                            return communicationFlowInfo.Name;
                        }
                    }
                }
            }

            return string.Empty;
        }

        #endregion

        #region IEntityFieldType

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
            var guid = value.AsGuidOrNull();

            if ( guid.HasValue )
            {
                rockContext = rockContext ?? new RockContext();
                return new CommunicationFlowService( rockContext ).Get( guid.Value );
            }

            return null;
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
                var communicationFlowId = new CommunicationFlowService( rockContext ).GetId( guid.Value );

                if ( !communicationFlowId.HasValue )
                {
                    return null;
                }

                return new List<ReferencedEntity>()
                {
                    new ReferencedEntity( EntityTypeCache.GetId<CommunicationFlow>().Value, communicationFlowId.Value )
                };
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<CommunicationFlow>().Value, nameof( CommunicationFlow.Name ) )
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
            configKeys.Add( FILTER_TRIGGER_TYPES );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            // Filter by Trigger Types (e.g., Recurring, On-Demand, One-Time, etc.)
            var cblTriggerTypes = new RockCheckBoxList();
            cblTriggerTypes.Label = "Trigger Types";
            cblTriggerTypes.Help = "When set, communication flows will be filtered by these trigger types.";
            cblTriggerTypes.BindToEnum<CommunicationFlowTriggerType>();
            cblTriggerTypes.AutoPostBack = true;
            cblTriggerTypes.SelectedIndexChanged += OnQualifierUpdated;

            var controls = base.ConfigurationControls();
            controls.Add( cblTriggerTypes );

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            var configurationValues = new Dictionary<string, ConfigurationValue>
            {
                { FILTER_TRIGGER_TYPES, new ConfigurationValue( "Trigger Types", "When set, communication flows will be filtered by these trigger types.", string.Empty ) }
            };

            if ( controls != null )
            {
                var cblTriggerTypes = controls.Count > 0 ? controls[0] as RockCheckBoxList : null;

                if ( cblTriggerTypes != null )
                {
                    configurationValues[FILTER_TRIGGER_TYPES].Value = cblTriggerTypes.SelectedValues.AsDelimited( "," );
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
                var cblTriggerTypes = controls.Count > 0 ? controls[0] as RockCheckBoxList : null;

                if ( cblTriggerTypes != null )
                {
                    cblTriggerTypes.SetValues( configurationValues.GetValueOrNull( FILTER_TRIGGER_TYPES ).SplitDelimitedValues( "," ) );
                }
            }
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
            var editControl = new RockDropDownList { ID = id };
            editControl.Items.Add( new ListItem() );

            var filterTriggerTypes = new List<CommunicationFlowTriggerType>();
            var noTriggerTypeFilter = true;

            if ( configurationValues.ContainsKey( FILTER_TRIGGER_TYPES ) )
            {
                filterTriggerTypes = ( configurationValues[ FILTER_TRIGGER_TYPES ].Value ?? "" ).SplitDelimitedValues( "," ).AsIntegerList().Select( t => ( CommunicationFlowTriggerType ) t ).ToList();
                noTriggerTypeFilter = !filterTriggerTypes.Any();
            }

            var communicationFlows = new CommunicationFlowService( new RockContext() )
                .Queryable()
                .Where( v => v.IsActive && ( noTriggerTypeFilter || filterTriggerTypes.Contains( v.TriggerType ) ) )
                .Select( a => new
                {
                    a.Guid,
                    a.Name,
                    a.PublicName
                } )
                .ToList()
                .Select( f => new
                {
                    f.Guid,
                    Name = f.PublicName.IsNotNullOrWhiteSpace() ? f.PublicName : f.Name
                } )
                .OrderBy( t => t.Name )
                .ToList();

            if ( communicationFlows.Any() )
            {
                foreach ( var communicationFlow in communicationFlows )
                {
                    editControl.Items.Add( new ListItem( communicationFlow.Name, communicationFlow.Guid.ToString() ) );
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
            var editControl = control as ListControl;

            if ( editControl != null )
            {
                return editControl.SelectedValue;
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
            if ( control is ListControl editControl )
            {
                if ( configurationValues != null )
                {
                    var listItem = editControl.Items.FindByValue( value );

                    if ( listItem == null )
                    {
                        var valueGuid = value.AsGuid();

                        var communicationFlow = new CommunicationFlowService( new RockContext() )
                            .Queryable()
                            .Where( v => v.Guid == valueGuid )
                            .Select( f => new
                            {
                                f.Guid,
                                f.PublicName,
                                f.Name
                            } )
                            .FirstOrDefault();

                        if ( communicationFlow != null )
                        {
                            editControl.Items.Add( new ListItem( communicationFlow.PublicName.IsNotNullOrWhiteSpace() ? communicationFlow.PublicName : communicationFlow.Name, communicationFlow.Guid.ToString() ) );
                        }
                    }
                }

                editControl.SetValue( value );
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
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var guid = GetEditValue( control, configurationValues ).AsGuid();
            var itemId = new CommunicationFlowService( new RockContext() ).GetId( guid );
            return itemId;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            var itemGuid = new CommunicationFlowService( new RockContext() ).GetGuid( id ?? 0 );
            var guidValue = itemGuid.HasValue ? itemGuid.Value.ToString() : string.Empty;
            SetEditValue( control, configurationValues, guidValue );
        }

#endif
        #endregion
    }
}