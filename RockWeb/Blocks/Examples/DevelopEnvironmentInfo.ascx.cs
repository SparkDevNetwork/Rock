﻿// <copyright>
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
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Examples
{
    /// <summary>
    /// A sample block that uses many of the Rock UI controls.
    /// </summary>
    [DisplayName( "Developer Environment Info" )]
    [Category( "Examples" )]
    [Description( "Shows Information about the Development environment" )]
    public partial class DevelopEnvironmentInfo : RockBlock
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
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                var rockContext = new RockContext();
                lDatabaseName.Text = string.Format(
                    @"
Database: {0}
Server: {1}",
            rockContext.Database.Connection.Database,
            rockContext.Database.Connection.DataSource );

                lHostingEnvironment.Text = string.Format(
                    @"
SiteName: {0}
IsDevelopmentEnvironment: {1}
Path: {2}",
                    System.Web.Hosting.HostingEnvironment.SiteName,
                    System.Web.Hosting.HostingEnvironment.IsDevelopmentEnvironment,
                    System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath );
            }
        }

        #region Events

        /// <summary>
        /// Handles the Click event of the btnShutdown control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnShutdown_Click( object sender, EventArgs e )
        {
            System.Web.Hosting.HostingEnvironment.InitiateShutdown();
            NavigateToPage( new Rock.Web.PageReference( this.RockPage.PageId ) );
        }

        /// <summary>
        /// Handles the Click event of the btnStartLogSQL control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnStartLogSQL_Click( object sender, EventArgs e )
        {
            DebugHelper.SQLLoggingStart();
        }

        /// <summary>
        /// Handles the Click event of the btnStopLogSQL control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnStopLogSQL_Click( object sender, EventArgs e )
        {
            DebugHelper.SQLLoggingStop();
        }

        /// <summary>
        /// Handles the Click event of the btnLoadBlockTypesAndPages control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnLoadBlockTypesAndPages_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            // ensure update attributes is called on every blocktype
            foreach( var blockType in new BlockTypeService(rockContext).Queryable().AsNoTracking().ToList())
            {
                System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var blockTypeCache = BlockTypeCache.Get( blockType.Guid );
                if ( !blockTypeCache.IsInstancePropertiesVerified )
                {
                    try
                    {
                        var blockControl = this.Page.LoadControl( blockTypeCache.Path ) as RockBlock;
                        int? blockEntityTypeId = EntityTypeCache.Get( typeof( Block ) ).Id;
                        Rock.Attribute.Helper.UpdateAttributes( blockControl.GetType(), blockEntityTypeId, "BlockTypeId", blockType.Id.ToString(), rockContext );
                        blockTypeCache.MarkInstancePropertiesVerified( true );
                        System.Diagnostics.Debug.WriteLine( string.Format( "[{1}ms] BlockType {0}", blockTypeCache.Path, stopwatch.Elapsed.TotalMilliseconds ) );
                    }
                    catch (Exception ex)
                    {
                        ExceptionLogService.LogException( ex );
                    }
                }

                stopwatch.Stop();
            }

            foreach ( var page in new PageService( rockContext ).Queryable() )
            {
                string url = string.Empty;
                try
                {
                    System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

                    url = string.Format( "http{0}://{1}:{2}{3}",
                        ( Request.IsSecureConnection ) ? "s" : "",
                        WebRequestHelper.GetHostNameFromRequest( HttpContext.Current ),
                        Request.Url.Port,
                        ResolveRockUrl( new PageReference( page.Id ).BuildUrl() ) );

                    WebRequest request = WebRequest.Create( url );
                    request.Timeout = 10000;
                    WebResponse response = request.GetResponse();
                    stopwatch.Stop();
                    System.Diagnostics.Debug.WriteLine( string.Format( "[{2}ms] Loaded {0} {1} ", page.InternalName, url, stopwatch.Elapsed.TotalMilliseconds ) );
                }
                catch ( Exception ex )
                {
                    System.Diagnostics.Debug.WriteLine( string.Format( "Exception Loading {0} {1}, {2}", page.InternalName, url, ex ) );
                }
            }
        }

        #endregion
    }
}
