//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using Rock;
using Rock.Crm;
using Rock.Web.UI;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    public partial class Bio : Rock.Web.UI.PersonBlock
    {
        protected string RecordStatus = string.Empty;

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            using ( new Rock.Data.UnitOfWorkScope() )
            {
                var workflowTypeService = new Rock.Util.WorkflowTypeService();
                var workflowType = workflowTypeService.Get( 1 );

                var workflowService = new Rock.Util.WorkflowService();
                //var workflow = workflowService.Activate( workflowType, "Test", CurrentPersonId );
                var workflow = workflowService.Get( 1 );
                workflowService.Process( workflow, CurrentPersonId );
                workflowService.Save( workflow, CurrentPersonId );
            }

            // Name
            var page = Page as RockPage;
            if ( page != null )
                page.SetTitle( Person.FullName );

            if ( Person.PhotoId.HasValue )
            {
                var imgLink = new HtmlAnchor();
                phImage.Controls.Add( imgLink );
                imgLink.HRef = "~/image.ashx?" + Person.PhotoId.Value.ToString();
                imgLink.Target = "_blank";

                var img = new HtmlImage();
                imgLink.Controls.Add( img );
                img.Src = string.Format( "~/image.ashx?{0}&maxwidth=165&maxheight=165", Person.PhotoId.Value );
                img.Alt = Person.FullName;
            }

            lPersonStatus.Text = Person.PersonStatusId.DefinedValue();
            RecordStatus = Person.RecordStatusId.DefinedValue();

            var families = PersonGroups( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
            if ( families != null )
            {
                var campusNames = new List<string>();
                foreach(int campusId in families
                    .Where( g => g.CampusId.HasValue)
                    .Select( g => g.CampusId)
                    .ToList())
                    campusNames.Add(Rock.Web.Cache.CampusCache.Read(campusId).Name);
                lCampus.Text = campusNames.OrderBy( n => n ).ToList().AsDelimited( ", " );
            }

            if ( Person.BirthDate.HasValue)
                lAge.Text = string.Format( "{0} yrs old <em>{1}</em>", Person.BirthDate.Value.Age(), Person.BirthDate.Value.ToString( "MM/dd" ) );
    
            lGender.Text = Person.Gender.ToString();

            lMaritalStatus.Text = Person.MaritalStatusId.DefinedValue();
            if ( Person.AnniversaryDate.HasValue )
                lAnniversary.Text = string.Format( "{0} yrs <em>{1}</em>", Person.AnniversaryDate.Value.Age(), Person.AnniversaryDate.Value.ToString( "MM/dd" ) );
        }
    }
}