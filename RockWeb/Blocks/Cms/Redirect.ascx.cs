using System;

namespace RockWeb.Blocks.Cms
{
    [Rock.Attribute.Property( 0, "Url", "The path to redirect to" )]
    public partial class Redirect : Rock.Cms.CmsBlock
    {
        protected override void OnInit( EventArgs e )
        {
            if ( !string.IsNullOrEmpty( AttributeValue("Url") ) )
                Response.Redirect( AttributeValue("Url") );
            base.OnInit( e );
        }
    }
}