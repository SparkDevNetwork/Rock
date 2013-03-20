//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//


using System;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance.Administration
{
    public partial class PledgeDetail : RockBlock, IDetailBlock
    {
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
        }

        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            throw new NotImplementedException();
        }

        protected void btnBackToList_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }
    }   
}