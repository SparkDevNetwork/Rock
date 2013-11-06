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
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Security;

namespace RockWeb.Blocks.Cms
{
    [MemoField( "Layout", "The layout of the Ad details", true, @"<h1>{{ MarketingCampaign.Title }}</h1><br/><br/>{{ SummaryText }}<br/><br/>{{ DetailHtml }}" )]
    public partial class MarketingCampaignAdDetail : Rock.Web.UI.RockBlock
    {
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                int? adId = PageParameter( "Ad" ).AsInteger();
                if ( adId.HasValue )
                {
                    MarketingCampaignAd ad = new MarketingCampaignAdService().Get( adId.Value );
                    if ( ad != null )
                    {
                        // TODO: Still need to add checks for Ad approval, ad type, start/end date etc.

                        var dict = ad.ToLiquid() as Dictionary<string, object>;
                        
                        string layout = GetAttributeValue( "Layout" );
                        string mergedLayout = layout.ResolveMergeFields( dict );

                        phDetails.Controls.Add( new LiteralControl( mergedLayout ) );
                    }
                }
            }
        }
    }
}

