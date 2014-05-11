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
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
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
    [PersonBadgesField("Badges")]
    [LinkedPage( "Business Detail Page" )]
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

                // Set the browser page title to include person's name
                RockPage.BrowserTitle = Person.FullName;
            }

        }

        protected override void OnLoad(EventArgs e)
        {
 	        base.OnLoad(e);

            if ( !Page.IsPostBack )
            {
                if ( Person != null )
                {
                    var rockContext = new RockContext();

                    if ( Person.NickName == Person.FirstName )
                    {
                        lName.Text = Person.FullName.FormatAsHtmlTitle();
                    }
                    else
                    {
                        lName.Text = String.Format( "{0} <span class='full-name'>({1})</span> {2}", Person.NickName.FormatAsHtmlTitle(), Person.FirstName, Person.LastName );
                    }

                    // Setup Image
                    string imgTag = Rock.Model.Person.GetPhotoImageTag( Person.PhotoId, Person.Gender, 200, 200 );
                    if ( Person.PhotoId.HasValue )
                    {
                        lImage.Text = string.Format("<a href='{0}'>{1}</a>", Person.PhotoUrl, imgTag);
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
                        var result = attributes.Join( Person.AttributeValues, a => a.Key, v => v.Key, ( a, v ) => new { Attribute = a.Value, Values = v.Value } );

                        rptSocial.DataSource = result
                            .Where( r =>
                                r.Values.Count > 0 &&
                                r.Values[0].Value != string.Empty )
                            .OrderBy( r => r.Attribute.Order )
                            .Select( r => new
                            {
                                url = r.Values[0].Value,
                                name = r.Attribute.Name,
                                icon = r.Attribute.IconCssClass
                            } )
                            .ToList();
                        rptSocial.DataBind();
                    }

                    if ( Person.PhoneNumbers != null )
                    {
                        rptPhones.DataSource = Person.PhoneNumbers.ToList();
                        rptPhones.DataBind();
                    }

                    if ( Person.BirthDate.HasValue )
                    {
                        string ageText = ( Person.BirthYear.HasValue && Person.BirthYear != DateTime.MinValue.Year ) ?
                            string.Format( "{0} yrs old ", Person.BirthDate.Value.Age() ) : string.Empty;
                        lAge.Text = string.Format( "{0}<small>({1})</small><br/>", ageText, Person.BirthDate.Value.ToString( "MM/dd" ) );
                    }

                    lGender.Text = Person.Gender.ToString();

                    if ( Person.GraduationDate.HasValue )
                    {
                        lGraduation.Text = string.Format( "{0} {1}",
                            Person.GraduationDate.Value.CompareTo( RockDateTime.Today ) <= 0 ? "Graduated " : "Graduates ",
                            Person.GraduationDate.Value.Year );

                        string grade = Person.GradeFormatted;
                        if ( grade != string.Empty )
                        {
                            lGrade.Text = string.Format( "<small>({0})</small>", grade );
                        }
                    }

                    lMaritalStatus.Text = Person.MaritalStatusValueId.DefinedValue();
                    if ( Person.AnniversaryDate.HasValue )
                        lAnniversary.Text = string.Format( "{0} yrs <small>({1})</small>", Person.AnniversaryDate.Value.Age(), Person.AnniversaryDate.Value.ToString( "MM/dd" ) );

                    if ( Person.PhoneNumbers != null )
                    {
                        rptPhones.DataSource = Person.PhoneNumbers.ToList();
                        rptPhones.DataBind();
                    }

                    if ( !string.IsNullOrWhiteSpace( Person.Email ) )
                    {
                        if ( !Person.IsEmailActive.HasValue || Person.IsEmailActive.Value )
                        {
                            switch ( Person.EmailPreference )
                            {
                                case EmailPreference.EmailAllowed:
                                    {
                                        lEmail.Text = string.Format( "<a href='{0}?person={1}'>{2}</a>",
                                            ResolveRockUrl( "/Communication" ), Person.Id, Person.Email );
                                        break;
                                    }
                                case EmailPreference.NoMassEmails:
                                    {
                                        lEmail.Text = string.Format( "<span class='js-email-status email-status no-mass-email' data-toggle='tooltip' data-placement='top' title='Email Preference is set to \"No Mass Emails\"'><a href='{0}?person={1}'>{2}</a> <i class='fa fa-exchange'></i></span>",
                                            ResolveRockUrl( "/Communication" ), Person.Id, Person.Email );
                                        break;
                                    }
                                case EmailPreference.DoNotEmail:
                                    {
                                        lEmail.Text = string.Format( "<span class='js-email-status email-status do-not-email' data-toggle='tooltip' data-placement='top' title='Email Preference is set to \"Do Not Email\"'>{0} <i class='fa fa-ban'></i></span>", Person.Email );
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            lEmail.Text = string.Format( "<span class='js-email-status not-active email-status' data-toggle='tooltip' data-placement='top' title='Email is not active. {0}'>{1} <i class='fa fa-exclamation-triangle'></i></span>",
                                Person.EmailNote, Person.Email );
                        }
                    }

                    taglPersonTags.EntityTypeId = Person.TypeId;
                    taglPersonTags.EntityGuid = Person.Guid;
                    taglPersonTags.GetTagValues( CurrentPersonId );

                    if ( !Page.IsPostBack )
                    {
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
                    }

                    // Every person should have an alias record with same id.  If it's missing, create it
                    if ( !Person.Aliases.Any( a => a.AliasPersonId == Person.Id ) )
                    {
                        var personService = new PersonService( rockContext );
                        var person = personService.Get( Person.Id );
                        if ( person != null )
                        {
                            person.Aliases.Add( new PersonAlias { AliasPersonId = person.Id, AliasPersonGuid = person.Guid } );
                            rockContext.SaveChanges();
                            Person = person;
                        }
                    }
                }
            }
        }

        #endregion

        #region Events

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
        /// <param name="countryCode">The country code.</param>
        /// <param name="number">The number.</param>
        /// <returns></returns>
        protected string FormatPhoneNumber(object countryCode, object number)
        {
            string cc = countryCode as string ?? string.Empty;
            string n = number as string ?? string.Empty;
            return PhoneNumber.FormattedNumber(cc, n);
        }

        private void SetFollowing()
        {
            var personEntityType = EntityTypeCache.Read( "Rock.Model.Person" );
            if ( Person != null && CurrentPersonId.HasValue && CurrentPersonAlias != null && personEntityType != null )
            {
                if ( new FollowingService( new RockContext() ).Queryable()
                    .Where( f =>
                        f.EntityTypeId == personEntityType.Id &&
                        f.EntityId == Person.Id &&
                        f.PersonAlias.PersonId == CurrentPersonId )
                    .Any())
                {
                    pnlFollow.AddCssClass( "following" );
                }
                else
                {
                    pnlFollow.RemoveCssClass( "following" );
                }

                string script = string.Format( @"
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
        }});
    ", personEntityType.Id, Person.Id, CurrentPersonId.Value, CurrentPersonAlias.Id );
                ScriptManager.RegisterStartupScript( lImage, lImage.GetType(), "following", script, true );
            }
        }

        #endregion

}
}