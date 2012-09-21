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

namespace RockWeb.Blocks.Crm.PersonDetail
{
    public partial class Bio : Rock.Web.UI.PersonBlock
    {
		protected string RecordStatus = string.Empty;

		protected override void OnInit( EventArgs e )
		{
			base.OnInit( e );

			// Name
			var page = Page as Rock.Web.UI.Page;
			if ( page != null )
				page.SetTitle( Person.FullName );

			if ( Person.PhotoId.HasValue )
			{
				var img = new HtmlImage();
				phImage.Controls.Add( img );
				img.Src = string.Format( "~/image.ashx?id={0}&maxwidth=165&maxheight=165", Person.PhotoId.Value );
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
					campusNames.Add(Rock.Web.Cache.Campus.Read(campusId).Name);
				lCampus.Text = campusNames.OrderBy( n => n ).ToList().AsDelimited( ", " );
			}

			if ( Person.BirthDate.HasValue)
				lAge.Text = string.Format( "{0} yrs old <em>{1}</em>", Person.BirthDate.Age(), Person.BirthDate.Value.ToString( "MM/dd" ) );
	
			lGender.Text = Person.Gender.ToString();

			lMaritalStatus.Text = Person.MaritalStatusId.DefinedValue();
			if ( Person.AnniversaryDate.HasValue )
				lAnniversary.Text = string.Format( "{0} yrs <em>{1}</em>", Person.AnniversaryDate.Age(), Person.AnniversaryDate.Value.ToString("MM/dd") );
        }
    }
}