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
    [DisplayName( "Series List Lava" )]
    [Category( "CCV > Podcast" )]
    [Description( "Presents the available Podcasts in a desired Series Category" )]
    [CodeEditorField( "Lava Template", "The lava template to use to format the page.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true)]
    [LinkedPage( "Series Detail Page", "Optional page to link to if a series is clicked.", false )]
    [LinkedPage( "Message Detail Page", "Optional page to link to if a message is clicked.", false  )]
    [IntegerField( "Items Per Page", "The number of series per page to display.", false, 12 )]
    [IntegerField( "Podcast Category", "The category ID to use for listing podcasts (Weekend Series, Group Videos, etc.)", false, PodcastUtil.WeekendVideos_CategoryId )]
    public partial class SeriesListLava : Rock.Web.UI.RockBlock
    {        
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            int pageNum = PageParameter( "PageNum" ).AsInteger( );
            int numPerPage = PageParameter( "NumPerPage" ).AsInteger( );

            int numItems = int.Parse( GetAttributeValue( "ItemsPerPage" ) );
            int podcastCategory = int.Parse( GetAttributeValue( "PodcastCategory" ) );

            ShowDetail( pageNum, numPerPage != 0 ? numPerPage : numItems, podcastCategory );
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
        protected void ShowDetail( int pageNum, int numPerPage, int podcastCategory )
        {
            // to improve performance, get the minimum number of series required to fulfill this request, which would be (pageNum + 1) * numPerPage
            int numSeries = (pageNum + 1) * numPerPage;

            // wrap this in a try / catch so that we handle the category not existing, or it failing to load due to the user not
            // having view permissions. In any failure case, we'll simply render no lava objects.
            try
            {
                PodcastUtil.PodcastCategory podcastSeriesList = PodcastUtil.PodcastsAsModel( podcastCategory, false, numSeries, CurrentPersonAliasId.HasValue ? CurrentPersonAliasId.Value : 0 );

                PodcastUtil.PodcastCategory pagedPodcastList = new PodcastUtil.PodcastCategory( podcastSeriesList.Name, podcastSeriesList.Id );
                pagedPodcastList.Children = podcastSeriesList.Children.Skip( pageNum * numPerPage ).Take( numPerPage ).ToList( );

                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                mergeFields.Add( "SeriesList", pagedPodcastList );
                mergeFields.Add( "FirstSeries", podcastSeriesList.Children [ 0 ] );

                Dictionary<string, object> linkedPages = new Dictionary<string, object>();
                linkedPages.Add( "SeriesDetailPage", LinkedPageUrl( "SeriesDetailPage", null ) );
                linkedPages.Add( "MessageDetailPage", LinkedPageUrl( "MessageDetailPage", null ) );
                mergeFields.Add( "LinkedPages", linkedPages );

                string template = GetAttributeValue( "LavaTemplate" );

                lContent.Text = template.ResolveMergeFields( mergeFields );
            }
            catch
            {
                // don't do anything special--the lava objects will simply be empty.
            }
        }
        #endregion
    }
}
