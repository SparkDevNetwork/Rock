// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
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
using System.ComponentModel;
using Rock.Data;

namespace RockWeb.Blocks.Cms
{
    [DisplayName("Ad Detail")]
    [Category("CMS")]
    [Description("Displays the details of an ad for public consuption.")]
    [CodeEditorField( "Layout", "The layout of the Ad details", CodeEditorMode.Liquid, CodeEditorTheme.Rock, 200, true, @"{% include 'AdDetail' %}" )]
    public partial class AdDetail : Rock.Web.UI.RockBlock
    {
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                int? adId = PageParameter( "Ad" ).AsIntegerOrNull();
                if ( adId.HasValue )
                {
                    MarketingCampaignAd ad = new MarketingCampaignAdService( new RockContext() ).Get( adId.Value );
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

