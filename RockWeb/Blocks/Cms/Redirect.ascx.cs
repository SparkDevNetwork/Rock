//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;

using Rock.Web.UI;

namespace RockWeb.Blocks.Cms
    
    [BlockProperty( 0, "Url", "The path to redirect to", true )]
    public partial class Redirect : Rock.Web.UI.RockBlock
        
        protected override void OnInit( EventArgs e )
            
            if ( !string.IsNullOrEmpty( AttributeValue("Url") ) )
                Response.Redirect( AttributeValue("Url") );
            base.OnInit( e );
        }
    }
}