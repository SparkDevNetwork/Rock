using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

using Microsoft.AspNet.SignalR;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_blueboxmoon.DatabaseThinner
{
    [DisplayName( "Database Growth" )]
    [Category( "Blue Box Moon > Database Thinner" )]
    [Description( "Gets the statistics about how fast the database is growing." )]
    public partial class DatabaseGrowth : RockBlock
    {
        #region Properties

        /// <summary>
        /// Gets or sets the table details.
        /// </summary>
        /// <value>
        /// The table details.
        /// </value>
        private List<TableDetail> TableDetails
        {
            get
            {
                return ( List<TableDetail> ) ViewState["TableDetails"];
            }
            set
            {
                ViewState["TableDetails"] = value;
            }
        }

        /// <summary>
        /// This holds the reference to the RockMessageHub SignalR Hub context.
        /// </summary>
        private IHubContext HubContext = GlobalHost.ConnectionManager.GetHubContext<RockMessageHub>();

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/jquery.signalR-2.2.0.min.js", false );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        protected void BindGrid()
        {
            var tables = TableDetails;

            if ( gTables.SortProperty != null )
            {
                tables = tables.AsQueryable().Sort( gTables.SortProperty ).ToList();
            }
            else
            {
                tables = tables.OrderByDescending( t => t.BytesPerYear ).ToList();
            }

            gTables.DataSource = tables;
            gTables.DataBind();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the lbScan control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbScan_Click( object sender, EventArgs e )
        {
            //
            // Define the task that will run to process the data.
            //
            var task = new Task( () =>
            {
                //
                // Wait for the postback to settle.
                //
                Task.Delay( 1000 ).Wait();

                var tableList = new List<TableDetail>();
                HubContext.Clients.Client( hfConnectionId.Value ).growthScanProgress( 0, 0, "Scanning tables..." );

                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.Connection.Open();

                    var tables = rockContext.Database.SqlQuery<TableDetail>( @"
SELECT 
    t.[name] AS [Name],
    p.[rows] AS [Rows],
    SUM(a.[total_pages]) * 8 * 1024 AS [Bytes]
FROM [sys].[tables] AS t
INNER JOIN [sys].[indexes] AS i ON t.[object_id] = i.[object_id]
INNER JOIN [sys].[partitions] AS p ON i.[object_id] = p.[object_id] AND i.[index_id] = p.[index_id]
INNER JOIN [sys].[allocation_units] AS a ON p.[partition_id] = a.[container_id]
WHERE t.[name] NOT LIKE 'dt%' AND t.[name] != '__MigrationHistory'
  AND t.[is_ms_shipped] = 0
  AND i.[object_id] > 255
  AND i.[type] = 1
GROUP BY t.[name], p.[rows]
" ).ToList();

                    for ( int i = 0; i < tables.Count; i++ )
                    {
                        try
                        {
                            tables[i].RowsPerYear = rockContext.Database.SqlQuery<int>( string.Format( @"
SELECT COUNT(*)
FROM [{0}]
WHERE [CreatedDateTime] >= DATEADD(YEAR, -1, GETDATE())", tables[i].Name ) ).Single();

                            tables[i].RowsPriorYear = rockContext.Database.SqlQuery<int>( string.Format( @"
SELECT COUNT(*)
FROM [{0}]
WHERE [CreatedDateTime] >= DATEADD(YEAR, -2, GETDATE())
  AND [CreatedDateTime] < DATEADD(YEAR, -1, GETDATE())", tables[i].Name ) ).Single();

                            tableList.Add( tables[i] );
                        }
                        catch
                        {
                            /* Intentionally ignored. */
                        }

                        HubContext.Clients.Client( hfConnectionId.Value ).growthScanProgress( i, tables.Count, "Scanning tables..." );
                    }
                }

                //
                // Show the final status.
                //
                HubContext.Clients.Client( hfConnectionId.Value ).growthScanStatus( tableList.ToJson() );
            } );

            //
            // Define an error handler for the task.
            //
            task.ContinueWith( ( t ) =>
            {
                if ( t.IsFaulted )
                {
                    HubContext.Clients.Client( hfConnectionId.Value ).growthScanStatus( string.Empty );
                }
            } );

            task.Start();

            lbScan.Enabled = false;
        }

        /// <summary>
        /// Handles the Click event of the lbScanCompleted control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbScanCompleted_Click( object sender, EventArgs e )
        {
            TableDetails = hfScanState.Value.FromJsonOrNull<List<TableDetail>>();

            pnlScan.Visible = false;
            pnlDetails.Visible = true;

            var growth = TableDetails.Sum( t => t.BytesPerYear ) / 1024.0 / 1024.0;
            ltEstimatedGrowth.Text = string.Format( "{0:N2} MB", growth );

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gTables control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gTables_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Support Classes

        [Serializable]
        protected class TableDetail
        {
            public string Name { get; set; }

            public long Rows { get; set; }

            public long Bytes { get; set; }

            public long RowsPerYear { get; set; }

            public long RowsPriorYear { get; set; }

            public long BytesPerRow
            {
                get
                {
                    return Rows > 0 ? Bytes / Rows : 0;
                }
            }

            public long BytesPerYear
            {
                get
                {
                    return RowsPerYear * BytesPerRow;
                }
            }

            public double BytesMB
            {
                get
                {
                    return Bytes / 1024.0 / 1024.0;
                }
            }

            public double BytesPerYearMB
            {
                get
                {
                    return BytesPerYear / 1024.0 / 1024.0;
                }
            }

            public double YearOverYearGrowth
            {
                get
                {
                    return RowsPriorYear > 0 ? RowsPerYear / ( double ) RowsPriorYear * 100.0 : double.PositiveInfinity;
                }
            }
        }

        #endregion
    }
}