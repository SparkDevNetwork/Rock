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
        "Requestor",
        "Attribute Value",
        Description = "Lava template for selecting the person making the request or attribute that contains the requestor. If an attribute is used, it should be a person attribute. <span class='tip tip-lava'></span>",
        IsRequired = false,
        DefaultValue = "",
        Category = "",
        Order = 1,
        Key = AttributeKey.Requestor,
        FieldTypeClassNames = new string[] {
            "Rock.Field.Types.TextFieldType",
            "Rock.Field.Types.PersonFieldType" } )]

    [WorkflowAttribute(
        "First Name Attribute",
        Description = "The text attribute that contains the first name of the person that this prayer request is about.",
        IsRequired = false,
        DefaultValue = "",
        Category = "",
        Order = 2,
        Key = AttributeKey.FirstName,
        FieldTypeClassNames = new string[] {
            "Rock.Field.Types.TextFieldType" } )]

    [WorkflowAttribute(
        "Last Name Attribute",
        Description = "The text attribute that contains the last name of the person that this prayer request is about.",
        IsRequired = false,
        DefaultValue = "",
        Category = "",
        Order = 3,
        Key = AttributeKey.LastName,
        FieldTypeClassNames = new string[] {
            "Rock.Field.Types.TextFieldType" } )]

    [WorkflowAttribute(
        "Email Attribute",
        Description = "The text or email attribute that contains the email address of the person that this prayer request is about.",
        IsRequired = false,
        DefaultValue = "",
        Category = "",
        Order = 4,
        Key = AttributeKey.Email,
        FieldTypeClassNames = new string[] {
            "Rock.Field.Types.TextFieldType",
            "Rock.Field.Types.EmailFieldType" } )]

    [BooleanField(
        "Enable Person Matching",
        Description = "Is person matching enabled?",
        Order = 5,
        Key = AttributeKey.IsPersonMatchingEnabled )]

    [WorkflowTextOrAttribute(
        "Campus",
        "Attribute Value",
        Description = "Lava template for selecting the campus for the request or attriibute that contains the campus. If an attribute is used, it must be a campus attribute type. <span class='tip tip-lava'></span>",
        IsRequired = false,
        DefaultValue = "",
        Category = "",
        Order = 6,
        Key = AttributeKey.Campus,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.CampusFieldType" } )]

    [WorkflowTextOrAttribute(
        "Request",
        "Attribute Value",
        Description = "Lava template for the request text or attribute that contains the request text. <span class='tip tip-lava'></span>",
        IsRequired = true,
        DefaultValue = "",
        Category = "",
        Order = 7,
        Key = AttributeKey.Request,
        FieldTypeClassNames = new string[] {
            "Rock.Field.Types.TextFieldType",
            "Rock.Field.Types.MemoFieldType" } )]

    [WorkflowAttribute(
        "Prayer Category Attribute",
        Description = "The category attribute that contains the category of this prayer request.",
        IsRequired = true,
        Order = 8,
        Key = AttributeKey.Category,
        FieldTypeClassNames = new string[] {
            "Rock.Field.Types.CategoryFieldType" } )]

    [WorkflowAttribute(
        "Is Public Attribute",
        Description = "The boolean attribute that indicates if the prayer request is public.",
        Order = 9,
        IsRequired = false,
        Key = AttributeKey.IsPublic,
        FieldTypeClassNames = new string[] {
            "Rock.Field.Types.BooleanFieldType" } )]

    [WorkflowAttribute(
        "Is Urgent Attribute",
        Description = "The boolean attribute that indicates if the prayer request is urgent.",
        Order = 10,
        IsRequired = false,
        Key = AttributeKey.IsUrgent,
        FieldTypeClassNames = new string[] {
            "Rock.Field.Types.BooleanFieldType" } )]

    [BooleanField(
        "Is Approved",
        Description = "Is the prayer request approved?",
        Order = 11,
        Key = AttributeKey.IsApproved )]

    [WorkflowAttribute(
        "Allow Comments Attribute",
        Description = "The boolean attribute that indicates if comments are allowed on the prayer request.",
        Order = 12,
        IsRequired = false,
        Key = AttributeKey.AreCommentsAllowed,
        FieldTypeClassNames = new string[] {
            "Rock.Field.Types.BooleanFieldType" } )]

    [IntegerField(
        "Expire After (Days)",
        Description = "How many days will this prayer request be shown before being marked as expired?",
        IsRequired = true,
        DefaultIntegerValue = AttributeDefaults.ExpireAfterDays,
        Order = 13,
        Key = AttributeKey.ExpireAfterDays )]

    [WorkflowAttribute(
        "Prayer Request Attribute",
        Description = "The optional attribute to store the prayer request into. This should be a prayer request attribute.",
        IsRequired = false,
        Order = 14,
        Key = AttributeKey.PrayerRequestAttribute,
        FieldTypeClassNames = new string[] {
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

        #region Instance Properties

        private Dictionary<string, object> _mergeFields;
        private WorkflowAction _action;
        private RockContext _rockContext;

        #endregion Instance Properties

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

            // Initialize instance properties
            _action = action;
            _rockContext = rockContext;
            _mergeFields = GetMergeFields( action );

            // Create the prayer request
            var prayerRequestService = new PrayerRequestService( rockContext );
            var isApproved = GetBoolean( AttributeKey.IsApproved );
            var now = RockDateTime.Now;
            var category = GetCategory();

            var prayerRequest = new PrayerRequest
            {
                AllowComments = GetBooleanFromSelectedAttribute( AttributeKey.AreCommentsAllowed ),
                ApprovedOnDateTime = isApproved == true ? now : ( DateTime? ) null,
                IsApproved = isApproved,
                CampusId = GetCampusId(),
                CategoryId = category?.Id,
                EnteredDateTime = now,
                ExpirationDate = GetExpirationDate( now ),
                IsActive = true,
                IsPublic = GetBooleanFromSelectedAttribute( AttributeKey.IsPublic ),
                IsUrgent = GetBooleanFromSelectedAttribute( AttributeKey.IsUrgent ),
                Text = GetResolvedLava( AttributeKey.Request )
            };

            // Set the requestor fields dependent on the attributes set
            var requestor = GetRequestor();

            if ( requestor == null )
            {
                prayerRequest.Email = GetTextFromSelectedAttribute( AttributeKey.Email );
                prayerRequest.FirstName = GetTextFromSelectedAttribute( AttributeKey.FirstName );
                prayerRequest.LastName = GetTextFromSelectedAttribute( AttributeKey.LastName );
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
        /// <param name="attributeKey">The attribute key.</param>
        /// <returns></returns>
        private bool? GetBoolean( string attributeKey )
        {
            return GetAttributeValue( _action, attributeKey ).AsBooleanOrNull();
        }

        /// <summary>
        /// Gets the campus identifier.
        /// </summary>
        /// <returns></returns>
        private int? GetCampusId()
        {
            var guid = GetGuidFromTextOrAttribute( AttributeKey.Campus );

            if ( !guid.HasValue )
            {
                return null;
            }

            return CampusCache.GetId( guid.Value );
        }

        /// <summary>
        /// Gets the category.
        /// </summary>
        /// <returns></returns>
        private CategoryCache GetCategory()
        {
            var guid = GetGuidFromSelectedAttribute( AttributeKey.Category );

            if ( !guid.HasValue )
            {
                return null;
            }

            return CategoryCache.Get( guid.Value );
        }

        /// <summary>
        /// Gets the expiration date.
        /// </summary>
        /// <returns></returns>
        private DateTime GetExpirationDate( DateTime now )
        {
            var days = GetAttributeValue( _action, AttributeKey.ExpireAfterDays ).AsIntegerOrNull();

            if ( !days.HasValue || days.Value < 1 )
            {
                days = AttributeDefaults.ExpireAfterDays;
            }

            return now.AddDays( days.Value );
        }

        /// <summary>
        /// Gets the lava resolved string using a text value or text from an attribute value.
        /// </summary>
        /// <param name="attributeKey">The attribute key.</param>
        /// <returns></returns>
        private string GetResolvedLava( string attributeKey )
        {
            var attributeGuid = GetAttributeValue( _action, attributeKey ).AsGuidOrNull();

            // If it's just text then get the text and resolve the lava
            if ( !attributeGuid.HasValue )
            {
                return GetAttributeValue( _action, attributeKey ).ResolveMergeFields( _mergeFields );
            }

            // If it's text within an attribute then resolve using that
            return _action.GetWorkflowAttributeValue( attributeGuid.Value ).ResolveMergeFields( _mergeFields );
        }

        /// <summary>
        /// Gets the requestor person either from the attribute or using person matching if allowed
        /// </summary>
        /// <returns></returns>
        private Person GetRequestor()
        {
            Person person;
            var personAliasGuid = GetGuidFromTextOrAttribute( AttributeKey.Requestor );

            if ( personAliasGuid.HasValue )
            {
                var personAliasService = new PersonAliasService( _rockContext );
                person = personAliasService.GetPerson( personAliasGuid.Value );

                if ( person != null )
                {
                    return person;
                }
            }

            if ( GetBoolean( AttributeKey.IsPersonMatchingEnabled ) != true )
            {
                return null;
            }

            var email = GetTextFromSelectedAttribute( AttributeKey.Email );
            var firstName = GetTextFromSelectedAttribute( AttributeKey.FirstName );
            var lastName = GetTextFromSelectedAttribute( AttributeKey.LastName );

            // Email, first, and last name are all required to do person matching
            if ( email.IsNullOrWhiteSpace() || firstName.IsNullOrWhiteSpace() || lastName.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var personService = new PersonService( _rockContext );
            var query = new PersonService.PersonMatchQuery( firstName, lastName, email, null );
            return personService.FindPerson( query, false, true, false );
        }

        /// <summary>
        /// Gets the unique identifier from text or attribute.
        /// </summary>
        /// <param name="attributeKey">The attribute key.</param>
        /// <returns></returns>
        private Guid? GetGuidFromTextOrAttribute( string attributeKey )
        {
            var value = GetAttributeValue( _action, attributeKey, true );
            var guid = value.AsGuidOrNull();

            if ( !guid.HasValue )
            {
                guid = value.ResolveMergeFields( _mergeFields ).AsGuidOrNull();
            }

            return guid;
        }

        /// <summary>
        /// Get a string value from a workflow attribute
        /// </summary>
        /// <param name="attributeKey"></param>
        /// <returns></returns>
        private string GetTextFromSelectedAttribute( string attributeKey )
        {
            var attributeGuid = GetAttributeValue( _action, attributeKey ).AsGuidOrNull();

            if ( !attributeGuid.HasValue )
            {
                return string.Empty;
            }

            return _action.GetWorkflowAttributeValue( attributeGuid.Value );
        }

        /// <summary>
        /// Get a bool value from a workflow attribute
        /// </summary>
        /// <param name="attributeKey"></param>
        /// <returns></returns>
        private bool? GetBooleanFromSelectedAttribute( string attributeKey )
        {
            return GetTextFromSelectedAttribute( attributeKey ).AsBooleanOrNull();
        }

        /// <summary>
        /// Get a guid value from a workflow attribute
        /// </summary>
        /// <param name="attributeKey"></param>
        /// <returns></returns>
        private Guid? GetGuidFromSelectedAttribute( string attributeKey )
        {
            return GetTextFromSelectedAttribute( attributeKey ).AsGuidOrNull();
        }
    }
}