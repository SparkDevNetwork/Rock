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
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Web.Cache;
using Rock.Model;
using Rock.Web.UI;
using System.Reflection;
using Rock.Web.UI.Controls;
using Rock.Security;

namespace RockWeb.Blocks.Cms
{
    [DisplayName( "Cache Viewer" )]
    [Category( "CMS" )]
    [Description( "Block used to view a cached entity for debugging purposes." )]
    [Rock.SystemGuid.BlockTypeGuid( "7D54B356-D0AF-4D10-96BE-26DE4F38CEBF" )]
    public partial class CacheManager : RockBlock
    {
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
            if ( !IsUserAuthorized( Authorization.ADMINISTRATE ) )
            {
                // This block essentially allows full database read access (for models that are cached)
                upnlContent.Visible = false;
                base.OnLoad( e );
                return;
            }

            if ( !IsPostBack )
            {
                PopulateCacheTypes();
            }

            base.OnLoad( e );
        }

        #region Cache Types

        /// <summary>
        /// Populates the drop down list with an ordered list of cache type names.
        /// The selected item is used for the button to clear all the cache of
        /// that type.
        /// </summary>
        protected void PopulateCacheTypes()
        {
            ddlCacheTypes.Items.Clear();
            var entityCacheTypes = RockCache.GetAllModelCacheTypes();

            foreach ( var entityCacheType in entityCacheTypes.OrderBy( s => s.Name ) )
            {
                ddlCacheTypes.Items.Add( new ListItem( entityCacheType.Name, entityCacheType.Name ) );
            }
        }

        #endregion Cache Types

        #region Events

        /// <summary>
        /// Handles the Click event of the btnRefresh control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRefresh_Click( object sender, EventArgs e )
        {
            ClearNotification();

            var cacheTypeName = ddlCacheTypes.SelectedValue;

            if ( cacheTypeName.IsNullOrWhiteSpace() || !cacheTypeName.EndsWith( "Cache" ) )
            {
                ShowError( "Please select a valid cache type" );
                return;
            }

            var cacheType = Type.GetType( string.Format( "Rock.Web.Cache.{0},Rock", cacheTypeName ) );

            if ( cacheType == null )
            {
                ShowError( string.Format( "{0} type could not be loaded", cacheTypeName ) );
                return;
            }

            var modelTypeName = cacheTypeName.Replace( "Cache", string.Empty );
            var modelType = Type.GetType( string.Format( "Rock.Model.{0},Rock", modelTypeName ) );

            if ( modelType == null )
            {
                ShowError( string.Format( "{0} type could not be loaded", modelTypeName ) );
                return;
            }

            var entityCacheType = typeof( EntityCache<,> ).MakeGenericType( cacheType, modelType );

            if ( entityCacheType == null )
            {
                ShowError( string.Format( "Cache type of {0} and {1} could not be loaded", cacheTypeName, modelTypeName ) );
                return;
            }

            var entityId = rtbEntityId.Text.AsIntegerOrNull();

            if ( !entityId.HasValue )
            {
                ShowError( "Please enter a valid ID" );
                return;
            }

            var cachedItem = entityCacheType.InvokeMember( "Get", BindingFlags.InvokeMethod, null, null, new object[] { entityId.Value } );

            if ( cachedItem == null )
            {
                ShowInfo( string.Format( "The {0} with ID {1} was not found", modelType.Name, entityId ) );
            }

            preResult.InnerText = cachedItem.ToJson( indentOutput: true );
        }

        #endregion Events

        #region Notifications

        /// <summary>
        /// Clears the notification.
        /// </summary>
        private void ClearNotification()
        {
            nbNotification.Visible = false;
        }

        /// <summary>
        /// Shows the information.
        /// </summary>
        /// <param name="text">The text.</param>
        private void ShowInfo( string text )
        {
            nbNotification.Visible = true;
            nbNotification.Title = string.Empty;
            nbNotification.Text = text;
            nbNotification.NotificationBoxType = NotificationBoxType.Info;
        }

        /// <summary>
        /// Shows the error.
        /// </summary>
        /// <param name="text">The text.</param>
        private void ShowError( string text )
        {
            nbNotification.Visible = true;
            nbNotification.Title = "Oops";
            nbNotification.Text = text;
            nbNotification.NotificationBoxType = NotificationBoxType.Warning;
        }

        #endregion
    }
}