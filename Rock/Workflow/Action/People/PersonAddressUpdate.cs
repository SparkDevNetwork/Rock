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

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Adds person to organization tag
    /// </summary>
    [ActionCategory( "People" )]
    [Description( "Updates an address for a person's family." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Person Address Update" )]

    [WorkflowAttribute("Person", "Workflow attribute that contains the person to update.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [WorkflowAttribute( "Location Type (From Attribute)", "The attribute that contains the the location type to update.", false, "", "", 1, "LocationTypeAttribute",
        new string[] { "Rock.Field.Types.DefinedValueFieldType" } )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.GROUP_LOCATION_TYPE, "Location Type", "The type of location to update (if attribute is not specified or is an invalid value).", true, false, Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME, "", 2 )]
    [LocationField("Location", "The location to use for updating person's record.", false, "", "", 3 )]
    [WorkflowAttribute("Location (From Attribute)", "A location attribute to use for updating person's record. This will be used if a value is not entered for the Location field above.", false, "", "", 4, "LocationAttribute",
        new string[] { "Rock.Field.Types.LocationFieldType", "Rock.Field.Types.AddressFieldType" } )]
    [WorkflowTextOrAttribute( "Is Mailing Location", "Attribute Value", "The value or attribute value to indicate if the location is the mailing address. Only valid values are 'True' or 'False' any other value will be ignored. <span class='tip tip-lava'></span>", false, "", "", 5, "IsMailing" )]
    [WorkflowTextOrAttribute( "Is Mapped Location", "Attribute Value", "The value or attribute value to indicate if the location should be mapped location. Only valid values are 'True' or 'False' any other value will be ignored. <span class='tip tip-lava'></span>", false, "", "", 6, "IsMapped" )]
    [BooleanField( "Save Current Address as Previous Address", "Determines whether this will overwrite an existing address of the specified type or change its type to Previous and add a new address.", order: 7, key: "SavePreviousAddress" )]
    public class PersonAddressUpdate : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            // get person
            Person person = null;

            string personAttributeValue = GetAttributeValue( action, "Person" );
            Guid? guidPersonAttribute = personAttributeValue.AsGuidOrNull();
            if ( guidPersonAttribute.HasValue )
            {
                var attributePerson = AttributeCache.Read( guidPersonAttribute.Value, rockContext );
                if ( attributePerson != null || attributePerson.FieldType.Class != "Rock.Field.Types.PersonFieldType" )
                {
                    string attributePersonValue = action.GetWorklowAttributeValue( guidPersonAttribute.Value );
                    if ( !string.IsNullOrWhiteSpace( attributePersonValue ) )
                    {
                        Guid personAliasGuid = attributePersonValue.AsGuid();
                        if ( !personAliasGuid.IsEmpty() )
                        {
                            person = new PersonAliasService( rockContext ).Queryable()
                                .Where( a => a.Guid.Equals( personAliasGuid ) )
                                .Select( a => a.Person )
                                .FirstOrDefault();
                            if ( person == null )
                            {
                                errorMessages.Add( string.Format( "Person could not be found for selected value ('{0}')!", guidPersonAttribute.ToString() ) );
                                return false;
                            }
                        }
                    }
                }
            }

            if ( person == null )
            {
                errorMessages.Add( "The attribute used to provide the person was invalid, or not of type 'Person'." );
                return false;
            }

            // determine the location type to edit
            DefinedValueCache locationType = null;
            var locationTypeAttributeValue = action.GetWorklowAttributeValue( GetAttributeValue( action, "LocationTypeAttribute" ).AsGuid() );
            if ( locationTypeAttributeValue != null )
            {
                locationType = DefinedValueCache.Read( locationTypeAttributeValue.AsGuid() );
            }
            if ( locationType == null )
            {
                locationType = DefinedValueCache.Read( GetAttributeValue( action, "LocationType" ).AsGuid() );
            }
            if ( locationType == null )
            {
                errorMessages.Add( "The location type to be updated was not selected." );
                return false;
            }

            // get the new phone number value
            Location location = null;
            string locationValue = GetAttributeValue( action, "Location" );
            Guid? locationGuid = locationValue.AsGuidOrNull();
            if ( !locationGuid.HasValue || locationGuid.Value.IsEmpty() )
            {
                string locationAttributeValue = GetAttributeValue( action, "LocationAttribute" );
                Guid? locationAttributeValueGuid = locationAttributeValue.AsGuidOrNull();
                if ( locationAttributeValueGuid.HasValue )
                {
                    locationGuid = action.GetWorklowAttributeValue( locationAttributeValueGuid.Value ).AsGuidOrNull();
                }
            }

            if ( locationGuid.HasValue )
            {
                location = new LocationService( rockContext ).Get( locationGuid.Value );
            }

            if ( location == null )
            {
                errorMessages.Add( "The location value could not be determined." );
                return false;
            }

            // gets value indicating if location is a mailing location
            string mailingValue = GetAttributeValue( action, "IsMailing" );
            Guid? mailingValueGuid = mailingValue.AsGuidOrNull();
            if ( mailingValueGuid.HasValue )
            {
                mailingValue = action.GetWorklowAttributeValue( mailingValueGuid.Value );
            }
            else
            {
                mailingValue = mailingValue.ResolveMergeFields( GetMergeFields( action ) );
            }
            bool? mailing = mailingValue.AsBooleanOrNull();

            // gets value indicating if location is a mapped location
            string mappedValue = GetAttributeValue( action, "IsMapped" );
            Guid? mappedValueGuid = mappedValue.AsGuidOrNull();
            if ( mappedValueGuid.HasValue )
            {
                mappedValue = action.GetWorklowAttributeValue( mappedValueGuid.Value );
            }
            else
            {
                mappedValue = mappedValue.ResolveMergeFields( GetMergeFields( action ) );
            }
            bool? mapped = mappedValue.AsBooleanOrNull();

            var savePreviousAddress = GetAttributeValue( action, "SavePreviousAddress" ).AsBoolean();

            var locationService = new LocationService( rockContext );
            locationService.Verify( location, false );

            var groupLocationService = new GroupLocationService( rockContext );
            foreach ( var family in person.GetFamilies( rockContext ).ToList() )
            {
                var groupChanges = new List<string>();

                if ( savePreviousAddress )
                {
                    // Get all existing addresses of the specified type
                    var groupLocations = family.GroupLocations.Where( l => l.GroupLocationTypeValueId == locationType.Id ).ToList();

                    // Create a new address of the specified type, saving all existing addresses of that type as Previous Addresses
                    // Use the specified Is Mailing and Is Mapped values from the action's parameters if they are set,
                    // otherwise set them to true if any of the existing addresses of that type have those values set to true
                    GroupService.AddNewGroupAddress( rockContext, family, locationType.Guid.ToString(), location.Id, true,
                        $"the {action.ActionType.ActivityType.WorkflowType.Name} workflow",
                        mailing ?? groupLocations.Any( x => x.IsMailingLocation ),
                        mapped ?? groupLocations.Any( x => x.IsMappedLocation ) );
                }
                else
                {
                    var groupLocation = family.GroupLocations.FirstOrDefault( l => l.GroupLocationTypeValueId == locationType.Id );
                    string oldValue = string.Empty;
                    if ( groupLocation == null )
                    {
                        groupLocation = new GroupLocation();
                        groupLocation.GroupId = family.Id;
                        groupLocation.GroupLocationTypeValueId = locationType.Id;
                        groupLocationService.Add( groupLocation );
                    }
                    else
                    {
                        oldValue = groupLocation.Location.ToString();
                    }


                    History.EvaluateChange(
                        groupChanges,
                        locationType.Value + " Location",
                        oldValue,
                        location.ToString() );

                    groupLocation.Location = location;

                    if ( mailing.HasValue )
                    {
                        History.EvaluateChange(
                            groupChanges,
                            locationType.Value + " Is Mailing",
                            ( oldValue == string.Empty ) ? null : groupLocation.IsMailingLocation.ToString(),
                            mailing.Value.ToString() );
                        groupLocation.IsMailingLocation = mailing.Value;
                    }

                    if ( mapped.HasValue )
                    {
                        History.EvaluateChange(
                            groupChanges,
                            locationType.Value + " Is Map Location",
                            ( oldValue == string.Empty ) ? null : groupLocation.IsMappedLocation.ToString(),
                            mapped.Value.ToString() );
                        groupLocation.IsMappedLocation = mapped.Value;
                    }
                }

                if ( groupChanges.Any() )
                {
                    groupChanges.Add( string.Format( "<em>(Updated by the '{0}' workflow)</em>", action.ActionType.ActivityType.WorkflowType.Name ) );
                    foreach ( var fm in family.Members )
                    {
                        HistoryService.SaveChanges(
                            rockContext,
                            typeof( Person ),
                            Rock.SystemGuid.Category.HISTORY_PERSON_FAMILY_CHANGES.AsGuid(),
                            fm.PersonId,
                            groupChanges,
                            family.Name,
                            typeof( Group ),
                            family.Id,
                            false );
                    }
                }

                rockContext.SaveChanges();

                action.AddLogEntry( string.Format( "Updated the {0} location for {1} (family: {2}) to {3}", locationType.Value, person.FullName, family.Name, location.ToString() ) );
            }

            return true;
        }
    }
}
