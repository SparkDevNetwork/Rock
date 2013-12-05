//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using Rock.Attribute;

namespace RockWeb.Blocks.Cms
{
    [TextField( "Url", "The path to redirect to" )]
    public partial class Redirect : Rock.Web.UI.RockBlock
    {
        protected override void OnInit( EventArgs e )
        {
            if ( !string.IsNullOrEmpty( GetAttributeValue( "Url" ) ) )
            {
                Response.Redirect( GetAttributeValue( "Url" ), false );
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
            base.OnInit( e );
        }
    }
}