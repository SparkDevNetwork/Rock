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
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    public partial class Bio : Rock.Web.UI.PersonBlock
    {
        protected string RecordStatus = string.Empty;

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( Person != null )
            {
                lName.Text = Person.FullName;
                lPersonStatus.Text = Person.PersonStatusValueId.DefinedValue();
                RecordStatus = Person.RecordStatusValueId.DefinedValue();

                // Campus is associated with the family group(s) person belongs to.
                var families = PersonGroups( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
                if ( families != null )
                {
                    var campusNames = new List<string>();
                    foreach ( int campusId in families
                        .Where( g => g.CampusId.HasValue )
                        .Select( g => g.CampusId )
                        .Distinct()
                        .ToList() )
                        campusNames.Add( Rock.Web.Cache.CampusCache.Read( campusId ).Name );
                    lCampus.Text = campusNames.OrderBy( n => n ).ToList().AsDelimited( ", " );
                }

                // TODO : Determine how neighborhood is calculated
                lNeighborhood.Text = "Neighborhood";

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
                tlPersonTags.EntityId = Person.Id;
            }
        }
    }
}