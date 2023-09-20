using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

using Microsoft.AspNet.SignalR;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

using com.blueboxmoon.DatabaseThinner;
using Helper = com.blueboxmoon.DatabaseThinner.Helper;

namespace RockWeb.Plugins.com_blueboxmoon.DatabaseThinner
{
    [DisplayName( "Compress Tables" )]
    [Category( "Blue Box Moon > Database Thinner" )]
    [Description( "Scans current table storage and recommends compression changes." )]
    [IntegerField( "Compression Sample Size", "The percentage of the table data to use as a sample size.", true, 25, order: 0 )]
    [BooleanField( "Use Online Mode", "If your SQL Server version supports online index rebuild, this will keep the table available for use, but it does take much longer to run.", false, order: 1 )]
    public partial class CompressTables : RockBlock
    {
        #region Properties

        /// <summary>
        /// This holds the reference to the RockMessageHub SignalR Hub context.
        /// </summary>
        private IHubContext HubContext = GlobalHost.ConnectionManager.GetHubContext<RockMessageHub>();

        /// <summary>
        /// The scanned table information.
        /// </summary>
        protected List<TableInformation> TableState
        {
            get
            {
                return hfTableState.Value.FromJsonOrNull<List<TableInformation>>();
            }
            set
            {
                hfTableState.Value = value.ToJson();
            }
        }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddCSSLink( "~/Plugins/com_blueboxmoon/DatabaseThinner/Styles/bootstrap-toggle.min.css" );
            RockPage.AddScriptLink( "~/Scripts/jquery.signalR-2.2.0.min.js", false );
            RockPage.AddScriptLink( "~/Plugins/com_blueboxmoon/DatabaseThinner/Scripts/bootstrap-toggle.min.js" );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                //
                // Ensure SQL Server supports table compression.
                //
                if ( !Helper.DoesSqlServerSupportCompression() )
                {
                    nbUnsupported.Visible = true;
                    pnlDetails.Visible = false;
                    return;
                }

                //
                // Find all the tables except the MigrationHistory table and populate the drop down.
                //
                var tables = Helper.GetAllTables()
                    .Where( t => t != "__MigrationHistory" )
                    .OrderBy( t => t );

                ddlScanTable.Items.Add( new ListItem( "All Tables", "" ) );
                foreach ( var table in tables )
                {
                    ddlScanTable.Items.Add( new ListItem( table, table ) );
                }

                //
                // Check for partial table compression.
                //
                var indexes = Helper.GetAllIndexes().ToList();
                var partialTables = tables.Where( t => indexes.Any( i => i.Table == t && i.IsCompressed ) )
                    .Where( t => indexes.Any( i => i.Table == t && !i.IsCompressed ) )
                    .ToList();
                if ( partialTables.Any() )
                {
                    ltPartialTables.Text = string.Join( "</li><li>", partialTables );
                    pnlPartialTables.Visible = true;
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        protected void BindGrid()
        {
            var tables = TableState ?? new List<TableInformation>();
            SortProperty sortProperty = gTables.SortProperty;

            if ( sortProperty != null )
            {
                gTables.DataSource = tables.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                gTables.DataSource = tables.OrderByDescending( t => t.EstimatedSavingsPerYear ).ToList();
            }

            gTables.DataBind();
        }

        /// <summary>
        /// Updates the recommendation.
        /// </summary>
        protected void UpdateRecommendation()
        {
            var tables = TableState;

            var spaceSavedPerYear = tables
                .Where( t => t.CompressionRecommended && !t.Compressed )
                .Sum( t => t.EstimatedSavingsPerYear ) / 1024.0 / 1024.0;
            var spaceSaved = tables
                .Where( t => t.CompressionRecommended && !t.Compressed )
                .Sum( t => t.SpaceSaved ) / 1024.0 / 1024.0;

            nbRecommendation.Text = string.Format( "Compressing all recommended tables could save an additional <strong>{0:N0} MB</strong> per year and <strong>{1:N0} MB</strong> total.",
                spaceSavedPerYear, spaceSaved );

            nbRecommendation.Visible = string.IsNullOrWhiteSpace( ddlScanTable.SelectedValue ) && ( spaceSavedPerYear > 0 || spaceSaved > 0 );
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the lbScanTables control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbScanTables_Click( object sender, EventArgs e )
        {
            //
            // Define the task that will run to process the data.
            //
            var task = new Task( () =>
            {
                //
                // Wait for the postback UI to settle.
                //
                Task.Delay( 1000 ).Wait();

                List<string> tablesToScan;
                int sampleSize = GetAttributeValue( "CompressionSampleSize" ).AsInteger();

                if ( sampleSize <= 0 || sampleSize > 100 )
                {
                    sampleSize = 25;
                }

                if ( !string.IsNullOrWhiteSpace( ddlScanTable.SelectedValue ) )
                {
                    tablesToScan = new List<string> { ddlScanTable.SelectedValue };
                }
                else
                {
                    tablesToScan = Helper.GetAllTables().ToList();
                }

                var tables = Helper.GetTableCompressionInformation( tablesToScan, sampleSize, ( processed, total ) =>
                {
                    HubContext.Clients.Client( hfConnectionId.Value ).scanProgress( processed, total );
                } );

                //
                // Show the final status.
                //
                HubContext.Clients.Client( hfConnectionId.Value ).scanStatus( tables.ToJson() );
            } );

            //
            // Define an error handler for the task.
            //
            task.ContinueWith( ( t ) =>
            {
                if ( t.IsFaulted )
                {
                    HubContext.Clients.Client( hfConnectionId.Value ).scanStatus( string.Empty );
                }
            } );

            task.Start();

            lbScanTables.Enabled = false;
            pnlScanResults.Visible = false;
            pnlPartialTables.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the lbScanCompleted control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbScanCompleted_Click( object sender, EventArgs e )
        {
            nbScanWarning.Visible = false;
            pnlScanResults.Visible = true;
            lbScanTables.Enabled = true;

            BindGrid();

            UpdateRecommendation();
        }

        /// <summary>
        /// Handles the GridRebind event of the gTables control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gTables_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Click event of the lbModifyTable control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbModifyTable_Click( object sender, EventArgs e )
        {
            string table = hfModifyTable.Value.SplitDelimitedValues().First();
            string action = hfModifyTable.Value.SplitDelimitedValues().Last();
            string mode = action.AsBoolean() ? "PAGE" : "NONE";
            string onlineMode = GetAttributeValue( "UseOnlineMode" ).AsBoolean() ? "ON" : "OFF";

            pnlScan.Visible = false;
            pnlScanResults.Visible = false;
            pnlCompressStatus.Visible = true;

            nbCompressStatus.Text = string.Format( "Now {0} the {1} table. This may take some time, please wait until we finish.",
                action.AsBoolean() ? "compressing" : "decompressing", table );

            //
            // Define the task that will run to process the data.
            //
            var task = new Task( () =>
            {
                //
                // Wait for the postback UI to settle.
                //
                Task.Delay( 1000 ).Wait();

                var indexes = Helper.GetAllIndexes()
                    .Where( i => i.Table == table )
                    .ToList();

                HubContext.Clients.Client( hfConnectionId.Value ).compressActionProgress( 0, indexes.Count );

                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.Connection.Open();

                    for ( int i = 0; i < indexes.Count; i++ )
                    {
                        if ( indexes[i].IsCompressed != action.AsBoolean() )
                        {
                            using ( var cmd = rockContext.Database.Connection.CreateCommand() )
                            {
                                cmd.CommandTimeout = 1800;
                                cmd.CommandText = string.Format( "ALTER INDEX [{1}] ON [{0}] REBUILD PARTITION = ALL WITH (DATA_COMPRESSION = {2}, ONLINE = {3})",
                                    table, indexes[i].Index, mode, onlineMode );

                                cmd.ExecuteNonQuery();
                            }
                        }

                        HubContext.Clients.Client( hfConnectionId.Value ).compressActionProgress( i + 1, indexes.Count );
                    }
                }

                //
                // Show the final status.
                //
                HubContext.Clients.Client( hfConnectionId.Value ).compressActionStatus( string.Empty );
            } );

            //
            // Define an error handler for the task.
            //
            task.ContinueWith( ( t ) =>
            {
                if ( t.IsFaulted )
                {
                    HubContext.Clients.Client( hfConnectionId.Value ).compressActionStatus( "An unknown error occurred." );
                }
            } );

            task.Start();
        }

        /// <summary>
        /// Handles the Click event of the lbCompressDone control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCompressDone_Click( object sender, EventArgs e )
        {
            pnlScan.Visible = true;
            pnlScanResults.Visible = true;
            pnlCompressStatus.Visible = false;

            if ( string.IsNullOrWhiteSpace( hfCompressStatus.Value ) )
            {
                string table = hfModifyTable.Value.SplitDelimitedValues().First();
                string action = hfModifyTable.Value.SplitDelimitedValues().Last();

                var tables = TableState;
                tables.Where( t => t.Name == table ).Single().Compressed = action.AsBoolean();
                TableState = tables;
            }
            else if ( hfCompressStatus.Value == "FixPartialTables" )
            {
                NavigateToCurrentPage();
                return;
            }

            BindGrid();
            UpdateRecommendation();
        }

        /// <summary>
        /// Handles the Click event of the lbFixPartialTables control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbFixPartialTables_Click( object sender, EventArgs e )
        {
            pnlPartialTables.Visible = false;
            pnlScan.Visible = false;
            pnlScanResults.Visible = false;
            pnlCompressStatus.Visible = true;

            nbCompressStatus.Text = "Fixing tables that are partially compressed. This may take some time, please wait until we finish.";

            //
            // Define the task that will run to process the data.
            //
            var task = new Task( () =>
            {
                //
                // Wait for the postback UI to settle.
                //
                Task.Delay( 1000 ).Wait();

                //
                // Check for partial table compression.
                //
                var tables = Helper.GetAllTables().Where( t => t != "__MigrationHistory" ).ToList();
                var indexes = Helper.GetAllIndexes().ToList();
                var partialTables = tables.Where( t => indexes.Any( i => i.Table == t && i.IsCompressed ) )
                    .Where( t => indexes.Any( i => i.Table == t && !i.IsCompressed ) )
                    .ToList();
                indexes = indexes.Where( i => partialTables.Contains( i.Table ) && !i.IsCompressed ).ToList();

                HubContext.Clients.Client( hfConnectionId.Value ).compressActionProgress( 0, indexes.Count );

                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.Connection.Open();

                    for ( int i = 0; i < indexes.Count; i++ )
                    {
                        using ( var cmd = rockContext.Database.Connection.CreateCommand() )
                        {
                            cmd.CommandTimeout = 1800;
                            cmd.CommandText = string.Format( "ALTER INDEX [{1}] ON [{0}] REBUILD PARTITION = ALL WITH (DATA_COMPRESSION = {2})",
                                indexes[i].Table, indexes[i].Index, "PAGE" );

                            cmd.ExecuteNonQuery();
                        }

                        HubContext.Clients.Client( hfConnectionId.Value ).compressActionProgress( i + 1, indexes.Count );
                    }
                }

                //
                // Show the final status.
                //
                HubContext.Clients.Client( hfConnectionId.Value ).compressActionStatus( "FixPartialTables" );
            } );

            //
            // Define an error handler for the task.
            //
            task.ContinueWith( ( t ) =>
            {
                if ( t.IsFaulted )
                {
                    HubContext.Clients.Client( hfConnectionId.Value ).compressActionStatus( "An unknown error occurred." );
                }
            } );

            task.Start();
        }

        #endregion
    }
}
