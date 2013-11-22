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

using System.Text;
using System.Text.RegularExpressions;

using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Cms
{
    [CodeEditorField( "Layout", "The layout of the Ad details", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 200, true, @"
<h1>{{ MarketingCampaign.Title }}</h1><br/><br/>
{{ SummaryText }}<br/><br/>
{{ DetailHtml }}
" )]
    public partial class ViewAdDetail : Rock.Web.UI.RockBlock
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
                        string content = layout.ResolveMergeFields( dict );

                        // check for errors
                        if (content.Contains("No such template"))
                        {
                            // get template name
                            Match match = Regex.Match(GetAttributeValue("Template"), @"'([^']*)");
                            if (match.Success)
                            {
                                content = String.Format("<div class='alert alert-warning'><h4>Warning</h4>Could not find the template _{1}.liquid in {0}.</div>", ResolveRockUrl("~~/Assets/Liquid"), match.Groups[1].Value);
                            }
                            else
                            {
                                content = "<div class='alert alert-warning'><h4>Warning</h4>Unable to parse the template name from settings.</div>";
                            }
                        }

                        if (content.Contains("error"))
                        {
                            content = "<div class='alert alert-warning'><h4>Warning</h4>" + content + "</div>";
                        }

                        phDetails.Controls.Add( new LiteralControl( content ) );
                    }
                }
            }
        }
    }
}

