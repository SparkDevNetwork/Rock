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
using System.IO.Compression;
using System.IO;
using System.Web.UI;

using Rock.Web.UI;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// Used to view the <see cref="Rock.Logging.RockLogEvent"/> logged via RockLogger.
    /// </summary>
    [System.ComponentModel.DisplayName( "Logs" )]
    [System.ComponentModel.Category( "Administration" )]
    [System.ComponentModel.Description( "Block to view system logs." )]

    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.LOG_VIEWER )]
    public partial class LogViewer : RockBlock, ICustomGridColumns, ISecondaryBlock
    {
        public void SetVisible( bool visible )
        {
            if ( visible && !pnlLogs.Visible )
            {
                // if setting back to visible, make sure the log grid is refreshed
                BindGrid();
            }

            pnlLogs.Visible = visible;
        }

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            rGrid.GridRebind += rGrid_GridRebind;
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
                BindGrid();
            }

            ScriptManager.GetCurrent( this.Page ).RegisterPostBackControl( lbDownload );
        }

        /// <summary>
        /// Downloads the log files from the filesystem as a zipped folder
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDownload_Click(object sender, EventArgs e)
        {
            var logFiles = Rock.Logging.RockLogger.Log.LogFiles;

            using (MemoryStream ms = new MemoryStream())
            {
                using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    foreach (var file in logFiles)
                    {
                        var entryName = Path.GetFileName(file);
                        var entry = zip.CreateEntry(entryName);
                        entry.LastWriteTime = System.IO.File.GetLastWriteTime(file);
                        using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (var entryStream = entry.Open())
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                }

                byte[] bytes = ms.ToArray();

                Response.ContentType = "application/octet-stream";
                Response.AddHeader("content-disposition", "attachment; filename= RockLogs.zip");
                Response.BufferOutput = true;
                Response.BinaryWrite(bytes);
                Response.Flush();
                Response.SuppressContent = true;
                System.Web.HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
        }
        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the fDevice control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var logReader = Rock.Logging.RockLogger.LogReader;
            var logIndex = rGrid.PageIndex * rGrid.PageSize;

            var events = logReader.GetEvents( logIndex, rGrid.PageSize );
            if ( logIndex > 0 )
            {
                rGrid.EmptyDataText = "The end of the log entries have been reached. <a href=''>Start Over.</a>";
            }

            rGrid.VirtualItemCount = logReader.RecordCount;
            rGrid.DataSource = events;
            rGrid.DataBind();
        }
        #endregion
    }
}