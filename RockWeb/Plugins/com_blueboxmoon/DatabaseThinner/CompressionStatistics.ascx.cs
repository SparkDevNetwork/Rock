using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

using com.blueboxmoon.DatabaseThinner;
using Helper = com.blueboxmoon.DatabaseThinner.Helper;
using Microsoft.AspNet.SignalR;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Plugins.com_blueboxmoon.DatabaseThinner
{
    [DisplayName( "Compression Statistics" )]
    [Category( "Blue Box Moon > Database Thinner" )]
    [Description( "Gets the statistics about the data in Rock that has been compressed." )]
    [IntegerField( "Compression Sample Size", "The percentage of the table data to use as a sample size.", true, 25, order: 0 )]
    public partial class CompressionStatistics : RockBlock
    {
        /// <summary>
        /// This holds the reference to the RockMessageHub SignalR Hub context.
        /// </summary>
        private IHubContext HubContext = GlobalHost.ConnectionManager.GetHubContext<RockMessageHub>();

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/jquery.signalR-2.2.0.min.js", false );
        }

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
                int sampleSize = GetAttributeValue( "CompressionSampleSize" ).AsInteger();

                if ( sampleSize <= 0 || sampleSize > 100 )
                {
                    sampleSize = 25;
                }

                //
                // Wait for the postback to settle.
                //
                Task.Delay( 1000 ).Wait();

                var tables = new List<TableInformation>();
                if ( Helper.DoesSqlServerSupportCompression() )
                {
                    var compressedTables = Helper.GetCompressedTables();
                    HubContext.Clients.Client( hfConnectionId.Value ).statisticScanProgress( 0, compressedTables.Count(), "Scanning tables..." );

                    tables = Helper.GetTableCompressionInformation( compressedTables, sampleSize, ( processed, total ) =>
                    {
                        HubContext.Clients.Client( hfConnectionId.Value ).statisticScanProgress( processed, total, "Scanning tables..." );
                    } );
                }

                int communicationCount = 0;
                double communicationUncompressedSize = 0;
                double communicationCompressedSize = 0;
                double communicationSavingsPerYear = 0;

                using ( var rockContext = new RockContext() )
                {
                    HubContext.Clients.Client( hfConnectionId.Value ).statisticScanProgress( 0, 0, "Scanning communications..." );

                    //
                    // Get the attribute Id that holds the compressed data information.
                    //
                    int communicationEntityTypeId = EntityTypeCache.GetId( typeof( Communication ) ) ?? 0;
                    int compressedDataAttributeId = AttributeCache.All()
                        .Where( a => a.EntityTypeId == communicationEntityTypeId && a.Key == Helper.COMMUNICATION_COMPRESSED_DATA )
                        .Select( a => a.Id )
                        .Single();

                    //
                    // Get the query for the communication Ids that have already been
                    // compressed.
                    //
                    var compressedCommunicationData = new AttributeValueService( rockContext ).Queryable()
                        .Where( a => a.AttributeId == compressedDataAttributeId )
                        .Where( a => !string.IsNullOrEmpty( a.Value ) )
                        .Join( rockContext.Communications, a => a.EntityId, c => c.Id, ( a, c ) => new
                        {
                            AttributeValue = a,
                            Communication = c
                        } )
                        .Select( g => new
                        {
                            g.AttributeValue.Value,
                            g.Communication.CreatedDateTime
                        } )
                        .ToList();

                    //
                    // Get the "one-year-window" of compressed data to calculate annual savings.
                    //
                    var communicationAnnualSavingsEndDate = new AttributeValueService( rockContext ).Queryable()
                        .Join( rockContext.Communications, a => a.EntityId, c => c.Id, ( a, c ) => new
                        {
                            AttributeValue = a,
                            Communication = c
                        } )
                        .Where( g => g.AttributeValue.AttributeId == compressedDataAttributeId )
                        .Where( g => !string.IsNullOrEmpty( g.AttributeValue.Value ) )
                        .Max( g => g.Communication.CreatedDateTime ) ?? DateTime.MaxValue;

                    //
                    // Get all the size values from the compression data.
                    //
                    var communicationAnnualSavingsStartDate = communicationAnnualSavingsEndDate.AddYears( -1 );
                    for ( int i = 0; i < compressedCommunicationData.Count; i++ )
                    {
                        var compressedData = compressedCommunicationData[i].Value.FromJsonOrNull<CompressedData>();

                        if ( compressedData != null )
                        {
                            communicationCount += 1;
                            communicationUncompressedSize += compressedData.UncompressedSize * 2 / 1024.0 / 1024.0; // * 2 for UTF-16 encoding
                            communicationCompressedSize += compressedData.CompressedSize / 1024.0 / 1024.0;

                            if ( compressedCommunicationData[i].CreatedDateTime > communicationAnnualSavingsStartDate && compressedCommunicationData[i].CreatedDateTime <= communicationAnnualSavingsEndDate )
                            {
                                communicationSavingsPerYear += ( compressedData.UncompressedSize * 2 / 1024.0 / 1024.0 ) - ( compressedData.CompressedSize / 1024.0 / 1024.0 );
                            }
                        }

                        if ( i % 10000 == 0 )
                        {
                            HubContext.Clients.Client( hfConnectionId.Value ).statisticScanProgress( i, compressedCommunicationData.Count, "Scanning communications..." );
                        }
                    }
                }

                double communicationSpaceSaved = communicationUncompressedSize - communicationCompressedSize;
                var json = new
                    {
                        SpaceSaved = tables.Sum( t => t.SpaceSavedMB ) + communicationSpaceSaved,
                        AnnualSpaceSaved = tables.Sum( t => t.EstimatedSavingsPerYearMB ) + communicationSavingsPerYear,
                        Tables = new
                        {
                            Count = tables.Count(),
                            SpaceSaved = tables.Sum( t => t.SpaceSavedMB ),
                            UncompressedSize = tables.Sum( t => t.UncompressedSizeMB ),
                            CompressedSize = tables.Sum( t => t.CompressedSizeMB ),
                            AnnualSpaceSaved = tables.Sum( t => t.EstimatedSavingsPerYearMB )
                        },
                        Communications = new
                        {
                            Count = communicationCount,
                            SpaceSaved = communicationSpaceSaved,
                            UncompressedSize = communicationUncompressedSize,
                            CompressedSize = communicationCompressedSize,
                            AnnualSpaceSaved = communicationSavingsPerYear
                        }
                    }.ToJson();

                //
                // Show the final status.
                //
                HubContext.Clients.Client( hfConnectionId.Value ).statisticScanStatus( json );
            } );

            //
            // Define an error handler for the task.
            //
            task.ContinueWith( ( t ) =>
            {
                if ( t.IsFaulted )
                {
                    HubContext.Clients.Client( hfConnectionId.Value ).statisticScanStatus( string.Empty );
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
            var scanData = hfScanState.Value.FromJsonOrNull<dynamic>();

            ltSpaceSaved.Text = string.Format( "{0:N2} MB", scanData.SpaceSaved );
            ltAnnualSavings.Text = string.Format( "{0:N2} MB", scanData.AnnualSpaceSaved );

            ltCompressedTableCount.Text = string.Format( "{0:N0}", scanData.Tables.Count );
            ltTableSpaceSaved.Text = string.Format( "{0:N2} MB", scanData.Tables.SpaceSaved );
            ltTableUncompressedSize.Text = string.Format( "{0:N2} MB", scanData.Tables.UncompressedSize );
            ltTableCompressedSize.Text = string.Format( "{0:N2} MB", scanData.Tables.CompressedSize );
            ltTableAnnualSavings.Text = string.Format( "{0:N2} MB", scanData.Tables.AnnualSpaceSaved );

            ltCompressedCommunicationsCount.Text = string.Format( "{0:N0}", scanData.Communications.Count );
            ltCompressedCommunicationsSpaceSaved.Text = string.Format( "{0:N2} MB", scanData.Communications.SpaceSaved );
            ltCompressedCommunicationsUncompressedSize.Text = string.Format( "{0:N2} MB", scanData.Communications.UncompressedSize );
            ltCompressedCommunicationsCompressedSize.Text = string.Format( "{0:N2} MB", scanData.Communications.CompressedSize );
            ltCompressedCommunicationsAnnualSavings.Text = string.Format( "{0:N2} MB", scanData.Communications.AnnualSpaceSaved );

            pnlScan.Visible = false;
            pnlDetails.Visible = true;

            if ( !Helper.DoesSqlServerSupportCompression() )
            {
                pnlTables.Visible = false;
            }
        }
    }
}