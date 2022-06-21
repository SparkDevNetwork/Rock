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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Account Field Type.  Stored as FinancialAccount's Guid
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    public class AccountFieldType : FieldType, IEntityFieldType
    {
        #region Configuration

        private const string DISPLAY_PUBLIC_NAME = "displaypublicname";
        private const string DISPLAY_CHILD_ITEM_COUNTS = "displaychilditemcounts";
        private const string DISPLAY_ACTIVE_ONLY = "displayactiveitemsonly";
        private const string ENHANCED_FOR_LONG_LISTS = "enhancedforlonglists";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( DISPLAY_PUBLIC_NAME );
            configKeys.Add( DISPLAY_CHILD_ITEM_COUNTS );
            configKeys.Add( DISPLAY_ACTIVE_ONLY );
            configKeys.Add( ENHANCED_FOR_LONG_LISTS );

            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // Add a check box for deciding if the text box is used for storing a password
            var cbPublicName = new RockCheckBox();
            controls.Add( cbPublicName );
            cbPublicName.AutoPostBack = true;
            cbPublicName.CheckedChanged += OnQualifierUpdated;
            cbPublicName.Checked = true;
            cbPublicName.Label = "Display Public Name";
            cbPublicName.Text = "Yes";
            cbPublicName.Help = "When set, public name will be displayed.";

            // Add a check box for deciding to display the child items count on a parent node
            var cbDisplayChildItemCounts = new RockCheckBox();
            controls.Add( cbDisplayChildItemCounts );
            cbDisplayChildItemCounts.AutoPostBack = true;
            cbDisplayChildItemCounts.CheckedChanged += OnQualifierUpdated;
            cbDisplayChildItemCounts.Checked = false;
            cbDisplayChildItemCounts.Label = "Display Child Item Counts";
            cbDisplayChildItemCounts.Text = "Yes";
            cbDisplayChildItemCounts.Help = "When set, child item counts will be displayed.";

            // Add a check box for deciding if only active items are displayed
            var cbActiveOnly = new RockCheckBox();
            controls.Add( cbActiveOnly );
            cbActiveOnly.AutoPostBack = true;
            cbActiveOnly.CheckedChanged += OnQualifierUpdated;
            cbActiveOnly.Checked = false;
            cbActiveOnly.Label = "Display Active Items Only";
            cbActiveOnly.Text = "Yes";
            cbActiveOnly.Help = "When set, only active item will be displayed.";

            // Add a check box for deciding to allow searching long lists via a REST call
            var cbEnhancedForLongLists = new RockCheckBox();
            controls.Add( cbEnhancedForLongLists );
            cbEnhancedForLongLists.AutoPostBack = true;
            cbEnhancedForLongLists.CheckedChanged += OnQualifierUpdated;
            cbEnhancedForLongLists.Checked = true;
            cbEnhancedForLongLists.Label = "Enhanced For Long Lists";
            cbEnhancedForLongLists.Text = "Yes";
            cbEnhancedForLongLists.Help = "When set, allows a searching for items.";

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            var configurationValues = base.ConfigurationValues( controls );
            configurationValues.Add( DISPLAY_PUBLIC_NAME, new ConfigurationValue( "Display Public Name", "When set, public name will be displayed.", "True" ) );
            configurationValues.Add( DISPLAY_CHILD_ITEM_COUNTS, new ConfigurationValue( "Display Child Item Counts", "When set, child item counts will be displayed.", "False" ) );
            configurationValues.Add( DISPLAY_ACTIVE_ONLY, new ConfigurationValue( "Display Active Items Only", "When set, only active item will be displayed.", "False" ) );
            configurationValues.Add( ENHANCED_FOR_LONG_LISTS, new ConfigurationValue( "Enhanced For Long Lists", "When set, allows a searching for items.", "True" ) );

            if ( controls != null && controls.Count >= 4 )
            {

                // DISPLAY_PUBLIC_NAME
                if ( controls[0] != null && controls[0] is CheckBox cbDisplayPublicName )
                {
                    configurationValues[DISPLAY_PUBLIC_NAME].Value = cbDisplayPublicName.Checked.ToString();
                }

                // DISPLAY_CHILD_ITEM_COUNTS
                if ( controls?[1] is CheckBox cbDisplayChildItemCounts )
                {
                    configurationValues[DISPLAY_CHILD_ITEM_COUNTS].Value = cbDisplayChildItemCounts.Checked.ToString();
                }

                // DISPLAY_ACTIVE_ONLY
                if ( controls?[2] is CheckBox cbDisplayActiveOnly )
                {
                    configurationValues[DISPLAY_ACTIVE_ONLY].Value = cbDisplayActiveOnly.Checked.ToString();
                }

                // ENHANCED_FOR_LONG_LISTS
                if ( controls?[3] is CheckBox cbEnhancedForLongLists )
                {
                    configurationValues[ENHANCED_FOR_LONG_LISTS].Value = cbEnhancedForLongLists.Checked.ToString();
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
            if ( controls != null && controls.Count >= 4 && configurationValues != null )
            {
                // DISPLAY_PUBLIC_NAME
                if ( controls[0] is CheckBox cbDisplayPublicName && configurationValues.ContainsKey( DISPLAY_PUBLIC_NAME ) )
                {
                    cbDisplayPublicName.Checked = configurationValues[DISPLAY_PUBLIC_NAME].Value.AsBoolean();
                }

                // DISPLAY_CHILD_ITEM_COUNTS
                if ( controls?[1] is CheckBox cbDisplayChildItemCounts && configurationValues.ContainsKey( DISPLAY_CHILD_ITEM_COUNTS ) )
                {
                    cbDisplayChildItemCounts.Checked = configurationValues[DISPLAY_CHILD_ITEM_COUNTS].Value.AsBoolean();
                }

                // DISPLAY_ACTIVE_ONLY
                if ( controls?[2] is CheckBox cbDisplayActiveOnly && configurationValues.ContainsKey( DISPLAY_ACTIVE_ONLY ) )
                {
                    cbDisplayActiveOnly.Checked = configurationValues[DISPLAY_ACTIVE_ONLY].Value.AsBoolean();
                }

                // ENHANCED_FOR_LONG_LISTS
                if ( controls?[3] is CheckBox cbEnhancedForLongLists && configurationValues.ContainsKey( ENHANCED_FOR_LONG_LISTS ) )
                {
                    cbEnhancedForLongLists.Checked = configurationValues[ENHANCED_FOR_LONG_LISTS].Value.AsBoolean();
                }
            }
        }

        #endregion

        #region Formatting

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
            string formattedValue = string.Empty;

            Guid? guid = value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                bool displayPublicName = true;

                if ( configurationValues != null &&
                     configurationValues.ContainsKey( DISPLAY_PUBLIC_NAME ) )
                {
                    displayPublicName = configurationValues[DISPLAY_PUBLIC_NAME].Value.AsBoolean();
                }

                using ( var rockContext = new RockContext() )
                {
                    var service = new FinancialAccountService( rockContext );
                    var account = service.GetNoTracking( guid.Value );

                    if ( account != null )
                    {
                        formattedValue = displayPublicName ? account.PublicName : account.Name;

                    }
                }
            }

            return base.FormatValue( parentControl, formattedValue, null, condensed );
        }

        #endregion

        #region Edit Control 

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
            bool displayPublicName = true;
            bool displayChildItemCounts = false;
            bool displayActiveOnly = false;
            bool enhancedForLongLists = true;
            if ( configurationValues != null )
            {
                if ( configurationValues.ContainsKey( DISPLAY_PUBLIC_NAME ) )
                {
                    displayPublicName = configurationValues[DISPLAY_PUBLIC_NAME].Value.AsBoolean();
                }

                if ( configurationValues.ContainsKey( DISPLAY_CHILD_ITEM_COUNTS ) )
                {
                    displayChildItemCounts = configurationValues[DISPLAY_CHILD_ITEM_COUNTS].Value.AsBoolean();
                }

                if ( configurationValues.ContainsKey( DISPLAY_ACTIVE_ONLY ) )
                {
                    displayActiveOnly = configurationValues[DISPLAY_ACTIVE_ONLY].Value.AsBoolean();
                }

                if ( configurationValues.ContainsKey( ENHANCED_FOR_LONG_LISTS ) )
                {
                    enhancedForLongLists = configurationValues[ENHANCED_FOR_LONG_LISTS].Value.AsBoolean();
                }
            }
            return new AccountPicker
            {
                ID = id,
                DisplayPublicName = displayPublicName,
                DisplayChildItemCountLabel = displayChildItemCounts,
                DisplayActiveOnly = displayActiveOnly,
                EnhanceForLongLists = enhancedForLongLists
            };
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// returns Account.Guid
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as AccountPicker;

            if ( picker != null )
            {
                int? id = picker.ItemId.AsIntegerOrNull();
                if ( id.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var account = new FinancialAccountService( rockContext ).GetNoTracking( id.Value );

                        if ( account != null )
                        {
                            return account.Guid.ToString();
                        }
                    }
                }
                else
                {
                    return string.Empty;
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// value is an Account.Guid
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var picker = control as AccountPicker;

            if ( picker != null )
            {
                Guid guid = value.AsGuid();

                // get the item (or null) and set it
                var account = new FinancialAccountService( new RockContext() ).Get( guid );
                picker.SetValue( account );
            }
        }

        #endregion

        #region Entity Methods

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Guid guid = GetEditValue( control, configurationValues ).AsGuid();
            var item = new FinancialAccountService( new RockContext() ).Get( guid );
            return item != null ? item.Id : ( int? ) null;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            var item = new FinancialAccountService( new RockContext() ).Get( id ?? 0 );
            string guidValue = item != null ? item.Guid.ToString() : string.Empty;
            SetEditValue( control, configurationValues, guidValue );
        }

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
                return new FinancialAccountService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        #endregion

    }
}
