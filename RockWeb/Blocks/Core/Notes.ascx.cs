//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Web.UI;

using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Core
{
    [ContextAware]
    [BlockProperty( 1, "Attribute Name", "Behavior", "The name of the notes attribute", false, "Notes" )]
    public partial class Notes : RockBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            //Verify Attributes



        }


        protected void Page_Load( object sender, EventArgs e )
        {

        }
    }
}