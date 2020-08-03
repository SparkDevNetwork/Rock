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

using Humanizer;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Web.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

using Newtonsoft.Json;

namespace RockWeb.Blocks.Cms
{
    [DisplayName( "Cache Manager" )]
    [Category( "CMS" )]
    [Description( "Block used to view cache statistics and clear the existing cache." )]
    public partial class CacheManager : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // need to check user permissions and add adjust grid and other options accordingly
            gCacheTagList.DataKeyNames = new string[] { "DefinedValueId" };
            gCacheTagList.GridRebind += new GridRebindEventHandler( gCacheTagList_GridRebind );
            gCacheTagList.Actions.AddClick += gCacheTagList_Add;
            gCacheTagList.Actions.ShowExcelExport = false;
            gCacheTagList.Actions.ShowMergeTemplate = false;

            if ( IsUserAuthorized( Authorization.EDIT ) )
            {
                gCacheTagList.Actions.ShowAdd = true;
            }
            else
            {
                gCacheTagList.Actions.ShowAdd = false;
            }
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load( object sender, EventArgs e )
        {
            ClearNotifications();

            if ( !IsPostBack )
            {
                BindGrid();
                PopulateDdlCacheTypes();
                PopulateCacheStatistics();
                PopulateRedisView();

                rcbEnableStatistics.Checked = SystemSettings.GetValueFromWebConfig( Rock.SystemKey.SystemSetting.CACHE_MANAGER_ENABLE_STATISTICS ).AsBoolean();
            }
        }

        /// <summary>
        /// Displays the notification.
        /// </summary>
        /// <param name="notificationBox">The notification box.</param>
        /// <param name="message">The message.</param>
        /// <param name="notificationBoxType">Type of the notification box.</param>
        protected void DisplayNotification( NotificationBox notificationBox, string message, NotificationBoxType notificationBoxType )
        {
            notificationBox.Text = message;
            notificationBox.NotificationBoxType = notificationBoxType;
            notificationBox.Visible = true;
        }

        /// <summary>
        /// Clear and hide all notification boxes.
        /// </summary>
        protected void ClearNotifications()
        {
            nbMessage.Text = string.Empty;
            nbMessage.NotificationBoxType = NotificationBoxType.Info;
            nbMessage.Visible = false;

            nbModalMessage.Text = string.Empty;
            nbModalMessage.NotificationBoxType = NotificationBoxType.Info;
            nbModalMessage.Visible = false;

            nbRedisSettings.Text = string.Empty;
            nbRedisSettings.NotificationBoxType = NotificationBoxType.Info;
            nbRedisSettings.Visible = false;
        }

        #region Grid

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            int cacheTagDefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.CACHE_TAGS ).Id;
            RockContext rockContext = new RockContext();
            DefinedValueService definedValueService = new DefinedValueService( rockContext );
            var cacheTags = definedValueService.Queryable().Where( v => v.DefinedTypeId == cacheTagDefinedTypeId ).ToList();

            var gridData = new List<CacheTagGridRow>();

            foreach ( var tag in cacheTags )
            {
                // do something here to get linked keys count
                long linkedKeys = RockCache.GetCountOfCachedItemsForTag( tag.Value );
                var row = new CacheTagGridRow
                {
                    TagName = tag.Value,
                    TagDescription = tag.Description,
                    LinkedKeys = linkedKeys,
                    DefinedValueId = tag.Id
                };

                gridData.Add( row );
            }

            gCacheTagList.DataSource = gridData;
            gCacheTagList.DataBind();
        }

        /// <summary>
        /// Handles the Add event of the gCacheTagList control, shows the add tag modal.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gCacheTagList_Add( object sender, EventArgs e )
        {
            ClearModal( false );
            dlgAddTag.Show();
        }

        /// <summary>
        /// Handles the ClearCacheTag event of the gCacheTagList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gCacheTagList_ClearCacheTag( object sender, RowEventArgs e )
        {
            var definedValueId = e.RowKeyId;
            var definedValue = DefinedValueCache.Get( definedValueId );
            RockCache.RemoveForTags( definedValue.Value );
            DisplayNotification( nbMessage, string.Format( "Removed cached items tagged with \"{0}\".", definedValue.Value ), NotificationBoxType.Success );
        }

        /// <summary>
        /// Handles the GridRebind event of the gCacheTagList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gCacheTagList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// POCO to list tags on the CacheManager grid
        /// </summary>
        private class CacheTagGridRow
        {
            /// <summary>
            /// Gets or sets the name of the tag.
            /// </summary>
            /// <value>
            /// The name of the tag.
            /// </value>
            public string TagName { get; set; }

            /// <summary>
            /// Gets or sets the tag description.
            /// </summary>
            /// <value>
            /// The tag description.
            /// </value>
            public string TagDescription { get; set; }

            /// <summary>
            /// Gets or sets the linked keys.
            /// </summary>
            /// <value>
            /// The linked keys.
            /// </value>
            public long LinkedKeys { get; set; }

            /// <summary>
            /// Gets or sets the DefinedValue identifier for the tag
            /// </summary>
            /// <value>
            /// The defined value identifier.
            /// </value>
            public int DefinedValueId { get; set; }
        }

        #endregion

        #region Redis

        /// <summary>
        /// Populates the Redis View.
        /// </summary>
        protected void PopulateRedisView()
        {
            // clear and hide edit
            ClearAndHideRedisEdit();
            
            RedisEndPointAvailabilityCheck();

            redisView.Visible = true;

            bool enabled = RockCache.IsCacheSerialized;
            if ( !enabled )
            {
                DisplayNotification( nbRedisSettings, "Redis is currently not enabled. Review documentation for more information on enabling Redis backplane support.", NotificationBoxType.Info );
                redisEnabled.Visible = false;
                return;
            }

            redisEnabled.Visible = true;

            string redisPassword = SystemSettings.GetValueFromWebConfig( Rock.SystemKey.SystemSetting.REDIS_PASSWORD ) ?? string.Empty;

            string serverList = string.Join(
                string.Empty,
                SystemSettings.GetValueFromWebConfig( Rock.SystemKey.SystemSetting.REDIS_ENDPOINT_LIST )
                    .Split( ',' )
                    .Select( s => 
                    (
                        RockCache.IsEndPointAvailable( s, redisPassword ) == true ? 
                            "<span class='label label-success'>" + s + " Endpoint is available </span><br />" : 
                            "<span class='label label-warning'>" + s + " Endpoint is not available </span><br />" 
                    ) ) );

            // show and populate view
            cbEnabled.Checked = enabled;
            lEndPointList.Text = serverList;
            lblPassword.Text = SystemSettings.GetValueFromWebConfig( Rock.SystemKey.SystemSetting.REDIS_PASSWORD ).IsNullOrWhiteSpace() ? string.Empty : "***********";
            lblDatabaseNumber.Text = SystemSettings.GetValueFromWebConfig( Rock.SystemKey.SystemSetting.REDIS_DATABASE_NUMBER );
        }

        /// <summary>
        /// Populates the Redis Edit view.
        /// </summary>
        protected void PopulateRedisEdit()
        {
            ClearAndHideRedisView();
            redisEdit.Visible = true;
            DisplayNotification( nbRedisSettings, "After clicking save Rock will be unavailable for a few minutes while the current cache is cleared and the configurations are reloaded.", NotificationBoxType.Warning );
            RedisEndPointAvailabilityCheck();

            cbEnabledEdit.Checked = SystemSettings.GetValueFromWebConfig( Rock.SystemKey.SystemSetting.REDIS_ENABLE_CACHE_CLUSTER ).AsBoolean();

            var keyValuePairs = new List<ListItems.KeyValuePair>();
            var endPoints = SystemSettings.GetValueFromWebConfig( Rock.SystemKey.SystemSetting.REDIS_ENDPOINT_LIST ).Split( ',' );
            foreach ( var endPoint in endPoints )
            {
                keyValuePairs.Add( new ListItems.KeyValuePair { Value = endPoint } );
            }

            var endPointsValue = JsonConvert.SerializeObject( keyValuePairs );
            liEndPoints.Value = endPointsValue;

            tbPassword.Text = SystemSettings.GetValueFromWebConfig( Rock.SystemKey.SystemSetting.REDIS_PASSWORD );
            tbDatabaseNumber.Text = SystemSettings.GetValueFromWebConfig( Rock.SystemKey.SystemSetting.REDIS_DATABASE_NUMBER );
        }

        /// <summary>
        /// Clears and hides the Redis Edit view.
        /// </summary>
        protected void ClearAndHideRedisEdit()
        {
            redisEdit.Visible = false;
            cbEnabledEdit.Checked = false;
            liEndPoints.Value = string.Empty;
            tbPassword.Text = string.Empty;
            tbDatabaseNumber.Text = string.Empty;
        }

        /// <summary>
        /// Clears and hides the Redis View.
        /// </summary>
        protected void ClearAndHideRedisView()
        {
            redisView.Visible = false;
            cbEnabled.Checked = false;
            lEndPointList.Text = string.Empty;
            lblPassword.Text = string.Empty;
            lblDatabaseNumber.Text = string.Empty;
        }

        /// <summary>
        /// Handles the Click event of the btnEditRedis control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEditRedis_Click( object sender, EventArgs e )
        {
            PopulateRedisEdit();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveRedis control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveRedis_Click( object sender, EventArgs e )
        {
            string serverList = string.Empty;
            var keyValuePairs = JsonConvert.DeserializeObject<List<Rock.Web.UI.Controls.ListItems.KeyValuePair>>( liEndPoints.Value );
            bool endPointsHaveAtLeastOneValue = false;

            foreach ( var keyValuePair in keyValuePairs )
            {
                serverList += keyValuePair.Value + ",";
                endPointsHaveAtLeastOneValue = keyValuePair.Value.Trim().IsNotNullOrWhiteSpace() ? true : endPointsHaveAtLeastOneValue;
            }

            serverList = serverList.TrimEnd( ',' );

            if ( cbEnabledEdit.Checked )
            {
                if ( !endPointsHaveAtLeastOneValue )
                {
                    DisplayNotification( nbRedisSettings, "At least one Redis endpoint must be entered.", NotificationBoxType.Warning );
                    return;
                }
                else if ( !IsRedisAvailable( serverList.Split( ',' ) ) )
                {
                    DisplayNotification( nbRedisSettings, "None of the Redis endpoints entered are available.", NotificationBoxType.Warning );
                    return;
                }
            }

            Dictionary<string, string> settings = new Dictionary<string, string>();
            settings.Add( Rock.SystemKey.SystemSetting.REDIS_ENABLE_CACHE_CLUSTER, cbEnabledEdit.Checked.ToString() );
            settings.Add( Rock.SystemKey.SystemSetting.REDIS_ENDPOINT_LIST, serverList );
            settings.Add( Rock.SystemKey.SystemSetting.REDIS_PASSWORD, tbPassword.Text );
            settings.Add( Rock.SystemKey.SystemSetting.REDIS_DATABASE_NUMBER, tbDatabaseNumber.Text );

            SystemSettings.SetValueToWebConfig( settings );

            Response.Redirect( Request.RawUrl );
        }

        /// <summary>
        /// Handles the Click event of the btnCancelRedis control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelRedis_Click( object sender, EventArgs e )
        {
            ClearAndHideRedisEdit();
            PopulateRedisView();
        }

        /// <summary>
        /// returns true if at least one of the endpoints is available.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is redis available]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsRedisAvailable( string[] endPoints )
        {
            string redisPassword = SystemSettings.GetValueFromWebConfig( Rock.SystemKey.SystemSetting.REDIS_PASSWORD ) ?? string.Empty;
            int endPointErrorCount = 0;
            
            foreach ( var endPoint in endPoints )
            {
                endPointErrorCount += RockCache.IsEndPointAvailable( endPoint, redisPassword ) == true ? 0 : 1;
            }

            return endPointErrorCount == endPoints.Length ? false : true;
        }

        /// <summary>
        /// Checks the availability of the configured Redis endpoints and updates the status label.
        /// </summary>
        private void RedisEndPointAvailabilityCheck()
        {
            bool redisEnabled = SystemSettings.GetValueFromWebConfig( Rock.SystemKey.SystemSetting.REDIS_ENABLE_CACHE_CLUSTER ).AsBooleanOrNull() ?? false;

            if ( !redisEnabled )
            {
                spRedisStatus.Visible = false;
                return;
            }

            spRedisStatus.Visible = true;

            string[] endPoints = SystemSettings.GetValueFromWebConfig( Rock.SystemKey.SystemSetting.REDIS_ENDPOINT_LIST ).Split( ',' );
            int endPointErrorCount = 0;

            string redisPassword = SystemSettings.GetValueFromWebConfig( Rock.SystemKey.SystemSetting.REDIS_PASSWORD ) ?? string.Empty;

            foreach ( var endPoint in endPoints )
            {
                endPointErrorCount += RockCache.IsEndPointAvailable( endPoint, redisPassword ) == true ? 0 : 1;
            }

            if ( endPointErrorCount == 0)
            {
                spRedisStatus.Attributes["class"] = "pull-right label label-success";
                spRedisStatus.InnerText = "All Redis endpoints are available";
            }
            else if ( endPointErrorCount < endPoints.Length )
            {
                spRedisStatus.Attributes["class"] = "pull-right label label-warning";
                spRedisStatus.InnerText = string.Format( "{0} of {1} Redis endpoints cannot connect", endPointErrorCount, endPoints.Length );
            }
            else
            {
                spRedisStatus.Attributes["class"] = "pull-right label label-danger";
                spRedisStatus.InnerText = string.Format( "All {0} {1} cannot connect", endPoints.Length, "endpoint".ToQuantity( endPoints.Length ) );
            }
        }

        #endregion

        #region Cache Statistics

        /// <summary>
        /// Populates the drop down list with an ordered list of cache type names.
        /// The selected item is used for the button to clear all the cache of
        /// that type.
        /// </summary>
        protected void PopulateDdlCacheTypes()
        {
            ddlCacheTypes.Items.Clear();
            ddlCacheTypes.Items.Add( new ListItem( "All Cached Items", "all" ) );

            var cacheStats = RockCache.GetAllStatistics();
            foreach ( CacheItemStatistics cacheItemStat in cacheStats.OrderBy( s => s.Name ) )
            {
                ddlCacheTypes.Items.Add( new ListItem( cacheItemStat.Name, cacheItemStat.Name ) );
            }
        }

        /// <summary>
        /// Handles the Click event of btnClearCache and clears the selected cache type.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnClearCache_Click( object sender, EventArgs e )
        {
            List<string> result = new List<string>();

            if ( ddlCacheTypes.SelectedValue == "all" )
            {
                result = RockCache.ClearAllCachedItems();
            }
            else
            {
                result.Add( RockCache.ClearCachedItemsForType( ddlCacheTypes.SelectedValue ) );
            }

            PopulateCacheStatistics();

            DisplayNotification( nbMessage, "All cached items have been cleared.", NotificationBoxType.Success );
        }

        /// <summary>
        /// Computes and displays a summary of cache statistics.
        /// </summary>
        protected void PopulateCacheStatistics()
        {
            long hits = 0;
            long misses = 0;
            long adds = 0;
            long gets = 0;
            long clears = 0;

            var cacheItemStatistics = new List<CacheItemStatistics>();

            if ( ddlCacheTypes.SelectedValue == "all" )
            {
                cacheItemStatistics = RockCache.GetAllStatistics().OrderBy( s => s.Name ).ToList();
            }
            else
            {
                cacheItemStatistics.Add( RockCache.GetStatisticsForType( ddlCacheTypes.SelectedValue ) );
            }

            foreach ( CacheItemStatistics cacheItemStat in cacheItemStatistics )
            {
                foreach ( CacheHandleStatistics cacheHandleStat in cacheItemStat.HandleStats )
                {
                    hits += cacheHandleStat.Stats.Where( s => s.CounterType.ConvertToString().ToLower() == "hits" ).Select( s => s.Count ).FirstOrDefault();
                    misses += cacheHandleStat.Stats.Where( s => s.CounterType.ConvertToString().ToLower() == "misses" ).Select( s => s.Count ).FirstOrDefault();
                    adds += cacheHandleStat.Stats.Where( s => s.CounterType.ConvertToString().ToLower() == "add calls" ).Select( s => s.Count ).FirstOrDefault();
                    adds += cacheHandleStat.Stats.Where( s => s.CounterType.ConvertToString().ToLower() == "put calls" ).Select( s => s.Count ).FirstOrDefault();
                    gets += cacheHandleStat.Stats.Where( s => s.CounterType.ConvertToString().ToLower() == "get calls" ).Select( s => s.Count ).FirstOrDefault();
                    clears += cacheHandleStat.Stats.Where( s => s.CounterType.ConvertToString().ToLower() == "clear calls" ).Select( s => s.Count ).FirstOrDefault();
                    clears += cacheHandleStat.Stats.Where( s => s.CounterType.ConvertToString().ToLower() == "clear region calls" ).Select( s => s.Count ).FirstOrDefault();
                }
            }

            string htmlText = "<tr><td>Hits</td><td>{0:N0}</td><tr><td>Misses</td><td>{1:N0}</td></tr><tr><td>Adds</td><td>{2:N0}</td></tr><tr><td>Gets</td><td>{3:N0}</td></tr><tr><td>Clears</td><td>{4:N0}</td></tr>";
            lCacheStatistics.Text = string.Format( htmlText, hits, misses, adds, gets, clears );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlCacheTypes control and updates statistics for the selected cache type.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlCacheTypes_SelectedIndexChanged( object sender, EventArgs e )
        {
            PopulateCacheStatistics();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the rcbEnableStatistics control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rcbEnableStatistics_CheckedChanged(object sender, EventArgs e)
        {
            // Save updated value.
            SystemSettings.SetValueToWebConfig( Rock.SystemKey.SystemSetting.CACHE_MANAGER_ENABLE_STATISTICS, rcbEnableStatistics.Checked.ToString() );

            // Reload this page to trigger application restart due to modification of web.config.
            Response.Redirect( Request.RawUrl );
        }

        #endregion

        #region Add Tag Modal

        /// <summary>
        /// Handles the SaveClick event of the dlgAddTag control.
        /// Adds a DefinedValue for DefinedType of CacheTags
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgAddTag_SaveClick( object sender, EventArgs e )
        {
            if ( IsValid() )
            {
                SaveTag();
                ClearModal();
                BindGrid();
            }
        }

        /// <summary>
        /// Verifies tag name is valid and unique.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </returns>
        private bool IsValid()
        {
            // Don't need to check for an edited tag as only the description is being changed.
            if(hfTagId.Value.IsNotNullOrWhiteSpace() )
            {
                return true;
            }

            string tagName = tbTagName.Text.Trim();

            if ( tagName.IsNullOrWhiteSpace() )
            {
                DisplayNotification( nbModalMessage, string.Format( "Tag name is required.", tagName ), NotificationBoxType.Warning );
                return false;
            }

            // see if the tag exists
            int cachedTagDefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.CACHE_TAGS ).Id;
            var rockContext = new RockContext();
            var definedValueService = new DefinedValueService( rockContext );

            if ( definedValueService.Queryable().AsNoTracking().Where( v => v.DefinedTypeId == cachedTagDefinedTypeId && v.Value == tbTagName.Text ).Any() )
            {
                // show key in use error
                DisplayNotification( nbModalMessage, string.Format( "Tag name {0} already in use.", tagName ), NotificationBoxType.Warning );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Saves the tag, caller is responsible for validation.
        /// </summary>
        private void SaveTag()
        {
            if (hfTagId.Value == string.Empty)
            {
                SaveNewTag();
            }
            else
            {
                UpdateExistingTag( hfTagId.ValueAsInt() );
                DefinedValueCache.Clear();
            }
        }

        /// <summary>
        /// Saves a new tag.
        /// </summary>
        private void SaveNewTag()
        {
            int cachedTagDefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.CACHE_TAGS ).Id;
            var rockContext = new RockContext();
            var definedValueService = new DefinedValueService( rockContext );
            int order = 0;

            if ( definedValueService.Queryable().AsNoTracking().Where( v => v.DefinedTypeId == cachedTagDefinedTypeId ).Any() )
            {
                order = definedValueService.Queryable().AsNoTracking().Where( v => v.DefinedTypeId == cachedTagDefinedTypeId ).Max( v => v.Order ) + 1;
            }

            var definedValue = new DefinedValue
            {
                DefinedTypeId = cachedTagDefinedTypeId,
                Value = tbTagName.Text.Trim().ToLower(),
                Description = tbTagDescription.Text,
                Order = order
            };

            definedValueService.Add( definedValue );
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Updates and exsiting tag.
        /// </summary>
        /// <param name="cacheTagId">The Id of the tag.</param>
        private void UpdateExistingTag(int cacheTagId )
        {
            using ( var rockContext = new RockContext() )
            {
                var definedValueService = new DefinedValueService( rockContext );
                var cacheTagDefinedValue = definedValueService.Get( cacheTagId );
                cacheTagDefinedValue.Description = tbTagDescription.Text.Trim();
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Clears and hides the modal used for adding/editing tags.
        /// </summary>
        /// <param name="hideAfterClearing">Controls whether the modal is hidden after being cleared.</param>
        protected void ClearModal( bool hideAfterClearing = true )
        {
            tbTagName.Text = string.Empty;
            tbTagName.Enabled = true;
            tbTagDescription.Text = string.Empty;
            hfTagId.Value = string.Empty;
            if ( hideAfterClearing )
            {
                dlgAddTag.Hide();
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gCacheTagList_RowSelected control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gCacheTagList_RowSelected( object sender, RowEventArgs e )
        {
            var definedValueId = e.RowKeyId;
            hfTagId.Value = definedValueId.ToString();

            var definedValue = DefinedValueCache.Get( definedValueId );

            tbTagDescription.Text = definedValue.Description;
            tbTagName.Text = definedValue.Value;
            tbTagName.Enabled = false;
            dlgAddTag.Show();
        }

        #endregion

    }
}