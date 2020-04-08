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
    [WorkflowAttribute( "Person", "Workflow attribute that contains the person to add to the group.", true, "", "", 0, null,
        new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [WorkflowTextOrAttribute( "Request Description", "Request Description Attribute", "Text or workflow attribute that contains the benevolence request description. <span class='tip tip-lava'></span>", false, "", "", 1, "RequestDescription",
        new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.MemoFieldType" }, 3 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.BENEVOLENCE_REQUEST_STATUS, "Request Status", "The request status to use.", true, false, Rock.SystemGuid.DefinedValue.BENEVOLENCE_PENDING, "", 2 )]
    [WorkflowAttribute( "Case Worker", "Workflow attribute that contains the person who should be assigned as the case worker.", false, "", "", 3, null,
        new string[] { "Rock.Field.Types.PersonFieldType" } )]
    [CampusField("Campus", "The campus for the request. If blank the person's campus will be used.", false, "", "", 4)]
    [WorkflowTextOrAttribute( "Government Id", "Government Id Attribute", "Text or workflow attribute that contains the government. <span class='tip tip-lava'></span>", false, "", "", 5, "GovernmentId",
        new string[] { "Rock.Field.Types.TextFieldType" } )]

    [WorkflowAttribute( "Benevolence Request", "Workflow attribute to set the returned benevolence request to.", false, "", "", 6, null,
        new string[] { "Rock.Field.Types.BenevolenceRequestFieldType" } )]
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

            rockContext.SaveChanges();

            SetWorkflowAttributeValue( action, "BenevolenceRequest", request.Guid );

            action.AddLogEntry( $"Set 'Benevolence Request' attribute to '{request.Guid}'." );
            return true;
        }
    }
}
