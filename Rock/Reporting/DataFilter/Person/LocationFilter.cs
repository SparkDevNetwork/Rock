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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    ///     A DataFilter that selects people associated with locations matching the filter.
    /// </summary>
    [Description( "Filter people by their family address." )]
    [Export( typeof(DataFilterComponent) )]
    [ExportMetadata( "ComponentName", "Location Filter" )]
    public class LocationFilter : DataFilterComponent
    {
        #region Settings

        /// <summary>
        ///     Settings for the Data Select Component.
        /// </summary>
        private class FilterSettings : SettingsStringBase
        {
            public Guid? LocationTypeGuid;
            public string Street1;
            public string City;
            public string State;
            public string PostalCode;
            public string Country;

            public FilterSettings()
            {
                //
            }

            public FilterSettings( string settingsString )
            {
                FromSelectionString( settingsString );
            }

            /// <summary>
            /// Indicates if the current settings are valid.
            /// </summary>
            /// <value>
            /// True if the settings are valid.
            /// </value>
            public override bool IsValid
            {
                get
                {
                    // at least one item should be set for this to be valid (otherwise it's just data view bloat).
                    return LocationTypeGuid.HasValue || ! string.IsNullOrWhiteSpace( Street1 ) || !string.IsNullOrWhiteSpace( City )
                        || !string.IsNullOrWhiteSpace( State ) || !string.IsNullOrWhiteSpace( PostalCode );
                }
            }

            /// <summary>
            /// Set the property values parsed from a settings string.
            /// </summary>
            /// <param name="version">The version number of the parameter set.</param>
            /// <param name="parameters">An ordered collection of strings representing the parameter values.</param>
            protected override void OnSetParameters( int version, IReadOnlyList<string> parameters )
            {
                // Parameter 1: Location Type
                LocationTypeGuid = DataComponentSettingsHelper.GetParameterOrEmpty( parameters, 0 ).AsGuidOrNull();

                // Parameter 2: Street1
                Street1 = DataComponentSettingsHelper.GetParameterOrEmpty( parameters, 1 ).ToStringSafe();

                // Parameter 3: City
                City = DataComponentSettingsHelper.GetParameterOrEmpty( parameters, 2 ).ToStringSafe();

                // Parameter 4: State
                State = DataComponentSettingsHelper.GetParameterOrEmpty( parameters, 3 ).ToStringSafe();

                // Parameter 5: Postal Code
                PostalCode = DataComponentSettingsHelper.GetParameterOrEmpty( parameters, 4 ).ToStringSafe();

                // Parameter 6: Country
                Country = DataComponentSettingsHelper.GetParameterOrEmpty( parameters, 5 ).ToStringSafe();
            }

            /// <summary>
            /// Gets an ordered set of property values that can be used to construct the
            /// settings string.
            /// </summary>
            /// <returns>
            /// An ordered collection of strings representing the parameter values.
            /// </returns>
            protected override IEnumerable<string> OnGetParameters()
            {
                var settings = new List<string>();

                // These are ORDERED. Don't mess the ordering up in the future.
                settings.Add( LocationTypeGuid.ToStringSafe() );
                settings.Add( Street1.ToStringSafe() );
                settings.Add( City.ToStringSafe() );
                settings.Add( State.ToStringSafe() );
                settings.Add( PostalCode.ToStringSafe() );
                settings.Add( Country.ToStringSafe() );

                return settings;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the entity type that the filter applies to.
        /// </summary>
        /// <value>
        /// The namespace-qualified Type name of the entity that the filter applies to, or an empty string if the filter applies to all entities.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return typeof( Rock.Model.Person ).FullName; }
        }

        /// <summary>
        /// Gets the name of the section in which the filter should be displayed in a browsable list.
        /// </summary>
        /// <value>
        /// The section name.
        /// </value>
        public override string Section
        {
            get { return "Additional Filters"; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the user-friendly title used to identify the filter component.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <returns>
        /// The name of the filter.
        /// </returns>
        public override string GetTitle( Type entityType )
        {
            return "Location";
        }

        /// <summary>
        ///     Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        ///     will set the description of the filter to whatever is returned by this property.  If including script, the
        ///     controls parent container can be referenced through a '$content' variable that is set by the control before
        ///     referencing this property.
        /// </summary>
        /// <value>
        ///     The client format script.
        /// </value>
        public override string GetClientFormatSelection( Type entityType )
        {
            return @"
function() {
  var locationType = $('.rock-drop-down-list,select:first', $content).find(':selected').text()
  var street = $('.js-addresscontrol > input[id$_tbStreet1""]', $content).text();
  var city = $('.js-addresscontrol > input[id$_tbCity""]', $content).text();
  var street = $('.js-addresscontrol > input[id$_tbStreet1""]', $content).text();
  var state = $('.js-addresscontrol > select[id$=""_ddlState""]', $content).find(':selected').text();
  var postalCode = $('.js-addresscontrol > input[id$='_tbPostalCode']', $content).text();
  var result = 'Location';

  if (locationType) {
     result = result + ' type ""' + locationType + "";
  }

  if (street) {
     result = result + ' street ""' + street + "";
  }

  if (city) {
     result = result + ' city ""' + city + "";
  }

  if (state) {
     result = result + ' state ""' + state + "";
  }

  if (postalCode) {
     result = result + ' zip ""' + postalCode + "";
  }

  return result;
}
";
        }

        /// <summary>
        /// Provides a user-friendly description of the specified filter values.
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="selection">A formatted string representing the filter settings.</param>
        /// <returns>
        /// A string containing the user-friendly description of the settings.
        /// </returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            var settings = new FilterSettings( selection );

            string result = "With Location";

            if ( !settings.IsValid )
            {
                return result;
            }

            using ( var context = new RockContext() )
            {
                string locationTypeName = null;
                string street1 = string.IsNullOrWhiteSpace( settings.Street1 ) ? null : settings.Street1;
                string city = string.IsNullOrWhiteSpace( settings.City ) ? null : settings.City;
                string state = string.IsNullOrWhiteSpace( settings.State ) ? null : settings.State;
                string postalCode = string.IsNullOrWhiteSpace( settings.PostalCode ) ? null : settings.PostalCode;

                string countryName = GlobalAttributesCache.Read().GetValue( "SupportInternationalAddresses" ).AsBoolean() &&
                    ! string.IsNullOrWhiteSpace( settings.Country ) ? settings.Country : null;

                if ( settings.LocationTypeGuid.HasValue)
                {
                    locationTypeName = DefinedValueCache.Read( settings.LocationTypeGuid.Value, context ).Value;
                }

                result = string.Format( "Location {0} with: {1} {2} {3} {4} {5}",
                                        ( locationTypeName != null ? "type \"" + locationTypeName + "\"" : string.Empty ),
                                        ( street1 != null ? "street \"" + street1 + "\"" : string.Empty ),
                                        ( city != null ? "city \"" + city + "\"" : string.Empty ),
                                        ( state != null ? "state \"" + state + "\"" : string.Empty ),
                                        ( postalCode != null ? "postal code \"" + postalCode + "\"" : string.Empty ),
                                        ( countryName != null ? "country \"" + countryName + "\"" : string.Empty )
                                      );
            }

            return result;
        }

        private readonly string _CtlLocationAddress = "acAddress";
        private readonly string _CtlLocationType = "ddlLocationType";

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="parentControl">The parent control.</param>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField parentControl )
        {
            // Define Control: Location Type DropDown List
            var ddlLocationType = new RockDropDownList();
            ddlLocationType.ID = parentControl.GetChildControlInstanceName( _CtlLocationType );
            ddlLocationType.Label = "Address Type";
            ddlLocationType.Help = "Specifies the type of address the filter will be applied to. If no value is selected, all of the Person's addresses will be considered.";

            var familyLocations = GroupTypeCache.GetFamilyGroupType().LocationTypeValues.OrderBy( a => a.Order ).ThenBy( a => a.Value );

            foreach (var value in familyLocations)
            {
                ddlLocationType.Items.Add( new ListItem( value.Value, value.Guid.ToString() ) );
            }

            ddlLocationType.Items.Insert( 0, None.ListItem );

            parentControl.Controls.Add( ddlLocationType );

            // Define Control: Address Control
            var acAddress = new AddressControl();
            acAddress.ID = parentControl.GetChildControlInstanceName( _CtlLocationAddress );
            acAddress.Label = "Address";
            acAddress.Help = "All or part of an address to which the Person is associated.";
            acAddress.AddCssClass( "js-addresscontrol" );
            parentControl.Controls.Add( acAddress );

            return new Control[] { acAddress, ddlLocationType };
        }

        /// <summary>
        /// Gets the selection.
        /// Implement this version of GetSelection if your DataFilterComponent works the same in all FilterModes
        /// </summary>
        /// <param name="entityType">The System Type of the entity to which the filter will be applied.</param>
        /// <param name="controls">The collection of controls used to set the filter values.</param>
        /// <returns>
        /// A formatted string.
        /// </returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            var ddlLocationType = controls.GetByName<RockDropDownList>( _CtlLocationType );
            var acAddress = controls.GetByName<AddressControl>( _CtlLocationAddress );

            var settings = new FilterSettings();

            settings.LocationTypeGuid = ddlLocationType.SelectedValue.AsGuidOrNull();
            if ( acAddress != null )
            {
                settings.Street1 = acAddress.Street1;
                settings.City = acAddress.City;
                settings.State = acAddress.State;
                settings.PostalCode = acAddress.PostalCode;
                settings.Country = acAddress.Country;
            }

            return settings.ToSelectionString();
        }

        /// <summary>
        /// Sets the selection.
        /// Implement this version of SetSelection if your DataFilterComponent works the same in all FilterModes
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            var ddlLocationType = controls.GetByName<RockDropDownList>( _CtlLocationType );
            var acAddress = controls.GetByName<AddressControl>( _CtlLocationAddress );

            var settings = new FilterSettings( selection );

            if ( !settings.IsValid )
            {
                return;
            }

            ddlLocationType.SelectedValue = settings.LocationTypeGuid.ToStringSafe();
            acAddress.Street1 = settings.Street1;
            acAddress.City = settings.City;
            acAddress.State = settings.State;
            acAddress.PostalCode = settings.PostalCode;
            acAddress.Country = settings.Country;            
        }

        /// <summary>
        /// Creates a Linq Expression that can be applied to an IQueryable to filter the result set.
        /// </summary>
        /// <param name="entityType">The type of entity in the result set.</param>
        /// <param name="serviceInstance">A service instance that can be queried to obtain the result set.</param>
        /// <param name="parameterExpression">The input parameter that will be injected into the filter expression.</param>
        /// <param name="selection">A formatted string representing the filter settings.</param>
        /// <returns>
        /// A Linq Expression that can be used to filter an IQueryable.
        /// </returns>
        /// <exception cref="System.Exception">Filter issue(s):  + errorMessages.AsDelimited( ;  )</exception>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            var settings = new FilterSettings( selection );

            var context = ( RockContext ) serviceInstance.Context;

            var locationService = new LocationService( context );
            var locationQuery = locationService.Queryable();

            if ( locationQuery != null )
            {
                if ( !string.IsNullOrWhiteSpace( settings.Street1 ) )
                {
                    locationQuery = locationQuery.Where( l => l.Street1.Contains( settings.Street1 ) );
                }

                if ( !string.IsNullOrWhiteSpace( settings.City ) )
                {
                    locationQuery = locationQuery.Where( l => l.City == settings.City );
                }

                if ( !string.IsNullOrWhiteSpace( settings.State ) )
                {
                    locationQuery = locationQuery.Where( l => l.State == settings.State );
                }

                if ( !string.IsNullOrWhiteSpace( settings.PostalCode ) )
                {
                    locationQuery = locationQuery.Where( l => l.PostalCode.StartsWith( settings.PostalCode ) );
                }

                if ( !string.IsNullOrWhiteSpace( settings.Country ) )
                {
                    locationQuery = locationQuery.Where( l => l.Country == settings.Country );
                }
            }

            // Get all the Family Groups that have a Location matching one of the candidate Locations.
            int familyGroupTypeId = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() ).Id;

            var groupLocationsQuery = new GroupLocationService( context ).Queryable()
                .Where( gl => gl.Group.GroupTypeId == familyGroupTypeId && locationQuery.Any( l => l.Id == gl.LocationId ) );

            // If a Location Type is specified, apply the filter condition.
            if (settings.LocationTypeGuid.HasValue)
            {
                int groupLocationTypeId = DefinedValueCache.Read( settings.LocationTypeGuid.Value ).Id;
                groupLocationsQuery = groupLocationsQuery.Where( x => x.GroupLocationTypeValue.Id == groupLocationTypeId );
            }

            // Get all of the Group Members of the qualifying Families.
            var groupMemberServiceQry = new GroupMemberService( context ).Queryable()
                .Where( gm => groupLocationsQuery.Any( gl => gl.GroupId == gm.GroupId ) );

            // Get all of the People corresponding to the qualifying Group Members.
            var qry = new PersonService( context ).Queryable()
                .Where( p => groupMemberServiceQry.Any( gm => gm.PersonId == p.Id ) );

            // Retrieve the Filter Expression.
            var extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );

            return extractedFilterExpression;
        }

        #endregion
    }
}