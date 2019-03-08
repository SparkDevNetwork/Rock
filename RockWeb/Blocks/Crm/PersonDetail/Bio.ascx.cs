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
using System.Web.UI;

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

    [PersonBadgesField( "Badges", "The label badges to display in this block.", false, "", "", 0 )]
    [WorkflowTypeField( "Workflow Actions", "The workflows to make available as actions.", true, false, "", "", 1 )]
    [CodeEditorField( "Additional Custom Actions", @"
Additional custom actions (will be displayed after the list of workflow actions). Any instance of '{0}' will be replaced with the current person's id.
Because the contents of this setting will be rendered inside a &lt;ul&gt; element, it is recommended to use an 
&lt;li&gt; element for each available action.  Example:
<pre>
    &lt;li&gt;&lt;a href='~/WorkflowEntry/4?PersonId={0}' tabindex='0'&gt;Fourth Action&lt;/a&gt;&lt;/li&gt;
</pre>
", Rock.Web.UI.Controls.CodeEditorMode.Html, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 200, false, "", "", 2, "Actions" )]
    [BooleanField( "Enable Impersonation", "Should the Impersonate custom action be enabled? Note: If enabled, it is only visible to users that are authorized to administrate the person.", false, "", 3 )]
    [LinkedPage( "Impersonation Start Page", "The page to navigate to after clicking the Impersonate action.", false, "", "", 4)]
    [LinkedPage( "Business Detail Page", "The page to redirect user to if a business is is requested.", false, "", "", 5 )]
    [BooleanField( "Display Country Code", "When enabled prepends the country code to all phone numbers.", false, "", 6 )]
    [BooleanField( "Display Middle Name", "Display the middle name of the person.", false, "", 7)]
    [CodeEditorField( "Custom Content", "Custom Content will be rendered after the person's demographic information <span class='tip tip-lava'></span>.",
        Rock.Web.UI.Controls.CodeEditorMode.Lava, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 200, false, "", "", 8, "CustomContent" )]
    [BooleanField( "Allow Following", "Should people be able to follow a person by selecting the star on the person's photo?", true, "", 9)]
    [BooleanField( "Display Tags", "Should tags be displayed?", true, "", 10 )]
    [BooleanField( "Display Graduation", "Should the Grade/Graduation be displayed", true, "", 11 )]
    [BooleanField( "Display Anniversary Date", "Should the Anniversary Date be displayed?", true, "", 12 )]
    [CategoryField( "Tag Category", "Optional category to limit the tags to. If specified all new personal tags will be added with this category.", false,
        "Rock.Model.Tag", "", "", false, "", "", 13 )]
    [AttributeCategoryField( "Social Media Category", "The Attribute Category to display attributes from", false, "Rock.Model.Person", false, "DD8F467D-B83C-444F-B04C-C681167046A1", "", 14 )]
    [BooleanField( "Enable Call Origination", "Should click-to-call links be added to phone numbers.", true, "", 14 )]
    [LinkedPage( "Communication Page", "The communication page to use for when the person's email address is clicked. Leave this blank to use the default.", false, "", "", 15 )]
    public partial class Bio : PersonBlock
    {

        #region Fields

        private const string NAME_KEY = "name";
        private const string ICONCSSCLASS_KEY = "iconcssclass";
        private const string COLOR_KEY = "color";
        private const string TEXT_TEMPLATE = "texttemplate";
        private const string BASEURL = "baseurl";

        #endregion

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

            if ( Person != null )
            {
                pnlFollow.Visible = GetAttributeValue( "AllowFollowing" ).AsBoolean();

                // Record Type - this is always "business". it will never change.
                if ( Person.RecordTypeValueId == DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id )
                {
                    var parms = new Dictionary<string, string>();
                    parms.Add( "businessId", Person.Id.ToString() );
                    NavigateToLinkedPage( "BusinessDetailPage", parms );
                }

                if ( Person.IsDeceased )
                {
                    divBio.AddCssClass( "deceased" );
                }

                // Set the browser page title to include person's name
                RockPage.BrowserTitle = Person.FullName;

                string badgeList = GetAttributeValue( "Badges" );
                if ( !string.IsNullOrWhiteSpace( badgeList ) )
                {
                    foreach ( string badgeGuid in badgeList.SplitDelimitedValues() )
                    {
                        Guid guid = badgeGuid.AsGuid();
                        if ( guid != Guid.Empty )
                        {
                            var personBadge = PersonBadgeCache.Get( guid );
                            if ( personBadge != null )
                            {
                                blStatus.PersonBadges.Add( personBadge );
                            }
                        }
                    }
                }

                lbEditPerson.Visible = IsUserAuthorized( Rock.Security.Authorization.EDIT );

                // only show if the Impersonation button if the feature is enabled, and the current user is authorized to Administrate the person
                bool enableImpersonation = this.GetAttributeValue( "EnableImpersonation" ).AsBoolean();
                lbImpersonate.Visible = false;
                if ( enableImpersonation && Person.Id != CurrentPersonId && Person.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, this.CurrentPerson ) )
                {
                    lbImpersonate.Visible = true;
                }
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
                if ( Person != null && Person.Id != 0 )
                {
                    SetPersonName();

                    // Setup Image
                    string imgTag = Rock.Model.Person.GetPersonPhotoImageTag( Person, 200, 200 );
                    if ( Person.PhotoId.HasValue )
                    {
                        lImage.Text = string.Format( "<a href='{0}'>{1}</a>", Person.PhotoUrl, imgTag );
                    }
                    else
                    {
                        lImage.Text = imgTag;
                    }

                    if ( GetAttributeValue( "AllowFollowing" ).AsBoolean() )
                    {
                        FollowingsHelper.SetFollowing( Person.PrimaryAlias, pnlFollow, this.CurrentPerson );
                    }

                    hlVCard.NavigateUrl = ResolveUrl( string.Format( "~/api/People/VCard/{0}", Person.Guid ) );

                    var socialCategoryGuid = GetAttributeValue( "SocialMediaCategory" ).AsGuidOrNull();
                    if ( socialCategoryGuid.HasValue )
                    {
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
                    }

                    if ( Person.BirthDate.HasValue )
                    {
                        var formattedAge = Person.FormatAge();
                        if ( formattedAge.IsNotNullOrWhiteSpace() )
                        {
                            formattedAge += " old";
                        }

                        lAge.Text = string.Format( "<dd>{0} <small>({1})</small></dd>", formattedAge, ( Person.BirthYear.HasValue && Person.BirthYear != DateTime.MinValue.Year ) ? Person.BirthDate.Value.ToShortDateString() : Person.BirthDate.Value.ToMonthDayString() );
                    }

                    lGender.Text = string.Format( "<dd>{0}</dd>", Person.Gender.ToString() );

                    if ( GetAttributeValue( "DisplayGraduation" ).AsBoolean() )
                    {
                        if ( Person.GraduationYear.HasValue && Person.HasGraduated.HasValue )
                        {
                            lGraduation.Text = string.Format(
                                "<dd><small>{0} {1}</small></dd>",
                                Person.HasGraduated.Value ? "Graduated " : "Graduates ",
                                Person.GraduationYear.Value );
                        }
                        lGrade.Text = Person.GradeFormatted;
                    }

                    if ( Person.AnniversaryDate.HasValue && GetAttributeValue("DisplayAnniversaryDate").AsBoolean() )
                    {
                        lMaritalStatus.Text = string.Format( "<dd>{0}",  Person.MaritalStatusValueId.DefinedValue() );
                        lAnniversary.Text = string.Format( "{0} yrs <small>({1})</small></dd>", Person.AnniversaryDate.Value.Age(), Person.AnniversaryDate.Value.ToMonthDayString() );
                    }
                    else
                    {
                        if ( Person.MaritalStatusValueId.HasValue )
                        {
                        lMaritalStatus.Text = string.Format( "<dd>{0}</dd>",  Person.MaritalStatusValueId.DefinedValue() );
                        }
                    }


                    if ( Person.PhoneNumbers != null )
                    {
                        rptPhones.DataSource = Person.PhoneNumbers.ToList();
                        rptPhones.DataBind();
                    }
                    
                    var communicationLinkedPageValue = this.GetAttributeValue( "CommunicationPage" );
                    Rock.Web.PageReference communicationPageReference;
                    if ( communicationLinkedPageValue.IsNotNullOrWhiteSpace() )
                    {
                        communicationPageReference = new Rock.Web.PageReference( communicationLinkedPageValue );
                    }
                    else
                    {
                        communicationPageReference = null;
                    }

                    lEmail.Text = Person.GetEmailTag( ResolveRockUrl( "/" ), communicationPageReference );

                    if ( GetAttributeValue( "DisplayTags" ).AsBoolean( true ) )
                    {
                        taglPersonTags.Visible = true;
                        taglPersonTags.EntityTypeId = Person.TypeId;
                        taglPersonTags.EntityGuid = Person.Guid;
                        taglPersonTags.CategoryGuid = GetAttributeValue( "TagCategory" ).AsGuidOrNull();
                        taglPersonTags.GetTagValues( CurrentPersonId );
                    }
                    else
                    {
                        taglPersonTags.Visible = false;
                    }

                    CreateActionMenu();

                    string customContent = GetAttributeValue( "CustomContent" );
                    if ( !string.IsNullOrWhiteSpace( customContent ) )
                    {
                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );
                        string resolvedContent = customContent.ResolveMergeFields( mergeFields );
                        phCustomContent.Controls.Add( new LiteralControl( resolvedContent ) );
                    }

                }
                else
                {
                    nbInvalidPerson.Visible = true;
                    pnlContent.Visible = false;
                }
            }
        }

        protected void CreateActionMenu()
        {
            StringBuilder sbActions = new StringBuilder();
            
            // First list the actions manually entered as html in the block settting
            var actions = GetAttributeValue( "Actions" );
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
            var workflowActions = GetAttributeValue( "WorkflowActions" );
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
                            if ( workflowType != null && (workflowType.IsActive ?? true) && workflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
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

        /// <summary>
        /// Sets the name of the person.
        /// </summary>
        private void SetPersonName()
        {
            // Check if this record represents a Business.
            bool isBusiness = false;

            if ( Person.RecordTypeValueId.HasValue )
            {
                int recordTypeValueIdBusiness = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;

                isBusiness = ( Person.RecordTypeValueId.Value == recordTypeValueIdBusiness );
            }

            // Get the Display Name.
            string nameText;

            if ( isBusiness )
            {
                nameText = Person.LastName;
            }
            else
            {
                if (GetAttributeValue( "DisplayMiddleName" ).AsBoolean() && !String.IsNullOrWhiteSpace(Person.MiddleName))
                {
                    nameText = string.Format( "<span class='first-word nickname'>{0}</span> <span class='middlename'>{1}</span> <span class='lastname'>{2}</span>", Person.NickName, Person.MiddleName, Person.LastName );
                }
                else
                {
                    nameText = string.Format( "<span class='first-word nickname'>{0}</span> <span class='lastname'>{1}</span>", Person.NickName, Person.LastName );
                }

                // Prefix with Title if they have a Title with IsFormal=True
                if ( Person.TitleValueId.HasValue )
                {
                    var personTitleValue = DefinedValueCache.Get( Person.TitleValueId.Value );
                    if ( personTitleValue != null && personTitleValue.GetAttributeValue( "IsFormal" ).AsBoolean() )
                    {
                        nameText = string.Format( "<span class='title'>{0}</span> ", personTitleValue.Value ) + nameText;
                    }
                }

                // Add First Name if different from NickName.
                if ( Person.NickName != Person.FirstName )
                {
                    if ( !string.IsNullOrWhiteSpace( Person.FirstName ) )
                    {
                        nameText += string.Format( " <span class='firstname'>({0})</span>", Person.FirstName );
                    }
                }

                // Add Suffix.
                if ( Person.SuffixValueId.HasValue )
                {
                    var suffix = DefinedValueCache.Get( Person.SuffixValueId.Value );
                    if ( suffix != null )
                    {
                        nameText += " " + suffix.Value;
                    }
                }

                // Add Previous Names. 
                using ( var rockContext = new RockContext() )
                {
                    var previousNames = Person.GetPreviousNames( rockContext ).Select( a => a.LastName );

                    if ( previousNames.Any() )
                    {
                        nameText += string.Format( Environment.NewLine + "<span class='previous-names'>(Previous Names: {0})</span>", previousNames.ToList().AsDelimited( ", " ) );
                    }
                }
            }

            lName.Text = nameText;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbEditPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEditPerson_Click( object sender, EventArgs e )
        {
            if ( Person != null )
            {
                Response.Redirect( string.Format( "~/Person/{0}/Edit", Person.Id ),false );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbImpersonate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbImpersonate_Click( object sender, EventArgs e )
        {
            if ( Person != null )
            {
                if ( Person.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, this.CurrentPerson ) )
                {
                    var impersonationToken = this.Person.GetImpersonationToken( RockDateTime.Now.AddMinutes( 5 ), 1, null );

                    // store the current user in Session["ImpersonatedByUser"] so that we can log back in as them from the Admin Bar
                    Session["ImpersonatedByUser"] = this.CurrentUser;

                    var qryParams = new Dictionary<string, string>();
                    qryParams.Add( "rckipid", impersonationToken );
                    if ( !string.IsNullOrEmpty( this.GetAttributeValue( "ImpersonationStartPage" ) ) )
                    {
                        NavigateToLinkedPage( "ImpersonationStartPage", qryParams );
                    }
                    else
                    {
                        NavigateToCurrentPageReference( qryParams );
                    }
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Formats the phone number.
        /// </summary>
        /// <param name="unlisted">if set to <c>true</c> [unlisted].</param>
        /// <param name="countryCode">The country code.</param>
        /// <param name="number">The number.</param>
        /// <param name="phoneNumberTypeId">The phone number type identifier.</param>
        /// <returns></returns>
        protected string FormatPhoneNumber( bool unlisted, object countryCode, object number, int phoneNumberTypeId, bool smsEnabled = false )
        {
            var originationEnabled = GetAttributeValue( "EnableCallOrigination" ).AsBoolean();

            string formattedNumber = "Unlisted";

            string cc = countryCode as string ?? string.Empty;
            string n = number as string ?? string.Empty;

            if ( !unlisted )
            {
                if ( GetAttributeValue( "DisplayCountryCode" ).AsBoolean() )
                {
                    formattedNumber = PhoneNumber.FormattedNumber( cc, n, true );
                }
                else
                {
                    formattedNumber = PhoneNumber.FormattedNumber( cc, n );
                }
            }

            var phoneType = DefinedValueCache.Get( phoneNumberTypeId );
            if ( phoneType != null )
            {
                string phoneMarkup = formattedNumber;

                if ( originationEnabled )
                {
                    var pbxComponent = Rock.Pbx.PbxContainer.GetAllowedActiveComponentWithOriginationSupport( CurrentPerson );

                    if ( pbxComponent != null )
                    {
                        var jsScript = string.Format( "javascript: Rock.controls.pbx.originate('{0}', '{1}', '{2}','{3}','{4}');", CurrentPerson.Guid, number.ToString(), CurrentPerson.FullName, Person.FullName, formattedNumber );
                        phoneMarkup = string.Format( "<a class='originate-call js-originate-call' href=\"{0}\">{1}</a>", jsScript, formattedNumber );
                    }
                    else if ( RockPage.IsMobileRequest ) // if the page is being loaded locally then add the tel:// link
                    {
                        formattedNumber = string.Format( "<a href=\"tel://{0}\">{1}</a>", n, formattedNumber );
                    }
                }                

                if ( smsEnabled )
                {
                    formattedNumber = string.Format( "{0} <small>{1} <i class='fa fa-comments'></i></small>", phoneMarkup, phoneType.Value );
                }
                else
                {
                    formattedNumber = string.Format( "{0} <small>{1}</small>", phoneMarkup, phoneType.Value );
                }

            }

            return formattedNumber;
        }

        #endregion
    }
}