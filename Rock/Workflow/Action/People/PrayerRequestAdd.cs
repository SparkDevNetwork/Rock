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
    /// Creates a new prayer request.
    /// </summary>
    [ActionCategory( "People" )]
    [Description( "This action will create a prayer request and optionally update a workflow attribute with the request. Use the requestor field to indicate who is being prayed for or provide first name, last name, and email." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Prayer Request Add" )]

    #region Attributes

    [WorkflowTextOrAttribute(
        textLabel: "Requestor",
        attributeLabel: "Attribute Value",
        description: "Lava template for selecting the person making the request or attribute that contains the requestor. If an attribute is used, it should be a person attribute. <span class='tip tip-lava'></span>",
        required: false,
        defaultValue: "",
        category: "",
        order: 1,
        key: AttributeKey.Requestor,
        fieldTypeClassNames: new string[] {
            "Rock.Field.Types.TextFieldType",
            "Rock.Field.Types.PersonFieldType" } )]

    [WorkflowAttribute(
        name: "First Name Attribute",
        description: "The text attribute that contains the first name of the person that this prayer request is about.",
        required: false,
        defaultValue: "",
        category: "",
        order: 2,
        key: AttributeKey.FirstName,
        fieldTypeClassNames: new string[] {
            "Rock.Field.Types.TextFieldType" } )]

    [WorkflowAttribute(
        name: "Last Name Attribute",
        description: "The text attribute that contains the last name of the person that this prayer request is about.",
        required: false,
        defaultValue: "",
        category: "",
        order: 3,
        key: AttributeKey.LastName,
        fieldTypeClassNames: new string[] {
            "Rock.Field.Types.TextFieldType" } )]

    [WorkflowAttribute(
        name: "Email Attribute",
        description: "The text or email attribute that contains the email address of the person that this prayer request is about.",
        required: false,
        defaultValue: "",
        category: "",
        order: 4,
        key: AttributeKey.Email,
        fieldTypeClassNames: new string[] {
            "Rock.Field.Types.TextFieldType",
            "Rock.Field.Types.EmailFieldType" } )]

    [BooleanField(
        name: "Enable Person Matching",
        description: "Is person matching enabled?",
        order: 5,
        key: AttributeKey.IsPersonMatchingEnabled )]

    [WorkflowTextOrAttribute(
        textLabel: "Campus",
        attributeLabel: "Attribute Value",
        description: "Lava template for selecting the campus for the request or attriibute that contains the campus. If an attribute is used, it must be a campus attribute type. <span class='tip tip-lava'></span>",
        required: false,
        defaultValue: "",
        category: "",
        order: 6,
        key: AttributeKey.Campus,
        fieldTypeClassNames: new string[] { "Rock.Field.Types.CampusFieldType" } )]

    [WorkflowTextOrAttribute(
        textLabel: "Request",
        attributeLabel: "Attribute Value",
        description: "Lava template for the request text or attribute that contains the request text. <span class='tip tip-lava'></span>",
        required: true,
        defaultValue: "",
        category: "",
        order: 7,
        key: AttributeKey.Request,
        fieldTypeClassNames: new string[] {
            "Rock.Field.Types.TextFieldType",
            "Rock.Field.Types.MemoFieldType" } )]

    [WorkflowAttribute(
        name: "Prayer Category Attribute",
        description: "The category attribute that contains the category of this prayer request.",
        required: true,
        order: 8,
        key: AttributeKey.Category,
        fieldTypeClassNames: new string[] {
            "Rock.Field.Types.CategoryFieldType" } )]

    [WorkflowAttribute(
        name: "Is Public Attribute",
        description: "The boolean attribute that indicates if the prayer request is public.",
        order: 9,
        required: false,
        key: AttributeKey.IsPublic,
        fieldTypeClassNames: new string[] {
            "Rock.Field.Types.BooleanFieldType" } )]

    [WorkflowAttribute(
        name: "Is Urgent Attribute",
        description: "The boolean attribute that indicates if the prayer request is urgent.",
        order: 10,
        required: false,
        key: AttributeKey.IsUrgent,
        fieldTypeClassNames: new string[] {
            "Rock.Field.Types.BooleanFieldType" } )]

    [BooleanField(
        name: "Is Approved",
        description: "Is the prayer request approved?",
        order: 11,
        key: AttributeKey.IsApproved )]

    [WorkflowAttribute(
        name: "Allow Comments Attribute",
        description: "The boolean attribute that indicates if comments are allowed on the prayer request.",
        order: 12,
        required: false,
        key: AttributeKey.AreCommentsAllowed,
        fieldTypeClassNames: new string[] {
            "Rock.Field.Types.BooleanFieldType" } )]

    [IntegerField(
        name: "Expire After (Days)",
        description: "How many days will this prayer request be shown before being marked as expired?",
        required: true,
        defaultValue: AttributeDefaults.ExpireAfterDays,
        order: 13,
        key: AttributeKey.ExpireAfterDays )]

    [WorkflowAttribute(
        name: "Prayer Request Attribute",
        description: "The optional attribute to store the prayer request into. This should be a prayer request attribute.",
        required: false,
        order: 14,
        key: AttributeKey.PrayerRequestAttribute,
        fieldTypeClassNames: new string[] {
            "Rock.Field.Types.TextFieldType",
            "Rock.Field.Types.PrayerRequestFieldType" } )]

    #endregion Attributes

    [Rock.SystemGuid.EntityTypeGuid( "E76463C5-C8CD-40AB-AD5A-0758937CA407")]
    public class PrayerRequestAdd : ActionComponent
    {
        #region Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The requestor
            /// </summary>
            public const string Requestor = "Requestor";

            /// <summary>
            /// The first name
            /// </summary>
            public const string FirstName = "FirstName";

            /// <summary>
            /// The last name
            /// </summary>
            public const string LastName = "LastName";

            /// <summary>
            /// The email
            /// </summary>
            public const string Email = "Email";

            /// <summary>
            /// The campus
            /// </summary>
            public const string Campus = "Campus";

            /// <summary>
            /// The request
            /// </summary>
            public const string Request = "Request";

            /// <summary>
            /// The category
            /// </summary>
            public const string Category = "Category";

            /// <summary>
            /// The is public
            /// </summary>
            public const string IsPublic = "IsPublic";

            /// <summary>
            /// The is urgent
            /// </summary>
            public const string IsUrgent = "IsUrgent";

            /// <summary>
            /// The is approved
            /// </summary>
            public const string IsApproved = "IsApproved";

            /// <summary>
            /// The are comments allowed
            /// </summary>
            public const string AreCommentsAllowed = "AreCommentsAllowed";

            /// <summary>
            /// The expire after days
            /// </summary>
            public const string ExpireAfterDays = "ExpireAfterDays";

            /// <summary>
            /// The is person matching enabled
            /// </summary>
            public const string IsPersonMatchingEnabled = "IsPersonMatchingEnabled";

            /// <summary>
            /// The prayer request attribute
            /// </summary>
            public const string PrayerRequestAttribute = "PrayerRequestAttribute";
        }

        /// <summary>
        /// Attribute value defaults
        /// </summary>
        private static class AttributeDefaults
        {
            /// <summary>
            /// The expire after days
            /// </summary>
            public const int ExpireAfterDays = 14;
        }

        #endregion Keys

        #region Execution Context

        /*
            11/25/2026 - MSE

            Action components are shared singletons: one instance runs every workflow.
            Per-run values live in this object, built in Execute and passed to each
            helper. Do not move them to fields on this class.

            Reason: Fields on the component let one run read another run's values.
        */
        /// <summary>
        /// The values for a single run of this action.
        /// </summary>
        private sealed class ExecutionContext
        {
            /// <summary>
            /// Gets or sets the workflow action being run.
            /// </summary>
            public WorkflowAction Action { get; set; }

            /// <summary>
            /// Gets or sets the rock context for this run.
            /// </summary>
            public RockContext RockContext { get; set; }

            /// <summary>
            /// Gets or sets the merge fields for this run.
            /// </summary>
            public Dictionary<string, object> MergeFields { get; set; }
        }

        #endregion Execution Context

        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var context = new ExecutionContext
            {
                Action = action,
                RockContext = rockContext,
                MergeFields = GetMergeFields( action )
            };

            // Create the prayer request
            var prayerRequestService = new PrayerRequestService( rockContext );
            var isApproved = GetBoolean( context, AttributeKey.IsApproved );
            var now = RockDateTime.Now;
            var category = GetCategory( context );

            var prayerRequest = new PrayerRequest
            {
                AllowComments = GetBooleanFromSelectedAttribute( context, AttributeKey.AreCommentsAllowed ),
                ApprovedOnDateTime = isApproved == true ? now : ( DateTime? ) null,
                IsApproved = isApproved,
                CampusId = GetCampusId( context ),
                CategoryId = category?.Id,
                EnteredDateTime = now,
                ExpirationDate = GetExpirationDate( context, now ),
                IsActive = true,
                IsPublic = GetBooleanFromSelectedAttribute( context, AttributeKey.IsPublic ),
                IsUrgent = GetBooleanFromSelectedAttribute( context, AttributeKey.IsUrgent ),
                Text = GetResolvedLava( context, AttributeKey.Request )
            };

            // Set the requestor fields dependent on the attributes set
            var requestor = GetRequestor( context );

            if ( requestor == null )
            {
                prayerRequest.Email = GetTextFromSelectedAttribute( context, AttributeKey.Email );
                prayerRequest.FirstName = GetTextFromSelectedAttribute( context, AttributeKey.FirstName );
                prayerRequest.LastName = GetTextFromSelectedAttribute( context, AttributeKey.LastName );
            }
            else
            {
                prayerRequest.Email = requestor.Email;
                prayerRequest.FirstName = requestor.FirstName;
                prayerRequest.LastName = requestor.LastName;
                prayerRequest.RequestedByPersonAliasId = requestor.PrimaryAliasId;
            }

            // Validate the prayer request requirements for this action
            if ( category == null )
            {
                errorMessages.Add( "The category is required" );
            }
            else if ( category.EntityTypeId != EntityTypeCache.GetId<PrayerRequest>() )
            {
                errorMessages.Add( "The category must be for prayer requests" );
            }

            if ( prayerRequest.Text.IsNullOrWhiteSpace() )
            {
                errorMessages.Add( "The request text is required" );
            }

            if ( prayerRequest.FirstName.IsNullOrWhiteSpace() )
            {
                errorMessages.Add( "The first name is required" );
            }

            if ( prayerRequest.LastName.IsNullOrWhiteSpace() )
            {
                errorMessages.Add( "The last name is required" );
            }

            if ( errorMessages.Any() )
            {
                return false;
            }

            // Validate the model requirements
            if ( !prayerRequest.IsValid )
            {
                errorMessages.AddRange( prayerRequest.ValidationResults.Select( vr => vr.ErrorMessage ) );
                return false;
            }

            // Save the prayer request to the database
            prayerRequestService.Add( prayerRequest );
            rockContext.SaveChanges();

            // If request attribute was specified, set the attribute's value
            SetWorkflowAttributeValue( action, AttributeKey.PrayerRequestAttribute, prayerRequest.Guid );

            return true;
        }

        /// <summary>
        /// Gets the nullable boolean attribute value.
        /// </summary>
        /// <param name="context">The values for this run.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <returns></returns>
        private bool? GetBoolean( ExecutionContext context, string attributeKey )
        {
            return GetAttributeValue( context.Action, attributeKey ).AsBooleanOrNull();
        }

        /// <summary>
        /// Gets the campus identifier.
        /// </summary>
        /// <param name="context">The values for this run.</param>
        /// <returns></returns>
        private int? GetCampusId( ExecutionContext context )
        {
            var guid = GetGuidFromTextOrAttribute( context, AttributeKey.Campus );

            if ( !guid.HasValue )
            {
                return null;
            }

            return CampusCache.GetId( guid.Value );
        }

        /// <summary>
        /// Gets the category.
        /// </summary>
        /// <param name="context">The values for this run.</param>
        /// <returns></returns>
        private CategoryCache GetCategory( ExecutionContext context )
        {
            var guid = GetGuidFromSelectedAttribute( context, AttributeKey.Category );

            if ( !guid.HasValue )
            {
                return null;
            }

            return CategoryCache.Get( guid.Value );
        }

        /// <summary>
        /// Gets the expiration date.
        /// </summary>
        /// <param name="context">The values for this run.</param>
        /// <param name="now">The current date and time.</param>
        /// <returns></returns>
        private DateTime GetExpirationDate( ExecutionContext context, DateTime now )
        {
            var days = GetAttributeValue( context.Action, AttributeKey.ExpireAfterDays ).AsIntegerOrNull();

            if ( !days.HasValue || days.Value < 1 )
            {
                days = AttributeDefaults.ExpireAfterDays;
            }

            return now.AddDays( days.Value );
        }

        /// <summary>
        /// Gets the lava resolved string using a text value or text from an attribute value.
        /// </summary>
        /// <param name="context">The values for this run.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <returns></returns>
        private string GetResolvedLava( ExecutionContext context, string attributeKey )
        {
            var attributeGuid = GetAttributeValue( context.Action, attributeKey ).AsGuidOrNull();

            // If it's just text then get the text and resolve the lava
            if ( !attributeGuid.HasValue )
            {
                return GetAttributeValue( context.Action, attributeKey ).ResolveMergeFields( context.MergeFields );
            }

            /*
                10/26/2026 - MSE

                An attribute's value can hold untrusted input (entry forms, page
                parameters), so it is used as data and is not rendered as Lava. The
                empty string fallback keeps the old result for a missing value.

                Reason: Attribute values can hold untrusted input and must not run as Lava.
            */
            // If it's text within an attribute then use the attribute value as data.
            return context.Action.GetWorkflowAttributeValue( attributeGuid.Value ) ?? string.Empty;
        }

        /// <summary>
        /// Gets the requestor person either from the attribute or using person matching if allowed
        /// </summary>
        /// <param name="context">The values for this run.</param>
        /// <returns></returns>
        private Person GetRequestor( ExecutionContext context )
        {
            Person person;
            var personAliasGuid = GetGuidFromTextOrAttribute( context, AttributeKey.Requestor );

            if ( personAliasGuid.HasValue )
            {
                var personAliasService = new PersonAliasService( context.RockContext );
                person = personAliasService.GetPerson( personAliasGuid.Value );

                if ( person != null )
                {
                    return person;
                }
            }

            if ( GetBoolean( context, AttributeKey.IsPersonMatchingEnabled ) != true )
            {
                return null;
            }

            var email = GetTextFromSelectedAttribute( context, AttributeKey.Email );
            var firstName = GetTextFromSelectedAttribute( context, AttributeKey.FirstName );
            var lastName = GetTextFromSelectedAttribute( context, AttributeKey.LastName );

            // Email, first, and last name are all required to do person matching
            if ( email.IsNullOrWhiteSpace() || firstName.IsNullOrWhiteSpace() || lastName.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var personService = new PersonService( context.RockContext );
            var query = new PersonService.PersonMatchQuery( firstName, lastName, email, null );
            return personService.FindPerson( query, false, true, false );
        }

        /// <summary>
        /// Gets the unique identifier from text or attribute.
        /// </summary>
        /// <param name="context">The values for this run.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <returns></returns>
        private Guid? GetGuidFromTextOrAttribute( ExecutionContext context, string attributeKey )
        {
            /*
                10/26/2026 - MSE

                Values read from a selected workflow attribute are data and are not rendered
                as Lava; only text typed into the setting is rendered. See
                ActionComponent.GetTextOrAttributeValue.

                Reason: Attribute values can hold untrusted input and must not run as Lava.
            */
            return GetTextOrAttributeValue( context.Action, attributeKey, context.MergeFields ).AsGuidOrNull();
        }

        /// <summary>
        /// Get a string value from a workflow attribute
        /// </summary>
        /// <param name="context">The values for this run.</param>
        /// <param name="attributeKey"></param>
        /// <returns></returns>
        private string GetTextFromSelectedAttribute( ExecutionContext context, string attributeKey )
        {
            var attributeGuid = GetAttributeValue( context.Action, attributeKey ).AsGuidOrNull();

            if ( !attributeGuid.HasValue )
            {
                return string.Empty;
            }

            return context.Action.GetWorkflowAttributeValue( attributeGuid.Value );
        }

        /// <summary>
        /// Get a bool value from a workflow attribute
        /// </summary>
        /// <param name="context">The values for this run.</param>
        /// <param name="attributeKey"></param>
        /// <returns></returns>
        private bool? GetBooleanFromSelectedAttribute( ExecutionContext context, string attributeKey )
        {
            return GetTextFromSelectedAttribute( context, attributeKey ).AsBooleanOrNull();
        }

        /// <summary>
        /// Get a guid value from a workflow attribute
        /// </summary>
        /// <param name="context">The values for this run.</param>
        /// <param name="attributeKey"></param>
        /// <returns></returns>
        private Guid? GetGuidFromSelectedAttribute( ExecutionContext context, string attributeKey )
        {
            return GetTextFromSelectedAttribute( context, attributeKey ).AsGuidOrNull();
        }
    }
}