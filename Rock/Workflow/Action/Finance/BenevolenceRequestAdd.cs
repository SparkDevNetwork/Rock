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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Runs a SQL query
    /// </summary>
    [ActionCategory( "Finance" )]
    [Description( "Adds a benevolence request." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Benevolence Request Add" )]
    [WorkflowAttribute( "Person",
        Description = "Workflow attribute that contains the person to add to the group.",
        IsRequired = true,
        Order = 0,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [WorkflowTextOrAttribute( "Request Description",
        "Request Description Attribute",
        Description = "Text or workflow attribute that contains the benevolence request description. <span class='tip tip-lava'></span>",
        IsRequired = false,
        Order = 1,
        Key = "RequestDescription",
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.MemoFieldType" },
        Rows = 3 )]
    [DefinedValueField( "Request Status",
        Description = "The request status to use.",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.BENEVOLENCE_REQUEST_STATUS,
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.BENEVOLENCE_PENDING,
        Order = 2 )]
    [BenevolenceTypeField( "Benevolence Type",
        Description = "The benevolence type to use.",
        IsRequired = true,
        DefaultValue = SystemGuid.BenevolenceType.BENEVOLENCE,
        Order = 3 )]
    [WorkflowAttribute( "Case Worker",
        Description = "Workflow attribute that contains the person who should be assigned as the case worker.",
        IsRequired = false,
        Order = 4,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [CampusField("Campus",
        Description = "The campus for the request. If blank the person's campus will be used.",
        IsRequired = false,
        Order = 5)]
    [WorkflowTextOrAttribute( "Government Id",
        "Government Id Attribute",
        Description = "Text or workflow attribute that contains the government. <span class='tip tip-lava'></span>",
        IsRequired = false,
        Order = 6,
        Key = "GovernmentId",
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType" } )]

    [WorkflowAttribute( "Benevolence Request",
        Description = "Workflow attribute to set the returned benevolence request to.",
        IsRequired = false,
        Order = 7,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.BenevolenceRequestFieldType" } )]
    [Rock.SystemGuid.EntityTypeGuid( "22B3C308-2333-4A11-8AEC-1AA7A201B5BB")]
    public class BenevolenceRequestAdd : ActionComponent
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

            var mergeFields = GetMergeFields( action );

            var homePhoneValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ).Id;
            var mobilePhoneValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;
            var workPhoneValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK ).Id;

            // get requester
            var requestPerson = new PersonAliasService( rockContext ).Get( GetAttributeValue( action, "Person", true ).AsGuid() ).Person;
            if (requestPerson == null )
            {
                var errorMessage = "Could not determine the person for the request.";
                errorMessages.Add( errorMessage );
                action.AddLogEntry( errorMessage, true );
                return false;
            }

            // get case worker
            var caseWorker = new PersonAliasService( rockContext ).Get( GetAttributeValue( action, "CaseWorker", true ).AsGuid() )?.Person;

            // get request status
            var statusValue = DefinedValueCache.Get( GetAttributeValue( action, "RequestStatus" ) );
            if ( statusValue == null )
            {
                var errorMessage = "Invalid request status provided.";
                errorMessages.Add( errorMessage );
                action.AddLogEntry( errorMessage, true );
                return false;
            }

            // get request type
            var requestType = new BenevolenceTypeService( rockContext ).Get( GetAttributeValue( action, "BenevolenceType" ) );
            if ( requestType == null )
            {
                var errorMessage = "Invalid benevolence type provided.";
                errorMessages.Add( errorMessage );
                action.AddLogEntry( errorMessage, true );
                return false;
            }

            // get request description
            var requestDescription = GetAttributeValue( action, "RequestDescription", true ).ResolveMergeFields( mergeFields );
            if ( string.IsNullOrWhiteSpace( requestDescription ) )
            {
                var errorMessage = "Request description is required.";
                errorMessages.Add( errorMessage );
                action.AddLogEntry( errorMessage, true );
                return false;
            }

            // get government id
            var governmentId = GetAttributeValue( action, "GovernmentId", true ).ResolveMergeFields( mergeFields );

            // get campus
            int? campusId = CampusCache.Get( GetAttributeValue( action, "Campus" ).AsGuid() )?.Id;

            // create benevolence request
            BenevolenceRequestService benevolenceRequestService = new BenevolenceRequestService( rockContext );

            BenevolenceRequest request = new BenevolenceRequest();
            benevolenceRequestService.Add( request );

            request.RequestDateTime = RockDateTime.Now;
            request.RequestText = requestDescription;
            request.RequestedByPersonAliasId = requestPerson.PrimaryAliasId;
            request.FirstName = requestPerson.NickName;
            request.LastName = requestPerson.LastName;
            request.Email = requestPerson.Email;
            request.LocationId = requestPerson.GetHomeLocation()?.Id;
            request.GovernmentId = governmentId;

            if ( campusId.HasValue )
            {
                request.CampusId = campusId.Value;
            }
            else
            {
                request.CampusId = requestPerson.GetCampus()?.Id;
            }

            var requestorPhoneNumbers = requestPerson.PhoneNumbers;

            if ( requestorPhoneNumbers != null )
            {
                request.HomePhoneNumber = requestorPhoneNumbers.Where( p => p.NumberTypeValueId == homePhoneValueId ).FirstOrDefault()?.NumberFormatted;
                request.CellPhoneNumber = requestorPhoneNumbers.Where( p => p.NumberTypeValueId == mobilePhoneValueId ).FirstOrDefault()?.NumberFormatted;
                request.WorkPhoneNumber = requestorPhoneNumbers.Where( p => p.NumberTypeValueId == workPhoneValueId ).FirstOrDefault()?.NumberFormatted;
            }

            if( caseWorker != null )
            {
                request.CaseWorkerPersonAliasId = caseWorker.PrimaryAliasId;
            }

            request.ConnectionStatusValueId = requestPerson.ConnectionStatusValueId;
            request.RequestStatusValueId = statusValue.Id;
            request.BenevolenceTypeId = requestType.Id;

            rockContext.SaveChanges();

            SetWorkflowAttributeValue( action, "BenevolenceRequest", request.Guid );

            action.AddLogEntry( $"Set 'Benevolence Request' attribute to '{request.Guid}'." );
            return true;
        }
    }
}
