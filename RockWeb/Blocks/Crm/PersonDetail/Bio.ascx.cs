//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Linq;
using System.Text;
using System.Web.UI.HtmlControls;

using Rock;
using Rock.Attribute;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// The main Person Profile blockthe main information about a peron 
    /// </summary>
    [ComponentsField( "Rock.PersonProfile.BadgeContainer, Rock", "Badges" )]
    public partial class Bio : PersonBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( Person != null )
            {
                // Set the browser page title to include person's name
                var pageCache = PageCache.Read( RockPage.PageId );
                if ( pageCache != null )
                {
                    Page.Title = pageCache.Title + ": " + Person.FirstLastName;
                }
                else
                {
                    Page.Title = Person.FirstLastName;
                }

                lName.Text = Person.FirstLastName.FormatAsHtmlTitle();

                
                var imgLink = new HtmlAnchor();
                phImage.Controls.Add(imgLink);
                imgLink.HRef = Person.PhotoUrl;
                imgLink.Target = "_blank";

                var img = new HtmlImage();
                imgLink.Controls.Add(img);
                img.Src = Person.PhotoUrl;
                img.Alt = Person.FullName;
                

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

                blStatus.ComponentGuids = GetAttributeValue( "Badges" );
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