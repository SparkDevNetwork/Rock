//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Web.UI;

using Rock;
using Rock.CRM;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    public partial class Bio : Rock.Web.UI.PersonBlock
    {
		protected string RecordStatus = string.Empty;

        protected void Page_Load( object sender, EventArgs e )
        {
			// Name
			var page = Page as Rock.Web.UI.Page;
			if ( page != null )
				page.SetTitle( Person.FullName );

			lPersonStatus.Text = Person.PersonStatusId.DefinedValue();
			RecordStatus = Person.RecordStatusId.DefinedValue();
			
			if ( Person.BirthDate.HasValue)
				lAge.Text = string.Format( "{0} yrs old <em>{1}</em>", Person.BirthDate.Age(), Person.BirthDate.Value.ToString( "MM/dd" ) );
	
			lGender.Text = Person.Gender.ToString();

			lMaritalStatus.Text = Person.MaritalStatusId.DefinedValue();
			if ( Person.AnniversaryDate.HasValue )
				lAnniversary.Text = string.Format( "{0} yrs <em>{1}</em>", Person.AnniversaryDate.Age(), Person.AnniversaryDate.Value.ToString("MM/dd") );
        }
    }
}