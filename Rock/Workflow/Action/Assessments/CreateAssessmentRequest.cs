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
using Rock.Security;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Creates a new connection request.
    /// </summary>
    [ActionCategory( "Assessments" )]
    [Description( "Creates a new assessment request." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Assessment Request Create" )]

    #region Block Atttributes

    [WorkflowAttribute( "Assessment Types",
        Key = AttributeKey.AssessmentTypesKey,
        Description = "The attribute that contains the selected list of assessments being requested.",
        IsRequired = true,
        Order = 0,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.AssessmentTypesFieldType" } )]

    [WorkflowAttribute( "Person",
        Key = AttributeKey.Person,
        Description = "The attribute containing the person being requested to take the assessment(s).",
        IsRequired = true,
        Order = 1,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType" } )]

    [WorkflowAttribute( "Requested By",
        Key = AttributeKey.RequestedBy,
        Description = "The attribute containing the person requesting the test be taken.",
        IsRequired = false,
        Order = 2,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType" } )]

    [WorkflowAttribute( "Due Date",
        Key = AttributeKey.DueDate,
        Description = "The attribute that contains the Due Date (if any) for the requests.",
        IsRequired = false,
        Order = 2,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.DateFieldType" } )]

    #endregion

    public class CreateAssessmentRequest : ActionComponent
    {
        #region Workflow Attributes

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        protected static class AttributeKey
        {
            /// <summary>
            /// The assessments
            /// </summary>
            public const string AssessmentTypesKey = "AssessmentTypesKey";

            /// <summary>
            /// The person taking the assessment(s)
            /// </summary>
            public const string Person = "Person";

            /// <summary>
            /// The person requesting the assessment(s)
            /// </summary>
            public const string RequestedBy = "RequestedBy";

            /// <summary>
            /// The due date for the assessment(s)
            /// </summary>
            public const string DueDate = "DueDate";
        }

        #endregion Workflow Attributes

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
            rockContext = rockContext ?? new RockContext();

            errorMessages = new List<string>();

            var assessmentTypesGuidString = GetAttributeValue( action, AttributeKey.AssessmentTypesKey, true );
            var assessmentTypeGuids = assessmentTypesGuidString.IsNullOrWhiteSpace() ? null : assessmentTypesGuidString.Split( new char[] { ',' } );
            
            var personAliasGuid = GetAttributeValue( action, AttributeKey.Person, true ).AsGuidOrNull();
            Guid? requestedByAliasGuid = GetAttributeValue( action, AttributeKey.RequestedBy, true ).AsGuidOrNull();
            var dueDate = GetAttributeValue( action, AttributeKey.DueDate, true ).AsDateTime();

            // Validate attribute data
            if ( !assessmentTypeGuids.Any() )
            {
                errorMessages.Add( "No Assessments selected." );
                return false;
            }

            if ( personAliasGuid == null )
            {
                errorMessages.Add( "Invalid Person Attribute or Value." );
                return false;
            }

            var personAlias = new PersonAliasService( rockContext ).Get( personAliasGuid.Value );
            if ( personAlias == null )
            {
                errorMessages.Add( "Invalid Person Attribute or Value." );
                return false;
            }

            var requestedByAlias = new PersonAliasService( rockContext ).Get( requestedByAliasGuid.Value );

            foreach( string assessmentTypeGuid in assessmentTypeGuids )
            {
                // Check for an existing record
                var assessmentTypeService = new AssessmentTypeService( rockContext );
                int? assessmentTypeId = assessmentTypeService.GetId( assessmentTypeGuid.AsGuid() );

                var assessmentService = new AssessmentService( rockContext );
                var existingAssessment = assessmentService
                    .Queryable()
                    .Where( a => a.PersonAliasId == personAlias.Id )
                    .Where( a => a.AssessmentTypeId == assessmentTypeId )
                    .Where( a => a.Status == AssessmentRequestStatus.Pending )
                    .FirstOrDefault();

                // If a pending record for this person/type is found mark it complete.
                if ( existingAssessment != null )
                {
                    existingAssessment.Status = AssessmentRequestStatus.Complete;
                }

                // Create a new assessment
                var assessment = new Assessment
                {
                    PersonAliasId = personAlias.Id,
                    AssessmentTypeId = assessmentTypeId.Value,
                    RequesterPersonAliasId = requestedByAlias.Id,
                    RequestedDateTime = RockDateTime.Now,
                    RequestedDueDate = dueDate,
                    Status = AssessmentRequestStatus.Pending
                };

                assessmentService.Add( assessment );
                rockContext.SaveChanges();
            }

            return true;
        }
    }
}