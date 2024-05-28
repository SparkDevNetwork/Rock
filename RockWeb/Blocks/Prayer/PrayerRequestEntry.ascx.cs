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
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Prayer
{
    [DisplayName( "Prayer Request Entry" )]
    [Category( "Prayer" )]
    [Description( "Allows prayer requests to be added via visitors on the website." )]

    // Category Selection
    [CategoryField( "Category Selection",
        Description = "A top level category. This controls which categories the person can choose from when entering their prayer request.",
        AllowMultiple = false,
        EntityTypeName = "Rock.Model.PrayerRequest",
        EntityTypeQualifierColumn = "",
        EntityTypeQualifierValue = "",
        IsRequired = false,
        DefaultValue = "",
        Category = "Category Selection",
        Order = 1,
        Key = AttributeKey.GroupCategoryId )]

    [CategoryField( "Default Category",
        Description = "If categories are not being shown, choose a default category to use for all new prayer requests.",
        AllowMultiple = false,
        EntityTypeName = "Rock.Model.PrayerRequest",
        EntityTypeQualifierColumn = "",
        EntityTypeQualifierValue = "",
        IsRequired = false,
        DefaultValue = "4B2D88F5-6E45-4B4B-8776-11118C8E8269",
        Category = "Category Selection",
        Order = 2,
        Key = AttributeKey.DefaultCategory )]

    // Features
    [BooleanField( "Enable Auto Approve",
        Description = "If enabled, prayer requests are automatically approved; otherwise they must be approved by an admin before they can be seen by the prayer team.",
        DefaultBooleanValue = true,
        Category = "Features",
        Order = 3,
        Key = AttributeKey.EnableAutoApprove )]

    [IntegerField( "Expires After (Days)",
        Description = "Number of days until the request will expire (only applies when auto-approved is enabled).",
        IsRequired = false,
        DefaultIntegerValue = 14,
        Category = "Features",
        Order = 4,
        Key = AttributeKey.ExpireDays )]

    [BooleanField( "Default Allow Comments Setting",
        Description = "This is the default setting for the 'Allow Comments' on prayer requests. If you enable the 'Comments Flag' below, the requestor can override this default setting.",
        DefaultBooleanValue = true,
        Category = "Features",
        Order = 5,
        Key = AttributeKey.DefaultAllowCommentsSetting )]

    [BooleanField( "Enable Urgent Flag",
        Description = "If enabled, requestors will be able to flag prayer requests as urgent.",
        DefaultBooleanValue = false,
        Category = "Features",
        Order = 6,
        Key = AttributeKey.EnableUrgentFlag )]

    [BooleanField( "Enable Comments Flag",
        Description = "If enabled, requestors will be able set whether or not they want to allow comments on their requests.",
        DefaultBooleanValue = false,
        Category = "Features",
        Order = 7,
        Key = AttributeKey.EnableCommentsFlag )]

    [BooleanField( "Enable Public Display Flag",
        Description = "If enabled, requestors will be able set whether or not they want their request displayed on the public website.",
        DefaultBooleanValue = false,
        Category = "Features",
        Order = 8,
        Key = AttributeKey.EnablePublicDisplayFlag )]

    [BooleanField( "Default To Public",
        Description = "If enabled, all prayers will be set to public by default",
        DefaultBooleanValue = false,
        Category = "Features",
        Order = 9,
        Key = AttributeKey.DefaultToPublic )]

    [IntegerField( "Character Limit",
        Description = "If set to something other than 0, this will limit the number of characters allowed when entering a new prayer request.",
        IsRequired = false,
        DefaultIntegerValue = 250,
        Category = "Features",
        Order = 10,
        Key = AttributeKey.CharacterLimit )]

    [BooleanField( "Require Last Name",
        Description = "Require that a last name be entered",
        DefaultBooleanValue = true,
        Category = "Features",
        Order = 11,
        Key = AttributeKey.RequireLastName )]

    [BooleanField( "Show Campus",
        Description = "Should the campus field be displayed? If there is only one active campus then the campus field will not show.",
        DefaultBooleanValue = true,
        Category = "Features",
        Order = 12,
        Key = AttributeKey.ShowCampus )]

    [BooleanField( "Require Campus",
        Description = "Require that a campus be selected. The campus will not be displayed if there is only one available campus, in which case if this is set to true then the single campus is automatically used.",
        DefaultBooleanValue = false,
        Category = "Features",
        Order = 13,
        Key = AttributeKey.RequireCampus )]

    [DefinedValueField(
        "Campus Types",
        Key = AttributeKey.CampusTypes,
        Description = "This setting filters the list of campuses by type that are displayed in the campus drop-down.",
        IsRequired = false,
        Category = "Features",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        AllowMultiple = true,
        Order = 14 )]

    [DefinedValueField(
        "Campus Statuses",
        Key = AttributeKey.CampusStatuses,
        Description = "This setting filters the list of campuses by statuses that are displayed in the campus drop-down.",
        IsRequired = false,
        Category = "Features",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        AllowMultiple = true,
        Order = 15 )]

    [BooleanField( "Enable Person Matching",
        Description = "If enabled, the request will be linked to an existing person if a match can be made between the requester and an existing person.",
        DefaultBooleanValue = false,
        Category = "Features",
        Order = 16,
        Key = AttributeKey.EnablePersonMatching )]

    [BooleanField( "Create Person If No Match Found",
        Description = "When person matching is enabled this setting determines if a person should be created if a matched record is not found. This setting has no impact if person matching is disabled.",
        DefaultBooleanValue = true,
        Category = "Features",
        Order = 17,
        Key = AttributeKey.CreatePersonIfNoMatchFound )]

    [DefinedValueField( "Connection Status",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        Description = "The connection status to use when creating new person records.",
        IsRequired = false,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PARTICIPANT,
        Category = "Features",
        Order = 18,
        Key = AttributeKey.ConnectionStatus )]

    [DefinedValueField( "Record Status",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS,
        Description = "The record status to use when creating new person records.",
        IsRequired = false,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING,
        Category = "Features",
        Order = 19,
        Key = AttributeKey.RecordStatus )]

    // On Save Behavior
    [BooleanField( "Navigate To Parent On Save",
        Description = "If enabled, on successful save control will redirect back to the parent page.",
        DefaultBooleanValue = false,
        Category = "On Save Behavior",
        Order = 20,
        Key = AttributeKey.NavigateToParentOnSave )]

    [BooleanField( "Refresh Page On Save",
        Description = "If enabled, on successful save control will reload the current page. NOTE: This is ignored if 'Navigate to Parent On Save' is enabled.",
        DefaultBooleanValue = false,
        Category = "On Save Behavior",
        Order = 21,
        Key = AttributeKey.RefreshPageOnSave )]

    [CodeEditorField( "Save Success Text",
        Description = "Text to display upon successful save. (Only applies if not navigating to parent page on save.) <span class='tip tip-lava'></span><span class='tip tip-html'></span>",
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        DefaultValue = "<p>Thank you for allowing us to pray for you.</p>",
        Category = "On Save Behavior",
        Order = 22,
        Key = AttributeKey.SaveSuccessText )]

    [WorkflowTypeField( "Workflow",
        Description = "An optional workflow to start when prayer request is created. The PrayerRequest will be set as the workflow 'Entity' attribute when processing is started.",
        AllowMultiple = false,
        IsRequired = false,
        DefaultValue = "",
        Category = "On Save Behavior",
        Order = 23,
        Key = AttributeKey.Workflow )]

    [ContextAware(typeof(Rock.Model.Person))]
    [Rock.SystemGuid.BlockTypeGuid( "4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6" )]
    public partial class PrayerRequestEntry : RockBlock
    {
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
            public const string CampusTypes = "CampusTypes";
            public const string CampusStatuses = "CampusStatuses";
            public const string EnablePersonMatching = "EnablePersonMatching";
            public const string CreatePersonIfNoMatchFound = "CreatePersonIfNoMatchFound";
            public const string ConnectionStatus = "ConnectionStatus";
            public const string RecordStatus = "RecordStatus";

            public const string NavigateToParentOnSave = "NavigateToParentOnSave";
            public const string RefreshPageOnSave = "RefreshPageOnSave";

            public const string SaveSuccessText = "SaveSuccessText";
            public const string Workflow = "Workflow";
        }

        #endregion

        #region Properties
        public int? PrayerRequestEntityTypeId { get; private set; }

        // note: the ascx uses these for rendering logic
        public bool EnableUrgentFlag { get; private set; }
        public bool EnableCommentsFlag { get; private set; }
        public bool EnablePublicDisplayFlag { get; private set; }
        public bool DefaultToPublic { get; private set; }

        #endregion

        #region Keys

        private static class PageParameterKey
        {
            public const string GroupGuid = "GroupGuid";
            public const string Request = "Request";
        }

        #endregion

        #region Base Control Methods
        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += PrayerRequestEntry_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upAdd );

            pnlRequester.Visible = this.ContextEntity<Rock.Model.Person>() == null;

            RockContext rockContext = new RockContext();

            this.EnableUrgentFlag = GetAttributeValue( AttributeKey.EnableUrgentFlag ).AsBoolean();
            this.EnableCommentsFlag = GetAttributeValue( AttributeKey.EnableCommentsFlag ).AsBoolean();
            this.EnablePublicDisplayFlag = GetAttributeValue( AttributeKey.EnablePublicDisplayFlag ).AsBoolean();
            this.DefaultToPublic = GetAttributeValue( AttributeKey.DefaultToPublic ).AsBoolean();
            tbLastName.Required = GetAttributeValue( AttributeKey.RequireLastName ).AsBooleanOrNull() ?? true;

            cpCampus.Campuses = CampusCache.All( false );
            cpCampus.Required = GetAttributeValue( AttributeKey.RequireCampus ).AsBoolean();

            var selectedCampusTypeIds = GetAttributeValue( AttributeKey.CampusTypes )
                .SplitDelimitedValues( true )
                .AsGuidList()
                .Select( a => DefinedValueCache.Get( a ) )
                .Where( a => a != null )
                .Select( a => a.Id )
                .ToList();

            if ( selectedCampusTypeIds.Any() )
            {
                cpCampus.CampusTypesFilter = selectedCampusTypeIds;
            }

            var selectedCampusStatusIds = GetAttributeValue( AttributeKey.CampusStatuses )
                .SplitDelimitedValues( true )
                .AsGuidList()
                .Select( a => DefinedValueCache.Get( a ) )
                .Where( a => a != null )
                .Select( a => a.Id )
                .ToList();

            if ( selectedCampusStatusIds.Any() )
            {
                cpCampus.CampusStatusFilter = selectedCampusStatusIds;
            }

            if ( cpCampus.Visible )
            {
                cpCampus.Visible = GetAttributeValue( AttributeKey.ShowCampus ).AsBoolean();
            }

            if ( EnableCommentsFlag )
            {
                cbAllowComments.Checked = GetAttributeValue( AttributeKey.DefaultAllowCommentsSetting ).AsBoolean();
            }

            pnbPhone.Visible = GetAttributeValue( AttributeKey.EnablePersonMatching ).AsBoolean();

            var categoryGuid = GetAttributeValue( AttributeKey.GroupCategoryId );
            if ( ! string.IsNullOrEmpty( categoryGuid ) )
            {
                BindCategories( categoryGuid );

                // set the default category
                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.DefaultCategory ) ) )
                {

                    Guid defaultCategoryGuid = GetAttributeValue( AttributeKey.DefaultCategory ).AsGuid();
                    var defaultCategoryId = CategoryCache.Get( defaultCategoryGuid, rockContext ).Id;

                    bddlCategory.SetValue( defaultCategoryId );
                }
            }
            else
            {
                bddlCategory.Visible = false;
            }

            Type type = new PrayerRequest().GetType();
            this.PrayerRequestEntityTypeId = EntityTypeCache.GetId( type.FullName );

            int charLimit = GetAttributeValue( AttributeKey.CharacterLimit ).AsInteger();
            if ( charLimit > 0 )
            {
                dtbRequest.Placeholder = string.Format( "Please pray that... (up to {0} characters)", charLimit );
                string scriptFormat = @"
    function SetCharacterLimit() {{
        $('#{0}').limit({{maxChars: {1}, counter:'#{2}', normalClass:'badge', warningClass:'badge-warning', overLimitClass: 'badge-danger'}});

        $('#{0}').on('cross', function(){{
            $('#{3}').prop('disabled', true);
        }});
        $('#{0}').on('uncross', function(){{
            $('#{3}').prop('disabled', false);
        }});
    }};
    $(document).ready(function () {{ SetCharacterLimit(); }});
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(SetCharacterLimit);
";
                string script = string.Format(scriptFormat , dtbRequest.ClientID, charLimit, lblCount.ClientID, lbSave.ClientID );
                ScriptManager.RegisterStartupScript( this.Page, this.GetType(), string.Format( "limit-{0}", this.ClientID ), script, true );
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the PrayerRequestEntry control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void PrayerRequestEntry_BlockUpdated( object sender, EventArgs e )
        {
            // reload the page to make sure we get fresh settings
            this.NavigateToCurrentPage();
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                if ( CurrentPerson != null )
                {
                    tbFirstName.Text = CurrentPerson.FirstName;
                    tbLastName.Text = CurrentPerson.LastName;
                    tbEmail.Text = CurrentPerson.Email;
                    cpCampus.SetValue( CurrentPerson.GetCampus() );
                }

                dtbRequest.Text = PageParameter( PageParameterKey.Request );
                cbAllowPublicDisplay.Checked = this.DefaultToPublic;


                var prayerRequest = new PrayerRequest { Id = 0 };
                prayerRequest.LoadAttributes();
                avcEditAttributes.ExcludedAttributes = prayerRequest.Attributes.Where( a => !a.Value.IsPublic || !a.Value.IsAuthorized( Rock.Security.Authorization.EDIT, this.CurrentPerson ) ).Select( a => a.Value ).ToArray();
                avcEditAttributes.AddEditControls( prayerRequest );
                avcEditAttributes.ValidationGroup = this.BlockValidationGroup;
            }
        }

        #endregion

        #region Events

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event to save the prayer request.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( ! IsValid() )
            {
                return;
            }

            bool isAutoApproved = GetAttributeValue( AttributeKey.EnableAutoApprove ).AsBoolean();
            bool defaultAllowComments = GetAttributeValue( AttributeKey.DefaultAllowCommentsSetting ).AsBoolean();
            bool isPersonMatchingEnabled = GetAttributeValue( AttributeKey.EnablePersonMatching ).AsBoolean();
            Guid? groupGuid = PageParameter( PageParameterKey.GroupGuid ).AsGuidOrNull();
            bool isCreatePersonIfNoMatchFoundEnabled = GetAttributeValue( AttributeKey.CreatePersonIfNoMatchFound ).AsBoolean();
            var dvcRecordStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordStatus ).AsGuid() );
            var dvcConnectionStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuid() );

            PrayerRequest prayerRequest = new PrayerRequest { Id = 0, IsActive = true, IsApproved = isAutoApproved, AllowComments = defaultAllowComments };

            var rockContext = new RockContext();
            prayerRequest.EnteredDateTime = RockDateTime.Now;

            if ( groupGuid != null )
            {
                Group group = new GroupService( rockContext ).Get( groupGuid.Value );

                if ( group != null )
                {
                    prayerRequest.GroupId = group.Id;
                    prayerRequest.Group = group;
                }
            }

            if ( isAutoApproved )
            {
                prayerRequest.ApprovedByPersonAliasId = CurrentPersonAliasId;
                prayerRequest.ApprovedOnDateTime = RockDateTime.Now;
                var expireDays = Convert.ToDouble( GetAttributeValue( AttributeKey.ExpireDays ) );
                prayerRequest.ExpirationDate = RockDateTime.Now.AddDays( expireDays );
            }

            // Now record all the bits...
            // Make sure the Category is hydrated so it's included for any Lava processing
            Category category;
            int? categoryId = bddlCategory.SelectedValueAsInt();
            Guid defaultCategoryGuid = GetAttributeValue( AttributeKey.DefaultCategory ).AsGuid();
            if ( categoryId == null && !defaultCategoryGuid.IsEmpty() )
            {
                category = new CategoryService( rockContext ).Get( defaultCategoryGuid );
                categoryId = category.Id;
            }
            else
            {
                category = new CategoryService( rockContext ).Get( categoryId.Value );
            }

            prayerRequest.CategoryId = categoryId;
            prayerRequest.Category = category;

            var personContext = this.ContextEntity<Person>();
            if ( personContext == null )
            {
                Person person = null;
                if ( isPersonMatchingEnabled )
                {
                    var personService = new PersonService( new RockContext() );
                    person = personService.FindPerson( new PersonService.PersonMatchQuery( tbFirstName.Text, tbLastName.Text, tbEmail.Text, pnbPhone.Number ), false, true, false );

                    bool isPhoneOrEmailProvided =  !string.IsNullOrWhiteSpace( tbEmail.Text ) || !string.IsNullOrWhiteSpace( PhoneNumber.CleanNumber( pnbPhone.Number ) ) ;
                    if ( person == null && isCreatePersonIfNoMatchFoundEnabled &&  isPhoneOrEmailProvided )
                    {
                        var personRecordTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;

                        person = new Person();
                        person.IsSystem = false;
                        person.RecordTypeValueId = personRecordTypeId;
                        person.FirstName = tbFirstName.Text;
                        person.LastName = tbLastName.Text;
                        person.Gender = Gender.Unknown;
                        person.ConnectionStatusValueId = dvcConnectionStatus.Id;
                        person.RecordStatusValueId = dvcRecordStatus.Id;

                        if ( !string.IsNullOrWhiteSpace( tbEmail.Text ) )
                        {
                            person.Email = tbEmail.Text;
                            person.IsEmailActive = true;
                            person.EmailPreference = EmailPreference.EmailAllowed;
                        }

                        PersonService.SaveNewPerson( person, rockContext, cpCampus.SelectedCampusId );

                        if ( !string.IsNullOrWhiteSpace( PhoneNumber.CleanNumber( pnbPhone.Number ) ) )
                        {
                            var mobilePhoneType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) );

                            var phoneNumber = new PhoneNumber { NumberTypeValueId = mobilePhoneType.Id };
                            phoneNumber.CountryCode = PhoneNumber.CleanNumber( pnbPhone.CountryCode );
                            phoneNumber.Number = PhoneNumber.CleanNumber( pnbPhone.Number );
                            person.PhoneNumbers.Add( phoneNumber );
                        }
                    }
                }

                prayerRequest.FirstName = tbFirstName.Text;
                prayerRequest.LastName = tbLastName.Text;
                prayerRequest.Email = tbEmail.Text;
                if ( person != null )
                {
                    prayerRequest.RequestedByPersonAliasId = person.PrimaryAliasId;

                    // If there is no active user, set the CreatedBy field to show that this record was created by the requester.
                    if ( CurrentPersonAliasId == null )
                    {
                        prayerRequest.CreatedByPersonAliasId = person.PrimaryAliasId;
                    }
                }
                else
                {
                    prayerRequest.RequestedByPersonAliasId = CurrentPersonAliasId;
                }
            }
            else
            {
                prayerRequest.RequestedByPersonAliasId = personContext.PrimaryAliasId;
                prayerRequest.FirstName = string.IsNullOrEmpty( personContext.NickName ) ? personContext.FirstName : personContext.NickName;
                prayerRequest.LastName = personContext.LastName;
                prayerRequest.Email = personContext.Email;
            }

            prayerRequest.CampusId = cpCampus.SelectedCampusId;

            prayerRequest.Text = dtbRequest.Text;

            if ( this.EnableUrgentFlag )
            {
                prayerRequest.IsUrgent = cbIsUrgent.Checked;
            }
            else
            {
                prayerRequest.IsUrgent = false;
            }

            if ( this.EnableCommentsFlag )
            {
                prayerRequest.AllowComments = cbAllowComments.Checked;
            }

            if ( this.EnablePublicDisplayFlag )
            {
                prayerRequest.IsPublic = cbAllowPublicDisplay.Checked;
            }
            else
            {
                prayerRequest.IsPublic = this.DefaultToPublic;
            }

            if ( !Page.IsValid )
            {
                return;
            }

            PrayerRequestService prayerRequestService = new PrayerRequestService( rockContext );
            prayerRequestService.Add( prayerRequest );
            prayerRequest.LoadAttributes( rockContext );
            avcEditAttributes.GetEditValues( prayerRequest );

            if ( !prayerRequest.IsValid )
            {
                // field controls render error messages
                return;
            }

            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();
                prayerRequest.SaveAttributeValues( rockContext );
            } );

            PrepareObjectForLavaContext( prayerRequest, rockContext );

            StartWorkflow( prayerRequest, rockContext );

            bool isNavigateToParent = GetAttributeValue( AttributeKey.NavigateToParentOnSave ).AsBoolean();

            if ( isNavigateToParent )
            {
                NavigateToParentPage();
            }
            else if (GetAttributeValue( AttributeKey.RefreshPageOnSave ).AsBoolean() )
            {
                NavigateToCurrentPage( this.PageParameters().Where(a => a.Value is string).ToDictionary( k => k.Key, v => v.Value.ToString()) );
            }
            else
            {
                pnlForm.Visible = false;
                pnlReceipt.Visible = true;

                // Build success text that is Lava capable
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                mergeFields.Add( "PrayerRequest", prayerRequest );
                nbMessage.Text = GetAttributeValue( AttributeKey.SaveSuccessText ).ResolveMergeFields( mergeFields );

                // Resolve any dynamic url references
                string appRoot = ResolveRockUrl( "~/" );
                string themeRoot = ResolveRockUrl( "~~/" );
                nbMessage.Text = nbMessage.Text.Replace( "~~/", themeRoot ).Replace( "~/", appRoot );

            }
        }

        /// <summary>
        /// Ensure that all of the Lava-accessible navigation properties of a Prayer Request object are populated.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="rockContext"></param>
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
        /// Set up the form for another request from the same person.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnAddAnother_Click( object sender, EventArgs e )
        {
            pnlForm.Visible = true;
            pnlReceipt.Visible = false;
            dtbRequest.Text = string.Empty;
            cbAllowPublicDisplay.Checked = this.DefaultToPublic;
            dtbRequest.Focus();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Bind the category selector to the correct set of categories.
        /// </summary>
        private void BindCategories( string categoryGuid )
        {
            Guid guid = new Guid( categoryGuid );

            bddlCategory.DataSource = new CategoryService( new RockContext() ).GetByEntityTypeId( this.PrayerRequestEntityTypeId )
                .Where( c => ( c.ParentCategory != null && c.ParentCategory.Guid == guid ) ).AsQueryable().ToList();
            bddlCategory.DataTextField = "Name";
            bddlCategory.DataValueField = "Id";
            bddlCategory.DataBind();
        }

        /// <summary>
        /// Returns true if the form is valid; false otherwise.
        /// </summary>
        /// <returns></returns>
        private bool IsValid()
        {
            IEnumerable<string> errors = Enumerable.Empty<string>();

            // Check length in case the client side js didn't
            int charLimit = GetAttributeValue( AttributeKey.CharacterLimit ).AsInteger();
            if ( charLimit > 0  && dtbRequest.Text.Length > charLimit )
            {
                errors = errors.Concat( new[] { string.Format( "Whoops. Would you mind reducing the length of your prayer request to {0} characters?", charLimit ) } );
            }

            if ( errors != null && errors.Count() > 0 )
            {
                nbWarningMessage.Visible = true;
                nbWarningMessage.Text = errors.Aggregate( new StringBuilder( "<ul>" ), ( sb, s ) => sb.AppendFormat( "<li>{0}</li>", s ) ).Append( "</ul>" ).ToString();
                return false;
            }
            else
            {
                nbWarningMessage.Visible = false;
                return true;
            }
        }

        /// <summary>
        /// Starts the workflow if one was defined in the block setting.
        /// </summary>
        /// <param name="prayerRequest">The prayer request.</param>
        /// <param name="rockContext">The rock context.</param>
        private void StartWorkflow( PrayerRequest prayerRequest, RockContext rockContext )
        {
            Guid? workflowTypeGuid = GetAttributeValue( AttributeKey.Workflow ).AsGuidOrNull();
            if ( workflowTypeGuid.HasValue )
            {
                var workflowType = WorkflowTypeCache.Get( workflowTypeGuid.Value );
                if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                {
                    try
                    {
                        var workflow = Workflow.Activate( workflowType, prayerRequest.Name );
                        List<string> workflowErrors;
                        new WorkflowService( rockContext ).Process( workflow, prayerRequest, out workflowErrors );
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( ex, this.Context );
                    }
                }
            }
        }

        #endregion
    }
}