using System;
using System.ComponentModel;
using Rock.Attribute;
using Rock.Model;

namespace RockWeb.Plugins.cc_newspring.Blocks.Video
{
    /// <summary>
    /// All Church Metrics Block
    /// </summary>
    [DisplayName( "Ooyala Video Block" )]
    [Category( "NewSpring" )]
    [Description( "Ooyala Video Block" )]
    [TextField( "Ooyala Content ID", "Paste the Ooyala Content ID here" )]
    public partial class Ooyala : Rock.Web.UI.RockBlock
    {
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            // Set the ooyala id
            ooyalaId.Value = GetAttributeValue( "OoyalaContentID" );
        }
    }
}