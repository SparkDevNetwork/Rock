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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Net;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Humanizer;
using RestSharp;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// The main Person Profile block the main information about a person
    /// </summary>
    [DisplayName( "Person Bio" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Person biographic/demographic information and picture (Person detail page)." )]

    [SecurityAction( SecurityActionKey.ViewProtectionProfile, "The roles and/or users that can view the protection profile alert for the selected person." )]

    #region Block Attributes

    [BadgesField(
        "Badges",
        Key = AttributeKey.Badges,
        Description = "The label badges to display in this block.",
        IsRequired = false,
        Order = 0 )]

    [WorkflowTypeField(
        "Workflow Actions",
        Key = AttributeKey.WorkflowActions,
        Description = "The workflows to make available as actions.",
        AllowMultiple = true,
        IsRequired = false,
        Order = 1 )]

    [CodeEditorField(
        "Additional Custom Actions",
        Key = AttributeKey.AdditionalCustomActions,
        Description = BlockAttributeDescription.AdditionalCustomActions,
        EditorMode = CodeEditorMode.Html,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        Order = 2 )]

    [BooleanField(
        "Enable Impersonation",
        Key = AttributeKey.EnableImpersonation,
        Description = "Should the Impersonate custom action be enabled? Note: If enabled, it is only visible to users that are authorized to administrate the person.",
        DefaultBooleanValue = false,
        Order = 3 )]

    [LinkedPage(
        "Impersonation Start Page",
        Key = AttributeKey.ImpersonationStartPage,
        Description = "The page to navigate to after clicking the Impersonate action.",
        IsRequired = false,
        Order = 4 )]

    [LinkedPage(
        "Business Detail Page",
        Key = AttributeKey.BusinessDetailPage,
        Description = "The page to redirect user to if a business is requested.",
        IsRequired = false,
        Order = 5 )]

    [LinkedPage(
        "Nameless Person Detail Page",
        Key = AttributeKey.NamelessPersonDetailPage,
        Description = "The page to redirect user to if the person record is a Nameless Person record type.",
        IsRequired = false,
        Order = 6 )]

    [BooleanField(
        "Display Country Code",
        Key = AttributeKey.DisplayCountryCode,
        Description = "When enabled prepends the country code to all phone numbers.",
        DefaultBooleanValue = false,
        Order = 7 )]

    [BooleanField( "Display Middle Name",
        Key = AttributeKey.DisplayMiddleName,
        Description = "Display the middle name of the person.",
        DefaultBooleanValue = false,
        Order = 8 )]

    [CodeEditorField(
        "Custom Content",
        Key = AttributeKey.CustomContent,
        Description = "Custom Content will be rendered after the person's demographic information <span class='tip tip-lava'></span>.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = false,
        Order = 9 )]

    [BooleanField(
        "Allow Following",
        Key = AttributeKey.AllowFollowing,
        Description = "Should people be able to follow a person by selecting the following badge?",
        DefaultBooleanValue = true,
        Order = 10 )]

    [BooleanField(
        "Display Graduation",
        Key = AttributeKey.DisplayGraduation,
        Description = "Should the Grade/Graduation be displayed?",
        DefaultBooleanValue = true,
        Order = 11 )]

    [BooleanField(
        "Display Anniversary Date",
        Key = AttributeKey.DisplayAnniversaryDate,
        Description = "Should the Anniversary Date be displayed?",
        DefaultBooleanValue = true,
        Order = 12 )]

    [AttributeCategoryField(
        "Social Media Category",
        Key = AttributeKey.SocialMediaCategory,
        Description = "The Attribute Category to display attributes from.",
        AllowMultiple = false,
        EntityType = typeof( Rock.Model.Person ),
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.Category.PERSON_ATTRIBUTES_SOCIAL,
        Order = 13 )]

    [BooleanField(
        "Enable Call Origination",
        Key = AttributeKey.EnableCallOrigination,
        Description = "Should click-to-call links be added to phone numbers.",
        DefaultBooleanValue = true,
        Order = 14 )]

    [LinkedPage(
        "Communication Page",
        Key = AttributeKey.CommunicationPage,
        Description = "The communication page to use when the email button or person's email address is clicked. Leave this blank to use the default.",
        IsRequired = false,
        Order = 15 )]

    [LinkedPage(
        "SMS Page",
        Key = AttributeKey.SmsPage,
        Description = "The communication page to use when the text button is clicked. Leave this blank to use the default.",
        IsRequired = false,
        Order = 16 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "030CCDDC-8D43-40F8-A298-78B416F9E828" )]
    public partial class Bio : PersonBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string Badges = "Badges";
            public const string WorkflowActions = "WorkflowActions";
            public const string AdditionalCustomActions = "Actions";
            public const string EnableImpersonation = "EnableImpersonation";
            public const string ImpersonationStartPage = "ImpersonationStartPage";
            public const string BusinessDetailPage = "BusinessDetailPage";
            public const string NamelessPersonDetailPage = "NamelessPersonDetailPage";
            public const string DisplayCountryCode = "DisplayCountryCode";
            public const string DisplayMiddleName = "DisplayMiddleName";
            public const string CustomContent = "CustomContent";
            public const string AllowFollowing = "AllowFollowing";
            public const string DisplayTags = "DisplayTags";
            public const string DisplayGraduation = "DisplayGraduation";
            public const string DisplayAnniversaryDate = "DisplayAnniversaryDate";
            public const string TagCategory = "TagCategory";
            public const string SocialMediaCategory = "SocialMediaCategory";
            public const string EnableCallOrigination = "EnableCallOrigination";
            public const string CommunicationPage = "CommunicationPage";
            public const string SmsPage = "SmsPage";
        }

        private static class PageParameterKey
        {
            public const string PersonId = "PersonId";
            public const string BusinessId = "BusinessId";
            public const string NamelessPersonId = "NamelessPersonId";
        }

        private static class BlockAttributeDescription
        {
            public const string AdditionalCustomActions = @"
Additional custom actions (will be displayed after the list of workflow actions). Any instance of '{0}' will be replaced with the current person's id.
Because the contents of this setting will be rendered inside a &lt;ul&gt; element, it is recommended to use an
&lt;li&gt; element for each available action.  Example:
<pre>
    &lt;li&gt;&lt;a href='~/WorkflowEntry/4?PersonId={0}' tabindex='0'&gt;Fourth Action&lt;/a&gt;&lt;/li&gt;
</pre>";
        }

        #endregion Attribute Keys

        #region Security Actions

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class SecurityActionKey
        {
            public const string ViewProtectionProfile = "ViewProtectionProfile";
        }

        #endregion

        #region Fields

        private const string NAME_KEY = "name";
        private const string ICONCSSCLASS_KEY = "iconcssclass";
        private const string COLOR_KEY = "color";

        #endregion Fields

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddCSSLink( "~/Styles/fluidbox.css" );
            RockPage.AddScriptLink( "~/Scripts/imagesloaded.min.js" );
            RockPage.AddScriptLink( "~/Scripts/jquery.fluidbox.min.js" );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( pnlContent );

            if ( Person == null )
            {
                return;
            }

            // If this is the empty person, check for an old Alias record and if found, redirect permanent to it instead.
            if ( Person.Id == 0 )
            {
                //referring to aliasPersonId as person might be merged
                var personId = this.PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull();

                if ( personId.HasValue )
                {
                    var personAlias = new PersonAliasService( new RockContext() ).GetByAliasId( personId.Value );
                    if ( personAlias != null )
                    {
                        var pageReference = RockPage.PageReference;
                        pageReference.Parameters.AddOrReplace( PageParameterKey.PersonId, personAlias.PersonId.ToString() );
                        Response.RedirectPermanent( pageReference.BuildUrl(), false );
                    }
                }
            }

            // Record Type - this is always "business". it will never change.
            if ( Person.IsBusiness() )
            {
                var parms = new Dictionary<string, string>();
                parms.Add( PageParameterKey.BusinessId, Person.Id.ToString() );
                NavigateToLinkedPage( AttributeKey.BusinessDetailPage, parms );
            }
            else if ( Person.IsNameless() )
            {
                var parms = new Dictionary<string, string>();
                parms.Add( PageParameterKey.NamelessPersonId, Person.Id.ToString() );
                NavigateToLinkedPage( AttributeKey.NamelessPersonDetailPage, parms );
            }

            if ( Person.IsDeceased )
            {
                pnlContent.Attributes.Add( "class", "card card-profile card-profile-bio deceased" );
            }
            else
            {
                pnlContent.Attributes.Add( "class", "card card-profile card-profile-bio" );
            }

            // Set the browser page title to include person's name
            RockPage.BrowserTitle = Person.FullName;

            ShowProtectionLevel();
            ShowBadgeList();

            divEditButton.Visible = IsUserAuthorized( Rock.Security.Authorization.EDIT );

            // only show if the when all these are true
            //   -- EnableImpersonation is enabled
            //   -- Not the same as the current person
            //   -- The current user is authorized to Administrate the person
            //   -- PersonToken usage is allowed on the person (due to AccountProtectionProfile)

            bool enableImpersonation = this.GetAttributeValue( AttributeKey.EnableImpersonation ).AsBoolean();
            lbImpersonate.Visible = false;
            if ( enableImpersonation && Person.Id != CurrentPersonId && Person.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, this.CurrentPerson ) )
            {
                // Impersonate for anybody that has Token Usage Allowed. If this Person doesn't have TokenUsage allowed
                // and the logged-in user would normally see an Impersonate button disabled the button.
                lbImpersonate.Visible = true;
                lbImpersonate.Enabled = Person.IsPersonTokenUsageAllowed() == true;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                // dont' show if there isn't a person, or if it is a 'Nameless" person record type
                if ( Person == null || Person.Id == 0 || Person.RecordTypeValueId == DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS.AsGuid() ) )
                {
                    pnlContent.Visible = false;
                    return;
                }

                string quickReturnLava = "{{ Person.FullName | AddQuickReturn:'People', 10 }}";
                var quickReturnMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
                quickReturnMergeFields.Add( "Person", Person );
                quickReturnLava.ResolveMergeFields( quickReturnMergeFields );

                RegisterClientScript();
                ShowPersonImage();
                ShowPersonName();
                CheckForNamePronunciationOverride();
                CreatePronunciationDiv();
                ShowFollowingButton();
                ShowSmsButton();
                ShowEmailButton();
                CreateActionMenu();
                ShowDemographicsInfo();
                ShowPhoneInfo();
                ShowEmailText();

                divContactSection.Visible = lEmail.Text.IsNotNullOrWhiteSpace() || rptPhones.Visible == true;

                ShowSocialMediaButtons();
                ShowCustomContent();
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            // reload the page if block settings where changed
            Response.Redirect( Request.RawUrl, false );
            Context.ApplicationInstance.CompleteRequest();
        }

        #endregion Base Control Methods

        #region Control Events

        /// <summary>
        /// Handles the Click event of the lbEditPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEditPerson_Click( object sender, EventArgs e )
        {
            if ( Person != null )
            {
                Response.Redirect( string.Format( "~/Person/{0}/Edit", Person.Id ), false );
            }
        }

        protected void lbImpersonate_Click( object sender, EventArgs e )
        {
            if ( Person != null )
            {
                if ( Person.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, this.CurrentPerson ) )
                {
                    if ( !this.Person.IsPersonTokenUsageAllowed() )
                    {
                        // we hide/disable the lbImpersonate in this situation, but prevent just in case
                        return;
                    }

                    var impersonationToken = this.Person.GetImpersonationToken( RockDateTime.Now.AddMinutes( 5 ), 1, null );

                    // store the current user in Session["ImpersonatedByUser"] so that we can log back in as them from the Admin Bar
                    Session["ImpersonatedByUser"] = this.CurrentUser;

                    var qryParams = new Dictionary<string, string>();
                    qryParams.Add( "rckipid", impersonationToken );
                    if ( !string.IsNullOrEmpty( this.GetAttributeValue( AttributeKey.ImpersonationStartPage ) ) )
                    {
                        NavigateToLinkedPage( AttributeKey.ImpersonationStartPage, qryParams );
                    }
                    else
                    {
                        NavigateToCurrentPageReference( qryParams );
                    }
                }
            }
        }

        protected void rptPhones_ItemDataBound( object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem )
            {
                return;
            }

            if ( !( e.Item.DataItem is PhoneNumber phoneNumber ) )
            {
                return;
            }
            var originationEnabled = GetAttributeValue( AttributeKey.EnableCallOrigination ).AsBoolean();
            var showCountryCode = GetAttributeValue( AttributeKey.DisplayCountryCode ).AsBoolean();

            var phoneType = DefinedValueCache.Get( phoneNumber.NumberTypeValueId ?? 0 ).Value;
            var messaging = phoneNumber.IsMessagingEnabled ? @"<i class=""fa fa-comment text-muted text-sm ml-1""></i>" : string.Empty;
            string formattedNumber = phoneNumber.IsUnlisted ? "Unlisted" : PhoneNumber.FormattedNumber( phoneNumber.CountryCode, phoneNumber.Number, showCountryCode );

            var phoneMarkup = formattedNumber;
            var phoneEnabledClass = originationEnabled ? "orig-enabled" : "orig-disabled";

            if ( e.Item.FindControl( "litPhoneNumber" ) is Literal litPhoneNumber )
            {
                if ( originationEnabled )
                {
                    var pbxComponent = Rock.Pbx.PbxContainer.GetAllowedActiveComponentWithOriginationSupport( CurrentPerson );
                    if ( pbxComponent != null )
                    {
                        var jsScript = $"javascript: Rock.controls.pbx.originate('{CurrentPerson.Guid}', '{phoneNumber.Number}', '{CurrentPerson.FullName}','{Person.FullName}','{formattedNumber}');";
                        phoneMarkup = $"<a class='originate-call js-originate-call text-link stretched-link' href=\"{jsScript}\">{formattedNumber}</a>";
                    }
                    else if ( RockPage.IsMobileRequest ) // if the page is being loaded locally then add the tel:// link
                    {
                        phoneMarkup = $@"<a href=""tel://{phoneNumber.Number}"" class=""text-link stretched-link"">{formattedNumber}</a>";
                    }
                }

                litPhoneNumber.Text = $@"
                    <div class=""btn-phone"">
                        <dl class=""reversed-label"">
                            <dt>
                                {phoneMarkup}
                                {messaging}
                            </dt>
                            <dd>{phoneType}</dd>
                        </dl>
                        <span class=""profile-row-icon"">
                            <i class=""fa fa-phone""></i>
                        </span>
                    </div>";
            }
        }

        protected void lbFollowing_Click( object sender, EventArgs e )
        {
            var personAliasEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.PERSON_ALIAS ).Value;

            using ( var rockContext = new RockContext() )
            {
                var followingService = new FollowingService( rockContext );
                followingService.ToggleFollowing( personAliasEntityTypeId, Person.PrimaryAliasId.Value, CurrentPerson.PrimaryAliasId.Value );
                rockContext.SaveChanges();
            }

            ShowFollowingButton();
        }

        /// <summary>
        /// Handles the Click event of the lbOpenNamePronunciationPanel control. 
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public static bool firstTimePostingNamePronunciations;
        protected void lbOpenNamePronunciationPanel_Click( object sender, EventArgs e )
        {
            if ( Person != null )
            {
                // Check if the Name Pronunciation Panel is opened or close and then open or close it.
                if ( divNamePronunciationPanel.Visible == true )
                {
                    divNamePronunciationPanel.Visible = false;
                }
                else
                {
                    divNamePronunciationPanel.Visible = true;
                }        
            }
        }

        /// <summary>
        /// Handles the Click event of the lbEditNamePronunciation control. 
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEditNamePronunciation_Click( object sender, EventArgs e )
        {
            if ( Person != null )
            {
                dlgNamePronunciation.Show();
            }
        }

        /// <summary>
        /// Handles the Click event of the dlgSavePronunciation control. 
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgSavePronunciation_Click(object sender, EventArgs e)
        {
            using (var rockContext = new RockContext() )
            {
                if(Person != null )
                {
                    // Find the current person in the database.
                    var personService = new PersonService( rockContext );
                    var person = personService.Queryable()
                                        .FirstOrDefault( o => o.FirstName.Equals( Person.FirstName )
                                            && o.NickName.Equals( Person.NickName )
                                            && o.LastName.Equals( Person.LastName ) );

                    // If the name in the first name area is different than what is in the database, save the new data.
                    if (!(tbFirstName.Value.Equals(Person.FirstNamePronunciationOverride)) && tbFirstName.Value.IsNotNullOrWhiteSpace()
                        && !( tbFirstName.Value.Equals( spFirstNamePronunciation.InnerText ) ) )
                    {
                        // Set Values.
                        person.FirstNamePronunciationOverride = tbFirstName.Value;
                        spFirstNamePronunciation.InnerText = tbFirstName.Value;
                        spFirstNamePronunciation.Attributes["onclick"] = "";
                        spFirstNamePronunciation.AddCssClass( "text-muted" );
                        spFirstNamePronunciation.AddCssClass( "mr-3" );
                        lbOpenNamePronunciationPanel.AddCssClass( "text-info" );
                        firstNamePronunciationAudioUrl = "";
                        personHasFirstNamePronunciationOverride = true;

                        // If there was no previous pronunciation data then hide the aPlayAllNamePronunciations tag.
                        if ( aPlayAllNamePronunciations.InnerText.Equals("No system pronunciation exists."))
                        {
                            aPlayAllNamePronunciations.InnerText = "";
                            aPlayAllNamePronunciations.Visible = false;
                        }

                        // Check if the nick name does not already have pronunciation data. Add some data for the Nick name if no data exists.
                        if (spNickNamePronunciation.InnerText.IsNullOrWhiteSpace())
                        {
                            if ( Person.NickNamePronunciationOverride.IsNotNullOrWhiteSpace() )
                            {
                                spNickNamePronunciation.InnerText = Person.NickNamePronunciationOverride;
                            }
                            else
                            {
                                spNickNamePronunciation.InnerText = Person.NickName;
                            }
                            spNickNamePronunciation.AddCssClass( "text-muted" );
                            spNickNamePronunciation.AddCssClass( "mr-3" );
                        }

                        // Check if the last name does not already have pronunciation data. Add some data for the last name if no data exists.
                        if ( spLastNamePronunciation.InnerText.IsNullOrWhiteSpace() )
                        {
                            if ( Person.LastNamePronunciationOverride.IsNotNullOrWhiteSpace() )
                            {
                                spLastNamePronunciation.InnerText = Person.LastNamePronunciationOverride;
                            }
                            else
                            {
                                spLastNamePronunciation.InnerText = Person.LastName;
                            }
                            spLastNamePronunciation.AddCssClass( "text-muted" );
                            spLastNamePronunciation.AddCssClass( "mr-3" );
                        }
                    }

                    // If the name in the nick name area is different than what is in the database, save the new data.
                    if ( !( tbNickName.Value.Equals( Person.NickNamePronunciationOverride ) ) && tbNickName.Value.IsNotNullOrWhiteSpace()
                        && !(tbNickName.Value.Equals( spNickNamePronunciation.InnerText ) ) )
                    {
                        // Set Values.
                        person.NickNamePronunciationOverride = tbNickName.Value;
                        spNickNamePronunciation.InnerText = tbNickName.Value;
                        spNickNamePronunciation.Attributes["onclick"] = "";
                        spNickNamePronunciation.AddCssClass( "text-muted" );
                        lbOpenNamePronunciationPanel.AddCssClass( "text-info" );
                        nickNamePronunciationAudioUrl = "";
                        personHasNickNamePronunciationOverride = true;

                        // If there was no previous pronunciation data then hide the aPlayAllNamePronunciations tag.
                        if ( aPlayAllNamePronunciations.InnerText.Equals( "No system pronunciation exists." ) )
                        {
                            aPlayAllNamePronunciations.InnerText = "";
                            aPlayAllNamePronunciations.Visible = false;
                        }

                        // Check if the first name does not already have pronunciation data. Add some data for the first name if no data exists.
                        if ( spFirstNamePronunciation.InnerText.IsNullOrWhiteSpace() )
                        {
                            if ( Person.FirstNamePronunciationOverride.IsNotNullOrWhiteSpace() )
                            {
                                spFirstNamePronunciation.InnerText = Person.FirstNamePronunciationOverride;
                            }
                            else
                            {
                                spFirstNamePronunciation.InnerText = Person.FirstName;
                            }
                            spFirstNamePronunciation.AddCssClass( "text-muted" );
                            spFirstNamePronunciation.AddCssClass( "mr-3" );
                        }

                        // Check if the last name does not already have pronunciation data. Add some data for the last name if no data exists.
                        if ( spLastNamePronunciation.InnerText.IsNullOrWhiteSpace() )
                        {
                            if ( Person.LastNamePronunciationOverride.IsNotNullOrWhiteSpace() )
                            {
                                spLastNamePronunciation.InnerText = Person.LastNamePronunciationOverride;
                            }
                            else
                            {
                                spLastNamePronunciation.InnerText = Person.LastName;
                            }
                            spLastNamePronunciation.AddCssClass( "text-muted" );
                            spLastNamePronunciation.AddCssClass( "mr-3" );
                        }
                    }

                    // If the name in the last name area is different than what is in the database, save the new data.
                    if ( !( tbLastName.Value.Equals( Person.LastNamePronunciationOverride ) ) && tbLastName.Value.IsNotNullOrWhiteSpace()
                        && !( tbLastName.Value.Equals( spLastNamePronunciation.InnerText ) ) )
                    {
                        // Set Values.
                        person.LastNamePronunciationOverride = tbLastName.Value;
                        spLastNamePronunciation.InnerText = tbLastName.Value;
                        spLastNamePronunciation.Attributes["onclick"] = "";
                        spLastNamePronunciation.AddCssClass( "text-muted" );
                        lbOpenNamePronunciationPanel.AddCssClass( "text-info" );
                        lastNamePronunciationAudioUrl = "";
                        personHasLastNamePronunciationOverride = true;

                        // If there was no previous pronunciation data then hide the aPlayAllNamePronunciations tag.
                        if ( aPlayAllNamePronunciations.InnerText.Equals( "No system pronunciation exists." ) )
                        {
                            aPlayAllNamePronunciations.InnerText = "";
                            aPlayAllNamePronunciations.Visible = false;
                        }

                        // Check if the first name does not already have pronunciation data. Add some data for the first name if no data exists.
                        if ( spFirstNamePronunciation.InnerText.IsNullOrWhiteSpace() )
                        {
                            if ( Person.FirstNamePronunciationOverride.IsNotNullOrWhiteSpace() )
                            {
                                spFirstNamePronunciation.InnerText = Person.FirstNamePronunciationOverride;
                            }
                            else
                            {
                                spFirstNamePronunciation.InnerText = Person.FirstName;
                            }
                            spFirstNamePronunciation.AddCssClass( "text-muted" );
                            spFirstNamePronunciation.AddCssClass( "mr-3" );
                        }

                        // Check if the nick name does not already have pronunciation data. Add some data for the Nick name if no data exists.
                        if ( spNickNamePronunciation.InnerText.IsNullOrWhiteSpace() )
                        {
                            if ( Person.NickNamePronunciationOverride.IsNotNullOrWhiteSpace() )
                            {
                                spNickNamePronunciation.InnerText = Person.NickNamePronunciationOverride;
                            }
                            else
                            {
                                spNickNamePronunciation.InnerText = Person.NickName;
                            }
                            spNickNamePronunciation.AddCssClass( "text-muted" );
                            spNickNamePronunciation.AddCssClass( "mr-3" );
                        }

                        // Find all family related to the current person.
                        var groupMemberService = new GroupMemberService( new RockContext() );
                        List<GroupMember> groupMembers = groupMemberService.GetSortedGroupMemberListForPerson( this.Person.Id, 10, false ).ToList();

                        // Loop through family members and change all the last nams that are the same to the last name override.
                        foreach ( var groupMember in groupMembers )
                        {
                            if ( groupMember.Person.LastName.Equals( Person.LastName ) )
                            {
                                var familySharedLastName = personService.Queryable()
                                .FirstOrDefault( o => o.FirstName.Equals( groupMember.Person.FirstName )
                                    && o.NickName.Equals( groupMember.Person.NickName )
                                    && o.LastName.Equals( groupMember.Person.LastName ) );
                                familySharedLastName.LastNamePronunciationOverride = tbLastName.Value;
                            }
                        }
                        
                    }

                    // If the notes in the note area is different than what is in the database, save the new data.
                    if ( !( tbPronunciationNote.Value.Equals( Person.PronunciationNote ) ) && !( tbPronunciationNote.Value.Equals( lPronunciationNote.Text ) ) )
                    {
                        // Set Values.
                        person.PronunciationNote = tbPronunciationNote.Value;
                        lPronunciationNote.Text = "<small>" + tbPronunciationNote.Value + "</small>";
                        lPronunciationNote.AddCssClass("text-muted");
                        lPronunciationNote.AddCssClass( "ml-3" );
                        lPronunciationNote.AddCssClass( "mr-3" );
                        lbOpenNamePronunciationPanel.AddCssClass( "text-info" );

                        // Check if the note should be shown.
                        if ( tbPronunciationNote.Value.IsNotNullOrWhiteSpace())
                        {
                            lPronunciationNote.Visible = true;
                        }
                    }

                    // Update the play all button to remove any changes to the pronunciations.
                    aPlayAllNamePronunciations.Attributes["onclick"] = $"playAllSound('{firstNamePronunciationAudioUrl}', '{nickNamePronunciationAudioUrl}', '{lastNamePronunciationAudioUrl}');";
                    if( firstNamePronunciationAudioUrl.IsNullOrWhiteSpace() && nickNamePronunciationAudioUrl.IsNullOrWhiteSpace() && lastNamePronunciationAudioUrl.IsNullOrWhiteSpace() )
                    {
                        aPlayAllNamePronunciations.Visible = false;
                    }
                }

                // Save changes and hide the modal.
                rockContext.SaveChanges();
                dlgNamePronunciation.Hide();
            }
        }

        #endregion Control Events

        #region Methods

        private void ShowPersonImage()
        {
            lImage.Text = $@"<img src=""{Person.GetPersonPhotoUrl( Person, 400 )}&Style=icon"" alt class=""img-profile"">";
        }

        private void ShowProtectionLevel()
        {
            if ( Person.AccountProtectionProfile > Rock.Utility.Enums.AccountProtectionProfile.Low && IsUserAuthorized( SecurityActionKey.ViewProtectionProfile ) )
            {
                string acctProtectionLevel = $@"
                    <div class=""protection-profile"">
                        <span class=""profile-label"">Protection Profile: {Person.AccountProtectionProfile.ConvertToString( true )}</span>
                        <i class=""fa fa-fw fa-lock"" onmouseover=""$(this).parent().addClass('is-hovered')"" onmouseout=""$(this).parent().removeClass('is-hovered')""></i>
                    </div>";

                litAccountProtectionLevel.Text = acctProtectionLevel;
            }
        }

        private void ShowPersonName()
        {
            // Check if this record represents a Business.
            bool isBusiness = false;

            if ( Person.RecordTypeValueId.HasValue )
            {
                int recordTypeValueIdBusiness = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;
                isBusiness = ( Person.RecordTypeValueId.Value == recordTypeValueIdBusiness );
            }

            if ( isBusiness )
            {
                lName.Text = $@"<h1>{Person.LastName}</h1>";
                return;
            }

            // Prefix with Title if they have a Title with IsFormal=True
            string titleText = string.Empty;
            if ( Person.TitleValueId.HasValue )
            {
                var personTitleValue = DefinedValueCache.Get( Person.TitleValueId.Value );
                if ( personTitleValue != null && personTitleValue.GetAttributeValue( "IsFormal" ).AsBoolean() )
                {
                    titleText = $"{personTitleValue.Value} ";
                }
            }

            // Add Suffix.
            string suffixText = string.Empty;
            if ( Person.SuffixValueId.HasValue )
            {
                var suffix = DefinedValueCache.Get( Person.SuffixValueId.Value );
                if ( suffix != null )
                {
                    suffixText = $" {suffix.Value}";
                }
            }

            // Show Middle Name
            string middleName = string.Empty;
            if ( GetAttributeValue( AttributeKey.DisplayMiddleName ).AsBoolean() && !String.IsNullOrWhiteSpace( Person.MiddleName ) )
            {
                middleName = $" {Person.MiddleName}";
            }

            string nameText = $"{titleText}{Person.NickName}{middleName} {Person.LastName}{suffixText}";

            // Add First Name if different from NickName.
            string firstName = string.Empty;
            if ( Person.NickName != Person.FirstName )
            {
                if ( !string.IsNullOrWhiteSpace( Person.FirstName ) )
                {
                    firstName = $"{Person.FirstName}";
                }
            }

            // Add Previous Names.
            string previousNameText = string.Empty;
            using ( var rockContext = new RockContext() )
            {
                var previousNames = Person.GetPreviousNames( rockContext ).Select( a => a.LastName );

                if ( previousNames.Any() )
                {
                    previousNameText = $"<br><span class='previous-names'>(Previous Names: {previousNames.ToList().AsDelimited( ", " )})</span>";
                }
            }

            lName.Text = $"<h1 class='person-name text-left pr-2'>{nameText} <small class='person-first-name'>{firstName}</small></h1>{previousNameText}";
        }

        /// <summary>
        /// Check if the name pronunciation override columns have any values within them. 
        /// </summary>
        public static bool personHasFirstNamePronunciationOverride;
        public static bool personHasNickNamePronunciationOverride;
        public static bool personHasLastNamePronunciationOverride;
        public static bool personHasPronunciationNote;
        private void CheckForNamePronunciationOverride()
        {
            // Initialize static variables for Name Pronunciation Feature.
            divNamePronunciationPanel.Visible = false;
            dlgNamePronunciation.Hide();
            aPlayAllNamePronunciations.Visible = false;
            lPronunciationNote.Visible = false;
            firstTimePostingNamePronunciations = true;
            personHasFirstNamePronunciationOverride = false;
            personHasNickNamePronunciationOverride = false;
            personHasLastNamePronunciationOverride = false;

            if ( Person != null )
            {
                // Set the default values of the name pronunciation modal as the name of the current person.
                tbFirstName.Value = Person.FirstName;
                tbNickName.Value = Person.NickName;
                tbLastName.Value = Person.LastName;

                // The firstNamePronunciation span becomes a muted text with the override text applied.
                if ( Person.FirstNamePronunciationOverride.IsNotNullOrWhiteSpace() )
                {
                    spFirstNamePronunciation.InnerText = Person.FirstNamePronunciationOverride;
                    spFirstNamePronunciation.AddCssClass( "text-muted" );
                    spFirstNamePronunciation.AddCssClass( "mr-3" );
                    tbFirstName.Value = Person.FirstNamePronunciationOverride;
                    lbOpenNamePronunciationPanel.AddCssClass( "text-info" );
                    personHasFirstNamePronunciationOverride = true;
                }

                // The nickNamePronunciation span becomes a muted text with the override text applied.
                if ( Person.NickNamePronunciationOverride.IsNotNullOrWhiteSpace() )
                {
                    spNickNamePronunciation.InnerText = Person.NickNamePronunciationOverride;
                    spNickNamePronunciation.AddCssClass( "text-muted" );
                    spNickNamePronunciation.AddCssClass( "mr-3" );
                    tbNickName.Value = Person.NickNamePronunciationOverride;
                    lbOpenNamePronunciationPanel.AddCssClass( "text-info" );
                    personHasNickNamePronunciationOverride = true;
                }

                // The lastNamePronunciation span becomes a muted text with the override text applied.
                if ( Person.LastNamePronunciationOverride.IsNotNullOrWhiteSpace() )
                {
                    spLastNamePronunciation.InnerText = Person.LastNamePronunciationOverride;
                    spLastNamePronunciation.AddCssClass( "text-muted" );
                    tbLastName.Value = Person.LastNamePronunciationOverride;
                    lbOpenNamePronunciationPanel.AddCssClass( "text-info" );
                    personHasLastNamePronunciationOverride = true;
                }

                // The pronunciationNote label gets assigned the text in the notes column of the person table.
                if ( Person.PronunciationNote.IsNotNullOrWhiteSpace() )
                {
                    lPronunciationNote.Text = "<small>" + Person.PronunciationNote + "</small>";
                    lPronunciationNote.AddCssClass( "ml-3" );
                    lPronunciationNote.AddCssClass( "mr-3" );
                    lPronunciationNote.AddCssClass( "text-muted" );
                    tbPronunciationNote.Value = Person.PronunciationNote;
                    lbOpenNamePronunciationPanel.AddCssClass( "text-info" );
                    personHasPronunciationNote = true;
                    lPronunciationNote.Visible = true;
                }
            }
        }

        /// <summary>
        /// Creates the name pronunciation div. 
        /// </summary>
        static string firstNamePronunciationAudioUrl = "";
        static string nickNamePronunciationAudioUrl = "";
        static string lastNamePronunciationAudioUrl = "";
        private void CreatePronunciationDiv()
        {
            if ( Person != null )
            {
                // Reset audio url.
                firstNamePronunciationAudioUrl = "";
                nickNamePronunciationAudioUrl = "";
                lastNamePronunciationAudioUrl = "";

                // Check if a person is missing any names.
                bool personHasFirstName = true;
                bool personHasLastName = true;
                bool personHasNickName = true;
                if ( Person.FirstName == null )
                {
                    personHasFirstName = false;
                }
                if ( Person.LastName == null )
                {
                    personHasLastName = false;
                }
                if ( Person.NickName == null )
                {
                    personHasNickName = false;
                }

                // Create an API Client and request to call the requests for pronunciation.
var namePronunciationClient = new RestClient( "http://localhost:49999/api/pronunciations/person" );
//var namePronunciationClient = new RestClient( "https://api.rockrms.com/api/pronunciations/person" );
                var namePronunciationrequest = new RestRequest( Method.POST );

                // Create list of pronunciations that will be passed as a Json Body.
                List<NamePronunciationRequest> namePronunciationRequestList = new List<NamePronunciationRequest>();
                if ( personHasFirstName && !personHasFirstNamePronunciationOverride )
                {
                    NamePronunciationRequest tempNamePronunciation = new NamePronunciationRequest();
                    tempNamePronunciation.Name = Person.FirstName;
                    tempNamePronunciation.NameType = ( NameType ) 0;
                    namePronunciationRequestList.Add( tempNamePronunciation );
                }
                if ( personHasNickName && !personHasNickNamePronunciationOverride )
                {
                    NamePronunciationRequest tempNamePronunciation = new NamePronunciationRequest();
                    tempNamePronunciation.Name = Person.NickName;
                    tempNamePronunciation.NameType = ( NameType ) 0;
                    namePronunciationRequestList.Add( tempNamePronunciation );
                }
                if ( personHasLastName && !personHasLastNamePronunciationOverride )
                {
                    NamePronunciationRequest tempNamePronunciation = new NamePronunciationRequest();
                    tempNamePronunciation.Name = Person.LastName;
                    tempNamePronunciation.NameType = ( NameType ) 1;
                    namePronunciationRequestList.Add( tempNamePronunciation );
                }
                var rockInstanceId = Rock.Web.SystemSettings.GetRockInstanceId();

                //add the body of the HTML POST request
                namePronunciationrequest.AddJsonBody( new
                {
                    RockInstanceId = rockInstanceId,
                    IndividualRequests = namePronunciationRequestList
                } );

                // Call the name Pronunciation API and get a response that contains a list of Name Pronunciations.
                var namePronunciationResponseList = namePronunciationClient.Execute<List<NamePronunciationResponse>>( namePronunciationrequest );

                if ( namePronunciationResponseList.StatusCode == System.Net.HttpStatusCode.OK )
                {
                    // Create some variables.
                    bool personHasFirstNamePronunciation = false;
                    bool personHasNickNamePronunciation = false;
                    bool personHasLastNamePronunciation = false;

                    // Loop through the response data.
                    foreach ( NamePronunciationResponse namePronunciationResponse in namePronunciationResponseList.Data )
                    {
                        // Check if there is no prounciation.
                        if ( namePronunciationResponse.Pronunciation == null )
                        {
                            continue;
                        }

                        // Check if the returned object contains the first name and if so then assign a link to the link button.
                        if ( namePronunciationResponse.Name.Equals( Person.FirstName )
                            && namePronunciationResponse.NameType == 0
                            && !personHasFirstNamePronunciationOverride )
                        {
                            firstNamePronunciationAudioUrl = namePronunciationResponse.AudioUrl;
//firstNamePronunciationAudioUrl = "https://rockrms.blob.core.windows.net/pronunciations/firstnames/" + Person.FirstName.ToLower() + ".mp3";

                            // Check if there is audio to play.
                            if ( firstNamePronunciationAudioUrl.IsNotNullOrWhiteSpace())
                            {
                                spFirstNamePronunciation.Attributes["onclick"] = $"playSound('{firstNamePronunciationAudioUrl}')";
                            }
                            else
                            {
                                spFirstNamePronunciation.AddCssClass( "text-muted" );
                            }
                            
                            spFirstNamePronunciation.InnerText = namePronunciationResponse.Pronunciation;
                            spFirstNamePronunciation.AddCssClass( "mr-3" );
                            personHasFirstNamePronunciation = true;
                            tbFirstName.Value = namePronunciationResponse.Pronunciation;
                        }

                        // Check if the returned object contains the nick name and if so then assign a link to the link button.
                        if ( namePronunciationResponse.Name.Equals( Person.NickName )
                            && namePronunciationResponse.NameType == 0
                            && !personHasNickNamePronunciationOverride )
                        {
                            nickNamePronunciationAudioUrl = namePronunciationResponse.AudioUrl;
//nickNamePronunciationAudioUrl = "https://rockrms.blob.core.windows.net/pronunciations/firstnames/" + Person.NickName.ToLower() + ".mp3";

                            // Check if there is audio to play.
                            if ( nickNamePronunciationAudioUrl.IsNotNullOrWhiteSpace() )
                            {
                                spNickNamePronunciation.Attributes["onclick"] = $"playSound('{nickNamePronunciationAudioUrl}')";
                            }
                            else
                            {
                                spNickNamePronunciation.AddCssClass( "text-muted" );
                            }

                            spNickNamePronunciation.InnerText = namePronunciationResponse.Pronunciation;
                            spNickNamePronunciation.AddCssClass( "mr-3" );
                            personHasNickNamePronunciation = true;
                            tbNickName.Value = namePronunciationResponse.Pronunciation;
                        }

                        // Check if the returned object contains the last name and if so then assign a link to the link button.
                        if ( namePronunciationResponse.Name.Equals( Person.LastName )
                            && namePronunciationResponse.NameType == ( NameType ) 1
                            && !personHasLastNamePronunciationOverride )
                        {
                            lastNamePronunciationAudioUrl = namePronunciationResponse.AudioUrl;
//lastNamePronunciationAudioUrl = "https://rockrms.blob.core.windows.net/pronunciations/lastnames/" + Person.LastName.ToLower() + ".mp3";

                            // Check if there is audio to play.
                            if ( lastNamePronunciationAudioUrl.IsNotNullOrWhiteSpace() )
                            {
                                spLastNamePronunciation.Attributes["onclick"] = $"playSound('{lastNamePronunciationAudioUrl}')";
                            }
                            else
                            {
                                spLastNamePronunciation.AddCssClass( "text-muted" );
                            }

                            spLastNamePronunciation.InnerText = namePronunciationResponse.Pronunciation;
                            personHasLastNamePronunciation = true;
                            tbLastName.Value = namePronunciationResponse.Pronunciation;
                        }
                    }

                    bool thereIsOneNamePronunciation = false;
                    // Check if one of the names has a pronunciation.
                    if ( personHasFirstNamePronunciation || personHasNickNamePronunciation || personHasLastNamePronunciation)
                    {   
                        thereIsOneNamePronunciation = true;

                        //Check if there is at least one audio file to play.
                        if ( firstNamePronunciationAudioUrl.IsNotNullOrWhiteSpace() || nickNamePronunciationAudioUrl.IsNotNullOrWhiteSpace() || lastNamePronunciationAudioUrl.IsNotNullOrWhiteSpace() )
                        {
                            aPlayAllNamePronunciations.InnerHtml = "<i class='fa fa-play-circle-o fa-lg text-primary' runat=\"server\"></i>";
                            aPlayAllNamePronunciations.Attributes["onclick"] = $"playAllSound('{firstNamePronunciationAudioUrl}', '{nickNamePronunciationAudioUrl}', '{lastNamePronunciationAudioUrl}');";
                            aPlayAllNamePronunciations.Visible = true;
                        }
                    }

                    // If there is an override then a name pronunciation exists.
                    if( personHasFirstNamePronunciationOverride || personHasNickNamePronunciationOverride || personHasLastNamePronunciationOverride )
                    {
                        thereIsOneNamePronunciation = true;
                    }

                    // Check if Person.FirstName needs to be shown instead of pronunciation data.
                    if ( thereIsOneNamePronunciation && !personHasFirstNamePronunciation && !personHasFirstNamePronunciationOverride )
                    {
                        spFirstNamePronunciation.InnerText = Person.FirstName;
                        spFirstNamePronunciation.AddCssClass( "mr-3" );
                        spFirstNamePronunciation.AddCssClass( "text-muted" );
                        tbFirstName.Value = Person.FirstName;
                    }

                    // Check if Person.NickName needs to be shown instead of pronunciation data.
                    if ( thereIsOneNamePronunciation && !personHasNickNamePronunciation && !personHasNickNamePronunciationOverride )
                    {
                        spNickNamePronunciation.InnerText = Person.NickName;
                        spNickNamePronunciation.AddCssClass( "mr-3" );
                        spNickNamePronunciation.AddCssClass("text-muted");
                        tbNickName.Value = Person.NickName;
                    }

                    // Check if Person.LastName needs to be shown instead of pronunciation data.
                    if ( thereIsOneNamePronunciation && !personHasLastNamePronunciation && !personHasLastNamePronunciationOverride )
                    {
                        spLastNamePronunciation.InnerText = Person.LastName;
                        spLastNamePronunciation.AddCssClass( "text-muted" );
                        tbLastName.Value = Person.LastName;
                    }

                    // If all 3 names do not have a pronunciation, then say no pronunciation exists.
                    if ( !personHasFirstNamePronunciation && !personHasNickNamePronunciation && !personHasLastNamePronunciation
                        && !personHasFirstNamePronunciationOverride && !personHasNickNamePronunciationOverride && !personHasLastNamePronunciationOverride )
                    {
                        aPlayAllNamePronunciations.AddCssClass( "text-muted" );
                        aPlayAllNamePronunciations.InnerText = "No system pronunciation exists.";
                        aPlayAllNamePronunciations.Visible = true;
                    }

                    // Add the pencil button.
                    lbEditNamePronunciation.Text = "<i class='fa fa-pencil fa-xs' runat='server'></i>";

                    // Change the Modal controls for the name pronunciation override Modal.
                    formLink.HRef = "https://community.rockrms.com/namepronunciation?FirstName=" + Person.FirstName + "&NickName=" + Person.NickName + "&LastName=" + Person.LastName;
                }

                // If the response code did not go through hide the button audio.
                if ( namePronunciationResponseList.StatusCode != System.Net.HttpStatusCode.OK )
                {
                    lbOpenNamePronunciationPanel.Visible = false;
                }
            }
        }

        private void ShowBadgeList()
        {
            string badgeList = GetAttributeValue( AttributeKey.Badges );
            if ( !string.IsNullOrWhiteSpace( badgeList ) )
            {
                foreach ( string badgeGuid in badgeList.SplitDelimitedValues() )
                {
                    Guid guid = badgeGuid.AsGuid();
                    if ( guid != Guid.Empty )
                    {
                        var badge = BadgeCache.Get( guid );
                        if ( badge != null )
                        {
                            blStatus.BadgeTypes.Add( badge );
                        }
                    }
                }
            }
        }

        private void ShowFollowingButton()
        {
            if ( !GetAttributeValue( AttributeKey.AllowFollowing ).AsBoolean() || CurrentPerson == null )
            {
                lbFollowing.Visible = false;
                return;
            }

            var personAliasEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.PERSON_ALIAS );

            lbFollowing.Visible = true;
            using ( var rockContext = new RockContext() )
            {
                var followingList = new FollowingService( rockContext )
                    .Queryable()
                    .Where( f => f.EntityTypeId == personAliasEntityTypeId && f.EntityId == Person.PrimaryAliasId )
                    .ToList();

                if ( followingList.Where( f => f.PersonAlias.PersonId == CurrentPerson.Id ).Any() )
                {
                    lbFollowing.AddCssClass( "is-followed" );
                    lbFollowing.Text = $@"
                        <span class=""text"">Following</span>
                        <span class=""font-weight-normal"">{followingList.Count}</span>";
                }
                else
                {
                    lbFollowing.RemoveCssClass( "is-followed" );
                    lbFollowing.Text = $@"<span class=""text"">Follow</span>";

                    if ( followingList.Any() )
                    {
                        lbFollowing.Text += $@"<span class=""font-weight-normal""> {followingList.Count}</span>";
                    }
                }
            }
        }

        private void ShowSmsButton()
        {
            if ( !Person.PhoneNumbers.Where( p => p.IsMessagingEnabled ).Any() )
            {
                divSmsButton.Visible = false;
                return;
            }

            divSmsButton.Visible = true;

            Rock.Web.PageReference communicationPageReference = null;

            var communicationLinkedPageValue = this.GetAttributeValue( AttributeKey.SmsPage );
            if ( communicationLinkedPageValue.IsNotNullOrWhiteSpace() )
            {
                communicationPageReference = new Rock.Web.PageReference( communicationLinkedPageValue );
            }

            var mediums = GetCommunicationMediums();
            var smsLink = string.Empty;

            if ( communicationPageReference != null )
            {
                communicationPageReference.QueryString = new System.Collections.Specialized.NameValueCollection( communicationPageReference.QueryString ?? new System.Collections.Specialized.NameValueCollection() )
                {
                    ["person"] = Person.Id.ToString()
                };

                if ( mediums.ContainsKey( "SMS" ) )
                {
                    communicationPageReference.QueryString.Add( "MediumId", mediums["SMS"].Value.ToString() );
                }

                smsLink = new Rock.Web.PageReference( communicationPageReference.PageId, communicationPageReference.RouteId, communicationPageReference.Parameters, communicationPageReference.QueryString ).BuildUrl();
            }
            else
            {
                smsLink = $"{ResolveRockUrl( "/" )}communications/new/simple?person={Person.Id}";

                if ( mediums.ContainsKey( "SMS" ) )
                {
                    smsLink += $"&MediumId={mediums["SMS"].Value}";
                }
            }

            lSmsButton.Text = $@"<a href='{smsLink}' class='btn btn-default btn-go btn-square stretched-link' title='Send a SMS' aria-label='Send a SMS'><i class='fa fa-comment-alt'></i></a><span>Text</span>";
        }

        private void ShowEmailButton()
        {
            if ( string.IsNullOrWhiteSpace( Person.Email ) || !Person.IsEmailActive || Person.EmailPreference == EmailPreference.DoNotEmail )
            {
                divEmailButton.Visible = false;
                return;
            }

            divEmailButton.Visible = true;

            Rock.Web.PageReference communicationPageReference = null;

            var communicationLinkedPageValue = this.GetAttributeValue( AttributeKey.CommunicationPage );
            if ( communicationLinkedPageValue.IsNotNullOrWhiteSpace() )
            {
                communicationPageReference = new Rock.Web.PageReference( communicationLinkedPageValue );
            }

            var globalAttributes = GlobalAttributesCache.Get();
            var emailLinkPreference = globalAttributes.GetValue( "PreferredEmailLinkType" );
            var emailLink = $"mailto:{Person.Email}";

            if ( string.IsNullOrWhiteSpace( emailLinkPreference ) || emailLinkPreference == "1" )
            {
                if ( communicationPageReference != null )
                {
                    communicationPageReference.QueryString = new System.Collections.Specialized.NameValueCollection( communicationPageReference.QueryString ?? new System.Collections.Specialized.NameValueCollection() )
                    {
                        ["person"] = Person.Id.ToString()
                    };

                    emailLink = new Rock.Web.PageReference( communicationPageReference.PageId, communicationPageReference.RouteId, communicationPageReference.Parameters, communicationPageReference.QueryString ).BuildUrl();
                }
                else
                {
                    emailLink = $"{ResolveRockUrl( "/" )}communications/new?person={Person.Id}";
                }
            }

            var emailButtonTitle = Person.EmailPreference == EmailPreference.NoMassEmails ? @"Email Preference is set to ""No Mass Emails""" : "Send an email";

            lEmailButton.Text = $@"<a href='{emailLink}' class='btn btn-default btn-go btn-square stretched-link' title='{emailButtonTitle}' aria-label='{emailButtonTitle}'><i class='fa fa-envelope'></i></a><span>Email</span>";
        }

        /// <summary>
        /// Creates an "Actions" menu with workflow and/or custom actions, as specified in the block settings.
        /// </summary>
        protected void CreateActionMenu()
        {
            hlVCard.NavigateUrl = ResolveUrl( string.Format( "~/api/People/VCard/{0}", Person.Guid ) );
            StringBuilder sbActions = new StringBuilder();

            // First list the actions manually entered as html in the block settting
            var actions = GetAttributeValue( AttributeKey.AdditionalCustomActions );
            if ( !string.IsNullOrWhiteSpace( actions ) )
            {
                string appRoot = ResolveRockUrl( "~/" );
                string themeRoot = ResolveRockUrl( "~~/" );
                actions = actions.Replace( "~~/", themeRoot ).Replace( "~/", appRoot );

                if ( actions.Contains( "{0}" ) )
                {
                    actions = string.Format( actions, Person.Id );
                }

                sbActions.Append( "<li role=\"separator\" class=\"divider\"></li>" );
                sbActions.Append( actions );
            }

            // Next list the workflow actions selected in the picker
            var workflowActions = GetAttributeValue( AttributeKey.WorkflowActions );
            if ( !string.IsNullOrWhiteSpace( workflowActions ) )
            {
                List<WorkflowType> workflowTypes = new List<WorkflowType>();

                using ( var rockContext = new RockContext() )
                {
                    var workflowTypeService = new WorkflowTypeService( rockContext );
                    foreach ( string guidValue in workflowActions.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                    {
                        Guid? guid = guidValue.AsGuidOrNull();
                        if ( guid.HasValue )
                        {
                            var workflowType = workflowTypeService.Get( guid.Value );
                            if ( workflowType != null && ( workflowType.IsActive ?? true ) && workflowType.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) )
                            {
                                workflowTypes.Add( workflowType );
                            }
                        }
                    }
                }

                workflowTypes = workflowTypes.OrderBy( w => w.Name ).ToList();

                if ( workflowTypes.Count() > 0 )
                {
                    sbActions.Append( "<li role=\"separator\" class=\"divider\"></li>" );
                }

                foreach ( var workflowType in workflowTypes )
                {
                    string url = string.Format( "~/WorkflowEntry/{0}?PersonId={1}", workflowType.Id, Person.Id );
                    sbActions.AppendFormat(
                        "<li><a href='{0}'><i class='fa-fw {1}'></i> {2}</a></li>",
                        ResolveRockUrl( url ),
                        workflowType.IconCssClass,
                        workflowType.Name );
                    sbActions.AppendLine();
                }
            }

            lActions.Text = sbActions.ToString();
        }

        private void ShowDemographicsInfo()
        {
            lGender.Text =
                $@"<dt title=""Gender"">{Person.Gender}</dt>
                <dd class=""d-none"">Gender</dd>";

            var raceAndEthnicity = new List<string>();

            if ( Person.RaceValue != null )
            {
                raceAndEthnicity.Add( Person.RaceValue.Value );
            }

            if ( Person.EthnicityValue != null )
            {
                raceAndEthnicity.Add( Person.EthnicityValue.Value );
            }

            if ( raceAndEthnicity.Count > 0 )
            {
                var title = $"{Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.PERSON_RACE_LABEL, "Race" )}/{Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.PERSON_ETHNICITY_LABEL, "Ethnicity" )}";
                lRaceAndEthnicity.Text =
                    $@"<dt title=""{title}"">{raceAndEthnicity.AsDelimited( "/" )}</dt>
                    <dd class=""d-none"">{title}</dd>";
            }

            if ( Person.BirthDate.HasValue )
            {
                if ( Person.BirthYear.HasValue && Person.BirthYear != DateTime.MinValue.Year )
                {
                    var formattedAge = Person.FormatAge();
                    if ( formattedAge.IsNotNullOrWhiteSpace() )
                    {
                        formattedAge += " old";
                    }

                    var birthdateText = Person.BirthDate.Value.ToShortDateString();
                    lAge.Text = $"<dt>{formattedAge}</dt><dd>{birthdateText}</dd>";
                }
                else
                {
                    var birthdateText = Person.BirthDate.Value.ToString( "MMM d" );
                    lAge.Text = $"<dt>{birthdateText}</dt><dd>Birthdate</dd>";

                }
            }

            if ( !Person.IsDeceased && Person.AnniversaryDate.HasValue && GetAttributeValue( AttributeKey.DisplayAnniversaryDate ).AsBoolean() )
            {
                lMaritalStatus.Text = $"<dt>{Person.MaritalStatusValueId.DefinedValue()} {Person.AnniversaryDate.Value.Humanize().Replace( "ago", "" )}</dt><dd>{Person.AnniversaryDate.Value.ToShortDateString()}</dd>";
            }
            else
            {
                if ( Person.MaritalStatusValueId.HasValue )
                {
                    lMaritalStatus.Text = $@"<dt>{Person.MaritalStatusValueId.DefinedValue()}</dt><dd class=""d-none"">Marital Status</dd>";
                }
            }

            if ( GetAttributeValue( AttributeKey.DisplayGraduation ).AsBoolean() )
            {
                if ( !string.IsNullOrWhiteSpace( Person.GradeFormatted ) )
                {
                    lGrade.Text = $"<dt>{Person.GradeFormatted}</dt>";
                }

                if ( Person.GraduationYear.HasValue && Person.HasGraduated.HasValue )
                {
                    lGraduation.Text = Person.HasGraduated.Value ? $@"<dt>Graduated {Person.GraduationYear.Value}</dt>" : $@"<dd>Graduates {Person.GraduationYear.Value}</dd>";
                    lGraduation.Text += @"<dd class=""d-none"">Graduation</dd>";
                }
            }
        }

        private void ShowPhoneInfo()
        {
            if ( Person.PhoneNumbers == null )
            {
                return;
            }

            var phoneNumbers = Person.PhoneNumbers.AsEnumerable();
            var phoneNumberTypes = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_PHONE_TYPE ) );
            if ( phoneNumberTypes.DefinedValues.Any() )
            {
                var phoneNumberTypeIds = phoneNumberTypes.DefinedValues.Select( a => a.Id ).ToList();
                phoneNumbers = phoneNumbers.OrderBy( a => phoneNumberTypeIds.IndexOf( a.NumberTypeValueId.Value ) );
            }

            // if phoneNumbers exist then bind
            if ( phoneNumbers.Any() )
            {
                rptPhones.DataSource = phoneNumbers;
                rptPhones.DataBind();
            }
            else
            {
                rptPhones.Visible = false;
            }
        }

        private void ShowEmailText()
        {
            var communicationLinkedPageValue = this.GetAttributeValue( AttributeKey.CommunicationPage );
            Rock.Web.PageReference communicationPageReference;
            if ( communicationLinkedPageValue.IsNotNullOrWhiteSpace() )
            {
                communicationPageReference = new Rock.Web.PageReference( communicationLinkedPageValue );
            }
            else
            {
                communicationPageReference = null;
            }

            lEmail.Text = Person.GetEmailTag( ResolveRockUrl( "/" ), communicationPageReference, "d-inline-block mw-100 text-link text-truncate" );
            if ( lEmail == null )
            {
                lEmail.Visible = false;
            }
        }

        private void ShowSocialMediaButtons()
        {
            var socialCategoryGuid = GetAttributeValue( AttributeKey.SocialMediaCategory ).AsGuidOrNull();

            if ( !socialCategoryGuid.HasValue )
            {
                rptSocial.Visible = false;
                return;
            }

            var attributes = Person.Attributes.Where( p => p.Value.Categories.Select( c => c.Guid ).Contains( socialCategoryGuid.Value ) );
            var result = attributes.Join( Person.AttributeValues, a => a.Key, v => v.Key, ( a, v ) => new { Attribute = a.Value, Value = v.Value, QualifierValues = a.Value.QualifierValues } );

            rptSocial.DataSource = result
                .Where( r =>
                    r.Value != null &&
                    r.Value.Value != string.Empty &&
                    r.QualifierValues != null &&
                    r.QualifierValues.ContainsKey( NAME_KEY ) &&
                    r.QualifierValues.ContainsKey( ICONCSSCLASS_KEY ) &&
                    r.QualifierValues.ContainsKey( COLOR_KEY ) )
                .OrderBy( r => r.Attribute.Order )
                .Select( r => new
                {
                    url = r.Value.Value,
                    name = r.QualifierValues[NAME_KEY].Value,
                    icon = r.Attribute.QualifierValues[ICONCSSCLASS_KEY].Value.Contains( "fa-fw" ) ?
                        r.Attribute.QualifierValues[ICONCSSCLASS_KEY].Value :
                        r.Attribute.QualifierValues[ICONCSSCLASS_KEY].Value + " fa-fw",
                    color = r.Attribute.QualifierValues[COLOR_KEY].Value,
                } )
                .ToList();

            rptSocial.DataBind();
            rptSocial.Visible = rptSocial.Items.Count > 0;
        }

        private void ShowCustomContent()
        {
            string customContent = GetAttributeValue( AttributeKey.CustomContent );

            if ( customContent.IsNullOrWhiteSpace() )
            {
                divCustomContent.Visible = false;
                return;
            }

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );
            string resolvedContent = customContent.ResolveMergeFields( mergeFields );
            phCustomContent.Controls.Add( new LiteralControl( resolvedContent ) );
        }

        private Dictionary<string, int?> GetCommunicationMediums()
        {
            var mediums = new Dictionary<string, int?>();

            foreach ( var item in Rock.Communication.MediumContainer.Instance.Components.Values )
            {
                if ( item.Value.IsActive && item.Value.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) )
                {
                    mediums.AddOrIgnore( item.Value.EntityType.FriendlyName, item.Value.EntityType.Id );
                }
            }

            return mediums;
        }

        private void NavigateToCommunicationPage( string mediumType )
        {
            var communicationLinkedPageValue = this.GetAttributeValue( AttributeKey.CommunicationPage );

            var queryParams = new Dictionary<string, string>
            {
                { "person", Person.Id.ToString() }
            };

            if ( communicationLinkedPageValue != null )
            {
                var emailMediums = GetCommunicationMediums();
                if ( emailMediums.ContainsKey( mediumType ) )
                {
                    queryParams.Add( "MediumId", emailMediums[mediumType].Value.ToString() );
                }

                NavigateToLinkedPage( AttributeKey.CommunicationPage, queryParams );
            }
            else
            {
                NavigateToPage( Rock.SystemGuid.Page.NEW_COMMUNICATION.AsGuid(), queryParams );
            }
        }

        /// <summary>
        /// Registers the client script for the name pronunciation feature.
        /// </summary>
        protected void RegisterClientScript()
        {
            string script = @"
            function playSound(urlLink) {
                var a = new Audio(urlLink);
                a.play().catch(function(error) {
                    console.log('Failed to play audio: ' + error);
                    });
            }

            function playAllSound(firstNameUrl, nickNameUrl, lastNameUrl) {
	            var sleepTime = 0;
	            if(firstNameUrl !== """"){
                    setTimeout( () => playSound(firstNameUrl), sleepTime );
                    sleepTime += 1200;
                }
                if(nickNameUrl !== """"){
                    setTimeout( () => playSound(nickNameUrl), sleepTime);
                    sleepTime += 1200;
	            }
                if(lastNameUrl !== """"){
                    setTimeout( () => playSound(lastNameUrl), sleepTime);
                    sleepTime += 1200;
	            }
            }
            ";

            ScriptManager.RegisterStartupScript( this, this.GetType(), "play-name-pronunciation-audio", script, true );
        }
        #endregion Methods

        #region Name Pronunciation Classes
        /// <summary>
        /// Represents a name pronunciation request of a certain name and the type of name.
        /// </summary>
        public class NamePronunciationRequest
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the name type (0 = First Name, 1 = Last Name)
            /// </summary>
            /// <value>
            /// The name type.
            /// </value>
            public NameType NameType { get; set; }
        }

        /// <summary>
        /// Represents a pronunciation of a certain name and the type of name.
        /// </summary>
        public class NamePronunciationResponse
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the name type (0 = First Name, 1 = Last Name)
            /// </summary>
            /// <value>
            /// The name type.
            /// </value>
            public NameType NameType { get; set; }

            /// <summary>
            /// Gets or sets the pronunciation.
            /// </summary>
            /// <value>
            /// The pronunciation.
            /// </value>
            public string Pronunciation { get; set; }

            /// <summary>
            /// Gets or sets the audio url.
            /// </summary>
            /// <value>
            /// The audio Url.
            /// </value>
            public string AudioUrl { get; set; }
        }

        // Name Type Enum
        public enum NameType
        {
            FirstName,
            LastName
        }
        #endregion Name Pronunciation Classes
    }
}