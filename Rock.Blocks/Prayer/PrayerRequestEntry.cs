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
using System.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.Logging;
using Rock.Model;
using Rock.ViewModels.Blocks.Prayer.PrayerRequestEntry;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Prayer
{
    [DisplayName( "Prayer Request Entry" )]
    [Category( "Prayer" )]
    [Description( "Allows prayer requests to be added via visitors on the website." )]
    //[SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [CategoryField( "Category Selection",
        Description = "A top level category. This controls which categories the person can choose from when entering their prayer request.",
        AllowMultiple = false,
        EntityTypeName = "Rock.Model.PrayerRequest",
        EntityTypeQualifierColumn = "",
        EntityTypeQualifierValue = "",
        IsRequired = false,
        DefaultValue = "",
        Category = AttributeCategory.CategorySelection,
        Order = 0,
        Key = AttributeKey.GroupCategoryId )]

    [CategoryField( "Default Category",
        Description = "If categories are not being shown, choose a default category to use for all new prayer requests.",
        AllowMultiple = false,
        EntityTypeName = "Rock.Model.PrayerRequest",
        EntityTypeQualifierColumn = "",
        EntityTypeQualifierValue = "",
        IsRequired = false,
        DefaultValue = "4B2D88F5-6E45-4B4B-8776-11118C8E8269",
        Category = AttributeCategory.CategorySelection,
        Order = 1,
        Key = AttributeKey.DefaultCategory )]

    [BooleanField( "Enable Auto Approve",
        Description = "If enabled, prayer requests are automatically approved; otherwise they must be approved by an admin before they can be seen by the prayer team.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.Features,
        Order = 2,
        Key = AttributeKey.EnableAutoApprove )]

    [IntegerField( "Expires After (Days)",
        Description = "Number of days until the request will expire (only applies when auto-approve is enabled).",
        IsRequired = false,
        DefaultIntegerValue = 14,
        Category = AttributeCategory.Features,
        Order = 3,
        Key = AttributeKey.ExpireDays )]

    [BooleanField( "Enable Urgent Flag",
        Description = "If enabled, requesters will be able to flag prayer requests as urgent.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.Features,
        Order = 4,
        Key = AttributeKey.EnableUrgentFlag )]

    [BooleanField( "Enable Comments Flag",
        Description = "If enabled, requesters will be able to set whether or not they want to allow comments on their requests.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.Features,
        Order = 5,
        Key = AttributeKey.EnableCommentsFlag )]

    [BooleanField( "Default Allow Comments Setting",
        Description = "This is the default setting for 'Allow Comments' on prayer requests. If the 'Enable Comments Flag' setting is enabled, the requester can override this default setting.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.Features,
        Order = 6,
        Key = AttributeKey.DefaultAllowCommentsSetting )]

    [BooleanField( "Enable Public Display Flag",
        Description = "If enabled, requesters will be able set whether or not they want their request displayed on the public website.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.Features,
        Order = 7,
        Key = AttributeKey.EnablePublicDisplayFlag )]

    [BooleanField( "Default To Public",
        Description = "If enabled, all prayers will be set to public by default",
        DefaultBooleanValue = false,
        Category = AttributeCategory.Features,
        Order = 8,
        Key = AttributeKey.DefaultToPublic )]

    [IntegerField( "Character Limit",
        Description = "If set to something other than 0, this will limit the number of characters allowed when entering a new prayer request.",
        IsRequired = false,
        DefaultIntegerValue = 250,
        Category = AttributeCategory.Features,
        Order = 9,
        Key = AttributeKey.CharacterLimit )]

    [BooleanField( "Require Last Name",
        Description = "Require that a last name be entered.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.Features,
        Order = 10,
        Key = AttributeKey.RequireLastName )]

    [BooleanField( "Show Campus",
        Description = "Should the campus field be displayed? If there is only one active campus then the campus field will not show.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.Features,
        Order = 11,
        Key = AttributeKey.ShowCampus )]

    [BooleanField( "Require Campus",
        Description = "Require that a campus be selected. The campus will not be displayed if there is only one available campus, in which case if this is set to true then the single campus is automatically used.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.Features,
        Order = 12,
        Key = AttributeKey.RequireCampus )]

    [BooleanField( "Enable Person Matching",
        Description = "If enabled, the request will be linked to an existing person if a match can be made between the requester and an existing person.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.Features,
        Order = 13,
        Key = AttributeKey.EnablePersonMatching )]

    [BooleanField( "Create Person If No Match Found",
        Description = "When person matching is enabled this setting determines if a person should be created if a matched record is not found. This setting has no impact if person matching is disabled.",
        DefaultBooleanValue = true,
        Category = AttributeCategory.Features,
        Order = 14,
        Key = AttributeKey.CreatePersonIfNoMatchFound )]

    [DefinedValueField( "Connection Status",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        Description = "The connection status to use when creating new person records.",
        IsRequired = false,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PARTICIPANT,
        Category = AttributeCategory.Features,
        Order = 15,
        Key = AttributeKey.ConnectionStatus )]

    [DefinedValueField( "Record Status",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS,
        Description = "The record status to use when creating new person records.",
        IsRequired = false,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING,
        Category = AttributeCategory.Features,
        Order = 16,
        Key = AttributeKey.RecordStatus )]

    [BooleanField( "Navigate To Parent On Save",
        Description = "If enabled, on successful save control will redirect back to the parent page.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.OnSaveBehavior,
        Order = 17,
        Key = AttributeKey.NavigateToParentOnSave )]

    [BooleanField( "Refresh Page On Save",
        Description = "If enabled, on successful save control will reload the current page. NOTE: This is ignored if 'Navigate to Parent On Save' is enabled.",
        DefaultBooleanValue = false,
        Category = AttributeCategory.OnSaveBehavior,
        Order = 18,
        Key = AttributeKey.RefreshPageOnSave )]

    [CodeEditorField( "Save Success Text",
        Description = "Text to display upon successful save. The 'PrayerRequest' merge field will contain the saved PrayerRequest. (Only applies if not navigating to parent page on save.) <span class='tip tip-lava'></span><span class='tip tip-html'></span>",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "<p>Thank you for allowing us to pray for you.</p>",
        Category = AttributeCategory.OnSaveBehavior,
        Order = 19,
        Key = AttributeKey.SaveSuccessText )]

    [WorkflowTypeField( "Workflow",
        Description = "An optional workflow to start when prayer request is created. The PrayerRequest will be set as the workflow 'Entity' attribute when processing is started.",
        AllowMultiple = false,
        IsRequired = false,
        DefaultValue = "",
        Category = AttributeCategory.OnSaveBehavior,
        Order = 20,
        Key = AttributeKey.Workflow )]

    #endregion

    [ContextAware( typeof( Person ) )]
    [SystemGuid.EntityTypeGuid( "EC1DB1C6-17C2-43A0-8968-10A1DFF7AA03" )]
    [SystemGuid.BlockTypeGuid( "5AA30F53-1B7D-4CA9-89B6-C10592968870" )]

    /// <summary>
    /// Allows prayer requests to be added by visitors on the website.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    public class PrayerRequestEntry : RockBlockType
    {
        #region Categories

        private static class AttributeCategory
        {
            public const string OnSaveBehavior = "On Save Behavior";
            public const string CategorySelection = "Category Selection";
            public const string Features = "Features";
        }

        #endregion

        #region Keys

        /// <summary>
        /// Attribute Key
        /// </summary>
        private static class AttributeKey
        {
            public const string GroupCategoryId = "GroupCategoryId";
            public const string DefaultCategory = "DefaultCategory";

            public const string EnableAutoApprove = "EnableAutoApprove";
            public const string ExpireDays = "ExpireDays";
            public const string DefaultAllowCommentsSetting = "DefaultAllowCommentsSetting";
            public const string EnableUrgentFlag = "EnableUrgentFlag";
            public const string EnableCommentsFlag = "EnableCommentsFlag";
            public const string EnablePublicDisplayFlag = "EnablePublicDisplayFlag";
            public const string DefaultToPublic = "DefaultToPublic";
            public const string CharacterLimit = "CharacterLimit";
            public const string RequireLastName = "RequireLastName";
            public const string ShowCampus = "ShowCampus";
            public const string RequireCampus = "RequireCampus";
            public const string EnablePersonMatching = "EnablePersonMatching";
            public const string CreatePersonIfNoMatchFound = "CreatePersonIfNoMatchFound";
            public const string ConnectionStatus = "ConnectionStatus";
            public const string RecordStatus = "RecordStatus";

            public const string NavigateToParentOnSave = "NavigateToParentOnSave";
            public const string RefreshPageOnSave = "RefreshPageOnSave";

            public const string SaveSuccessText = "SaveSuccessText";
            public const string Workflow = "Workflow";
        }

        private static class MergeFieldKey
        {
            public const string PrayerRequest = "PrayerRequest";
        }

        private static class PageParameterKey
        {
            public const string GroupGuid = "GroupGuid";
            public const string Request = "Request";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the prayer request entity type identifier.
        /// </summary>
        public int? PrayerRequestEntityTypeId => EntityTypeCache.GetId( typeof( PrayerRequest ) );

        #region Block Attributes

        /// <summary>
        /// A top level category. This controls which categories the person can choose from when entering their prayer request.
        /// </summary>
        private Guid? TopLevelCategoryGuid => this.GetAttributeValue( AttributeKey.GroupCategoryId ).AsGuidOrNull();

        /// <summary>
        /// If categories are not being shown, choose a default category to use for all new prayer requests.
        /// </summary>
        private Guid DefaultCategoryGuid => this.GetAttributeValue( AttributeKey.DefaultCategory ).AsGuid();

        /// <summary>
        /// If enabled, prayer requests are automatically approved; otherwise they must be approved by an admin before they can be seen by the prayer team.
        /// </summary>
        private bool IsAutoApproveEnabled => this.GetAttributeValue( AttributeKey.EnableAutoApprove ).AsBoolean();

        /// <summary>
        /// Number of days until the request will expire (only applies when auto-approved is enabled).
        /// </summary>
        private int AutoApproveExpireDays => this.GetAttributeValue( AttributeKey.ExpireDays ).AsInteger();

        /// <summary>
        /// If enabled, requesters will be able to flag prayer requests as urgent.
        /// </summary>
        private bool IsUrgentShown => this.GetAttributeValue( AttributeKey.EnableUrgentFlag ).AsBoolean();

        /// <summary>
        /// If enabled, requesters will be able set whether or not they want to allow comments on their requests.
        /// </summary>
        private bool IsAllowCommentsShown => this.GetAttributeValue( AttributeKey.EnableCommentsFlag ).AsBoolean();

        /// <summary>
        /// This is the default setting for the 'Allow Comments' on prayer requests. If you enable the 'Comments Flag' below, the requester can override this default setting.
        /// </summary>
        private bool AllowCommentsDefaultValue => this.GetAttributeValue( AttributeKey.DefaultAllowCommentsSetting ).AsBoolean();

        /// <summary>
        /// If enabled, requesters will be able set whether or not they want their request displayed on the public website.
        /// </summary>
        private bool IsIsPublicShown => this.GetAttributeValue( AttributeKey.EnablePublicDisplayFlag ).AsBoolean();

        /// <summary>
        /// If enabled, all prayers will be set to public by default.
        /// </summary>
        private bool IsPublicDefaultValue => this.GetAttributeValue( AttributeKey.DefaultToPublic ).AsBoolean();

        /// <summary>
        /// If set to something other than 0, this will limit the number of characters allowed when entering a new prayer request.
        /// </summary>
        private int CharacterLimit => this.GetAttributeValue( AttributeKey.CharacterLimit ).AsInteger();

        /// <summary>
        /// Require that a last name be entered.
        /// </summary>
        private bool IsLastNameRequired => this.GetAttributeValue( AttributeKey.RequireLastName ).AsBoolean();

        /// <summary>
        /// Should the campus field be displayed? If there is only one active campus then the campus field will not show.
        /// </summary>
        private bool IsCampusShown => this.GetAttributeValue( AttributeKey.ShowCampus ).AsBoolean();

        /// <summary>
        /// Require that a campus be selected. The campus will not be displayed if there is only one available campus, in which case if this is set to true then the single campus is automatically used.
        /// </summary>
        private bool IsCampusRequired => this.GetAttributeValue( AttributeKey.RequireCampus ).AsBoolean();

        /// <summary>
        /// If enabled, the request will be linked to an existing person if a match can be made between the requester and an existing person.
        /// </summary>
        private bool IsPersonMatchingEnabled => this.GetAttributeValue( AttributeKey.EnablePersonMatching ).AsBoolean();

        /// <summary>
        /// When person matching is enabled this setting determines if a person should be created if a matched record is not found. This setting has no impact if person matching is disabled.
        /// </summary>
        private bool IsPersonCreatedIfNoMatchFound => this.GetAttributeValue( AttributeKey.CreatePersonIfNoMatchFound ).AsBoolean();

        /// <summary>
        /// The connection status to use when creating new person records.
        /// </summary>
        private Guid ConnectionStatusGuid => this.GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuid();

        /// <summary>
        /// The record status to use when creating new person records.
        /// </summary>
        private Guid RecordStatusGuid => this.GetAttributeValue( AttributeKey.RecordStatus ).AsGuid();

        /// <summary>
        /// If enabled, on successful save control will redirect back to the parent page.
        /// </summary>
        private bool IsPageRedirectedToParentOnSave => this.GetAttributeValue( AttributeKey.NavigateToParentOnSave ).AsBoolean();

        /// <summary>
        /// If enabled, on successful save control will reload the current page. NOTE: This is ignored if 'Navigate to Parent On Save' is enabled.
        /// </summary>
        private bool IsPageRefreshedOnSave => this.GetAttributeValue( AttributeKey.RefreshPageOnSave ).AsBoolean();

        /// <summary>
        /// Lava to display upon successful save. (Only applies if not navigating to parent page on save.)
        /// </value>
        private string SaveSuccessLava => this.GetAttributeValue( AttributeKey.SaveSuccessText );

        /// <summary>
        /// An optional workflow to start when prayer request is created. The PrayerRequest will be set as the workflow 'Entity' attribute when processing is started.
        /// </summary>
        private Guid? WorkflowTypeGuid => this.GetAttributeValue( AttributeKey.Workflow ).AsGuidOrNull();

        #endregion

        #region Page Parameters

        /// <summary>
        /// Gets the group unique identifier page parameter.
        /// </summary>
        private Guid? GroupGuidPageParameter => this.PageParameter( PageParameterKey.GroupGuid ).AsGuidOrNull();

        /// <summary>
        /// 
        /// </summary>
        private string RequestPageParameter => this.PageParameter( PageParameterKey.Request );

        #endregion

        #endregion

        #region IRockObsidianBlockType Implementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return GetInitializationBox();
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Saves the prayer request.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [BlockAction( "Save" )]
        public BlockActionResult Save( PrayerRequestEntrySaveRequestBag bag )
        {
            if ( !IsValid( bag, out var errors ) )
            {
                return ActionOk( new PrayerRequestEntrySaveResponseBag
                {
                    Errors = errors
                } );
            }

            var now = RockDateTime.Now;

            int prayerRequestId = 0;
            var recordStatusDefinedValue = GetDefinedValue( this.RecordStatusGuid );
            var connectionStatusDefinedValue = GetDefinedValue( this.ConnectionStatusGuid );
            var groupGuidPageParameter = this.GroupGuidPageParameter;
            var currentPerson = this.GetCurrentPerson();
            var currentPersonAliasId = currentPerson?.PrimaryAliasId;
            var campusId = bag.CampusGuid.HasValue ? CampusCache.GetId( bag.CampusGuid.Value ) : null;

            using ( var rockContext = new RockContext() )
            {
                var prayerRequest = new PrayerRequest
                {
                    Id = 0,
                    IsActive = true,
                    IsApproved = this.IsAutoApproveEnabled,
                    AllowComments = this.AllowCommentsDefaultValue,
                    EnteredDateTime = now
                };

                // Set the Group for the PrayerRequest from the page parameter.
                if ( groupGuidPageParameter.HasValue )
                {
                    var group = new GroupService( rockContext ).Get( groupGuidPageParameter.Value );

                    if ( group != null )
                    {
                        prayerRequest.GroupId = group.Id;
                        prayerRequest.Group = group;
                    }
                }

                // Auto-approve the request if configured to do so.
                if ( this.IsAutoApproveEnabled )
                {
                    prayerRequest.ApprovedByPersonAliasId = currentPersonAliasId;
                    prayerRequest.ApprovedOnDateTime = now;
                    prayerRequest.ExpirationDate = now.AddDays( this.AutoApproveExpireDays );
                }

                // Now record all the bits...
                // Make sure the Category is hydrated so it's included for any Lava processing.
                Category category;
                var categoryGuid = bag.CategoryGuid;
                var defaultCategoryGuid = this.DefaultCategoryGuid;

                if ( categoryGuid != null )
                {
                    category = new CategoryService( rockContext ).Get( categoryGuid.Value );
                }
                else if ( !defaultCategoryGuid.IsEmpty() )
                {
                    category = new CategoryService( rockContext ).Get( defaultCategoryGuid );
                }
                else
                {
                    category = null;
                }

                prayerRequest.CategoryId = category?.Id;
                prayerRequest.Category = category;

                var personContext = this.RequestContext.GetContextEntity<Person>();
                if ( personContext == null )
                {
                    Person person = null;
                    if ( this.IsPersonMatchingEnabled )
                    {
                        var personService = new PersonService( rockContext );
                        person = personService.FindPerson(
                            new PersonService.PersonMatchQuery( bag.FirstName, bag.LastName, bag.Email, bag.MobilePhoneNumber ),
                            updatePrimaryEmail: false,
                            includeDeceased: true,
                            includeBusinesses: false );

                        var cleanPhoneNumber = PhoneNumber.CleanNumber( bag.MobilePhoneNumber );
                        var isPhoneNumberProvided = cleanPhoneNumber.IsNotNullOrWhiteSpace();
                        var isEmailProvided = bag.Email.IsNotNullOrWhiteSpace();
                        var isPhoneOrEmailProvided = isPhoneNumberProvided || isEmailProvided;

                        if ( person == null && isPhoneOrEmailProvided && this.IsPersonCreatedIfNoMatchFound )
                        {
                            person = new Person
                            {
                                IsSystem = false,
                                RecordTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ),
                                FirstName = bag.FirstName,
                                LastName = bag.LastName,
                                Gender = Gender.Unknown,
                                ConnectionStatusValueId = connectionStatusDefinedValue.Id,
                                RecordStatusValueId = recordStatusDefinedValue.Id
                            };

                            if ( isEmailProvided )
                            {
                                person.Email = bag.Email;
                                person.IsEmailActive = true;
                                person.EmailPreference = EmailPreference.EmailAllowed;
                            }

                            if ( isPhoneNumberProvided )
                            {
                                var phoneNumber = new PhoneNumber
                                {
                                    NumberTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ),
                                    CountryCode = PhoneNumber.CleanNumber( bag.MobilePhoneNumberCountryCode ),
                                    Number = cleanPhoneNumber
                                };

                                person.PhoneNumbers.Add( phoneNumber );
                            }

                            PersonService.SaveNewPerson( person, rockContext, campusId );
                        }
                    }

                    prayerRequest.FirstName = bag.FirstName;
                    prayerRequest.LastName = bag.LastName;
                    prayerRequest.Email = bag.Email;

                    if ( person != null )
                    {
                        prayerRequest.RequestedByPersonAliasId = person.PrimaryAliasId;

                        // If there is no active user, set the CreatedBy field to show that this record was created by the requester.
                        if ( !currentPersonAliasId.HasValue )
                        {
                            prayerRequest.CreatedByPersonAliasId = person.PrimaryAliasId;
                        }
                    }
                    else
                    {
                        prayerRequest.RequestedByPersonAliasId = currentPersonAliasId;
                    }
                }
                else
                {
                    // Update the PrayerRequest with the Person ContextEntity.
                    prayerRequest.RequestedByPersonAliasId = personContext.PrimaryAliasId;
                    prayerRequest.FirstName = personContext.NickName.IsNullOrWhiteSpace() ? personContext.FirstName : personContext.NickName;
                    prayerRequest.LastName = personContext.LastName;
                    prayerRequest.Email = personContext.Email;
                }

                prayerRequest.CampusId = campusId;
                prayerRequest.Text = bag.Request;

                if ( this.IsUrgentShown )
                {
                    prayerRequest.IsUrgent = bag.IsUrgent;
                }
                else
                {
                    prayerRequest.IsUrgent = false;
                }

                if ( this.IsAllowCommentsShown )
                {
                    prayerRequest.AllowComments = bag.AllowComments;
                }

                if ( this.IsIsPublicShown )
                {
                    prayerRequest.IsPublic = bag.IsPublic;
                }
                else
                {
                    prayerRequest.IsPublic = this.IsPublicDefaultValue;
                }

                var prayerRequestService = new PrayerRequestService( rockContext );
                prayerRequestService.Add( prayerRequest );

                // Set the attribute values.
                if ( bag.AttributeValues?.Any() == true )
                {
                    prayerRequest.LoadAttributes( rockContext );
                    prayerRequest.SetPublicAttributeValues( bag.AttributeValues, currentPerson );
                }

                if ( !prayerRequest.IsValid )
                {
                    return ActionOk( new PrayerRequestEntrySaveResponseBag
                    {
                        Errors = prayerRequest.ValidationResults.Select( x => x.ErrorMessage ).ToList()
                    } );
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    prayerRequest.SaveAttributeValues( rockContext );
                } );

                prayerRequestId = prayerRequest.Id;
            }

            using ( var rockContext = new RockContext() )
            {
                // Load the PrayerRequest from a new context to ensure the latest data is available and navigation properties work correctly.
                var prayerRequest = new PrayerRequestService( rockContext ).Get( prayerRequestId );

                StartWorkflow( prayerRequest, rockContext );

                if ( this.IsPageRedirectedToParentOnSave || this.IsPageRefreshedOnSave )
                {
                    return ActionOk( new PrayerRequestEntrySaveResponseBag() );
                }
                else
                {
                    // Build success text that is Lava capable.
                    var mergeFields = this.RequestContext.GetCommonMergeFields( currentPerson );
                    mergeFields.Add( MergeFieldKey.PrayerRequest, prayerRequest );

                    var successMessage = this.SaveSuccessLava.ResolveMergeFields( mergeFields )
                        .Replace( "~~/", this.RequestContext.ResolveRockUrl("~~/") )
                        .Replace( "~/", "/" );

                    return ActionOk( new PrayerRequestEntrySaveResponseBag
                    {
                        SuccessMessage = successMessage
                    } );
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the defined value for a given <paramref name="definedValueGuid"/>.
        /// </summary>
        private DefinedValueCache GetDefinedValue( Guid definedValueGuid )
        {
            return DefinedValueCache.Get( definedValueGuid );
        }

        /// <summary>
        /// Gets the initialization box.
        /// </summary>
        private PrayerRequestEntryInitializationBox GetInitializationBox()
        {
            var currentPerson = this.GetCurrentPerson();

            var box = new PrayerRequestEntryInitializationBox
            {
                AllowCommentsDefaultValue = this.AllowCommentsDefaultValue,
                CharacterLimit = this.CharacterLimit,
                IsAllowCommentsShown = this.IsAllowCommentsShown,
                IsCampusRequired = this.IsCampusRequired,
                IsCampusShown = this.IsCampusShown,
                IsIsPublicShown = this.IsIsPublicShown,
                IsLastNameRequired = this.IsLastNameRequired,
                IsPageRedirectedToParentOnSave = this.IsPageRedirectedToParentOnSave,
                IsPageRefreshedOnSave = this.IsPageRefreshedOnSave,
                IsPublicDefaultValue = this.IsPublicDefaultValue,
                IsRequesterInfoShown = this.RequestContext.GetContextEntity<Person>() == null,
                IsUrgentShown = this.IsUrgentShown,
                ParentPageUrl = this.IsPageRedirectedToParentOnSave ? this.GetParentPageUrl() : null,
                DefaultRequest = this.RequestPageParameter,
                IsMobilePhoneShown = this.IsPersonMatchingEnabled
            };

            // Load the categories. The Category drop down will not be shown if they are not loaded.
            var categoryGuid = this.TopLevelCategoryGuid;

            if ( categoryGuid.HasValue )
            {
                box.Categories = new CategoryService( new RockContext() )
                    .GetByEntityTypeId( this.PrayerRequestEntityTypeId )
                    .Where( c => c.ParentCategory != null && c.ParentCategory.Guid == categoryGuid.Value )
                    .Select( c => new ViewModels.Utility.ListItemBag
                    {
                        Text = c.Name,
                        Value = c.Guid.ToString()
                    } )
                    .ToList();

                // Set the default category.
                var defaultCategoryGuid = this.DefaultCategoryGuid;

                if ( !defaultCategoryGuid.IsEmpty() )
                {
                    box.DefaultCategoryGuid = box.Categories.Select( c => c.Value.AsGuid() ).FirstOrDefault( c => c == defaultCategoryGuid ); 
                }
            }

            // Load the initial field values from the current person.
            if ( currentPerson != null )
            {
                box.DefaultFirstName =  currentPerson.FirstName;
                box.DefaultLastName = currentPerson.LastName;
                box.DefaultEmail = currentPerson.Email;

                var campus = currentPerson.GetCampus();

                if ( campus != null )
                {
                    box.DefaultCampus = campus.ToListItemBag();
                }
            }

            // Load the attributes.
            var prayerRequest = new PrayerRequest { Id = 0 };
            prayerRequest.LoadAttributes();
            box.Attributes = prayerRequest.GetPublicAttributesForEdit( currentPerson );

            return box;
        }

        /// <summary>
        /// Returns true if the request bag appears valid.
        /// <para>Does not validate Person-related values if a new person needs to be created.</para>
        /// </summary>
        private bool IsValid( PrayerRequestEntrySaveRequestBag bag, out List<string> errors )
        {
            var box = GetInitializationBox();

            errors = new List<string>();

            // Check length in case the client side js didn't
            var characterLimit = box.CharacterLimit;
            if ( characterLimit > 0 && bag.Request?.Length > characterLimit )
            {
                errors.Add( $"Whoops. Would you mind reducing the length of your prayer request to {characterLimit} characters?" );
            }

            if ( box.IsLastNameRequired && bag.LastName.IsNullOrWhiteSpace() )
            {
                errors.Add( "Last Name is required." );
            }

            if ( box.IsCampusShown && box.IsCampusRequired && !bag.CampusGuid.HasValue )
            {
                errors.Add( "Campus is required." );
            }

            if ( box.Categories != null && !box.Categories.Any( c => c.Value.AsGuid() == bag.CategoryGuid ) )
            {
                errors.Add( "The selected Category is not allowed." );
            }

            return !errors.Any();
        }

        /// <summary>
        /// Ensure that all of the Lava-accessible navigation properties of a Prayer Request object are populated.
        /// </summary>
        private void PrepareObjectForLavaContext( PrayerRequest request, RockContext rockContext )
        {
            var personAliasService = new PersonAliasService( rockContext );

            if ( request.RequestedByPersonAliasId != null
                && request.RequestedByPersonAlias == null )
            {
                request.RequestedByPersonAlias = personAliasService.Get( request.RequestedByPersonAliasId.Value );
            }

            if ( request.ApprovedByPersonAliasId != null
                && request.ApprovedByPersonAlias == null )
            {
                request.ApprovedByPersonAlias = personAliasService.Get( request.ApprovedByPersonAliasId.Value );
            }

            if ( request.CreatedByPersonAliasId != null
                 && request.CreatedByPersonAlias == null )
            {
                request.CreatedByPersonAlias = personAliasService.Get( request.CreatedByPersonAliasId.Value );
            }

            if ( request.ModifiedByPersonAliasId != null
                 && request.ModifiedByPersonAlias == null )
            {
                request.ModifiedByPersonAlias = personAliasService.Get( request.ModifiedByPersonAliasId.Value );
            }
        }

        /// <summary>
        /// Starts the workflow if one was defined in the block setting.
        /// </summary>
        /// <param name="prayerRequest">The prayer request.</param>
        /// <param name="rockContext">The rock context.</param>
        private void StartWorkflow( PrayerRequest prayerRequest, RockContext rockContext )
        {
            var workflowTypeGuid = this.WorkflowTypeGuid;

            if ( !workflowTypeGuid.HasValue )
            {
                return;
            }

            var workflowType = WorkflowTypeCache.Get( workflowTypeGuid.Value );

            if ( workflowType != null && ( workflowType.IsActive ?? true ) )
            {
                try
                {
                    var workflow = Rock.Model.Workflow.Activate( workflowType, prayerRequest.Name );
                    new WorkflowService( rockContext ).Process( workflow, prayerRequest, out var workflowErrors );
                }
                catch ( Exception ex )
                {
                    RockLogger.Log.Error( RockLogDomains.Prayer, ex, "Unable to start workflow after prayer request was created." );
                }
            }
        }

        #endregion
    }
}
