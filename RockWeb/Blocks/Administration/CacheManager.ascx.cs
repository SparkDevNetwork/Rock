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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.SystemKey;
using Rock.Utility.Settings.DataAutomation;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

[DisplayName( "Cache Manager" )]
[Category( "Administration" )]
[Description( "Block used to view cache statistics and clear the existing cache." )]
public partial class CacheManager : RockBlock
{
    protected override void OnInit( EventArgs e )
    {
        base.OnInit( e );

        // need to check user permissions and add adjust grid and other options accordingly
        gCacheTagList.DataKeyNames = new string[] { "Id" };
        gCacheTagList.GridRebind += new GridRebindEventHandler( gCacheTagList_GridRebind );
        gCacheTagList.Actions.AddClick += gCacheTagList_Add;

        if( IsUserAuthorized( Authorization.EDIT ))
        {
            gCacheTagList.Actions.ShowAdd = true;
        }
    }

    protected void Page_Load( object sender, EventArgs e )
    {
        if(!IsPostBack)
        {
            //BindGrid();
            PopulateDdlCacheTypes();
            PopulateCacheStatistics();
        }
    }

    #region Grid
    private void BindGrid()
    {
        RockContext rockContext = new RockContext();
        DefinedTypeService definedTypeService = new DefinedTypeService( rockContext );

    }


    protected void gCacheTagList_Add( object sender, EventArgs e )
    {
        dlgAddTag.Show();
    }

    protected void gCacheTagList_RowDataBound( object sender, GridViewRowEventArgs e )
    {

    }

    protected void gCacheTagList_RowSelected( object sender, RowEventArgs e )
    {

    }

    protected void gCacheTagList_ClearCacheTag( object sender, RowEventArgs e )
    {

    }

    protected void gCacheTagList_GridRebind( object sender, EventArgs e )
    {
        BindGrid();
    }

    #endregion

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
        if ( ddlCacheTypes.SelectedValue == "all" )
        {
            RockCache.ClearAllCachedItems();
        }
        else
        {
            RockCache.ClearCachedItemsForType( ddlCacheTypes.SelectedValue );
        }
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

        foreach ( CacheItemStatistics cacheItemStat in RockCache.GetAllStatistics().OrderBy( s => s.Name ) )
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

        bool isValid = true;

        // make sure key is unique

        if ( isValid )
        {
            SaveTag();
            ClearModal();
        }
        else
        {
            // show error
        }
    }

    private bool isValid()
    {
        if( tbTagName.Text.Trim().IsNullOrWhiteSpace() )
        {
            // show key required error
            return false;
        }
        
        // see if the tag exists
        int cachedTagDefinedTypeId = CacheDefinedType.Get( Rock.SystemGuid.DefinedType.CACHE_TAGS ).Id;
        var rockContext = new RockContext();
        var definedValueService = new DefinedValueService( rockContext );

        if ( definedValueService.Queryable().AsNoTracking().Where( v => v.Value == tbTagName.Text ).Any() )
        {
            // show key in use error
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
        int order = definedValueService.Queryable().AsNoTracking().Where( v => v.DefinedTypeId == cachedTagDefinedTypeId ).Max( v => v.Order ) + 1;

        var definedValue = new DefinedValue
        {
            DefinedTypeId = cachedTagDefinedTypeId,
            Value = tbTagName.Text.ToLower(),
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
}