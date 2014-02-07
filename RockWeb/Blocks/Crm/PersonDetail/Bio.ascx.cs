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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
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
    public partial class Bio : PersonBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddCSSLink( ResolveRockUrl( "~/Styles/fluidbox.css" ) );
            RockPage.AddScriptLink( ResolveRockUrl( "~/Scripts/imagesloaded.min.js" ) );
            RockPage.AddScriptLink( ResolveRockUrl( "~/Scripts/jquery.fluidbox.js" ) );

            if ( Person != null )
            {
                // Set the browser page title to include person's name
                RockPage.BrowserTitle = Person.FullName;

                lName.Text = Person.FullName.FormatAsHtmlTitle();

                // Setup Image
                var imgTag = new LiteralControl( Rock.Model.Person.GetPhotoImageTag( Person.PhotoId, Person.Gender, 188, 188 ) );
                if ( Person.PhotoId.HasValue )
                {
                    var imgLink = new HyperLink();
                    imgLink.Attributes.Add( "href", Person.PhotoUrl );
                    phImage.Controls.Add( imgLink );
                    imgLink.Controls.Add( imgTag );
                }
                else
                {
                    phImage.Controls.Add( imgTag );
                }

                if ( Person.BirthDate.HasValue )
                {
                    string ageText = ( Person.BirthYear.HasValue && Person.BirthYear != DateTime.MinValue.Year ) ?
                        string.Format( "{0} yrs old ", Person.BirthDate.Value.Age() ) : string.Empty;
                    lAge.Text = string.Format( "{0}<small>({1})</small><br/>", ageText, Person.BirthDate.Value.ToString( "MM/dd" ) );
                }

                lGender.Text = Person.Gender.ToString();

                lMaritalStatus.Text = Person.MaritalStatusValueId.DefinedValue();
                if ( Person.AnniversaryDate.HasValue )
                    lAnniversary.Text = string.Format( "{0} yrs <small>({1})</small>", Person.AnniversaryDate.Value.Age(), Person.AnniversaryDate.Value.ToString( "MM/dd" ) );

                if ( Person.PhoneNumbers != null )
                {
                    rptPhones.DataSource = Person.PhoneNumbers.ToList();
                    rptPhones.DataBind();
                }

                lEmail.Text = Person.Email;

                taglPersonTags.EntityTypeId = Person.TypeId;
                taglPersonTags.EntityGuid = Person.Guid;
                taglPersonTags.GetTagValues( CurrentPersonId );

                if (!Page.IsPostBack)
                {
                    string badgeList = GetAttributeValue( "Badges" );
                    if (!string.IsNullOrWhiteSpace(badgeList))
                    {
                        var personBadgeService = new PersonBadgeService();
                        foreach ( string badgeGuid in badgeList.SplitDelimitedValues() ) 
                        {
                            Guid guid = badgeGuid.AsGuid();
                            if (guid != Guid.Empty)
                            {
                                var personBadge = personBadgeService.Get( guid );
                                if (personBadge != null)
                                {
                                    personBadge.LoadAttributes();
                                    blStatus.PersonBadges.Add( personBadge );
                                }
                            }
                        }
                    }
                }

                // Every person should have an alias record with same id.  If it's missing, create it
                if ( !Person.Aliases.Any( a => a.AliasPersonId == Person.Id ) )
                {
                    var personService = new PersonService();
                    var person = personService.Get( Person.Id );
                    if ( person != null )
                    {
                        person.Aliases.Add( new PersonAlias { AliasPersonId = person.Id, AliasPersonGuid = person.Guid } );
                        personService.Save( person, CurrentPersonAlias );
                        Person = person;
                    }
                }
            }
        }

        protected void lbEditPerson_Click( object sender, EventArgs e )
        {
            if ( Person != null )
            {
                Response.Redirect( string.Format( "~/Person/{0}/Edit", Person.Id ) );
            }
        }

    }
}