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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

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

        if ( IsUserAuthorized( Authorization.EDIT ))
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

        if (!IsPostBack)
        {
            BindGrid();
            PopulateDdlCacheTypes();
            PopulateCacheStatistics();
        }
    }

    #region Grid

    /// <summary>
    /// Binds the grid.
    /// </summary>
    private void BindGrid()
    {
        int cacheTagDefinedTypeId = CacheDefinedType.Get( Rock.SystemGuid.DefinedType.CACHE_TAGS ).Id;
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
        var definedValue = CacheDefinedValue.Get( definedValueId );
        RockCache.RemoveForTags( definedValue.Value );
        DisplayNotification( nbMessage, string.Format( "Remove Cache command for tab \"{0}\" sent.", definedValue.Value), NotificationBoxType.Success );
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

    #endregion

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
    }

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

        DisplayNotification( nbMessage, result.Aggregate( (a, b) => a + Environment.NewLine + b ), NotificationBoxType.Success );
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

        string htmlText = "Hits: {0:N0}<br />Misses: {1:N0}<br />Adds: {2:N0}<br />Gets: {3:N0}<br />Clears: {4:N0}";
        lCacheStatistics.Text = string.Format( htmlText, hits, misses, adds, gets, clears );
    }

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
            DisplayNotification( nbMessage, string.Format( "tag {0} added successfully.", tbTagName.Text.Trim().ToLower() ), NotificationBoxType.Success );
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
        string tagName = tbTagName.Text.Trim();

        if ( tagName.IsNullOrWhiteSpace() )
        {
            DisplayNotification( nbModalMessage, string.Format( "Tag name is required.", tagName ), NotificationBoxType.Warning );
            return false;
        }
        
        // see if the tag exists
        int cachedTagDefinedTypeId = CacheDefinedType.Get( Rock.SystemGuid.DefinedType.CACHE_TAGS ).Id;
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
        int cachedTagDefinedTypeId = CacheDefinedType.Get( Rock.SystemGuid.DefinedType.CACHE_TAGS ).Id;
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
    /// Clears the and hides modal.
    /// </summary>
    protected void ClearModal()
    {
        tbTagName.Text = string.Empty;
        tbTagDescription.Text = string.Empty;
        dlgAddTag.Hide();
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

    /// <summary>
    /// Handles the SelectedIndexChanged event of the ddlCacheTypes control and updates statistics for the selected cache type.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    protected void ddlCacheTypes_SelectedIndexChanged( object sender, EventArgs e )
    {
        PopulateCacheStatistics();
    }
}