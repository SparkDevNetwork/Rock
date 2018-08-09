// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;
using Rock.Model;
using Rock.Web.UI;
using Rock.Data;
using church.ccv.PersonalizationEngine.Data;
using church.ccv.PersonalizationEngine.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock;

namespace RockWeb.Plugins.church_ccv.PersonalizationEngine
{
    [DisplayName( "Dashboard Center Card" )]
    [Category( "CCV > Personalization Engine" )]
    [Description( "Displays personalized data on the dashboard's center card." )]
    [CodeEditorField( "Lava Template", "The lava template to use to format the group list.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, "{% include '~~/Assets/Lava/GroupListSidebar.lava' %}", "", 6 )]
    public partial class DashboardCenterCard : RockBlock
    {
        #region Properties
        
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );   
        }
        
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            
            var relevantCampaign = PersonalizationEngineUtil.GetRelevantCampaign( new Campaign.CampaignType[] { Campaign.CampaignType.DashboardCard }, CurrentPerson.Id );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            string campaignImage = "/Themes/church_ccv_External_v8/Assets/Images/dashboard/feature-dashboard-next-step-go-on-a-trip.jpg";
            string campaignTitle = "Impact The World";
            string campaignSubTitle = "Go on a Mission Trip";
            string campaignBody = "Jesus calls us to spread His gospel throughout the world. Have you considered going on a Mission Trip?";
            string campaignLinkText = "Learn More";
            string campaignLink = "/ministries/missions";

            if( relevantCampaign != null )
            {
                JObject jsonBlob = JObject.Parse( relevantCampaign.ContentJson );
                    
                campaignImage =  jsonBlob["img"].ToString( );
                campaignTitle = jsonBlob["title"].ToString( );
                campaignSubTitle = jsonBlob["sub-title"].ToString( );
                campaignBody = jsonBlob["body"].ToString( );
                campaignLinkText = jsonBlob["link-text"].ToString( );
                campaignLink = jsonBlob["link"].ToString( );
            }

            mergeFields.Add( "CampaignImage", campaignImage );
            mergeFields.Add( "CampaignTitle", campaignTitle );
            mergeFields.Add( "CampaignSubTitle", campaignSubTitle );
            mergeFields.Add( "CampaignBody", campaignBody );
            mergeFields.Add( "CampaignLinkText", campaignLinkText );
            mergeFields.Add( "CampaignLink", campaignLink );

            string template = GetAttributeValue( "LavaTemplate" );
            lContent.Text = template.ResolveMergeFields( mergeFields );
        }
        
        #endregion
    }
}