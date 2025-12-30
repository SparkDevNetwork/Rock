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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataSelect.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Select an Address of the Person" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Select Person's Address" )]
    [Rock.SystemGuid.EntityTypeGuid( "0A1731E9-CBFC-4CB9-920C-107C8D275583" )]
    public class AddressSelect : DataSelectComponent
    {
        #region Properties

        /// <summary>
        /// Gets the name of the entity type. Filter should be an empty string
        /// if it applies to all entities
        /// </summary>
        /// <value>
        /// The name of the entity type.
        /// </value>
        public override string AppliesToEntityType
        {
            get
            {
                return typeof( Rock.Model.Person ).FullName;
            }
        }

        /// <summary>
        /// Gets the section that this will appear in in the Field Selector
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get
            {
                return base.Section;
            }
        }

        /// <summary>
        /// The PropertyName of the property in the anonymous class returned by the SelectExpression
        /// </summary>
        /// <value>
        /// The name of the column property.
        /// </value>
        public override string ColumnPropertyName
        {
            get
            {
                return "Address";
            }
        }

        /// <summary>
        /// Gets the type of the column field.
        /// </summary>
        /// <value>
        /// The type of the column field.
        /// </value>
        public override Type ColumnFieldType
        {
            get { return typeof( string ); }
        }

        /// <summary>
        /// Gets the default column header text.
        /// </summary>
        /// <value>
        /// The default column header text.
        /// </value>
        public override string ColumnHeaderText
        {
            get
            {
                return "Address";
            }
        }

        #endregion

        #region Configuration

        /// <inheritdoc/>
        public override DynamicComponentDefinitionBag GetComponentDefinition( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var locationTypeList = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.GROUP_LOCATION_TYPE.AsGuid() )
                .DefinedValues
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Value )
                .Select( glt => new ListItemBag { Text = glt.Value, Value = glt.Guid.ToString() } )
                .ToList();


            var addressPartList = new List<ListItemBag>();
            var newLabels = new Dictionary<string, string>();
            var globalAttributesCache = GlobalAttributesCache.Get();
            var defaultCountry = ( !string.IsNullOrWhiteSpace( globalAttributesCache.OrganizationCountry ) ) ? globalAttributesCache.OrganizationCountry : "US";
            var countryValue = DefinedTypeCache.Get( new Guid( SystemGuid.DefinedType.LOCATION_COUNTRIES ) )
                    .DefinedValues
                    .Where( v => v.Value.Equals( defaultCountry, StringComparison.OrdinalIgnoreCase ) )
                    .FirstOrDefault();

            if ( countryValue != null )
            {
                newLabels.Add( "City", countryValue.GetAttributeValue( "CityLabel" ) );
                newLabels.Add( "State", countryValue.GetAttributeValue( "StateLabel" ) );
                newLabels.Add( "Postal Code", countryValue.GetAttributeValue( "PostalCodeLabel" ) );
            }

            foreach ( RockUdfHelper.AddressNamePart addressPart in Enum.GetValues( typeof( RockUdfHelper.AddressNamePart ) ) )
            {
                var Text = addressPart.ConvertToString();
                if ( newLabels.ContainsKey( Text ) )
                {
                    Text = newLabels[Text];
                }
                addressPartList.Add( new ListItemBag { Text = Text, Value = addressPart.ConvertToInt().ToString() } );
            }


            var options = new Dictionary<string, string>
            {
                ["locationTypeList"] = locationTypeList.ToCamelCaseJson( false, true ),
                ["addressPartList"] = addressPartList.ToCamelCaseJson( false, true ),
            };

            return new DynamicComponentDefinitionBag
            {
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Reporting/DataSelects/Person/addressSelect.obs" ),
                Options = options,
            };
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetObsidianComponentData( Type entityType, string selection, RockContext rockContext, RockRequestContext requestContext )
        {
            var locationType = string.Empty;
            var addressPart = string.Empty;
            var selections = selection.Split( '|' );

            if ( selections.Length >= 1 )
            {
                locationType = selections[0];
            }

            if ( selections.Length >= 2 )
            {
                addressPart = selections[1];
            }

            return new Dictionary<string, string>
            {
                ["locationType"] = locationType,
                ["addressPart"] = addressPart
            };
        }

        /// <inheritdoc/>
        public override string GetSelectionFromObsidianComponentData( Type entityType, Dictionary<string, string> data, RockContext rockContext, RockRequestContext requestContext )
        {
            var locationType = data.GetValueOrDefault( "locationType", string.Empty );
            var addressPart = data.GetValueOrDefault( "addressPart", string.Empty );

            return $"{locationType}|{addressPart}";
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The title.
        /// </value>
        public override string GetTitle( Type entityType )
        {
            return "Address";
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="entityIdProperty">The entity identifier property.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( RockContext context, MemberExpression entityIdProperty, string selection )
        {
            string[] values = selection.Split( '|' );
            Guid groupLocationTypeValueGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid();
            RockUdfHelper.AddressNamePart addressNamePart = RockUdfHelper.AddressNamePart.Full;

            if ( values.Length >= 1 )
            {
                groupLocationTypeValueGuid = values[0].AsGuidOrNull() ?? Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid();
            }

            if ( values.Length >= 2 )
            {
                addressNamePart = values[1].ConvertToEnumOrNull<RockUdfHelper.AddressNamePart>() ?? RockUdfHelper.AddressNamePart.Full;
            }

            string addressTypeId = DefinedValueCache.Get( groupLocationTypeValueGuid ).Id.ToString();
            string addressComponent = addressNamePart.ConvertToString( false );
            var personLocationQuery = new PersonService( context ).Queryable()
                .Select( p => RockUdfHelper.ufnCrm_GetAddress( p.Id, addressTypeId, addressComponent ) );

            return SelectExpressionExtractor.Extract( personLocationQuery, entityIdProperty, "p" );
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="parentControl"></param>
        /// <returns></returns>
        public override System.Web.UI.Control[] CreateChildControls( System.Web.UI.Control parentControl )
        {
            RockDropDownList locationTypeList = new RockDropDownList();
            locationTypeList.Items.Clear();
            foreach ( var value in DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.GROUP_LOCATION_TYPE.AsGuid() ).DefinedValues.OrderBy( a => a.Order ).ThenBy( a => a.Value ) )
            {
                locationTypeList.Items.Add( new ListItem( value.Value, value.Guid.ToString() ) );
            }

            locationTypeList.ID = parentControl.ID + "_grouplocationType";
            locationTypeList.Label = "Address Type";
            locationTypeList.SelectedIndex = 0;
            parentControl.Controls.Add( locationTypeList );

            RockRadioButtonList addressPartRadioButtonList = new RockRadioButtonList();
            addressPartRadioButtonList.Items.Clear();
            addressPartRadioButtonList.BindToEnum<RockUdfHelper.AddressNamePart>( false );

            // Localises the radio button list by modifying the text Value of radio buttons
            Dictionary<string, string> newLabels = new Dictionary<string, string>();
            var globalAttributesCache = GlobalAttributesCache.Get();
            var defaultCountry = ( !string.IsNullOrWhiteSpace( globalAttributesCache.OrganizationCountry ) ) ? globalAttributesCache.OrganizationCountry : "US";
            var countryValue = DefinedTypeCache.Get( new Guid( SystemGuid.DefinedType.LOCATION_COUNTRIES ) )
                    .DefinedValues
                    .Where( v => v.Value.Equals( defaultCountry, StringComparison.OrdinalIgnoreCase ) )
                    .FirstOrDefault();

            if ( countryValue != null )
            {
                if ( !newLabels.ContainsKey( "City" ) )
                {
                    newLabels.Add( "City", countryValue.GetAttributeValue( "CityLabel" ) );
                }

                if ( !newLabels.ContainsKey( "Region" ) )
                {
                    newLabels.Add( "Region", countryValue.GetAttributeValue( "StateLabel" ) );
                }

                if ( !newLabels.ContainsKey( "PostalCode" ) )
                {
                    newLabels.Add( "PostalCode", countryValue.GetAttributeValue( "PostalCodeLabel" ) );
                }
            }

            foreach ( KeyValuePair<string, string> pair in newLabels )
            {
                string oldValue = pair.Key.SplitCase();
                string newValue = pair.Value.SplitCase();
                var listItem = addressPartRadioButtonList.Items.FindByText( oldValue );
                if ( listItem != null )
                {
                    listItem.Text = newValue;
                }
            }

            // default to first one
            addressPartRadioButtonList.SelectedIndex = 0;
            addressPartRadioButtonList.ID = parentControl.ID + "_addressPartRadioButtonList";
            addressPartRadioButtonList.Label = "Address Part";
            addressPartRadioButtonList.Help = "Select the part of the address to show in the grid, or select Full to show the full address";
            parentControl.Controls.Add( addressPartRadioButtonList );

            return new System.Web.UI.Control[] { locationTypeList, addressPartRadioButtonList };
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( System.Web.UI.Control parentControl, System.Web.UI.HtmlTextWriter writer, System.Web.UI.Control[] controls )
        {
            base.RenderControls( parentControl, writer, controls );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( System.Web.UI.Control[] controls )
        {
            RockDropDownList locationTypeList = controls[0] as RockDropDownList;
            RockRadioButtonList addressPartRadioButtonList = controls[1] as RockRadioButtonList;
            return string.Format( "{0}|{1}", locationTypeList.SelectedValue, addressPartRadioButtonList.SelectedValue );
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( System.Web.UI.Control[] controls, string selection )
        {
            RockDropDownList locationTypeList = controls[0] as RockDropDownList;
            RockRadioButtonList addressPartRadioButtonList = controls[1] as RockRadioButtonList;
            string[] values = selection.Split( '|' );

            if ( values.Length >= 1 )
            {
                locationTypeList.SetValue( values[0] );
            }

            if ( values.Length >= 2 )
            {
                addressPartRadioButtonList.SetValue( values[1] );
            }
        }

        #endregion
    }
}
