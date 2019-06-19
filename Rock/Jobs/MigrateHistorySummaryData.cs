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
using System.Linq;
using System.Text.RegularExpressions;

using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// This job is used to convert a history record's Summary to the actual fields that were added in v8. Once all the values have been 
    /// converted, this job will delete itself.
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [IntegerField( "How Many Records", "The number of history records to process on each run of this job.", false, 500000, "", 0, "HowMany" )]
    [IntegerField( "Command Timeout", "Maximum amount of time (in seconds) to wait for the SQL Query to complete. Leave blank to use the default for this job (3600). Note, it could take several minutes, so you might want to set it at 3600 (60 minutes) or higher", false, 60 * 60, "General", 1, "CommandTimeout" )]
    public class MigrateHistorySummaryData : IJob
    {
        // derived from v7 version of PersonHistory.cs that used to have to parse History.Summary
        Regex AddedRegex = new Regex( "Added.*<span class=['\"]field-name['\"]>(.*)<\\/span>.*<span class=['\"]field-value['\"]>(.*)<\\/span>", RegexOptions.Compiled | RegexOptions.Singleline );
        Regex ModifiedRegex = new Regex( "Modified.*<span class=['\"]field-name['\"]>(.*)<\\/span>.*<span class=['\"]field-value['\"]>(.*)<\\/span>.*<span class=['\"]field-value['\"]>(.*)<\\/span>", RegexOptions.Compiled | RegexOptions.Singleline );
        Regex DeletedRegex = new Regex( "Deleted.*<span class=['\"]field-name['\"]>(.*)<\\/span>.*<span class=['\"]field-value['\"]>(.*)<\\/span>", RegexOptions.Compiled | RegexOptions.Singleline );

        Regex UserLoginRegex = new Regex( "User logged in with.*<span class=['\"]field-name['\"]>(.*)<\\/span>.*<span class=['\"]field-value['\"]>(.*)<\\/span>.*<span class=['\"]field-value['\"]>(.*)<\\/span>", RegexOptions.Compiled );
        Regex SentRegEx = new Regex( "Sent (.*).*<span class=['\"]field-value['\"]>(.*)<\\/span>", RegexOptions.Compiled );
        Regex MergedRegex = new Regex( "Merged <span class=['\"]field-value['\"]>(.*)<\\/span> with this record.", RegexOptions.Compiled );

        // Group Member history was written in the following format pre-v7.4
        Regex AddedToGroupRegex = new Regex( "Added to (.*)", RegexOptions.Compiled );
        Regex RemovedFromGroupRegex = new Regex( "Removed from (.*)", RegexOptions.Compiled );
        Regex GroupMemberStatusChangeV6 = new Regex( "Group member status changed from (.*) to (.*)", RegexOptions.Compiled );
        Regex GroupRoleChangeV6 = new Regex( "Group role changed from (.*) to (.*)", RegexOptions.Compiled );

        // pre-v8 had inconsistent summary for Add/Modify/Remove Photo
        Regex AddedPhoto = new Regex( "Added a photo.", RegexOptions.Compiled );
        Regex ModifiedPhoto = new Regex( "Modified the photo.", RegexOptions.Compiled );
        Regex DeletedPhoto = new Regex( "Deleted the photo.", RegexOptions.Compiled );

        // Some history was logged as 'Created ..' or 'Generated..' instead of Add
        Regex CreatedGeneratedRegex = new Regex( "Created(.*)|Generated(.*)", RegexOptions.Compiled );

        Regex DeletedTransactionRegex = new Regex( "Deleted transaction", RegexOptions.Compiled );
        Regex DeletedRegistrationRegex = new Regex( "Deleted registration", RegexOptions.Compiled );
        Regex ResentConfirmationRegex = new Regex( "Resent Confirmation", RegexOptions.Compiled );

        Regex DeletedTheBatchRegex = new Regex( "Deleted the batch", RegexOptions.Compiled );

        Regex TransactionMatchRegex = new Regex( "Matched transaction", RegexOptions.Compiled );
        Regex TransactionUnmatchRegex = new Regex( "Unmatched transaction", RegexOptions.Compiled );

        Regex MadePaymentRegex = new Regex( "Made (.*) payment", RegexOptions.Compiled );
        Regex IsSensitiveRegex = new Regex( @"\(Sensitive attribute values are not logged in history\)", RegexOptions.Compiled );

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            int howMany = dataMap.GetString( "HowMany" ).AsIntegerOrNull() ?? 500000;
            var commandTimeout = dataMap.GetString( "CommandTimeout" ).AsIntegerOrNull() ?? 3600;

            bool anyRemaining = UpdateHistoryRecords( context, howMany, commandTimeout );

            if ( !anyRemaining )
            {
                // Verify that there are not any history records that haven't been migrated
                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.CommandTimeout = commandTimeout;

                    if ( !new HistoryService( rockContext )
                        .Queryable()
                        .Where( c => c.ChangeType == null )
                        .Any() )
                    {

                        // delete job if there are no un-migrated history rows  left
                        var jobId = context.GetJobId();
                        var jobService = new ServiceJobService( rockContext );
                        var job = jobService.Get( jobId );
                        if ( job != null )
                        {
                            jobService.Delete( job );
                            rockContext.SaveChanges();
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the history records.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="howManyToConvert">The how many to convert.</param>
        /// <param name="commandTimeout">The command timeout.</param>
        /// <returns></returns>
#pragma warning disable 612, 618
        private bool UpdateHistoryRecords( IJobExecutionContext context, int howManyToConvert, int commandTimeout )
        {
            bool anyRemaining = true;

            int howManyLeft = howManyToConvert;
            while ( howManyLeft > 0 )
            {
                using ( var rockContext = new RockContext() )
                {
                    int take = howManyLeft < 100 ? howManyLeft : 100;

                    // if there is any pre-v8 History Summary Data, ChangeType would be null
                    var historyRecords = new HistoryService( rockContext ).Queryable()
                        .Where( c => c.ChangeType == null )
                        .OrderByDescending( c => c.Id )
                        .Take( take )
                        .ToList();

                    anyRemaining = historyRecords.Count >= take;
                    howManyLeft = anyRemaining ? howManyLeft - take : 0;

                    foreach ( var historyRecord in historyRecords )
                    {
                        Match modifiedMatch = ModifiedRegex.Match( historyRecord.Summary );
                        Match addedMatch = AddedRegex.Match( historyRecord.Summary );
                        Match deletedMatch = DeletedRegex.Match( historyRecord.Summary );
                        Match userLoginMatch = UserLoginRegex.Match( historyRecord.Summary );
                        Match sentMatch = SentRegEx.Match( historyRecord.Summary );
                        Match mergeMatch = MergedRegex.Match( historyRecord.Summary );
                        Match addedToGroupMatch = AddedToGroupRegex.Match( historyRecord.Summary );
                        Match removedFromGroupMatch = RemovedFromGroupRegex.Match( historyRecord.Summary );
                        Match groupMemberStatusChangeMatch = GroupMemberStatusChangeV6.Match( historyRecord.Summary );
                        Match groupRoleChangeMatch = GroupRoleChangeV6.Match( historyRecord.Summary );
                        Match addedPhotoMatch = AddedPhoto.Match( historyRecord.Summary );
                        Match modifiedPhotoMatch = ModifiedPhoto.Match( historyRecord.Summary );
                        Match deletedPhotoMatch = DeletedPhoto.Match( historyRecord.Summary );
                        Match createdGeneratedMatch = CreatedGeneratedRegex.Match( historyRecord.Summary );
                        Match transactionMatchMatch = TransactionMatchRegex.Match( historyRecord.Summary );
                        Match transactionUnmatchMatch = TransactionUnmatchRegex.Match( historyRecord.Summary );
                        Match deletedTheBatchMatch = DeletedTheBatchRegex.Match( historyRecord.Summary );
                        Match madePaymentMatch = MadePaymentRegex.Match( historyRecord.Summary );
                        Match resentConfirmationRegexMatch = ResentConfirmationRegex.Match( historyRecord.Summary );
                        Match deletedTransactionRegexMatch = DeletedTransactionRegex.Match( historyRecord.Summary );
                        Match deletedRegistrationRegexMatch = DeletedRegistrationRegex.Match( historyRecord.Summary );

                        History.HistoryVerb? historyVerb = null;
                        History.HistoryChangeType historyChangeType = History.HistoryChangeType.Record;
                        var origSummary = historyRecord.Summary;

                        if ( modifiedMatch.Success )
                        {
                            historyVerb = History.HistoryVerb.Modify;
                            historyChangeType = History.HistoryChangeType.Property;
                            historyRecord.ValueName = modifiedMatch.Groups[1].Value.Trim();
                            historyRecord.OldValue = modifiedMatch.Groups[2].Value;
                            historyRecord.NewValue = modifiedMatch.Groups[3].Value;
                            historyRecord.Summary = null;
                        }
                        else if ( addedMatch.Success )
                        {
                            historyVerb = History.HistoryVerb.Modify;
                            historyChangeType = History.HistoryChangeType.Property;

                            historyRecord.ValueName = addedMatch.Groups[1].Value.Trim();
                            historyRecord.OldValue = null;
                            historyRecord.NewValue = addedMatch.Groups[2].Value;
                            historyRecord.Summary = null;
                        }
                        else if ( deletedMatch.Success )
                        {
                            historyVerb = History.HistoryVerb.Modify;
                            historyChangeType = History.HistoryChangeType.Property;
                            historyRecord.ValueName = deletedMatch.Groups[1].Value.Trim();
                            historyRecord.OldValue = deletedMatch.Groups[2].Value;
                            historyRecord.NewValue = null;
                            historyRecord.Summary = null;
                        }
                        else if ( userLoginMatch.Success )
                        {
                            /* User logged in with <span class='field-name'>admin</span> username, to <span class='field-value'>https://somesite/page/3?returnurl=%252f</span>, from <span class='field-value'>10.0.10.10</span>. */
                            historyVerb = History.HistoryVerb.Login;
                            historyChangeType = History.HistoryChangeType.Record;
                            var userName = userLoginMatch.Groups[1].Value.Trim();
                            historyRecord.ValueName = userName;
                            var startSummary = $"User logged in with <span class='field-name'>{userName}</span> username,";

                            // move any extra info in the summary to RelatedData
                            historyRecord.RelatedData = historyRecord.Summary.Replace( startSummary, string.Empty ).Trim();
                            historyRecord.Summary = null;
                        }
                        else if ( sentMatch.Success )
                        {
                            // Sent communication from <span class='field-value'>From Name</span>.
                            // Sent SMS ...
                            historyVerb = History.HistoryVerb.Sent;
                            historyChangeType = History.HistoryChangeType.Record;
                            var sentValue = sentMatch.Groups[1].Value.Trim();
                            historyRecord.ValueName = sentValue;
                            if ( string.IsNullOrEmpty( historyRecord.RelatedData ) )
                            {
                                historyRecord.RelatedData = sentMatch.Groups[2].Value;
                            }

                            historyRecord.Summary = null;
                        }
                        else if ( mergeMatch.Success )
                        {
                            // Merged <span class='field-value'>Ted Decker [ID: 1234]</span> with this record.
                            historyVerb = History.HistoryVerb.Merge;
                            historyChangeType = History.HistoryChangeType.Record;
                            historyRecord.ValueName = mergeMatch.Groups[1].Value.Trim();

                            historyRecord.Summary = null;
                        }
                        else if ( addedToGroupMatch.Success )
                        {
                            historyVerb = History.HistoryVerb.AddedToGroup;
                            historyChangeType = History.HistoryChangeType.Record;
                            historyRecord.ValueName = addedToGroupMatch.Groups[1].Value.Trim().TrimEnd( new char[] { '.' } );

                            historyRecord.Summary = null;
                        }
                        else if ( removedFromGroupMatch.Success )
                        {
                            historyVerb = History.HistoryVerb.RemovedFromGroup;
                            historyChangeType = History.HistoryChangeType.Record;
                            historyRecord.ValueName = removedFromGroupMatch.Groups[1].Value.Trim().TrimEnd( new char[] { '.' } );

                            historyRecord.Summary = null;
                        }
                        else if ( groupMemberStatusChangeMatch.Success )
                        {
                            historyVerb = History.HistoryVerb.Modify;
                            historyChangeType = History.HistoryChangeType.Property;
                            historyRecord.ValueName = "Member Status";
                            historyRecord.OldValue = groupMemberStatusChangeMatch.Groups[1].Value;
                            historyRecord.NewValue = groupMemberStatusChangeMatch.Groups[2].Value;
                            historyRecord.Summary = null;
                        }
                        else if ( groupRoleChangeMatch.Success )
                        {
                            historyVerb = History.HistoryVerb.Modify;
                            historyChangeType = History.HistoryChangeType.Property;
                            historyRecord.ValueName = "Group Role";
                            historyRecord.OldValue = groupRoleChangeMatch.Groups[1].Value;
                            historyRecord.NewValue = groupRoleChangeMatch.Groups[2].Value;
                            historyRecord.Summary = null;
                        }
                        else if ( addedPhotoMatch.Success )
                        {
                            historyVerb = History.HistoryVerb.Add;
                            historyChangeType = History.HistoryChangeType.Property;
                            historyRecord.ValueName = "Photo";
                            historyRecord.Summary = null;
                        }
                        else if ( modifiedPhotoMatch.Success )
                        {
                            historyVerb = History.HistoryVerb.Modify;
                            historyChangeType = History.HistoryChangeType.Property;
                            historyRecord.ValueName = "Photo";
                            historyRecord.Summary = null;
                        }
                        else if ( deletedPhotoMatch.Success )
                        {
                            historyVerb = History.HistoryVerb.Delete;
                            historyChangeType = History.HistoryChangeType.Property;
                            historyRecord.ValueName = "Photo";
                            historyRecord.Summary = null;
                        }
                        else if ( createdGeneratedMatch.Success )
                        {
                            historyVerb = History.HistoryVerb.Add;
                            historyChangeType = History.HistoryChangeType.Record;

                            historyRecord.ValueName = createdGeneratedMatch.Groups[1].Value.Trim();
                            historyRecord.OldValue = null;
                            historyRecord.NewValue = null;
                            historyRecord.Summary = null;
                        }
                        else if ( transactionMatchMatch.Success )
                        {
                            historyVerb = History.HistoryVerb.Matched;
                            historyChangeType = History.HistoryChangeType.Record;

                            historyRecord.ValueName = "transaction";
                            historyRecord.Summary = null;
                        }
                        else if ( transactionUnmatchMatch.Success )
                        {
                            historyVerb = History.HistoryVerb.Unmatched;
                            historyChangeType = History.HistoryChangeType.Record;

                            historyRecord.ValueName = "transaction";
                            historyRecord.Summary = null;
                        }
                        else if ( deletedTheBatchMatch.Success )
                        {
                            historyVerb = History.HistoryVerb.Delete;
                            historyChangeType = History.HistoryChangeType.Record;

                            historyRecord.ValueName = "batch";
                            historyRecord.Summary = null;
                        }
                        else if ( madePaymentMatch.Success )
                        {
                            historyVerb = History.HistoryVerb.Add;
                            historyChangeType = History.HistoryChangeType.Record;

                            historyRecord.ValueName = "payment";
                            historyRecord.OldValue = null;
                            historyRecord.NewValue = madePaymentMatch.Groups[1].Value;
                            historyRecord.Summary = null;
                        }
                        else if ( deletedTransactionRegexMatch.Success )
                        {
                            historyVerb = History.HistoryVerb.Delete;
                            historyChangeType = History.HistoryChangeType.Record;

                            historyRecord.ValueName = "transaction";
                            historyRecord.Summary = null;
                        }
                        else if ( resentConfirmationRegexMatch.Success )
                        {
                            historyVerb = History.HistoryVerb.Sent;
                            historyChangeType = History.HistoryChangeType.Record;

                            historyRecord.ValueName = "Confirmation";
                            historyRecord.RelatedData = "Resent";
                            historyRecord.Summary = null;
                        }
                        else if ( deletedRegistrationRegexMatch.Success )
                        {
                            historyVerb = History.HistoryVerb.Delete;
                            historyChangeType = History.HistoryChangeType.Record;

                            historyRecord.ValueName = "registration";
                            historyRecord.Summary = null;
                        }
                        else
                        {
                            // some other type of history record. Set History.ChangeType so that we know that it has been migrated, but just leave Summary alone and the History UIs will use that as a fallback
                            historyChangeType = History.HistoryChangeType.Record;
                        }

                        // just in case the ValueName is over 250, truncate it (it could be a really long attribute name)
                        historyRecord.ValueName = historyRecord.ValueName.Truncate( 250 );

                        historyRecord.Verb = historyVerb?.ConvertToString( false ).ToUpper();
                        historyRecord.ChangeType = historyChangeType.ConvertToString( false );

                        if ( IsSensitiveRegex.IsMatch( origSummary ) )
                        {
                            historyRecord.IsSensitive = true;
                        }

                        var summaryHtml = historyRecord.SummaryHtml;
                    }

                    int numberMigrated = howManyToConvert - howManyLeft;
                    var percentComplete = howManyToConvert > 0 ? ( numberMigrated * 100.0 ) / howManyToConvert : 100.0;
                    var statusMessage = $@"Progress: {numberMigrated} of {howManyToConvert} ({Math.Round( percentComplete, 1 )}%) History summary data migrated to history verbs and nouns";
                    context.UpdateLastStatusMessage( statusMessage );

                    rockContext.SaveChanges( disablePrePostProcessing: true );
                }
            }

            return anyRemaining;
        }
#pragma warning restore 612, 618
    }
}
