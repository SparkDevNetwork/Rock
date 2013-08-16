//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// The main Person Profile blockthe main information about a peron 
    /// </summary>
    [ComponentsField( "Rock.PersonProfile.BadgeContainer, Rock", "Badges" )]
    public partial class Bio : Rock.Web.UI.PersonBlock
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
                Page.Title = CurrentPage.Title + ": " + Person.FullName;

                lName.Text = Person.FullName.FormatAsHtmlTitle();

                if ( Person.PhotoId.HasValue )
                {
                    var imgLink = new HtmlAnchor();
                    phImage.Controls.Add( imgLink );
                    imgLink.HRef = "~/image.ashx?" + Person.PhotoId.Value.ToString();
                    imgLink.Target = "_blank";

                    var img = new HtmlImage();
                    imgLink.Controls.Add( img );
                    img.Src = "~/image.ashx?" + Person.PhotoId.Value.ToString();
                    img.Alt = Person.FullName;
                }

                if ( Person.BirthDate.HasValue )
                    lAge.Text = string.Format( "{0} yrs old <small>({1})</small><br/>", Person.BirthDate.Value.Age(), Person.BirthDate.Value.ToString( "MM/dd" ) );

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

                tlPersonTags.EntityTypeId = Person.TypeId;
                tlPersonTags.EntityGuid = Person.Guid;

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