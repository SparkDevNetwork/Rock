// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// The main Person Profile block the main information about a peron 
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
    [LinkedPage( "Business Detail Page", "The page to redirect user to if a business is is requested.", false, "", "", 3 )]
    [BooleanField( "Display Country Code", "When enabled prepends the country code to all phone numbers." )]
    public partial class Bio : PersonBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddCSSLink( ResolveRockUrl( "~/Styles/fluidbox.css" ) );
            RockPage.AddScriptLink( ResolveRockUrl( "~/Scripts/imagesloaded.min.js" ) );
            RockPage.AddScriptLink( ResolveRockUrl( "~/Scripts/jquery.fluidbox.min.js" ) );

            if ( Person != null )
            {
                // Record Type - this is always "business". it will never change.
                if ( Person.RecordTypeValueId == DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id )
                {
                    var parms = new Dictionary<string, string>();
                    parms.Add( "businessId", Person.Id.ToString() );
                    NavigateToLinkedPage( "BusinessDetailPage", parms );
                }

                if ( Person.IsDeceased ?? false )
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
                            var personBadge = PersonBadgeCache.Read( guid );
                            if ( personBadge != null )
                            {
                                blStatus.PersonBadges.Add( personBadge );
                            }
                        }
                    }
                }

                lbEditPerson.Visible = IsUserAuthorized( Rock.Security.Authorization.EDIT );
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
                    if ( Person.NickName == Person.FirstName )
                    {
                        lName.Text = Person.FullName.FormatAsHtmlTitle();
                    }
                    else
                    {
                        lName.Text = string.Format(
                            "{0} {2} <span class='full-name'>{1}</span>",
                            Person.NickName.FormatAsHtmlTitle(),
                            string.IsNullOrWhiteSpace( Person.FirstName ) ? string.Empty : string.Format( "({0})", Person.FirstName ),
                            Person.LastName );

                        using ( var rockContext = new RockContext() )
                        {
                            var previousNames = Person.GetPreviousNames( rockContext ).Select( a => a.LastName );
                            if ( previousNames.Any() )
                            {
                                lName.Text += string.Format( Environment.NewLine + "<span class='previous-names'>(Previous Names: {0})</span>", previousNames.ToList().AsDelimited( ", " ) );
                            }
                        }
                    }

                    // Setup Image
                    string imgTag = Rock.Model.Person.GetPhotoImageTag( Person.PhotoId, Person.Age, Person.Gender, 200, 200 );
                    if ( Person.PhotoId.HasValue )
                    {
                        lImage.Text = string.Format( "<a href='{0}'>{1}</a>", Person.PhotoUrl, imgTag );
                    }
                    else
                    {
                        lImage.Text = imgTag;
                    }

                    SetFollowing();

                    var socialCategoryGuid = Rock.SystemGuid.Category.PERSON_ATTRIBUTES_SOCIAL.AsGuid();
                    if ( !socialCategoryGuid.IsEmpty() )
                    {
                        var attributes = Person.Attributes.Where( p => p.Value.Categories.Select( c => c.Guid ).Contains( socialCategoryGuid ) );
                        var result = attributes.Join( Person.AttributeValues, a => a.Key, v => v.Key, ( a, v ) => new { Attribute = a.Value, Value = v.Value } );

                        rptSocial.DataSource = result
                            .Where( r =>
                                r.Value != null &&
                                r.Value.Value != string.Empty )
                            .OrderBy( r => r.Attribute.Order )
                            .Select( r => new
                            {
                                url = r.Value.Value,
                                name = r.Attribute.Name,
                                icon = r.Attribute.IconCssClass
                            } )
                            .ToList();
                        rptSocial.DataBind();
                    }

                    if ( Person.BirthDate.HasValue )
                    {
                        string ageText = ( Person.BirthYear.HasValue && Person.BirthYear != DateTime.MinValue.Year ) ?
                            string.Format( "{0} yrs old ", Person.BirthDate.Value.Age() ) : string.Empty;
                        lAge.Text = string.Format( "{0}<small>({1})</small><br/>", ageText, Person.BirthDate.Value.ToMonthDayString() );
                    }

                    lGender.Text = Person.Gender.ToString();

                    if ( Person.GraduationYear.HasValue && Person.HasGraduated.HasValue )
                    {
                        lGraduation.Text = string.Format(
                            "<small>({0} {1})</small>",
                            Person.HasGraduated.Value ? "Graduated " : "Graduates ",
                            Person.GraduationYear.Value );
                    }

                    lGrade.Text = Person.GradeFormatted;

                    lMaritalStatus.Text = Person.MaritalStatusValueId.DefinedValue();
                    if ( Person.AnniversaryDate.HasValue )
                    {
                        lAnniversary.Text = string.Format( "{0} yrs <small>({1})</small>", Person.AnniversaryDate.Value.Age(), Person.AnniversaryDate.Value.ToMonthDayString() );
                    }

                    if ( Person.PhoneNumbers != null )
                    {
                        rptPhones.DataSource = Person.PhoneNumbers.ToList();
                        rptPhones.DataBind();
                    }

                    lEmail.Text = Person.GetEmailTag( ResolveRockUrl( "/" ) );

                    taglPersonTags.EntityTypeId = Person.TypeId;
                    taglPersonTags.EntityGuid = Person.Guid;
                    taglPersonTags.GetTagValues( CurrentPersonId );

                    StringBuilder sbActions = new StringBuilder();
                    var workflowActions = GetAttributeValue( "WorkflowActions" );
                    if ( !string.IsNullOrWhiteSpace( workflowActions ) )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            var workflowTypeService = new WorkflowTypeService( rockContext );
                            foreach ( string guidValue in workflowActions.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                            {
                                Guid? guid = guidValue.AsGuidOrNull();
                                if ( guid.HasValue )
                                {
                                    var workflowType = workflowTypeService.Get( guid.Value );
                                    if ( workflowType != null && workflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                                    {
                                        string url = string.Format( "~/WorkflowEntry/{0}?PersonId={1}", workflowType.Id, Person.Id );
                                        sbActions.AppendFormat(
                                            "<li><a href='{0}'><i class='{1}'></i> {2}</a></li>",
                                            ResolveRockUrl( url ),
                                            workflowType.IconCssClass,
                                            workflowType.Name );
                                        sbActions.AppendLine();
                                    }
                                }
                            }
                        }
                    }

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

                        sbActions.Append( actions );
                    }

                    lActions.Text = sbActions.ToString();
                    ulActions.Visible = !string.IsNullOrWhiteSpace( lActions.Text );
                }
                else
                {
                    nbInvalidPerson.Visible = true;
                    pnlContent.Visible = false;
                }
            }
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
                Response.Redirect( string.Format( "~/Person/{0}/Edit", Person.Id ) );
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
        protected string FormatPhoneNumber( bool unlisted, object countryCode, object number, int phoneNumberTypeId )
        {
            string formattedNumber = "Unlisted";
            if ( !unlisted )
            {
                string cc = countryCode as string ?? string.Empty;
                string n = number as string ?? string.Empty;

                if ( GetAttributeValue( "DisplayCountryCode" ).AsBoolean() )
                {
                    formattedNumber = PhoneNumber.FormattedNumber( cc, n, true );
                }
                else
                {
                    formattedNumber = PhoneNumber.FormattedNumber( cc, n );
                }
            }

            var phoneType = DefinedValueCache.Read( phoneNumberTypeId );
            if ( phoneType != null )
            {
                return string.Format( "{0} <small>{1}</small>", formattedNumber, phoneType.Value );
            }

            return formattedNumber;
        }

        /// <summary>
        /// Sets the following.
        /// </summary>
        private void SetFollowing()
        {
            var personAliasEntityType = EntityTypeCache.Read( "Rock.Model.PersonAlias" );
            if ( Person != null && CurrentPersonId.HasValue && CurrentPersonAlias != null && personAliasEntityType != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var personAliasService = new PersonAliasService( rockContext );
                    var followingService = new FollowingService( rockContext );

                    var paQry = personAliasService.Queryable()
                        .Where( p => p.PersonId == Person.Id )
                        .Select( p => p.Id );

                    if ( followingService.Queryable()
                        .Where( f =>
                            f.EntityTypeId == personAliasEntityType.Id &&
                            paQry.Contains( f.EntityId ) &&
                            f.PersonAlias.PersonId == CurrentPersonId )
                        .Any() )
                    {
                        pnlFollow.AddCssClass( "following" );
                    }
                    else
                    {
                        pnlFollow.RemoveCssClass( "following" );
                    }
                }

                string script = string.Format(
@"
        $('.following-status').click(function () {{
            var $followingDiv = $(this);
            if ($followingDiv.hasClass('following')) {{
                $.ajax({{
                    type: 'DELETE',
                    url: Rock.settings.get('baseUrl') + 'api/followings/{0}/{1}/{2}',
                    success: function(data, status, xhr){{ 
                        $followingDiv.removeClass('following');
                    }},
                }});
            }} else {{
                var following = {{ EntityTypeId:{0}, EntityId:{1}, PersonAliasId:{3} }};
                $.ajax({{
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify(following),
                    url: Rock.settings.get('baseUrl') + 'api/followings',
                    statusCode: {{
                        201: function () {{
                            $followingDiv.addClass('following');
                        }}
                    }}
                }});
            }}
        }});",
                personAliasEntityType.Id,
                Person.PrimaryAliasId,
                CurrentPersonId.Value,
                CurrentPersonAlias.Id );

                ScriptManager.RegisterStartupScript( lImage, lImage.GetType(), "following", script, true );
            }
        }

        #endregion
    }
}