// <copyright>
// Copyright by the Spark Development Network
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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using church.ccv.Podcast;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.church_ccv.Podcast
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Message Detail Lava" )]
    [Category( "CCV > Podcast" )]
    [Description( "Presents the given Podcast Message Detail" )]
    [CodeEditorField( "Lava Template", "The lava template to use to format the page.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true)]
    [LinkedPage( "Series List Page" )]
    [LinkedPage( "Series Detail Page" )]
    public partial class MessageDetailLava : Rock.Web.UI.RockBlock
    {        
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            ShowDetail( PageParameter( "MessageId" ).AsInteger() );
        }
        
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
        }
        #region Methods
        
        /// Displays the view group  using a lava template
        /// 
        protected void ShowDetail( int messageId )
        {
            PodcastUtil.PodcastMessage podcastMessage = PodcastUtil.GetMessage( messageId);
            
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "PodcastMessage", podcastMessage );

            Dictionary<string, object> linkedPages = new Dictionary<string, object>();
            linkedPages.Add("SeriesListPage", LinkedPageUrl("SeriesListPage", null));
            linkedPages.Add("SeriesDetailPage", LinkedPageUrl("SeriesDetailPage", null));
            mergeFields.Add("LinkedPages", linkedPages);
    
            string template = GetAttributeValue( "LavaTemplate" );
            
            // now set the main HTML, including lava merge fields.
            lContent.Text = template.ResolveMergeFields( mergeFields );
        }
        #endregion
    }
}
