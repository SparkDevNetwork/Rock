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
using Rock.Reporting;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Stored as a delimited list of FinancialAccount Guids
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.FINANCIAL_ACCOUNTS )]
    public class AccountsFieldType : FieldType, IEntityReferenceFieldType
    {
        #region Configuration

        private const string DISPLAY_PUBLIC_NAME = "displaypublicname";
        private const string DISPLAY_CHILD_ITEM_COUNTS = "displaychilditemcounts";
        private const string DISPLAY_ACTIVE_ONLY = "displayactiveitemsonly";
        private const string ENHANCED_FOR_LONG_LISTS = "enhancedforlonglists";

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override bool IsPersistedValueInvalidated( Dictionary<string, string> oldPrivateConfigurationValues, Dictionary<string, string> newPrivateConfigurationValues )
        {
            var oldDisplayPublicName = oldPrivateConfigurationValues.GetValueOrNull( DISPLAY_PUBLIC_NAME ) ?? string.Empty;
            var newDisplayPublicName = newPrivateConfigurationValues.GetValueOrNull( DISPLAY_PUBLIC_NAME ) ?? string.Empty;

            return oldDisplayPublicName != newDisplayPublicName;
        }

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( !string.IsNullOrWhiteSpace( privateValue ) )
            {
                bool displayPublicName = true;

                if ( privateConfigurationValues != null &&
                     privateConfigurationValues.ContainsKey( DISPLAY_PUBLIC_NAME ) )
                {
                    displayPublicName = privateConfigurationValues[DISPLAY_PUBLIC_NAME].AsBoolean();
                }

                var guids = privateValue.SplitDelimitedValues();

                using ( var rockContext = new RockContext() )
                {
                    var accounts = new FinancialAccountService( rockContext ).Queryable().AsNoTracking().Where( a => guids.Contains( a.Guid.ToString() ) );
                    if ( accounts.Any() )
                    {
                        return string.Join( ", ", ( from account in accounts select displayPublicName && account.PublicName != null && account.PublicName != string.Empty ? account.PublicName : account.Name ).ToArray() );
                    }
                }
            }

            return string.Empty;
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
            var accountsValues = publicValue.FromJsonOrNull<List<ListItemBag>>();

            if ( accountsValues != null && accountsValues.Any() )
            {
                return string.Join( ",", accountsValues.Select( s => s.Value ) );
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( !string.IsNullOrWhiteSpace( privateValue ) )
            {
                var accountValues = new List<ListItemBag>();

                var displayPublicName = true;

                if ( privateConfigurationValues != null &&
                     privateConfigurationValues.ContainsKey( DISPLAY_PUBLIC_NAME ) )
                {
                    displayPublicName = privateConfigurationValues[DISPLAY_PUBLIC_NAME].AsBoolean();
                }

                foreach ( string guidValue in privateValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                {
                    Guid? guid = guidValue.AsGuidOrNull();
                    if ( guid.HasValue )
                    {
                        var account = FinancialAccountCache.Get( guid.Value );
                        if ( account != null )
                        {
                            var scheduleValue = new ListItemBag()
                            {
                                Text = displayPublicName ? account.PublicName : account.Name,
                                Value = account.Guid.ToString(),
                            };

                            accountValues.Add( scheduleValue );
                        }
                    }
                }

                if ( accountValues.Any() )
                {
                    return accountValues.ToCamelCaseJson( false, true );
                }
            }

            return string.Empty;
        }

        #endregion

        #region Filter Control

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

        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var guids = privateValue.SplitDelimitedValues();

            if ( guids.Length == 0 )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var accountIds = new FinancialAccountService( rockContext ).Queryable().AsNoTracking().Where( a => guids.Contains( a.Guid.ToString() ) ).Select( a => a.Id );
                if ( accountIds.Any() )
                {
                    var referencedEntities = new List<ReferencedEntity>();
                    foreach ( var accountId in accountIds )
                    {
                        referencedEntities.Add( new ReferencedEntity( EntityTypeCache.GetId<FinancialAccount>().Value, accountId ) );
                    }

                    return referencedEntities;
                }
            }

            return null;
        }

        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<FinancialAccount>().Value, nameof( FinancialAccount.Name ) ),
                new ReferencedProperty( EntityTypeCache.GetId<FinancialAccount>().Value, nameof( FinancialAccount.PublicName ) ),
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
            cbPublicName.Help = "When set, public name will be displayed.";

            // Add a check box for deciding to display the child items count on a parent node
            var cbDisplayChildItemCounts = new RockCheckBox();
            controls.Add( cbDisplayChildItemCounts );
            cbDisplayChildItemCounts.AutoPostBack = true;
            cbDisplayChildItemCounts.CheckedChanged += OnQualifierUpdated;
            cbDisplayChildItemCounts.Checked = false;
            cbDisplayChildItemCounts.Label = "Display Child Item Counts";
            cbDisplayChildItemCounts.Help = "When set, child item counts will be displayed.";

            // Add a check box for deciding if only active items are displayed
            var cbActiveOnly = new RockCheckBox();
            controls.Add( cbActiveOnly );
            cbActiveOnly.AutoPostBack = true;
            cbActiveOnly.CheckedChanged += OnQualifierUpdated;
            cbActiveOnly.Checked = false;
            cbActiveOnly.Label = "Display Active Items Only";
            cbActiveOnly.Help = "When set, only active item will be displayed.";

            // Add a check box for deciding to allow searching long lists via a REST call
            var cbEnhancedForLongLists = new RockCheckBox();
            controls.Add( cbEnhancedForLongLists );
            cbEnhancedForLongLists.AutoPostBack = true;
            cbEnhancedForLongLists.CheckedChanged += OnQualifierUpdated;
            cbEnhancedForLongLists.Checked = true;
            cbEnhancedForLongLists.Label = "Enhanced For Long Lists";
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
                AllowMultiSelect = true,
                DisplayPublicName = displayPublicName,
                DisplayChildItemCountLabel = displayChildItemCounts,
                DisplayActiveOnly = displayActiveOnly,
                EnhanceForLongLists = enhancedForLongLists
            };
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as AccountPicker;

            if ( picker != null )
            {
                var guids = new List<Guid>();
                var ids = picker.SelectedValuesAsInt();
                using ( var rockContext = new RockContext() )
                {
                    var accounts = new FinancialAccountService( rockContext ).Queryable().AsNoTracking().Where( a => ids.Contains( a.Id ) );

                    if ( accounts.Any() )
                    {
                        guids = accounts.Select( a => a.Guid ).ToList();
                    }
                }

                return string.Join( ",", guids );
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
            if ( value != null )
            {
                var picker = control as AccountPicker;
                var guids = new List<Guid>();

                if ( picker != null )
                {
                    var ids = value.Split( new[] { ',' } );

                    foreach ( var id in ids )
                    {
                        Guid guid;

                        if ( Guid.TryParse( id, out guid ) )
                        {
                            guids.Add( guid );
                        }
                    }

                    var accounts = new FinancialAccountService( new RockContext() ).Queryable().Where( a => guids.Contains( a.Guid ) );
                    picker.SetValues( accounts );
                }
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
            if ( control is AccountPicker )
            {
                var accountPicker = ( AccountPicker ) control;
                accountPicker.AllowMultiSelect = false;
                accountPicker.Required = required;
            }
            return control;
        }

#endif
        #endregion
    }
}
