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
using System.Text;
using System.Web;

using Quartz;

using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Utility.Settings.SparkData;
using Rock.Utility.SparkDataApi;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to get a National Change of Address (NCOA) report for all active people's addresses.
    /// </summary>
    [DisallowConcurrentExecution]
    public class GetNcoa : IJob
    {
        /// <summary>
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public GetNcoa()
        {
        }

        /// <summary>
        /// Job to get a National Change of Address (NCOA) report for all active people's addresses.
        ///
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            Exception exception = null;
            // Get the job setting(s)
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            SparkDataConfig sparkDataConfig = Ncoa.GetSettings();

            if ( !sparkDataConfig.NcoaSettings.IsEnabled || !sparkDataConfig.NcoaSettings.IsValid() )
            {
                return;
            }

            try
            {
                Guid? sparkDataApiKeyGuid = sparkDataConfig.SparkDataApiKey.AsGuidOrNull();
                if ( sparkDataApiKeyGuid == null )
                {
                    exception = new NoRetryException( $"Spark Data API Key '{sparkDataConfig.SparkDataApiKey.ToStringSafe()}' is empty or invalid. The Spark Data API Key can be configured in System Settings > Spark Data Settings." );
                    return;
                }

                switch ( sparkDataConfig.NcoaSettings.CurrentReportStatus )
                {
                    case "":
                    case null:
                        if ( sparkDataConfig.NcoaSettings.RecurringEnabled )
                        {
                            StatusStart( sparkDataConfig );
                        }

                        break;
                    case "Start":
                        StatusStart( sparkDataConfig );
                        break;
                    case "Failed":
                        StatusFailed( sparkDataConfig );
                        break;
                    case "Pending: Report":
                        StatusPendingReport( sparkDataConfig );
                        break;
                    case "Pending: Export":
                        StatusPendingExport( sparkDataConfig );
                        break;
                    case "Complete":
                        StatusComplete( sparkDataConfig );
                        break;
                }
            }
            catch ( Exception ex )
            {
                exception = ex;
            }
            finally
            {
                if ( exception != null )
                {
                    context.Result = $"NCOA Job failed: {exception.Message}";

                    if ( exception is NoRetryException || exception is NoRetryAggregateException )
                    {
                        sparkDataConfig.NcoaSettings.CurrentReportStatus = "Complete";
                        sparkDataConfig.NcoaSettings.LastRunDate = RockDateTime.Now;
                    }
                    else
                    {
                        sparkDataConfig.NcoaSettings.CurrentReportStatus = "Failed";
                    }

                    StringBuilder sb = new StringBuilder( $"NOCA job failed: {RockDateTime.Now.ToString()} - {exception.Message}" );
                    Exception innerException = exception;
                    while ( innerException.InnerException != null )
                    {
                        innerException = innerException.InnerException;
                        sb.AppendLine( innerException.Message );
                    }

                    sparkDataConfig.Messages.Add( sb.ToString() );
                    Ncoa.SaveSettings( sparkDataConfig );

                    try
                    {
                        var ncoa = new Ncoa();
                        ncoa.SendNotification( sparkDataConfig, "failed" );
                    }
                    catch { }


                    if ( sparkDataConfig.SparkDataApiKey.IsNotNullOrWhiteSpace() && sparkDataConfig.NcoaSettings.FileName.IsNotNullOrWhiteSpace() )
                    {
                        SparkDataApi sparkDataApi = new SparkDataApi();
                    }

                    Exception ex = new AggregateException( "NCOA job failed.", exception );
                    HttpContext context2 = HttpContext.Current;
                    ExceptionLogService.LogException( ex, context2 );
                    throw ex;
                }
                else
                {
                    string msg;
                    if ( sparkDataConfig.NcoaSettings.CurrentReportStatus == "Complete" )
                    {
                        using ( RockContext rockContext = new RockContext() )
                        {
                            NcoaHistoryService ncoaHistoryService = new NcoaHistoryService( rockContext );
                            msg = $"NCOA request processed, {ncoaHistoryService.Count()} {(ncoaHistoryService.Count() == 1 ? "address" : "addresses")} processed, {ncoaHistoryService.MovedCount()} {(ncoaHistoryService.MovedCount() > 1 ? "were" : "was")} marked as 'moved'";
                        }
                    }
                    else
                    {
                        msg = $"Job complete. NCOA status: {sparkDataConfig.NcoaSettings.CurrentReportStatus}";
                    }

                    context.Result = msg;
                    sparkDataConfig.Messages.Add( $"{msg}: {RockDateTime.Now.ToString()}" );
                    Ncoa.SaveSettings( sparkDataConfig );
                }
            }
        }

        /// <summary>
        /// Current State is Failed. If recurring is enabled, retry.
        /// </summary>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        private void StatusFailed( SparkDataConfig sparkDataConfig )
        {
            StatusStart( sparkDataConfig );
        }

        /// <summary>
        /// Current state is start. Start NCOA
        /// </summary>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        private void StatusStart( SparkDataConfig sparkDataConfig )
        {
            if ( sparkDataConfig.NcoaSettings.IsAckPrice && sparkDataConfig.NcoaSettings.IsAcceptedTerms )
            {
                var ncoa = new Ncoa();
                ncoa.Start( sparkDataConfig );
            }
            else
            {
                if ( !sparkDataConfig.NcoaSettings.IsAckPrice && !sparkDataConfig.NcoaSettings.IsAcceptedTerms )
                {
                    throw new NoRetryException( "The NCOA terms of service have not been accepted." );
                }
                else if ( !sparkDataConfig.NcoaSettings.IsAcceptedTerms )
                {
                    throw new NoRetryException( "The NCOA terms of service have not been accepted." );
                }
                else
                {
                    throw new NoRetryException( "The price of the NCOA service has not been acknowledged." );
                }
            }
        }

        /// <summary>
        /// Current state is complete. Check if recurring is enabled and recurring interval have been reached,
        /// and if so set the state back to Start.
        /// </summary>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        private void StatusComplete( SparkDataConfig sparkDataConfig )
        {
            if ( sparkDataConfig.NcoaSettings.IsEnabled &&
                (
                    !sparkDataConfig.NcoaSettings.LastRunDate.HasValue ||
                    ( sparkDataConfig.NcoaSettings.RecurringEnabled && sparkDataConfig.NcoaSettings.LastRunDate.Value.AddDays( sparkDataConfig.NcoaSettings.RecurrenceInterval ) < RockDateTime.Now )
                ) )
            {
                sparkDataConfig.NcoaSettings.CurrentReportStatus = "Start";
                sparkDataConfig.NcoaSettings.PersonFullName = null;
                Ncoa.SaveSettings( sparkDataConfig );
                StatusStart( sparkDataConfig );
            }
        }

        /// <summary>
        /// Current state is pending report. Try to resume a pending report.
        /// </summary>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        private void StatusPendingReport( SparkDataConfig sparkDataConfig )
        {
            var ncoa = new Ncoa();
            ncoa.PendingReport( sparkDataConfig );
        }

        /// <summary>
        /// Current state is pending export report. Try to resume a pending export report.
        /// </summary>
        /// <param name="sparkDataConfig">The spark data configuration.</param>
        private void StatusPendingExport( SparkDataConfig sparkDataConfig )
        {
            var ncoa = new Ncoa();
            ncoa.PendingExport( sparkDataConfig );
        }
    }
}
