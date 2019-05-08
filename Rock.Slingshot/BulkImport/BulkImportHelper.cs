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
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Slingshot.Model;
using Rock.Web.Cache;
using System.Security.Cryptography;

namespace Rock.Slingshot
{
    /// <summary>
    ///
    /// </summary>
    public class BulkImporter
    {
        #region util

        /// <summary>
        /// Gets the response message.
        /// </summary>
        /// <param name="recordsInserted">The records inserted.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="milliseconds">The milliseconds.</param>
        /// <returns></returns>
        private string GetResponseMessage( int recordsInserted, string tableName, long milliseconds )
        {
            if ( recordsInserted == 0 )
            {
                return $"No {tableName} records were imported [{milliseconds}ms]";
            }
            else
            {
                return $"Imported {recordsInserted} {tableName} records [{milliseconds}ms]";
            }
        }

        /// <summary>
        /// Definition for OnProgress delegate
        /// </summary>
        public delegate void OnProgressEvent( string message );

        /// <summary>
        /// Delegate for handling ProgressMessages
        /// </summary>
        public OnProgressEvent OnProgress;

        /// <summary>
        ///
        /// </summary>
        public enum ImportUpdateType
        {
            AlwaysUpdate,
            AddOnly,
            MostRecentWins
        }

        /// <summary>
        /// Gets or sets the import update option.
        /// </summary>
        /// <value>
        /// The import update option.
        /// </value>
        public ImportUpdateType ImportUpdateOption { get; set; }

        #endregion util

        #region AttendanceImport

        /// <summary>
        /// Bulks the attendance import.
        /// </summary>
        /// <param name="attendanceImports">The attendance imports.</param>
        /// <returns></returns>
        public string BulkAttendanceImport( List<AttendanceImport> attendanceImports, string foreignSystemKey )
        {
            Stopwatch stopwatchTotal = Stopwatch.StartNew();
            Stopwatch stopwatch = Stopwatch.StartNew();

            RockContext rockContext = new RockContext();
            StringBuilder sbStats = new StringBuilder();

            int groupTypeIdFamily = GroupTypeCache.GetFamilyGroupType().Id;
            DateTime importDateTime = RockDateTime.Now;

            // Get all of the existing group ids that have been imported (excluding families)
            var groupIdLookup = new GroupService( rockContext ).Queryable().Where( a => a.GroupTypeId != groupTypeIdFamily && a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey )
                .Select( a => new { a.Id, a.ForeignId } ).ToDictionary( k => k.ForeignId.Value, v => v.Id );

            // Get all the existing location ids that have been imported
            var locationIdLookup = new LocationService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey )
                .Select( a => new { a.Id, a.ForeignId } ).ToDictionary( k => k.ForeignId.Value, v => v.Id );

            // Get all the existing schedule ids that have been imported
            var scheduleIdLookup = new ScheduleService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey )
                .Select( a => new { a.Id, a.ForeignId } ).ToDictionary( k => k.ForeignId.Value, v => v.Id );

            // Get the primary alias id lookup for each person foreign id
            var personAliasIdLookup = new PersonAliasService( rockContext ).Queryable().Where( a => a.Person.ForeignId.HasValue && a.Person.ForeignKey == foreignSystemKey && a.PersonId == a.AliasPersonId )
                .Select( a => new { PersonAliasId = a.Id, PersonForeignId = a.Person.ForeignId } ).ToDictionary( k => k.PersonForeignId.Value, v => v.PersonAliasId );

            // Get list of existing attendance records that have already been imported
            var qryAttendancesWithForeignIds = new AttendanceService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey );
            var attendancesAlreadyExistForeignIdHash = new HashSet<int>( qryAttendancesWithForeignIds.Select( a => a.ForeignId.Value ).ToList() );

            // Get list of existing occurrence records that have already been created
            var existingOccurrencesLookup = new AttendanceOccurrenceService( rockContext ).Queryable()
                .Select( o => new
                {
                    Id = o.Id,
                    GroupId = o.GroupId,
                    LocationId = o.LocationId,
                    ScheduleId = o.ScheduleId,
                    OccurrenceDate = o.OccurrenceDate
                } ).ToDictionary( k => $"{k.GroupId}|{k.LocationId}|{k.ScheduleId}|{k.OccurrenceDate}", v => v.Id );

            // Get the attendance records being imported that are new
            var newAttendanceImports = attendanceImports.Where( a => !a.AttendanceForeignId.HasValue || !attendancesAlreadyExistForeignIdHash.Contains( a.AttendanceForeignId.Value ) ).ToList();

            // Create list of occurrences to be bulk inserted
            var newOccurrences = new List<ImportOccurrence>();

            // Get unique combination of group/location/schedule/date for attendance records being added
            var newAttendanceOccurrenceKeys = newAttendanceImports
                .GroupBy( a => new
                {
                    a.GroupForeignId,
                    a.LocationForeignId,
                    a.ScheduleForeignId,
                    OccurrenceDate = a.StartDateTime.Date
                } )
                .Select( a => a.Key )
                .ToList();
            foreach ( var groupKey in newAttendanceOccurrenceKeys )
            {
                var occurrence = new ImportOccurrence();

                if ( groupKey.GroupForeignId.HasValue )
                {
                    occurrence.GroupId = groupIdLookup.GetValueOrNull( groupKey.GroupForeignId.Value );
                }

                if ( groupKey.LocationForeignId.HasValue )
                {
                    occurrence.LocationId = locationIdLookup.GetValueOrNull( groupKey.LocationForeignId.Value );
                }

                if ( groupKey.ScheduleForeignId.HasValue )
                {
                    occurrence.ScheduleId = scheduleIdLookup.GetValueOrNull( groupKey.ScheduleForeignId.Value );
                }

                occurrence.OccurrenceDate = groupKey.OccurrenceDate;

                // If we haven'r already added it to list, and it doesn't already exist, add it to list
                if ( !existingOccurrencesLookup.ContainsKey( $"{occurrence.GroupId}|{occurrence.LocationId}|{occurrence.ScheduleId}|{occurrence.OccurrenceDate}" ))
                {
                    newOccurrences.Add( occurrence );
                }
            }

            var occurrencesToInsert = newOccurrences
                .GroupBy( n => new 
                {
                    n.GroupId,
                    n.LocationId,
                    n.ScheduleId,
                    n.OccurrenceDate
                } )
                .Select( o => new AttendanceOccurrence
                {
                    GroupId = o.Key.GroupId,
                    LocationId = o.Key.LocationId,
                    ScheduleId = o.Key.ScheduleId,
                    OccurrenceDate = o.Key.OccurrenceDate 
                } )
                .ToList();

            // Add all the new occurrences
            rockContext.BulkInsert( occurrencesToInsert );

            // Load all the existing occurrences again.
            existingOccurrencesLookup = new AttendanceOccurrenceService( rockContext ).Queryable()
                .Select( o => new
                {
                    Id = o.Id,
                    GroupId = o.GroupId,
                    LocationId = o.LocationId,
                    ScheduleId = o.ScheduleId,
                    OccurrenceDate = o.OccurrenceDate
                } ).ToDictionary( k => $"{k.GroupId}|{k.LocationId}|{k.ScheduleId}|{k.OccurrenceDate}", v => v.Id );

            var attendancesToInsert = new List<Attendance>( newAttendanceImports.Count );

            foreach ( var attendanceImport in newAttendanceImports )
            {
                var attendance = new Attendance();
                attendance.ForeignId = attendanceImport.AttendanceForeignId;
                attendance.ForeignKey = foreignSystemKey;

                attendance.CampusId = attendanceImport.CampusId;
                attendance.StartDateTime = attendanceImport.StartDateTime;
                attendance.EndDateTime = attendanceImport.EndDateTime;

                int? groupId = null;
                int? locationId = null;
                int? scheduleId = null;
                DateTime occurrenceDate = attendanceImport.StartDateTime.Date;

                if ( attendanceImport.GroupForeignId.HasValue )
                {
                    groupId = groupIdLookup.GetValueOrNull( attendanceImport.GroupForeignId.Value );
                }

                if ( attendanceImport.LocationForeignId.HasValue )
                {
                    locationId = locationIdLookup.GetValueOrNull( attendanceImport.LocationForeignId.Value );
                }

                if ( attendanceImport.ScheduleForeignId.HasValue )
                {
                    scheduleId = scheduleIdLookup.GetValueOrNull( attendanceImport.ScheduleForeignId.Value );
                }

                int? occurrenceId = existingOccurrencesLookup.GetValueOrNull( $"{groupId}|{locationId}|{scheduleId}|{occurrenceDate}" );

                if ( occurrenceId.HasValue )
                {
                    attendance.OccurrenceId = occurrenceId.Value;
                    attendance.PersonAliasId = personAliasIdLookup.GetValueOrNull( attendanceImport.PersonForeignId );
                    attendance.Note = attendanceImport.Note;
                    attendance.DidAttend = true;
                    attendance.CreatedDateTime = importDateTime;
                    attendance.ModifiedDateTime = importDateTime;

                    attendancesToInsert.Add( attendance );
                }
            }

            rockContext.BulkInsert( attendancesToInsert );

            sbStats.AppendLine( GetResponseMessage( newAttendanceImports.Count, "Attendance", stopwatchTotal.ElapsedMilliseconds ) );
            var responseText = sbStats.ToString();

            return responseText;
        }

        #endregion AttendanceImport

        #region FinancialAccountImport

        /// <summary>
        /// Bulks the financial account import.
        /// </summary>
        /// <param name="financialAccountImports">The financial account imports.</param>
        /// <returns></returns>
        public string BulkFinancialAccountImport( List<FinancialAccountImport> financialAccountImports, string foreignSystemKey )
        {
            Stopwatch stopwatchTotal = Stopwatch.StartNew();

            RockContext rockContext = new RockContext();

            var qryFinancialAccountsWithForeignIds = new FinancialAccountService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey );

            var financialAccountAlreadyExistForeignIdHash = new HashSet<int>( qryFinancialAccountsWithForeignIds.Select( a => a.ForeignId.Value ).ToList() );

            List<FinancialAccount> financialAccountsToInsert = new List<FinancialAccount>();
            var newFinancialAccountImports = financialAccountImports.Where( a => !financialAccountAlreadyExistForeignIdHash.Contains( a.FinancialAccountForeignId ) ).ToList();

            var importDateTime = RockDateTime.Now;

            foreach ( var financialAccountImport in newFinancialAccountImports )
            {
                var financialAccount = new FinancialAccount();
                financialAccount.ForeignId = financialAccountImport.FinancialAccountForeignId;
                financialAccount.ForeignKey = foreignSystemKey;
                if ( financialAccountImport.Name.Length > 50 )
                {
                    financialAccount.Name = financialAccountImport.Name.Left( 50 );
                    financialAccount.Description = financialAccountImport.Name;
                }
                else
                {
                    financialAccount.Name = financialAccountImport.Name;
                }

                financialAccount.CampusId = financialAccountImport.CampusId;
                financialAccount.IsTaxDeductible = financialAccountImport.IsTaxDeductible;
                financialAccount.CreatedDateTime = importDateTime;
                financialAccount.ModifiedDateTime = importDateTime;

                financialAccountsToInsert.Add( financialAccount );
            }

            rockContext.BulkInsert( financialAccountsToInsert );

            var financialAccountsUpdated = false;
            var financialAccountImportsWithParentFinancialAccount = newFinancialAccountImports.Where( a => a.ParentFinancialAccountForeignId.HasValue ).ToList();
            var financialAccountLookup = qryFinancialAccountsWithForeignIds.ToDictionary( k => k.ForeignId.Value, v => v );
            foreach ( var financialAccountImport in financialAccountImportsWithParentFinancialAccount )
            {
                var financialAccount = financialAccountLookup.GetValueOrNull( financialAccountImport.FinancialAccountForeignId );
                if ( financialAccount != null )
                {
                    var parentFinancialAccount = financialAccountLookup.GetValueOrNull( financialAccountImport.ParentFinancialAccountForeignId.Value );
                    if ( parentFinancialAccount != null && financialAccount.ParentAccountId != parentFinancialAccount.Id )
                    {
                        financialAccount.ParentAccountId = parentFinancialAccount.Id;
                        financialAccountsUpdated = true;
                    }
                    else
                    {
                        throw new Exception( $"ERROR: Unable to lookup ParentFinancialAccount {financialAccountImport.ParentFinancialAccountForeignId} for FinancialAccount {financialAccountImport.Name}:{financialAccountImport.FinancialAccountForeignId} " );
                    }
                }
                else
                {
                    throw new Exception( "Unable to lookup FinancialAccount with ParentFinancialAccount" );
                }
            }

            if ( financialAccountsUpdated )
            {
                rockContext.SaveChanges( true );
            }

            stopwatchTotal.Stop();

            return GetResponseMessage( financialAccountsToInsert.Count, "Financial Accounts", stopwatchTotal.ElapsedMilliseconds );
        }

        /// <summary>
        /// Tables the that have a ForeignKey == foreign system key.
        /// </summary>
        /// <param name="foreignSystemKey">The foreign system key.</param>
        /// <returns></returns>
        public static List<string> TablesThatHaveForeignSystemKey( string foreignSystemKey )
        {
            var rockContext = new RockContext();
            var tableList = new List<string>();

            // Don't check Attendance ForeignId since it might not have a ForeignId from the source system
            if ( new AttendanceService( rockContext ).Queryable().Any( a => a.ForeignKey == foreignSystemKey ) )
            {
                tableList.Add( "Attendance" );
            }

            if ( new CampusService( rockContext ).Queryable().Any( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey ) )
            {
                tableList.Add( "Campus" );
            }

            if ( new FinancialAccountService( rockContext ).Queryable().Any( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey ) )
            {
                tableList.Add( "Financial Account" );
            }

            if ( new FinancialBatchService( rockContext ).Queryable().Any( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey ) )
            {
                tableList.Add( "Financial Batch" );
            }

            if ( new FinancialPaymentDetailService( rockContext ).Queryable().Any( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey ) )
            {
                tableList.Add( "Financial Payment Detail" );
            }

            if ( new FinancialPledgeService( rockContext ).Queryable().Any( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey ) )
            {
                tableList.Add( "Financial Pledge" );
            }

            if ( new FinancialTransactionDetailService( rockContext ).Queryable().Any( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey ) )
            {
                tableList.Add( "Financial Detail Transaction" );
            }

            if ( new FinancialTransactionService( rockContext ).Queryable().Any( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey ) )
            {
                tableList.Add( "Financial Transaction" );
            }

            // Check Group as used as Family and non-Family
            int groupTypeIdFamily = GroupTypeCache.GetFamilyGroupType().Id;
            if ( new GroupService( rockContext ).Queryable().Any( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey && a.GroupTypeId == groupTypeIdFamily ) )
            {
                tableList.Add( "Family" );
            }

            if ( new GroupService( rockContext ).Queryable().Any( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey && a.GroupTypeId != groupTypeIdFamily ) )
            {
                tableList.Add( "Group" );
            }

            if ( new GroupTypeService( rockContext ).Queryable().Any( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey ) )
            {
                tableList.Add( "Group Type" );
            }

            if ( new LocationService( rockContext ).Queryable().Any( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey ) )
            {
                tableList.Add( "Location" );
            }

            if ( new NoteService( rockContext ).Queryable().Any( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey ) )
            {
                tableList.Add( "Note" );
            }

            if ( new PersonAliasService( rockContext ).Queryable().Any( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey ) )
            {
                tableList.Add( "PersonAlias" );
            }

            if ( new PersonService( rockContext ).Queryable().Any( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey ) )
            {
                tableList.Add( "Person" );
            }

            if ( new ScheduleService( rockContext ).Queryable().Any( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey ) )
            {
                tableList.Add( "Schedule" );
            }

            var binaryFileFamilyForeignKeyPrefix = $"FamilyForeignId_{foreignSystemKey}_";
            var binaryFilePersonForeignKeyPrefix = $"PersonForeignId_{foreignSystemKey}_";
            if ( new BinaryFileService( rockContext ).Queryable().Any( a => a.ForeignId.HasValue && a.ForeignKey.StartsWith( binaryFilePersonForeignKeyPrefix ) ) )
            {
                tableList.Add( "Person Photo" );
            }

            if ( new BinaryFileService( rockContext ).Queryable().Any( a => a.ForeignId.HasValue && a.ForeignKey.StartsWith( binaryFileFamilyForeignKeyPrefix ) ) )
            {
                tableList.Add( "Family Photo" );
            }

            return tableList;
        }

        /// <summary>
        /// Get foreign system keys that have been used in previous Imports
        /// </summary>
        /// <returns></returns>
        public static List<string> UsedForeignSystemKeys()
        {
            return new PersonService( new RockContext() ).Queryable().Where( a => a.ForeignId.HasValue && !string.IsNullOrEmpty( a.ForeignKey ) ).Select( a => a.ForeignKey ).Distinct().ToList();
        }

        #endregion FinancialAccountImport

        #region FinancialBatchImport

        /// <summary>
        /// Bulks the financial batch import.
        /// </summary>
        /// <param name="financialBatchImports">The financial batch imports.</param>
        /// <returns></returns>
        public string BulkFinancialBatchImport( List<FinancialBatchImport> financialBatchImports, string foreignSystemKey )
        {
            Stopwatch stopwatchTotal = Stopwatch.StartNew();

            RockContext rockContext = new RockContext();

            var qryFinancialBatchsWithForeignIds = new FinancialBatchService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey );

            var financialBatchAlreadyExistForeignIdHash = new HashSet<int>( qryFinancialBatchsWithForeignIds.Select( a => a.ForeignId.Value ).ToList() );

            List<FinancialBatch> financialBatchsToInsert = new List<FinancialBatch>();
            var newFinancialBatchImports = financialBatchImports.Where( a => !financialBatchAlreadyExistForeignIdHash.Contains( a.FinancialBatchForeignId ) ).ToList();

            // Get the primary alias id lookup for each person foreign id
            var personAliasIdLookup = new PersonAliasService( rockContext ).Queryable().Where( a => a.Person.ForeignId.HasValue && a.Person.ForeignKey == foreignSystemKey && a.PersonId == a.AliasPersonId )
                .Select( a => new { PersonAliasId = a.Id, PersonForeignId = a.Person.ForeignId } ).ToDictionary( k => k.PersonForeignId.Value, v => v.PersonAliasId );

            var importDateTime = RockDateTime.Now;

            foreach ( var financialBatchImport in newFinancialBatchImports )
            {
                var financialBatch = new FinancialBatch();
                financialBatch.ForeignId = financialBatchImport.FinancialBatchForeignId;
                financialBatch.ForeignKey = foreignSystemKey;
                if ( financialBatchImport.Name.Length > 50 )
                {
                    financialBatch.Name = financialBatchImport.Name.Left( 50 );
                }
                else
                {
                    financialBatch.Name = financialBatchImport.Name;
                }

                financialBatch.CampusId = financialBatchImport.CampusId;
                financialBatch.ControlAmount = financialBatchImport.ControlAmount;

                financialBatch.CreatedDateTime = financialBatchImport.CreatedDateTime ?? importDateTime;
                financialBatch.BatchEndDateTime = financialBatchImport.EndDate;

                financialBatch.ModifiedDateTime = financialBatchImport.ModifiedDateTime ?? importDateTime;
                financialBatch.BatchStartDateTime = financialBatchImport.StartDate;

                switch ( financialBatchImport.Status )
                {
                    case FinancialBatchImport.BatchStatus.Closed:
                        financialBatch.Status = BatchStatus.Closed;
                        break;

                    case FinancialBatchImport.BatchStatus.Open:
                        financialBatch.Status = BatchStatus.Open;
                        break;

                    case FinancialBatchImport.BatchStatus.Pending:
                        financialBatch.Status = BatchStatus.Pending;
                        break;
                }

                if ( financialBatchImport.CreatedByPersonForeignId.HasValue )
                {
                    financialBatch.CreatedByPersonAliasId = personAliasIdLookup.GetValueOrNull( financialBatchImport.CreatedByPersonForeignId.Value );
                }

                if ( financialBatchImport.ModifiedByPersonForeignId.HasValue )
                {
                    financialBatch.ModifiedByPersonAliasId = personAliasIdLookup.GetValueOrNull( financialBatchImport.ModifiedByPersonForeignId.Value );
                }

                financialBatchsToInsert.Add( financialBatch );
            }

            rockContext.BulkInsert( financialBatchsToInsert );

            stopwatchTotal.Stop();
            return GetResponseMessage( financialBatchsToInsert.Count, "Financial Batches", stopwatchTotal.ElapsedMilliseconds );
        }

        #endregion FinancialBatchImport

        #region FinancialTransactionImport

        /// <summary>
        /// Bulks the financial transaction import.
        /// </summary>
        /// <param name="financialTransactionImports">The financial transaction imports.</param>
        /// <returns></returns>
        public string BulkFinancialTransactionImport( List<FinancialTransactionImport> financialTransactionImports, string foreignSystemKey )
        {
            Stopwatch stopwatchTotal = Stopwatch.StartNew();

            RockContext rockContext = new RockContext();

            var qryFinancialTransactionsWithForeignIds = new FinancialTransactionService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey );

            var financialTransactionAlreadyExistForeignIdHash = new HashSet<int>( qryFinancialTransactionsWithForeignIds.Select( a => a.ForeignId.Value ).ToList() );

            var newFinancialTransactionImports = financialTransactionImports.Where( a => !financialTransactionAlreadyExistForeignIdHash.Contains( a.FinancialTransactionForeignId ) ).ToList();

            // Get the primary alias id lookup for each person foreign id
            var personAliasIdLookup = new PersonAliasService( rockContext ).Queryable().Where( a => a.Person.ForeignId.HasValue && a.Person.ForeignKey == foreignSystemKey && a.PersonId == a.AliasPersonId )
                .Select( a => new { PersonAliasId = a.Id, PersonForeignId = a.Person.ForeignId } ).ToDictionary( k => k.PersonForeignId.Value, v => v.PersonAliasId );

            var batchIdLookup = new FinancialBatchService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey )
                .Select( a => new { a.Id, a.ForeignId } ).ToDictionary( k => k.ForeignId.Value, v => v.Id );

            var accountIdLookup = new FinancialAccountService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey )
                .Select( a => new { a.Id, a.ForeignId } ).ToDictionary( k => k.ForeignId.Value, v => v.Id );

            var importDateTime = RockDateTime.Now;

            // Insert FinancialPaymentDetail for all the transactions first
            List<FinancialPaymentDetail> financialPaymentDetailToInsert = new List<FinancialPaymentDetail>( newFinancialTransactionImports.Count );
            foreach ( var financialTransactionImport in newFinancialTransactionImports )
            {
                var financialPaymentDetail = new FinancialPaymentDetail();
                financialPaymentDetail.CurrencyTypeValueId = financialTransactionImport.CurrencyTypeValueId;
                financialPaymentDetail.ForeignId = financialTransactionImport.FinancialTransactionForeignId;
                financialPaymentDetail.ForeignKey = foreignSystemKey;
                financialPaymentDetail.CreatedDateTime = financialTransactionImport.CreatedDateTime ?? importDateTime;
                financialPaymentDetail.ModifiedDateTime = financialTransactionImport.ModifiedDateTime ?? importDateTime;
                financialPaymentDetailToInsert.Add( financialPaymentDetail );
            }

            OnProgress?.Invoke( $"Bulk Importing FinancialTransactions ( Payment Details )... " );

            rockContext.BulkInsert( financialPaymentDetailToInsert );

            var financialPaymentDetailLookup = new FinancialPaymentDetailService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey )
                .Select( a => new { a.Id, a.ForeignId } ).ToDictionary( k => k.ForeignId.Value, v => v.Id );

            // Prepare and Insert FinancialTransactions
            List<FinancialTransaction> financialTransactionsToInsert = new List<FinancialTransaction>();
            foreach ( var financialTransactionImport in newFinancialTransactionImports )
            {
                var financialTransaction = new FinancialTransaction();
                financialTransaction.ForeignId = financialTransactionImport.FinancialTransactionForeignId;
                financialTransaction.ForeignKey = foreignSystemKey;

                if ( financialTransactionImport.AuthorizedPersonForeignId.HasValue )
                {
                    financialTransaction.AuthorizedPersonAliasId = personAliasIdLookup.GetValueOrNull( financialTransactionImport.AuthorizedPersonForeignId.Value );
                }

                financialTransaction.BatchId = batchIdLookup.GetValueOrNull( financialTransactionImport.BatchForeignId );
                financialTransaction.FinancialPaymentDetailId = financialPaymentDetailLookup.GetValueOrNull( financialTransactionImport.FinancialTransactionForeignId );

                financialTransaction.Summary = financialTransactionImport.Summary;
                financialTransaction.TransactionCode = financialTransactionImport.TransactionCode;
                financialTransaction.TransactionDateTime = financialTransactionImport.TransactionDate;
                financialTransaction.SourceTypeValueId = financialTransactionImport.TransactionSourceValueId;
                financialTransaction.TransactionTypeValueId = financialTransactionImport.TransactionTypeValueId;
                financialTransaction.CreatedDateTime = financialTransactionImport.CreatedDateTime ?? importDateTime;
                financialTransaction.ModifiedDateTime = financialTransactionImport.ModifiedDateTime ?? importDateTime;

                if ( financialTransactionImport.CreatedByPersonForeignId.HasValue )
                {
                    financialTransaction.CreatedByPersonAliasId = personAliasIdLookup.GetValueOrNull( financialTransactionImport.CreatedByPersonForeignId.Value );
                }

                if ( financialTransactionImport.ModifiedByPersonForeignId.HasValue )
                {
                    financialTransaction.ModifiedByPersonAliasId = personAliasIdLookup.GetValueOrNull( financialTransactionImport.ModifiedByPersonForeignId.Value );
                }

                financialTransactionsToInsert.Add( financialTransaction );
            }

            OnProgress?.Invoke( $"Bulk Importing FinancialTransactions... " );
            rockContext.BulkInsert( financialTransactionsToInsert );

            var financialTransactionIdLookup = new FinancialTransactionService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey )
                .Select( a => new { a.Id, a.ForeignId } )
                .ToList().ToDictionary( k => k.ForeignId.Value, v => v.Id );

            var financialAccountIdLookup = new FinancialAccountService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey )
                .Select( a => new { a.Id, a.ForeignId } )
                .ToList().ToDictionary( k => k.ForeignId.Value, v => v.Id );

            // Prepare and Insert the FinancialTransactionDetail records
            List<FinancialTransactionDetail> financialTransactionDetailsToInsert = new List<FinancialTransactionDetail>();
            foreach ( var financialTransactionImport in newFinancialTransactionImports )
            {
                foreach ( var financialTransactionDetailImport in financialTransactionImport.FinancialTransactionDetailImports )
                {
                    var financialTransactionDetail = new FinancialTransactionDetail();
                    financialTransactionDetail.TransactionId = financialTransactionIdLookup[financialTransactionImport.FinancialTransactionForeignId];
                    financialTransactionDetail.ForeignId = financialTransactionDetailImport.FinancialTransactionDetailForeignId;
                    financialTransactionDetail.ForeignKey = foreignSystemKey;
                    financialTransactionDetail.Amount = financialTransactionDetailImport.Amount;
                    financialTransactionDetail.AccountId = financialAccountIdLookup[financialTransactionDetailImport.FinancialAccountForeignId.Value];
                    financialTransactionDetail.Summary = financialTransactionDetailImport.Summary;
                    financialTransactionDetail.CreatedDateTime = financialTransactionDetailImport.CreatedDateTime ?? importDateTime;
                    financialTransactionDetail.ModifiedDateTime = financialTransactionDetailImport.ModifiedDateTime ?? importDateTime;

                    if ( financialTransactionDetailImport.CreatedByPersonForeignId.HasValue )
                    {
                        financialTransactionDetail.CreatedByPersonAliasId = personAliasIdLookup.GetValueOrNull( financialTransactionDetailImport.CreatedByPersonForeignId.Value );
                    }

                    if ( financialTransactionDetailImport.ModifiedByPersonForeignId.HasValue )
                    {
                        financialTransactionDetail.ModifiedByPersonAliasId = personAliasIdLookup.GetValueOrNull( financialTransactionDetailImport.ModifiedByPersonForeignId.Value );
                    }

                    financialTransactionDetailsToInsert.Add( financialTransactionDetail );
                }
            }

            OnProgress?.Invoke( $"Bulk Importing FinancialTransactions ( Transaction Details )... " );
            rockContext.BulkInsert( financialTransactionDetailsToInsert );

            stopwatchTotal.Stop();
            return GetResponseMessage( financialTransactionsToInsert.Count, "Financial Transactions", stopwatchTotal.ElapsedMilliseconds );
        }

        #endregion FinancialTransactionImport

        #region GroupImport

        /// <summary>
        /// Bulks the group import.
        /// </summary>
        /// <param name="groupImports">The group imports.</param>
        /// <returns></returns>
        public string BulkGroupImport( List<GroupImport> groupImports, string foreignSystemKey )
        {
            var initiatedWithWebRequest = HttpContext.Current?.Request != null;
            Stopwatch stopwatchTotal = Stopwatch.StartNew();
            Stopwatch stopwatch = Stopwatch.StartNew();

            RockContext rockContext = new RockContext();
            StringBuilder sbStats = new StringBuilder();

            int groupTypeIdFamily = GroupTypeCache.GetFamilyGroupType().Id;
            var entityTypeIdGroup = EntityTypeCache.Get<Group>().Id;
            var locationService = new LocationService( rockContext );
            Dictionary<int, List<AttributeValueCache>> attributeValuesLookup = new AttributeValueService( rockContext ).Queryable().Where( a => a.Attribute.EntityTypeId == entityTypeIdGroup && a.EntityId.HasValue )
                .Select( a => new
                {
                    GroupId = a.EntityId.Value,
                    a.AttributeId,
                    a.Value
                } )
                .GroupBy( a => a.GroupId )
                .ToDictionary(
                    k => k.Key,
                    v => v.Select( x => new AttributeValueCache { AttributeId = x.AttributeId, EntityId = x.GroupId, Value = x.Value } ).ToList() );

            Dictionary<int, Group> groupLookUp = new GroupService( rockContext ).Queryable().AsNoTracking().Where( a => a.GroupTypeId != groupTypeIdFamily && a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey )
                .ToList().ToDictionary( k => k.ForeignId.Value, v => v );
            
            var importedGroupTypeRoleNames = groupImports.GroupBy( a => a.GroupTypeId ).Select( a => new
            {
                GroupTypeId = a.Key,
                RoleNames = a.SelectMany( x => x.GroupMemberImports ).Select( x => x.RoleName ).Distinct().ToList()
            } );

            int groupUpdatesCount = 0;
            long groupUpdatesMS = 0;
            int progress = 0;
            int total = groupImports.Count();

            // Create any missing roles on the GroupType
            var groupTypeRolesToInsert = new List<GroupTypeRole>();

            var importedDateTime = RockDateTime.Now;

            foreach ( var importedGroupTypeRoleName in importedGroupTypeRoleNames )
            {
                var groupTypeCache = GroupTypeCache.Get( importedGroupTypeRoleName.GroupTypeId, rockContext );
                foreach ( var roleName in importedGroupTypeRoleName.RoleNames )
                {
                    if ( !groupTypeCache.Roles.Any( a => a.Name.Equals( roleName, StringComparison.OrdinalIgnoreCase ) ) )
                    {
                        var groupTypeRole = new GroupTypeRole();
                        groupTypeRole.GroupTypeId = groupTypeCache.Id;
                        groupTypeRole.Name = roleName.Left( 100 );
                        groupTypeRole.CreatedDateTime = importedDateTime;
                        groupTypeRole.ModifiedDateTime = importedDateTime;
                        groupTypeRolesToInsert.Add( groupTypeRole );
                    }
                }
            }

            var updatedGroupTypes = groupTypeRolesToInsert.Select( a => a.GroupTypeId.Value ).Distinct().ToList();
            updatedGroupTypes.ForEach( id => GroupTypeCache.UpdateCachedEntity( id, EntityState.Detached ) );

            if ( groupTypeRolesToInsert.Any() )
            {
                rockContext.BulkInsert( groupTypeRolesToInsert );
            }

            foreach ( var groupImport in groupImports )
            {
                progress++;
                if ( progress % 100 == 0 && groupUpdatesMS > 0 )
                {
                    if ( initiatedWithWebRequest && HttpContext.Current?.Response?.IsClientConnected != true )
                    {
                        // if this was called from a WebRequest (versus a job or utility), quit if the client has disconnected
                        return "Client Disconnected";
                    }

                    OnProgress?.Invoke( $"Bulk Importing Group {progress} of {total}" );
                }

                Group group = null;

                if ( groupLookUp.ContainsKey ( groupImport.GroupForeignId ) )
                {
                    group = groupLookUp[groupImport.GroupForeignId];
                }

                if ( group == null )
                {
                    group = new Group();
                    UpdateGroupPropertiesFromGroupImport( group, groupImport, foreignSystemKey, importedDateTime );
                    group.CreatedDateTime = importedDateTime;
                    group.ModifiedDateTime = importedDateTime;
                    groupLookUp.Add( groupImport.GroupForeignId, group );

                    // set weekly schedule for newly created groups
                    DayOfWeek meetingDay;
                    if ( !string.IsNullOrWhiteSpace( groupImport.MeetingDay ) && Enum.TryParse( groupImport.MeetingDay, out meetingDay ) )
                    {
                        TimeSpan meetingTime;
                        TimeSpan.TryParse( groupImport.MeetingTime, out meetingTime );
                        group.Schedule = new Schedule()
                        {
                            Name = group.Name,
                            IsActive = group.IsActive,
                            WeeklyDayOfWeek = meetingDay,
                            WeeklyTimeOfDay = meetingTime,
                            ForeignId = groupImport.GroupForeignId,
                            ForeignKey = foreignSystemKey,
                            CreatedDateTime = importedDateTime,
                            ModifiedDateTime = importedDateTime
                        };
                    }
                }
                else
                {
                    if (this.ImportUpdateOption == ImportUpdateType.AlwaysUpdate )
                    {
                        Stopwatch stopwatchGroupUpdates = Stopwatch.StartNew();
                        bool wasChanged = UpdateGroupFromGroupImport( groupImport, group, attributeValuesLookup, foreignSystemKey, importedDateTime );
                        stopwatchGroupUpdates.Stop();
                        groupUpdatesMS += stopwatchGroupUpdates.ElapsedMilliseconds;
                        if ( wasChanged )
                        {
                            groupUpdatesCount++;
                        }
                    }
                }
            }

            if ( groupUpdatesMS > 0 || groupUpdatesCount > 0 )
            {
                stopwatch.Stop();
                sbStats.AppendLine( $"Check for Group Updates [{stopwatch.ElapsedMilliseconds - groupUpdatesMS}ms]" );
                if ( groupUpdatesCount > 0 )
                {
                    sbStats.AppendLine( $"Updated {groupUpdatesCount} Group records [{groupUpdatesMS}ms]" );
                }
                else
                {
                    sbStats.AppendLine( $"No Group records need to be updated [{groupUpdatesMS}ms]" );
                }
                stopwatch.Restart();
            }

            stopwatch.Restart();

            var groupsToInsert = groupLookUp.Where( a => a.Value.Id == 0 ).Select( a => a.Value ).ToList();

            rockContext.BulkInsert( groupsToInsert );

            // Get lookups for Group and Person so that we can populate the ParentGroups and GroupMembers
            var qryGroupTypeGroupLookup = new GroupService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey ).Select( a => new
            {
                Group = a,
                GroupForeignId = a.ForeignId.Value,
                GroupTypeId = a.GroupTypeId
            } );

            Dictionary<int, Dictionary<int, Group>> groupTypeGroupLookup = qryGroupTypeGroupLookup.GroupBy( a => a.GroupTypeId ).ToDictionary( k => k.Key, v => v.ToDictionary( k1 => k1.GroupForeignId, v1 => v1.Group ) );

            var personIdLookup = new PersonService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey )
                .Select( a => new { a.Id, ForeignId = a.ForeignId.Value } ).ToDictionary( k => k.ForeignId, v => v.Id );

            // populate GroupMembers
            List<GroupMember> groupMembersToInsert = new List<GroupMember>();
            var groupMemberImports = groupImports.SelectMany( a => a.GroupMemberImports ).ToList();

            foreach ( var groupWithMembers in groupImports.Where( g => groupsToInsert.Select( x => x.ForeignId ).ToList().Contains( g.GroupForeignId ) ) )
            {
                var groupTypeRoleLookup = GroupTypeCache.Get( groupWithMembers.GroupTypeId ).Roles.ToDictionary( k => k.Name, v => v.Id );

                var groupId = groupTypeGroupLookup.GetValueOrNull( groupWithMembers.GroupTypeId )?.GetValueOrNull( groupWithMembers.GroupForeignId )?.Id;

                foreach ( var groupMemberImport in groupWithMembers.GroupMemberImports )
                {
                    var groupRoleId = groupTypeRoleLookup.GetValueOrNull( groupMemberImport.RoleName );
                    var personId = personIdLookup.GetValueOrNull( groupMemberImport.PersonForeignId );
                    if ( groupId.HasValue && groupRoleId.HasValue && personId.HasValue )
                    {
                            var groupMember = new GroupMember();
                            groupMember.GroupId = groupId.Value;
                            groupMember.GroupRoleId = groupRoleId.Value;
                            groupMember.PersonId = personId.Value;
                            groupMember.CreatedDateTime = importedDateTime;
                            groupMember.ModifiedDateTime = importedDateTime;
                            groupMembersToInsert.Add( groupMember );
                    }
                    else
                    {
                        if ( !personId.HasValue )
                        {
                            Debug.WriteLine( $"### Unable to determine PersonId for GroupMember. Associated person may have been deleted or orphaned. Person.PersonForeignId {groupMemberImport.PersonForeignId} ##" );
                        }
                        else
                        {
                            Debug.WriteLine( $"### Unable to determine GroupId or GroupRoleId for GroupMember. GroupType may have been altered since last import. Group.ForeignId {groupWithMembers.GroupForeignId}, Person.PersonForeignId {groupMemberImport.PersonForeignId} ##" );
                        }
                    }
                }
            }

            rockContext.BulkInsert( groupMembersToInsert );

            // populate Schedules from the new groups that we added
            var groupSchedulesToInsert = new List<Schedule>();
            foreach ( var groupWithSchedule in groupsToInsert.Where( g => g.Schedule != null && g.ForeignId != null ) )
            {
                var groupId = groupTypeGroupLookup.GetValueOrNull( groupWithSchedule.GroupTypeId )?.GetValueOrNull( (int)groupWithSchedule.ForeignId )?.Id;
                groupSchedulesToInsert.Add( groupWithSchedule.Schedule );
            }

            rockContext.BulkInsert( groupSchedulesToInsert );

            if ( groupSchedulesToInsert.Any() )
            {
                // manually update Group.ScheduleId since BulkInsert doesn't
                rockContext.Database.ExecuteSqlCommand( string.Format( @"
UPDATE [Group]
SET ScheduleId = [Schedule].[Id]
FROM [Group]
JOIN [Schedule]
ON [Group].[ForeignId] = [Schedule].[ForeignId]
AND [Group].[Name] = [Schedule].[Name]
AND [Group].[ForeignKey] = '{0}'
AND [Schedule].[ForeignKey] = '{0}'
                ", foreignSystemKey ) );
            }

            // Attribute Values
            var attributeValuesToInsert = new List<AttributeValue>();
            foreach ( var groupWithAttributes in groupImports.Where( a => a.AttributeValues.Any() && groupsToInsert.Select( x => x.ForeignId ).ToList().Contains( a.GroupForeignId ) ) )
            {
                var groupId = groupTypeGroupLookup.GetValueOrNull( groupWithAttributes.GroupTypeId )?.GetValueOrNull( groupWithAttributes.GroupForeignId )?.Id;
                if ( groupId.HasValue )
                {
                    foreach ( var attributeValueImport in groupWithAttributes.AttributeValues )
                    {
                        var attributeValue = new AttributeValue();
                        attributeValue.EntityId = groupId;
                        attributeValue.AttributeId = attributeValueImport.AttributeId;
                        attributeValue.Value = attributeValueImport.Value;
                        attributeValue.CreatedDateTime = importedDateTime;
                        attributeValue.ModifiedDateTime = importedDateTime;
                        attributeValuesToInsert.Add( attributeValue );
                    }
                }
            }

            rockContext.BulkInsert( attributeValuesToInsert );

            if ( attributeValuesToInsert.Any() )
            {
                // manually update ValueAsDateTime since the tgrAttributeValue_InsertUpdate trigger won't fire during when using BulkInsert
                rockContext.Database.ExecuteSqlCommand( @"
UPDATE [AttributeValue] SET ValueAsDateTime =
		CASE WHEN
			LEN(value) < 50 and
			ISNULL(value,'') != '' and
			ISNUMERIC([value]) = 0 THEN
				CASE WHEN [value] LIKE '____-__-__T%__:__:%' THEN
					ISNULL( TRY_CAST( TRY_CAST( LEFT([value],19) AS datetimeoffset ) as datetime) , TRY_CAST( value as datetime ))
				ELSE
					TRY_CAST( [value] as datetime )
				END
		END
        where (CASE WHEN
			LEN(value) < 50 and
			ISNULL(value,'') != '' and
			ISNUMERIC([value]) = 0 THEN
				CASE WHEN [value] LIKE '____-__-__T%__:__:%' THEN
					ISNULL( TRY_CAST( TRY_CAST( LEFT([value],19) AS datetimeoffset ) as datetime) , TRY_CAST( value as datetime ))
				ELSE
					TRY_CAST( [value] as datetime )
				END
		END) is not null
        and ValueAsDateTime is null
" );
            }

            // Addresses
            var locationsToInsert = new List<Location>();
            var groupLocationsToInsert = new List<GroupLocation>();
            foreach ( var groupWithAddresses in groupImports.Where( a => a.Addresses.Any() && groupsToInsert.Select( x => x.ForeignId ).ToList().Contains( a.GroupForeignId ) ) )
            {
                var groupId = groupTypeGroupLookup.GetValueOrNull( groupWithAddresses.GroupTypeId )?.GetValueOrNull( groupWithAddresses.GroupForeignId )?.Id;
                if ( groupId.HasValue )
                {
                    // get the distinct addresses for each group in our import
                    var groupAddresses = groupWithAddresses.Addresses.DistinctBy( a => new { a.GroupLocationTypeValueId, a.Street1, a.Street2, a.City, a.County, a.State } ).ToList();

                    foreach ( var address in groupAddresses )
                    {
                        GroupLocation groupLocation = new GroupLocation();
                        groupLocation.GroupLocationTypeValueId = address.GroupLocationTypeValueId;
                        groupLocation.GroupId = groupId.Value;
                        groupLocation.IsMailingLocation = address.IsMailingLocation;
                        groupLocation.IsMappedLocation = address.IsMappedLocation;
                        groupLocation.CreatedDateTime = importedDateTime;
                        groupLocation.ModifiedDateTime = importedDateTime;

                        Location location = new Location();

                        location.Street1 = address.Street1.Left( 100 );
                        location.Street2 = address.Street2.Left( 100 );
                        location.City = address.City.Left( 50 );
                        location.County = address.County.Left( 50 );
                        location.State = address.State.Left( 50 );
                        location.Country = address.Country.Left( 50 );
                        location.PostalCode = address.PostalCode.Left( 50 );
                        location.CreatedDateTime = importedDateTime;
                        location.ModifiedDateTime = importedDateTime;
                        if ( address.Latitude.HasValue && address.Longitude.HasValue )
                        {
                            location.SetLocationPointFromLatLong( address.Latitude.Value, address.Longitude.Value );
                        }

                        // give the Location a Guid, and store a reference to which Location is associated with the GroupLocation record. Then we'll match them up later and do the bulk insert
                        location.Guid = Guid.NewGuid();
                        groupLocation.Location = location;

                        groupLocationsToInsert.Add( groupLocation );
                        locationsToInsert.Add( location );
                    }
                }
            }

            rockContext.BulkInsert( locationsToInsert );

            var locationIdLookup = locationService.Queryable().Select( a => new { a.Id, a.Guid } ).ToList().ToDictionary( k => k.Guid, v => v.Id );
            foreach ( var groupLocation in groupLocationsToInsert )
            {
                groupLocation.LocationId = locationIdLookup[groupLocation.Location.Guid];
            }

            rockContext.BulkInsert( groupLocationsToInsert );

            var groupsUpdated = false;
            var groupImportsWithParentGroup = groupImports.Where( a => a.ParentGroupForeignId.HasValue ).ToList();

            var parentGroupLookup = new GroupService( rockContext ).Queryable().Where( a => a.GroupTypeId != groupTypeIdFamily && a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey ).Select( a => new
            {
                GroupId = a.Id,
                a.ForeignId
            } ).ToDictionary( k => k.ForeignId, v => v.GroupId );

            foreach ( var groupImport in groupImportsWithParentGroup )
            {
                Group group = null;

                if ( groupTypeGroupLookup.ContainsKey( groupImport.GroupTypeId ) )
                {
                    if ( groupTypeGroupLookup[groupImport.GroupTypeId].ContainsKey( groupImport.GroupForeignId ) )
                    {
                        group = groupTypeGroupLookup[groupImport.GroupTypeId][groupImport.GroupForeignId];
                    }
                }

                if ( group != null )
                {
                    int? parentGroupId = parentGroupLookup.GetValueOrNull( groupImport.ParentGroupForeignId.Value );
                    if ( parentGroupId.HasValue && group.ParentGroupId != parentGroupId )
                    {
                        group.ParentGroupId = parentGroupId;
                        groupsUpdated = true;
                    }
                    else
                    {
                        sbStats.AppendLine( $"ERROR: Unable to lookup ParentGroup {groupImport.ParentGroupForeignId} for Group {groupImport.Name}:{groupImport.GroupForeignId} " );
                    }
                }
                else
                {
                    throw new Exception( "Unable to lookup Group with ParentGroup" );
                }
            }

            if ( groupsUpdated )
            {
                rockContext.SaveChanges( true );
            }

            // Update GroupTypes' Allowed Child GroupTypes based on groups that became child groups
            rockContext.Database.ExecuteSqlCommand( @"
INSERT INTO GroupTypeAssociation (
	GroupTypeId
	,ChildGroupTypeId
	)
SELECT DISTINCT pg.GroupTypeId [ParentGroupTypeId]
	,g.GroupTypeId [ChildGroupTypeId]
FROM [Group] g
INNER JOIN [Group] pg ON g.ParentGroupId = pg.id
INNER JOIN [GroupType] pgt ON pg.GroupTypeId = pgt.Id
INNER JOIN [GroupType] cgt ON g.GroupTypeId = cgt.Id
OUTER APPLY (
	SELECT *
	FROM GroupTypeAssociation
	WHERE GroupTypeId = pg.GroupTypeId
		AND ChildGroupTypeId = g.GroupTypeid
	) gta
WHERE gta.GroupTypeId IS NULL" );

            // make sure grouptype caches get updated in case 'allowed group types' changed
            foreach ( var groupTypeId in groupTypeGroupLookup.Keys )
            {
                GroupTypeCache.UpdateCachedEntity( groupTypeId, EntityState.Detached );
            }

            stopwatchTotal.Stop();

            if ( groupsToInsert.Any() || groupMembersToInsert.Any() )
            {
                sbStats.AppendLine( $"Imported {groupsToInsert.Count} Groups and {groupMembersToInsert.Count} Group Members [{stopwatchTotal.ElapsedMilliseconds}ms]" );
            }
            else
            {
                sbStats.AppendLine( $"No Groups were imported [{stopwatchTotal.ElapsedMilliseconds}ms]" );
            }

            var responseText = sbStats.ToString();

            return responseText;
        }

        private void UpdateGroupPropertiesFromGroupImport ( Group group, GroupImport groupImport, string foreignSystemKey, DateTime importedDateTime )
        {
            group.ForeignId = groupImport.GroupForeignId;
            group.ForeignKey = foreignSystemKey;
            group.GroupTypeId = groupImport.GroupTypeId;

            if ( groupImport.Name.Length > 100 )
            {
                group.Name = groupImport.Name.Left( 100 );
                group.Description = groupImport.Name;
            }
            else
            {
                group.Name = groupImport.Name;
                group.Description = groupImport.Description;
            }

            group.Order = groupImport.Order;
            group.CampusId = groupImport.CampusId;
            group.IsActive = groupImport.IsActive;
            group.IsPublic = groupImport.IsPublic;
            group.GroupCapacity = groupImport.Capacity;
        }
        private bool UpdateGroupFromGroupImport( GroupImport groupImport, Group lookupGroup, Dictionary<int, List<AttributeValueCache>> attributeValuesLookup, string foreignSystemKey, DateTime importDateTime )
        {
            using ( var rockContextForGroupUpdate = new RockContext() )
            {
                rockContextForGroupUpdate.Groups.Attach( lookupGroup );
                var group = lookupGroup;

                UpdateGroupPropertiesFromGroupImport( group, groupImport, foreignSystemKey, importDateTime );

                // update Attributes
                var groupAttributesUpdated = false;
                if ( groupImport.AttributeValues.Any() )
                {
                    var attributeValues = attributeValuesLookup.GetValueOrNull( group.Id );

                    foreach ( AttributeValueImport attributeValueImport in groupImport.AttributeValues )
                    {
                        var currentValue = attributeValues?.FirstOrDefault( a => a.AttributeId == attributeValueImport.AttributeId );

                        if ( ( currentValue == null ) || ( currentValue.Value != attributeValueImport.Value ) )
                        {
                            if ( group.Attributes == null )
                            {
                                group.LoadAttributes( rockContextForGroupUpdate );
                            }

                            var attributeCache = AttributeCache.Get( attributeValueImport.AttributeId );
                            if ( group.AttributeValues[attributeCache.Key].Value != attributeValueImport.Value )
                            {
                                group.SetAttributeValue( attributeCache.Key, attributeValueImport.Value );
                                groupAttributesUpdated = true;
                            }
                        }
                    }
                }

                // update Addresses
                var addressesUpdated = false;
                if ( groupImport.Addresses.Any() )
                {
                    var groupLocationService = new GroupLocationService( rockContextForGroupUpdate );
                    var groupLocations = groupLocationService.Queryable().Where( a => a.GroupId == group.Id ).Include( a => a.Location ).AsNoTracking().ToList();
                    foreach ( var groupAddressImport in groupImport.Addresses )
                    {
                        bool addressAlreadyExsistsExactMatch = groupLocations.Where( a =>
                            a.GroupLocationTypeValueId == groupAddressImport.GroupLocationTypeValueId
                            && (
                                 a.Location.Street1 == groupAddressImport.Street1
                                    && a.Location.Street2 == groupAddressImport.Street2
                                    && a.Location.City == groupAddressImport.City
                                    && a.Location.County == groupAddressImport.County
                                    && a.Location.State == groupAddressImport.State
                                    && a.Location.Country == groupAddressImport.Country
                                    && a.Location.PostalCode == groupAddressImport.PostalCode
                                 ) ).Any();

                        if ( !addressAlreadyExsistsExactMatch )
                        {
                            var locationService = new LocationService( rockContextForGroupUpdate );

                            Location location = locationService.Get( groupAddressImport.Street1, groupAddressImport.Street2, groupAddressImport.City, groupAddressImport.State, groupAddressImport.PostalCode, groupAddressImport.Country, false );

                            if ( !groupLocations.Where( a => a.GroupLocationTypeValueId == groupAddressImport.GroupLocationTypeValueId && a.LocationId == location.Id ).Any() )
                            {
                                var groupLocation = new GroupLocation();
                                groupLocation.GroupId = group.Id;
                                groupLocation.GroupLocationTypeValueId = groupAddressImport.GroupLocationTypeValueId;
                                groupLocation.IsMailingLocation = groupAddressImport.IsMailingLocation;
                                groupLocation.IsMappedLocation = groupAddressImport.IsMappedLocation;

                                if ( location.GeoPoint == null && groupAddressImport.Latitude.HasValue && groupAddressImport.Longitude.HasValue )
                                {
                                    location.SetLocationPointFromLatLong( groupAddressImport.Latitude.Value, groupAddressImport.Longitude.Value );
                                }

                                groupLocation.LocationId = location.Id;
                                groupLocationService.Add( groupLocation );

                                addressesUpdated = true;
                            }
                        }

                        // NOTE: Still need too add logic for removing addresses not in this groupImport
                    }
                }

                // update schedule
                bool scheduleUpdated = false;
                DayOfWeek meetingDay;
                if ( !string.IsNullOrWhiteSpace( groupImport.MeetingDay ) && Enum.TryParse( groupImport.MeetingDay, out meetingDay ) )
                {
                    TimeSpan meetingTime;
                    TimeSpan.TryParse( groupImport.MeetingTime, out meetingTime );
                    if( group.Schedule.WeeklyDayOfWeek != meetingDay || group.Schedule.WeeklyTimeOfDay != meetingTime )
                    {
                        group.Schedule = new Schedule()
                        {
                            Name = group.Name,
                            IsActive = group.IsActive,
                            WeeklyDayOfWeek = meetingDay,
                            WeeklyTimeOfDay = meetingTime,
                            ForeignId = groupImport.GroupForeignId,
                            ForeignKey = foreignSystemKey,
                            CreatedDateTime = importDateTime,
                            ModifiedDateTime = importDateTime
                        };
                        scheduleUpdated = true;
                    }
                }

                if ( groupAttributesUpdated )
                {
                    group.SaveAttributeValues();
                }

                //Update Members

               var groupMemberService = new GroupMemberService( rockContextForGroupUpdate );
               var personIdLookup = new PersonService( rockContextForGroupUpdate ).Queryable().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey )
                    .Select( a => new { a.Id, ForeignId = a.ForeignId.Value } ).ToDictionary( k => k.ForeignId, v => v.Id );

                var groupMemberList = group.Members.Where( x => x.Person.ForeignKey == foreignSystemKey ).Select( a => new
                {
                    a.Id,
                    a.Person.ForeignId
                } ).ToList();

                // populate/update GroupMembers
                foreach ( var groupMemberImport in groupImport.GroupMemberImports )
                {

                    var groupTypeRoleLookup = GroupTypeCache.Get( groupImport.GroupTypeId ).Roles.ToDictionary( k => k.Name, v => v.Id );
                    var groupRoleId = groupTypeRoleLookup.GetValueOrNull( groupMemberImport.RoleName );
                    var personId = personIdLookup.GetValueOrNull( groupMemberImport.PersonForeignId );
                    GroupMember groupMember = group.Members.Where( m => m.Person.ForeignId == groupMemberImport.PersonForeignId && m.Person.ForeignKey == foreignSystemKey ).FirstOrDefault();

                    if ( groupMember == null )
                    {
                        groupMember = new GroupMember();
                        groupMember.GroupId = group.Id;
                        groupMember.GroupRoleId = groupRoleId.Value;
                        groupMember.PersonId = personId.Value;
                        groupMember.CreatedDateTime = importDateTime;
                        groupMember.ModifiedDateTime = importDateTime;
                        groupMemberService.Add( groupMember );
                    }
                    else
                    {
                        groupMember.GroupRoleId = groupRoleId.Value;
                        groupMember.ModifiedDateTime = importDateTime;
                    }
                }

                foreach ( var member in groupMemberList.Where(gm => !groupImport.GroupMemberImports.Any( x => x.PersonForeignId == gm.ForeignId ) ) )
                {
                    var groupMember = groupMemberService.Get( member.Id );
                    if ( groupMember != null )
                    {
                        groupMemberService.Delete( groupMember );
                    }
                }
               
                var updatedRecords = rockContextForGroupUpdate.SaveChanges( true );

                return scheduleUpdated || addressesUpdated || groupAttributesUpdated || updatedRecords > 0;
            }
        }

        #endregion GroupImport

        #region LocationImport

        /// <summary>
        /// Bulks the location import.
        /// </summary>
        /// <param name="locationImports">The location imports.</param>
        /// <returns></returns>
        public string BulkLocationImport( List<LocationImport> locationImports, string foreignSystemKey )
        {
            Stopwatch stopwatchTotal = Stopwatch.StartNew();

            RockContext rockContext = new RockContext();

            var qryLocationsWithForeignIds = new LocationService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey );

            var locationsAlreadyExistForeignIdHash = new HashSet<int>( qryLocationsWithForeignIds.Select( a => a.ForeignId.Value ).ToList() );

            List<Location> locationsToInsert = new List<Location>();
            var newLocationImports = locationImports.Where( a => !locationsAlreadyExistForeignIdHash.Contains( a.LocationForeignId ) ).ToList();

            var importDateTime = RockDateTime.Now;

            foreach ( var locationImport in newLocationImports )
            {
                var location = new Location();
                location.ForeignId = locationImport.LocationForeignId;
                location.ForeignKey = foreignSystemKey;
                location.LocationTypeValueId = locationImport.LocationTypeValueId;

                location.Street1 = locationImport.Street1.Left( 100 );
                location.Street2 = locationImport.Street2.Left( 100 );
                location.City = locationImport.City.Left( 50 );
                location.County = locationImport.County.Left( 50 );
                location.State = locationImport.State.Left( 50 );
                location.Country = locationImport.Country.Left( 50 );
                location.PostalCode = locationImport.PostalCode.Left( 50 );

                location.Name = locationImport.Name.Left( 100 );
                location.IsActive = locationImport.IsActive;
                location.CreatedDateTime = importDateTime;
                location.ModifiedDateTime = importDateTime;
                locationsToInsert.Add( location );
            }

            rockContext.BulkInsert( locationsToInsert );

            // Get the Location records for the locations that we imported so that we can populate the ParentLocations
            var locationLookup = qryLocationsWithForeignIds.ToList().ToDictionary( k => k.ForeignId.Value, v => v );
            var locationsUpdated = false;
            foreach ( var locationImport in newLocationImports.Where( a => a.ParentLocationForeignId.HasValue ) )
            {
                var location = locationLookup.GetValueOrNull( locationImport.LocationForeignId );
                if ( location != null )
                {
                    var parentLocation = locationLookup.GetValueOrNull( locationImport.ParentLocationForeignId.Value );
                    if ( parentLocation != null && location.ParentLocationId != parentLocation.Id )
                    {
                        location.ParentLocationId = parentLocation.Id;
                        locationsUpdated = true;
                    }
                }
            }

            if ( locationsUpdated )
            {
                rockContext.SaveChanges();
            }

            stopwatchTotal.Stop();
            return GetResponseMessage( newLocationImports.Count, "Locations", stopwatchTotal.ElapsedMilliseconds );
        }

        #endregion LocationImport

        #region PersonImport

        private string _defaultPhoneCountryCode = null;
        private int _recordTypePersonId;

        /// <summary>
        /// Bulks the import.
        /// </summary>
        /// <param name="personImports">The person imports.</param>
        /// <returns></returns>
        public string BulkPersonImport( List<PersonImport> personImports, string foreignSystemKey )
        {
            var initiatedWithWebRequest = HttpContext.Current?.Request != null;
            Stopwatch stopwatchTotal = Stopwatch.StartNew();
            Stopwatch stopwatch = Stopwatch.StartNew();
            RockContext rockContext = new RockContext();
            var qryAllPersons = new PersonService( rockContext ).Queryable( true, true );
            var groupService = new GroupService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );
            var locationService = new LocationService( rockContext );

            var familyGroupType = GroupTypeCache.GetFamilyGroupType();
            int familyGroupTypeId = familyGroupType.Id;
            int familyChildRoleId = familyGroupType.Roles.First( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ).Id;
            _recordTypePersonId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;

            StringBuilder sbStats = new StringBuilder();

            Dictionary<int, Group> familiesLookup = groupService.Queryable().AsNoTracking().Where( a => a.GroupTypeId == familyGroupTypeId && a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey )
                .ToList().ToDictionary( k => k.ForeignId.Value, v => v );

            Dictionary<int, Person> personLookup = qryAllPersons.Include( a => a.PhoneNumbers ).AsNoTracking().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey )
                .ToList().ToDictionary( k => k.ForeignId.Value, v => v );

            _defaultPhoneCountryCode = PhoneNumber.DefaultCountryCode();

            var importDateTime = RockDateTime.Now;

            int nextNewFamilyForeignId = familiesLookup.Any() ? familiesLookup.Max( a => a.Key ) : 0;
            if ( personImports.Any() )
            {
                nextNewFamilyForeignId = Math.Max( nextNewFamilyForeignId, personImports.Where( a => a.FamilyForeignId.HasValue ).Max( a => a.FamilyForeignId.Value ) );
            }

            // Just In Case, ensure Entity Attributes are flushed (they might be stale if they were added directly via SQL)
            AttributeCache.RemoveEntityAttributes();

            var entityTypeIdPerson = EntityTypeCache.Get<Person>().Id;
            Dictionary<int, List<AttributeValueCache>> attributeValuesLookup = new AttributeValueService( rockContext ).Queryable().Where( a => a.Attribute.EntityTypeId == entityTypeIdPerson && a.EntityId.HasValue )
                .Select( a => new
                {
                    PersonId = a.EntityId.Value,
                    a.AttributeId,
                    a.Value
                } )
                .GroupBy( a => a.PersonId )
                .ToDictionary(
                    k => k.Key,
                    v => v.Select( x => new AttributeValueCache { AttributeId = x.AttributeId, EntityId = x.PersonId, Value = x.Value } ).ToList() );

            int personUpdatesCount = 0;
            long personUpdatesMS = 0;
            int progress = 0;
            int total = personImports.Count();

            foreach ( var personImport in personImports )
            {
                progress++;
                if ( progress % 100 == 0 && personUpdatesMS > 0 )
                {
                    if ( initiatedWithWebRequest && HttpContext.Current?.Response?.IsClientConnected != true )
                    {
                        // if this was called from a WebRequest (versus a job or utility), quit if the client has disconnected
                        return "Client Disconnected";
                    }

                    OnProgress?.Invoke( $"Bulk Importing Person {progress} of {total}..." );
                }

                Group family = null;

                if ( !personImport.FamilyForeignId.HasValue )
                {
                    // If personImport.FamilyForeignId is null, that means we need to create a new family
                    personImport.FamilyForeignId = ++nextNewFamilyForeignId;
                }

                if ( familiesLookup.ContainsKey( personImport.FamilyForeignId.Value ) )
                {
                    family = familiesLookup[personImport.FamilyForeignId.Value];
                }

                if ( family == null )
                {
                    family = new Group();
                    family.GroupTypeId = familyGroupTypeId;
                    family.Name = string.IsNullOrEmpty( personImport.FamilyName ) ? personImport.LastName : personImport.FamilyName;

                    if ( string.IsNullOrWhiteSpace( family.Name ) )
                    {
                        family.Name = "Family";
                    }

                    family.CampusId = personImport.CampusId;

                    family.ForeignId = personImport.FamilyForeignId;
                    family.ForeignKey = foreignSystemKey;
                    family.CreatedDateTime = personImport.CreatedDateTime ?? importDateTime;
                    family.ModifiedDateTime = personImport.ModifiedDateTime ?? importDateTime;
                    familiesLookup.Add( personImport.FamilyForeignId.Value, family );
                }

                Person person = null;
                if ( personLookup.ContainsKey( personImport.PersonForeignId ) )
                {
                    person = personLookup[personImport.PersonForeignId];
                }

                if ( person == null )
                {
                    person = new Person();
                    UpdatePersonPropertiesFromPersonImport( personImport, person, foreignSystemKey );
                    personLookup.Add( personImport.PersonForeignId, person );
                }
                else
                {
                    if ( this.ImportUpdateOption == ImportUpdateType.AlwaysUpdate )
                    {
                        Stopwatch stopwatchPersonUpdates = Stopwatch.StartNew();
                        bool wasChanged = UpdatePersonFromPersonImport( person, personImport, attributeValuesLookup, familiesLookup, foreignSystemKey, importDateTime );
                        stopwatchPersonUpdates.Stop();
                        personUpdatesMS += stopwatchPersonUpdates.ElapsedMilliseconds;
                        if ( wasChanged )
                        {
                            personUpdatesCount++;
                        }
                    }
                }
            }

            if ( personUpdatesMS > 0 || personUpdatesCount > 0 )
            {
                stopwatch.Stop();
                sbStats.AppendLine( $"Check for Person Updates [{stopwatch.ElapsedMilliseconds - personUpdatesMS}ms]" );
                if ( personUpdatesCount > 0 )
                {
                    sbStats.AppendLine( $"Updated {personUpdatesCount} Person records [{personUpdatesMS}ms]" );
                }
                else
                {
                    sbStats.AppendLine( $"No Person records need to be updated [{personUpdatesMS}ms]" );
                }
                stopwatch.Restart();
            }

            double buildImportListsMS = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            List<int> insertedPersonForeignIds = new List<int>();

            // insert all the [Group] records
            var familiesToInsert = familiesLookup.Where( a => a.Value.Id == 0 ).Select( a => a.Value ).ToList();

            // insert all the [Person] records.
            // NOTE: we are only inserting the [Person] record, not the PersonAlias or GroupMember records yet
            var personsToInsert = personLookup.Where( a => a.Value.Id == 0 ).Select( a => a.Value ).ToList();

            rockContext.BulkInsert( familiesToInsert );

            // lookup GroupId from Group.ForeignId
            var familyIdLookup = groupService.Queryable().AsNoTracking().Where( a => a.GroupTypeId == familyGroupTypeId && a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey )
                .ToList().ToDictionary( k => k.ForeignId.Value, v => v.Id );

            var personToInsertLookup = personsToInsert.ToDictionary( k => k.ForeignId.Value, v => v );

            // now that we have GroupId for each family, set the GivingGroupId for personImport's that don't give individually
            foreach ( var personImport in personImports )
            {
                if ( !personImport.GivingIndividually.HasValue )
                {
                    // If GivingIndividually is NULL, based it on GroupRole (Adults give with Family, Kids give as individuals)
                    personImport.GivingIndividually = personImport.GroupRoleId == familyChildRoleId;
                }

                if ( !personImport.GivingIndividually.Value && personImport.FamilyForeignId.HasValue )
                {
                    var personToInsert = personToInsertLookup.GetValueOrNull( personImport.PersonForeignId );
                    if ( personToInsert != null )
                    {
                        personToInsert.GivingGroupId = familyIdLookup[personImport.FamilyForeignId.Value];
                    }
                }
            }

            rockContext.BulkInsert( personsToInsert );

            insertedPersonForeignIds = personsToInsert.Select( a => a.ForeignId.Value ).ToList();

            // Make sure everybody has a PersonAlias
            PersonAliasService personAliasService = new PersonAliasService( rockContext );
            var personAliasServiceQry = personAliasService.Queryable();
            List<PersonAlias> personAliasesToInsert = qryAllPersons.Where( p => p.ForeignId.HasValue && p.ForeignKey == foreignSystemKey && !p.Aliases.Any() && !personAliasServiceQry.Any( pa => pa.AliasPersonId == p.Id ) )
                .Select( x => new { x.Id, x.Guid, x.ForeignId } )
                .ToList()
                .Select( person => new PersonAlias { AliasPersonId = person.Id, AliasPersonGuid = person.Guid, PersonId = person.Id, ForeignId = person.ForeignId, ForeignKey = foreignSystemKey } ).ToList();

            rockContext.BulkInsert( personAliasesToInsert );

            var familyGroupMembersQry = new GroupMemberService( rockContext ).Queryable( true ).Where( a => a.Group.GroupTypeId == familyGroupTypeId );

            // get the person Ids along with the PersonImport and GroupMember record
            var personsIdsForPersonImport = from p in qryAllPersons.AsNoTracking().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey )
                                                .Select( a => new { a.Id, a.ForeignId } ).ToList()
                                            join pi in personImports on p.ForeignId equals pi.PersonForeignId
                                            join f in groupService.Queryable().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey && a.GroupTypeId == familyGroupTypeId )
                                                .Select( a => new { a.Id, a.ForeignId } ).ToList() on pi.FamilyForeignId equals f.ForeignId
                                            join gm in familyGroupMembersQry.Select( a => new { a.Id, a.PersonId } ) on p.Id equals gm.PersonId into gmj
                                            from gm in gmj.DefaultIfEmpty()
                                            select new
                                            {
                                                PersonId = p.Id,
                                                PersonImport = pi,
                                                FamilyId = f.Id,
                                                HasGroupMemberRecord = gm != null
                                            };

            // narrow it down to just person records that we inserted
            personsIdsForPersonImport = personsIdsForPersonImport.Where( a => insertedPersonForeignIds.Contains( a.PersonImport.PersonForeignId ) );

            // Make the GroupMember records for all the imported person (unless they are already have a groupmember record for the family)
            var groupMemberRecordsToInsertQry = from ppi in personsIdsForPersonImport
                                                where !ppi.HasGroupMemberRecord
                                                select new GroupMember
                                                {
                                                    PersonId = ppi.PersonId,
                                                    GroupRoleId = ppi.PersonImport.GroupRoleId,
                                                    GroupId = ppi.FamilyId,
                                                    GroupMemberStatus = GroupMemberStatus.Active,
                                                    CreatedDateTime = ppi.PersonImport.CreatedDateTime ?? importDateTime,
                                                    ModifiedDateTime = ppi.PersonImport.ModifiedDateTime ?? importDateTime,
                                                };

            var groupMemberRecordsToInsertList = groupMemberRecordsToInsertQry.ToList();

            rockContext.BulkInsert( groupMemberRecordsToInsertList );

            List<Location> locationsToInsert = new List<Location>();
            List<GroupLocation> groupLocationsToInsert = new List<GroupLocation>();

            var locationCreatedDateTimeStart = RockDateTime.Now;

            foreach ( var familyRecord in personsIdsForPersonImport.GroupBy( a => a.FamilyId ) )
            {
                // get the distinct addresses for each family in our import
                var familyAddresses = familyRecord.Where( a => a.PersonImport?.Addresses != null ).SelectMany( a => a.PersonImport.Addresses ).DistinctBy( a => new { a.GroupLocationTypeValueId, a.Street1, a.Street2, a.City, a.County, a.State } ).ToList();

                foreach ( var address in familyAddresses )
                {
                    GroupLocation groupLocation = new GroupLocation();
                    groupLocation.GroupLocationTypeValueId = address.GroupLocationTypeValueId;
                    groupLocation.GroupId = familyRecord.Key;
                    groupLocation.IsMailingLocation = address.IsMailingLocation;
                    groupLocation.IsMappedLocation = address.IsMappedLocation;
                    groupLocation.CreatedDateTime = locationCreatedDateTimeStart;
                    groupLocation.ModifiedDateTime = locationCreatedDateTimeStart;

                    Location location = new Location();

                    location.Street1 = address.Street1.Left( 100 );
                    location.Street2 = address.Street2.Left( 100 );
                    location.City = address.City.Left( 50 );
                    location.County = address.County.Left( 50 );
                    location.State = address.State.Left( 50 );
                    location.Country = address.Country.Left( 50 );
                    location.PostalCode = address.PostalCode.Left( 50 );
                    location.CreatedDateTime = locationCreatedDateTimeStart;
                    location.ModifiedDateTime = locationCreatedDateTimeStart;
                    if ( address.Latitude.HasValue && address.Longitude.HasValue )
                    {
                        location.SetLocationPointFromLatLong( address.Latitude.Value, address.Longitude.Value );
                    }

                    // give the Location a Guid, and store a reference to which Location is associated with the GroupLocation record. Then we'll match them up later and do the bulk insert
                    location.Guid = Guid.NewGuid();
                    groupLocation.Location = location;

                    groupLocationsToInsert.Add( groupLocation );
                    locationsToInsert.Add( location );
                }
            }

            rockContext.BulkInsert( locationsToInsert );

            var locationIdLookup = locationService.Queryable().Select( a => new { a.Id, a.Guid } ).ToList().ToDictionary( k => k.Guid, v => v.Id );
            foreach ( var groupLocation in groupLocationsToInsert )
            {
                groupLocation.LocationId = locationIdLookup[groupLocation.Location.Guid];
            }

            rockContext.BulkInsert( groupLocationsToInsert );

            // PhoneNumbers
            List<PhoneNumber> phoneNumbersToInsert = new List<PhoneNumber>();

            foreach ( var personsIds in personsIdsForPersonImport )
            {
                foreach ( var phoneNumberImport in personsIds.PersonImport.PhoneNumbers )
                {
                    var phoneNumberToInsert = new PhoneNumber();
                    phoneNumberToInsert.PersonId = personsIds.PersonId;
                    UpdatePhoneNumberFromPhoneNumberImport( phoneNumberImport, phoneNumberToInsert, importDateTime );

                    phoneNumbersToInsert.Add( phoneNumberToInsert );
                }
            }

            rockContext.BulkInsert( phoneNumbersToInsert );

            // Attribute Values
            var attributeValuesToInsert = new List<AttributeValue>();
            foreach ( var personsIds in personsIdsForPersonImport )
            {
                foreach ( var attributeValueImport in personsIds.PersonImport.AttributeValues )
                {
                    var attributeValue = new AttributeValue();

                    attributeValue.EntityId = personsIds.PersonId;
                    attributeValue.AttributeId = attributeValueImport.AttributeId;
                    attributeValue.Value = attributeValueImport.Value;
                    attributeValue.CreatedDateTime = personsIds.PersonImport.CreatedDateTime ?? importDateTime;
                    attributeValue.ModifiedDateTime = personsIds.PersonImport.ModifiedDateTime ?? importDateTime;
                    attributeValuesToInsert.Add( attributeValue );
                }
            }

            rockContext.BulkInsert( attributeValuesToInsert );

            if ( attributeValuesToInsert.Any() )
            {
                // manually update ValueAsDateTime since the tgrAttributeValue_InsertUpdate trigger won't fire during when using BulkInsert
                var rowsUpdated = rockContext.Database.ExecuteSqlCommand( @"
UPDATE [AttributeValue] SET ValueAsDateTime =
		CASE WHEN
			LEN(value) < 50 and
			ISNULL(value,'') != '' and
			ISNUMERIC([value]) = 0 THEN
				CASE WHEN [value] LIKE '____-__-__T%__:__:%' THEN
					ISNULL( TRY_CAST( TRY_CAST( LEFT([value],19) AS datetimeoffset ) as datetime) , TRY_CAST( value as datetime ))
				ELSE
					TRY_CAST( [value] as datetime )
				END
		END
        where (CASE WHEN
			LEN(value) < 50 and
			ISNULL(value,'') != '' and
			ISNUMERIC([value]) = 0 THEN
				CASE WHEN [value] LIKE '____-__-__T%__:__:%' THEN
					ISNULL( TRY_CAST( TRY_CAST( LEFT([value],19) AS datetimeoffset ) as datetime) , TRY_CAST( value as datetime ))
				ELSE
					TRY_CAST( [value] as datetime )
				END
		END) is not null
        and ValueAsDateTime is null
" );

                Debug.WriteLine( "ValueAsDateTime RowsUpdated: " + rowsUpdated.ToString() );
            }

            // since we bypassed Rock SaveChanges when Inserting Person records, sweep thru and ensure the AgeClassification, PrimaryFamily, and GivingLeaderId is set
            PersonService.UpdatePersonAgeClassificationAll( rockContext );
            PersonService.UpdatePrimaryFamilyAll( rockContext );
            PersonService.UpdateGivingLeaderIdAll( rockContext );

            stopwatchTotal.Stop();
            if ( personsToInsert.Any() || groupMemberRecordsToInsertList.Any() || familiesToInsert.Any() )
            {
                sbStats.AppendLine( $"Imported {personsToInsert.Count} People and {familiesToInsert.Count} Families [{stopwatchTotal.ElapsedMilliseconds}ms] and {groupMemberRecordsToInsertList.Count} family members" );
            }
            else
            {
                sbStats.AppendLine( $"No People were imported [{stopwatchTotal.ElapsedMilliseconds}ms]" );
            }

            var responseText = sbStats.ToString();

            return responseText;
        }

        /// <summary>
        /// Updates the person properties from person import.
        /// </summary>
        /// <param name="personImport">The person import.</param>
        /// <param name="person">The person.</param>
        /// <param name="foreignSystemKey">The foreign system key.</param>
        private void UpdatePersonPropertiesFromPersonImport( PersonImport personImport, Person person, string foreignSystemKey )
        {
            person.RecordTypeValueId = personImport.RecordTypeValueId ?? _recordTypePersonId;
            person.RecordStatusValueId = personImport.RecordStatusValueId;
            person.RecordStatusLastModifiedDateTime = personImport.RecordStatusLastModifiedDateTime;
            person.RecordStatusReasonValueId = personImport.RecordStatusReasonValueId;
            person.ConnectionStatusValueId = personImport.ConnectionStatusValueId;
            person.ReviewReasonValueId = personImport.ReviewReasonValueId;
            person.IsDeceased = personImport.IsDeceased;
            person.TitleValueId = personImport.TitleValueId;
            person.FirstName = personImport.FirstName.FixCase().Left( 50 );
            person.NickName = personImport.NickName.FixCase().Left( 50 );

            if ( string.IsNullOrWhiteSpace( person.NickName ) )
            {
                person.NickName = person.FirstName.Left( 50 );
            }

            if ( string.IsNullOrWhiteSpace( person.FirstName ) )
            {
                person.FirstName = person.NickName.Left( 50 );
            }

            person.MiddleName = personImport.MiddleName.FixCase().Left( 50 );
            person.LastName = personImport.LastName.FixCase().Left( 50 );
            person.SuffixValueId = personImport.SuffixValueId;
            person.BirthDay = personImport.BirthDay;
            person.BirthMonth = personImport.BirthMonth;
            person.BirthYear = personImport.BirthYear;
            person.Gender = (Gender)personImport.Gender;
            person.MaritalStatusValueId = personImport.MaritalStatusValueId;
            person.AnniversaryDate = personImport.AnniversaryDate;
            person.GraduationYear = personImport.GraduationYear;
            person.Email = personImport.Email.Left( 75 );

            if ( !person.Email.IsValidEmail() )
            {
                person.Email = null;
            }

            person.IsEmailActive = personImport.IsEmailActive;
            person.EmailNote = personImport.EmailNote.Left( 250 );
            person.EmailPreference = (EmailPreference)personImport.EmailPreference;
            person.InactiveReasonNote = personImport.InactiveReasonNote.Left( 1000 );
            person.CreatedDateTime = personImport.CreatedDateTime;
            person.ModifiedDateTime = personImport.ModifiedDateTime;
            person.SystemNote = personImport.Note;
            person.ForeignId = personImport.PersonForeignId;
            person.ForeignKey = foreignSystemKey;
        }

        /// <summary>
        /// Updates the phone number from phone number import.
        /// </summary>
        /// <param name="phoneNumberImport">The phone number import.</param>
        /// <param name="phoneNumberToInsert">The phone number to insert.</param>
        /// <param name="importDateTime">The import date time.</param>
        private void UpdatePhoneNumberFromPhoneNumberImport( PhoneNumberImport phoneNumberImport, PhoneNumber phoneNumberToInsert, DateTime importDateTime )
        {
            phoneNumberToInsert.NumberTypeValueId = phoneNumberImport.NumberTypeValueId;
            phoneNumberToInsert.CountryCode = _defaultPhoneCountryCode;
            phoneNumberToInsert.Number = PhoneNumber.CleanNumber( phoneNumberImport.Number );
            phoneNumberToInsert.NumberFormatted = PhoneNumber.FormattedNumber( phoneNumberToInsert.CountryCode, phoneNumberToInsert.Number );
            phoneNumberToInsert.Extension = phoneNumberImport.Extension;
            phoneNumberToInsert.IsMessagingEnabled = phoneNumberImport.IsMessagingEnabled;
            phoneNumberToInsert.IsUnlisted = phoneNumberImport.IsUnlisted;
            phoneNumberToInsert.CreatedDateTime = importDateTime;
            phoneNumberToInsert.ModifiedDateTime = importDateTime;
        }

        /// <summary>
        /// Updates the person from person import and returns whether there were any changes to the person record
        /// </summary>
        /// <param name="lookupPerson">The lookup person.</param>
        /// <param name="personImport">The person import.</param>
        /// <param name="attributeValuesLookup">The attribute values lookup.</param>
        /// <param name="familiesLookup">The families lookup.</param>
        /// <param name="foreignSystemKey">The foreign system key.</param>
        /// <param name="importDateTime">The import date time.</param>
        /// <returns></returns>
        private bool UpdatePersonFromPersonImport( Person lookupPerson, PersonImport personImport, Dictionary<int, List<AttributeValueCache>> attributeValuesLookup, Dictionary<int, Group> familiesLookup, string foreignSystemKey, DateTime importDateTime )
        {
            using ( var rockContextForPersonUpdate = new RockContext() )
            {
                rockContextForPersonUpdate.People.Attach( lookupPerson );
                var person = lookupPerson;

                // Add/Update PhoneNumbers
                UpdatePersonPropertiesFromPersonImport( personImport, person, foreignSystemKey );
                var phoneNumberService = new PhoneNumberService( rockContextForPersonUpdate );
                var personPhoneNumberList = person.PhoneNumbers.Select( a => new
                {
                    a.Id,
                    a.Number
                } ).ToList();

                foreach ( var phoneNumberImport in personImport.PhoneNumbers )
                {
                    var hasPhoneNumber = personPhoneNumberList.Any( a => a.Number == PhoneNumber.CleanNumber( phoneNumberImport.Number ) );
                    if ( !hasPhoneNumber )
                    {
                        var personPhoneNumber = new PhoneNumber();
                        personPhoneNumber.PersonId = person.Id;
                        UpdatePhoneNumberFromPhoneNumberImport( phoneNumberImport, personPhoneNumber, importDateTime );
                        phoneNumberService.Add( personPhoneNumber );
                    }
                }

                // Remove any phonenumbers that are no longer in the PersonImport.PhoneNumbers list
                foreach ( var phone in personPhoneNumberList.Where( a => !personImport.PhoneNumbers.Any( x => PhoneNumber.CleanNumber( x.Number ) == a.Number ) ) )
                {
                    var personPhoneNumber = phoneNumberService.Get( phone.Id );
                    if ( personPhoneNumber != null )
                    {
                        phoneNumberService.Delete( personPhoneNumber );
                    }
                }

                var personAttributesUpdated = false;
                if ( personImport.AttributeValues.Any() )
                {
                    var attributeValues = attributeValuesLookup.GetValueOrNull( person.Id );

                    foreach ( AttributeValueImport attributeValueImport in personImport.AttributeValues )
                    {
                        var currentValue = attributeValues?.FirstOrDefault( a => a.AttributeId == attributeValueImport.AttributeId );

                        if ( ( currentValue == null ) || ( currentValue.Value != attributeValueImport.Value ) )
                        {
                            if ( person.Attributes == null )
                            {
                                person.LoadAttributes( rockContextForPersonUpdate );
                            }

                            var attributeCache = AttributeCache.Get( attributeValueImport.AttributeId );
                            if ( person.AttributeValues[attributeCache.Key].Value != attributeValueImport.Value )
                            {
                                person.SetAttributeValue( attributeCache.Key, attributeValueImport.Value );
                                personAttributesUpdated = true;
                            }
                        }
                    }
                }

                // update Addresses
                var addressesUpdated = false;
                if ( personImport.Addresses.Any() )
                {
                    var primaryFamily = familiesLookup.GetValueOrNull( personImport.FamilyForeignId ?? 0 );

                    if ( primaryFamily != null )
                    {
                        var groupLocationService = new GroupLocationService( rockContextForPersonUpdate );
                        var primaryFamilyGroupLocations = groupLocationService.Queryable().Where( a => a.GroupId == primaryFamily.Id ).Include( a => a.Location ).AsNoTracking().ToList();
                        foreach ( var personAddressImport in personImport.Addresses )
                        {
                            bool addressAlreadyExistsExactMatch = primaryFamilyGroupLocations.Where( a =>
                                 a.GroupLocationTypeValueId == personAddressImport.GroupLocationTypeValueId
                                 && (
                                    a.Location.Street1 == personAddressImport.Street1
                                    && a.Location.Street2 == personAddressImport.Street2
                                    && a.Location.City == personAddressImport.City
                                    && a.Location.County == personAddressImport.County
                                    && a.Location.State == personAddressImport.State
                                    && a.Location.Country == personAddressImport.Country
                                    && a.Location.PostalCode == personAddressImport.PostalCode
                                 ) ).Any();

                            if ( !addressAlreadyExistsExactMatch )
                            {
                                var locationService = new LocationService( rockContextForPersonUpdate );

                                Location location = locationService.Get( personAddressImport.Street1, personAddressImport.Street2, personAddressImport.City, personAddressImport.State, personAddressImport.PostalCode, personAddressImport.Country, false );

                                if ( !primaryFamilyGroupLocations.Where( a => a.GroupLocationTypeValueId == personAddressImport.GroupLocationTypeValueId && a.LocationId == location.Id ).Any() )
                                {
                                    var groupLocation = new GroupLocation();
                                    groupLocation.GroupId = primaryFamily.Id;
                                    groupLocation.GroupLocationTypeValueId = personAddressImport.GroupLocationTypeValueId;
                                    groupLocation.IsMailingLocation = personAddressImport.IsMailingLocation;
                                    groupLocation.IsMappedLocation = personAddressImport.IsMappedLocation;

                                    if ( location.GeoPoint == null && personAddressImport.Latitude.HasValue && personAddressImport.Longitude.HasValue )
                                    {
                                        location.SetLocationPointFromLatLong( personAddressImport.Latitude.Value, personAddressImport.Longitude.Value );
                                    }

                                    groupLocation.LocationId = location.Id;
                                    groupLocationService.Add( groupLocation );

                                    addressesUpdated = true;
                                }
                            }
                        }

                        // NOTE: Don't remove addresses that are part of family, but not included in the personImport.  It might be from another Person that is the same family which hasn't been included
                    }
                }

                if ( personAttributesUpdated )
                {
                    person.SaveAttributeValues();
                }

                var updatedRecords = rockContextForPersonUpdate.SaveChanges( true );

                return addressesUpdated || personAttributesUpdated || updatedRecords > 0;
            }
        }

        #endregion PersonImport

        #region BusinessImport

        private int _recordTypeBusinessId;

        /// <summary>
        /// Bulks the import.
        /// </summary>
        /// <param name="businessImports">The business imports.</param>
        /// <returns></returns>
        public string BulkBusinessImport( List<PersonImport> businessImports, string foreignSystemKey )
        {
            var initiatedWithWebRequest = HttpContext.Current?.Request != null;
            Stopwatch stopwatchTotal = Stopwatch.StartNew();
            Stopwatch stopwatch = Stopwatch.StartNew();
            RockContext rockContext = new RockContext();
            var qryAllPersons = new PersonService( rockContext ).Queryable( true, true );
            var groupService = new GroupService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );
            var locationService = new LocationService( rockContext );

            _recordTypeBusinessId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;


            var familyGroupType = GroupTypeCache.GetFamilyGroupType();
            int familyGroupTypeId = familyGroupType.Id;

            StringBuilder sbStats = new StringBuilder();

            Dictionary<int, Group> familiesLookup = groupService.Queryable().AsNoTracking().Where( a => a.GroupTypeId == familyGroupTypeId && a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey )
                .ToList().ToDictionary( k => k.ForeignId.Value, v => v );

            Dictionary<int, Person> businessLookup = qryAllPersons.Include( a => a.PhoneNumbers ).AsNoTracking().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey && a.RecordTypeValueId == _recordTypeBusinessId )
                .ToList().ToDictionary( k => k.ForeignId.Value, v => v );

            _defaultPhoneCountryCode = PhoneNumber.DefaultCountryCode();

            var importDateTime = RockDateTime.Now;

            int nextNewFamilyForeignId = familiesLookup.Any() ? familiesLookup.Max( a => a.Key ) : 0;
            if ( businessImports.Any() )
            {
                nextNewFamilyForeignId = Math.Max( nextNewFamilyForeignId, businessImports.Where( a => a.FamilyForeignId.HasValue ).Max( a => a.FamilyForeignId.Value ) );
            }

            // Just In Case, ensure Entity Attributes are flushed (they might be stale if they were added directly via SQL)
            AttributeCache.RemoveEntityAttributes();

            var entityTypeIdPerson = EntityTypeCache.Get<Person>().Id;
            Dictionary<int, List<AttributeValueCache>> attributeValuesLookup = new AttributeValueService( rockContext ).Queryable().Where( a => a.Attribute.EntityTypeId == entityTypeIdPerson && a.EntityId.HasValue )
                .Select( a => new
                {
                    BusinessId = a.EntityId.Value,
                    a.AttributeId,
                    a.Value
                } )
                .GroupBy( a => a.BusinessId )
                .ToDictionary(
                    k => k.Key,
                    v => v.Select( x => new AttributeValueCache { AttributeId = x.AttributeId, EntityId = x.BusinessId, Value = x.Value } ).ToList() );

            int businessUpdatesCount = 0;
            long businessUpdatesMS = 0;
            int progress = 0;
            int total = businessImports.Count();

            foreach ( var businessImport in businessImports )
            {
                progress++;
                if ( progress % 100 == 0 && businessUpdatesMS > 0 )
                {
                    if ( initiatedWithWebRequest && HttpContext.Current?.Response?.IsClientConnected != true )
                    {
                        // if this was called from a WebRequest (versus a job or utility), quit if the client has disconnected
                        return "Client Disconnected";
                    }

                    OnProgress?.Invoke( $"Bulk Importing Business {progress} of {total}..." );
                }

                Group family = null;

                if ( !businessImport.FamilyForeignId.HasValue )
                {
                    // If personImport.FamilyForeignId is null, that means we need to create a new family
                    businessImport.FamilyForeignId = ++nextNewFamilyForeignId;
                }

                if ( familiesLookup.ContainsKey( businessImport.FamilyForeignId.Value ) )
                {
                    family = familiesLookup[businessImport.FamilyForeignId.Value];
                }

                if ( family == null )
                {
                    family = new Group();
                    family.GroupTypeId = familyGroupTypeId;
                    family.Name = string.IsNullOrEmpty( businessImport.FamilyName ) ? businessImport.LastName : businessImport.FamilyName;

                    if ( string.IsNullOrWhiteSpace( family.Name ) )
                    {
                        family.Name = "Family";
                    }

                    family.CampusId = businessImport.CampusId;

                    family.ForeignId = businessImport.FamilyForeignId;
                    family.ForeignKey = foreignSystemKey;
                    family.CreatedDateTime = businessImport.CreatedDateTime ?? importDateTime;
                    family.ModifiedDateTime = businessImport.ModifiedDateTime ?? importDateTime;
                    familiesLookup.Add( businessImport.FamilyForeignId.Value, family );
                }

                Person business = null;
                if ( businessLookup.ContainsKey( businessImport.PersonForeignId ) )
                {
                    business = businessLookup[businessImport.PersonForeignId];
                }

                if ( business == null )
                {
                    business = new Person();
                    UpdateBusinessPropertiesFromPersonImport( businessImport, business, foreignSystemKey );
                    businessLookup.Add( businessImport.PersonForeignId, business );
                }
                else
                {
                    if ( this.ImportUpdateOption == ImportUpdateType.AlwaysUpdate )
                    {
                        Stopwatch stopwatchPersonUpdates = Stopwatch.StartNew();
                        bool wasChanged = UpdatePersonFromPersonImport( business, businessImport, attributeValuesLookup, familiesLookup, foreignSystemKey, importDateTime );
                        stopwatchPersonUpdates.Stop();
                        businessUpdatesMS += stopwatchPersonUpdates.ElapsedMilliseconds;
                        if ( wasChanged )
                        {
                            businessUpdatesCount++;
                        }
                    }
                }
            }

            if ( businessUpdatesMS > 0 || businessUpdatesCount > 0 )
            {
                stopwatch.Stop();
                sbStats.AppendLine( $"Check for Business Updates [{stopwatch.ElapsedMilliseconds - businessUpdatesMS}ms]" );
                if ( businessUpdatesCount > 0 )
                {
                    sbStats.AppendLine( $"Updated {businessUpdatesCount} Person records [{businessUpdatesMS}ms]" );
                }
                else
                {
                    sbStats.AppendLine( $"No Business records need to be updated [{businessUpdatesMS}ms]" );
                }
                stopwatch.Restart();
            }

            double buildImportListsMS = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
            List<int> insertedBusinessesForeignIds = new List<int>();

            // insert all the [Group] records
            var familiesToInsert = familiesLookup.Where( a => a.Value.Id == 0 ).Select( a => a.Value ).ToList();

            // insert all the [Person] records.
            // NOTE: we are only inserting the [Person] record, not the PersonAlias or GroupMember records yet
            var businessesToInsert = businessLookup.Where( a => a.Value.Id == 0 ).Select( a => a.Value ).ToList();

            rockContext.BulkInsert( familiesToInsert );

            // lookup GroupId from Group.ForeignId
            var familyIdLookup = groupService.Queryable().AsNoTracking().Where( a => a.GroupTypeId == familyGroupTypeId && a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey )
                .ToList().ToDictionary( k => k.ForeignId.Value, v => v.Id );

            var businessToInsertLookup = businessesToInsert.ToDictionary( k => k.ForeignId.Value, v => v );

            // now that we have GroupId for each family, set the GivingGroupId for personImport's that don't give individually
            foreach ( var businessImport in businessImports )
            {
                if ( !businessImport.GivingIndividually.HasValue )
                {
                    // If GivingIndividually is NULL, Set it to false
                    businessImport.GivingIndividually = false;
                }

                if ( !businessImport.GivingIndividually.Value && businessImport.FamilyForeignId.HasValue )
                {
                    var businessToInsert = businessToInsertLookup.GetValueOrNull( businessImport.PersonForeignId );
                    if ( businessToInsert != null )
                    {
                        businessToInsert.GivingGroupId = familyIdLookup[businessImport.FamilyForeignId.Value];
                    }
                }
            }

            rockContext.BulkInsert( businessesToInsert );

            insertedBusinessesForeignIds = businessesToInsert.Select( a => a.ForeignId.Value ).ToList();

            // Make sure everybody has a PersonAlias
            PersonAliasService personAliasService = new PersonAliasService( rockContext );
            var personAliasServiceQry = personAliasService.Queryable();
            List<PersonAlias> businessAliasesToInsert = qryAllPersons.Where( p => p.ForeignId.HasValue && p.ForeignKey == foreignSystemKey && !p.Aliases.Any() && !personAliasServiceQry.Any( pa => pa.AliasPersonId == p.Id ) )
                .Select( x => new { x.Id, x.Guid, x.ForeignId } )
                .ToList()
                .Select( person => new PersonAlias { AliasPersonId = person.Id, AliasPersonGuid = person.Guid, PersonId = person.Id, ForeignId = person.ForeignId, ForeignKey = foreignSystemKey } ).ToList();

            rockContext.BulkInsert( businessAliasesToInsert );

            var familyGroupMembersQry = new GroupMemberService( rockContext ).Queryable( true ).Where( a => a.Group.GroupTypeId == familyGroupTypeId );

            // get the person Ids along with the PersonImport and GroupMember record
            var businessIdsForBusinessImport = from p in qryAllPersons.AsNoTracking().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey && a.RecordTypeValueId == _recordTypeBusinessId )
                                                .Select( a => new { a.Id, a.ForeignId } ).ToList()
                                            join pi in businessImports on p.ForeignId equals pi.PersonForeignId
                                            join f in groupService.Queryable().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey && a.GroupTypeId == familyGroupTypeId )
                                                .Select( a => new { a.Id, a.ForeignId } ).ToList() on pi.FamilyForeignId equals f.ForeignId
                                            join gm in familyGroupMembersQry.Select( a => new { a.Id, a.PersonId } ) on p.Id equals gm.PersonId into gmj
                                            from gm in gmj.DefaultIfEmpty()
                                            select new
                                            {
                                                BusinessId = p.Id,
                                                BusinessImport = pi,
                                                FamilyId = f.Id,
                                                HasGroupMemberRecord = gm != null
                                            };

            // narrow it down to just person records that we inserted
            businessIdsForBusinessImport = businessIdsForBusinessImport.Where( a => insertedBusinessesForeignIds.Contains( a.BusinessImport.PersonForeignId ) );

            // Make the GroupMember records for all the imported person (unless they are already have a groupmember record for the family)
            var groupMemberRecordsToInsertQry = from ppi in businessIdsForBusinessImport
                                                where !ppi.HasGroupMemberRecord
                                                select new GroupMember
                                                {
                                                    PersonId = ppi.BusinessId,
                                                    GroupRoleId = ppi.BusinessImport.GroupRoleId,
                                                    GroupId = ppi.FamilyId,
                                                    GroupMemberStatus = GroupMemberStatus.Active,
                                                    CreatedDateTime = ppi.BusinessImport.CreatedDateTime ?? importDateTime,
                                                    ModifiedDateTime = ppi.BusinessImport.ModifiedDateTime ?? importDateTime,
                                                };

            var groupMemberRecordsToInsertList = groupMemberRecordsToInsertQry.ToList();

            rockContext.BulkInsert( groupMemberRecordsToInsertList );

            List<Location> locationsToInsert = new List<Location>();
            List<GroupLocation> groupLocationsToInsert = new List<GroupLocation>();

            var locationCreatedDateTimeStart = RockDateTime.Now;

            foreach ( var familyRecord in businessIdsForBusinessImport.GroupBy( a => a.FamilyId ) )
            {
                // get the distinct addresses for each family in our import
                var familyAddresses = familyRecord.Where( a => a.BusinessImport?.Addresses != null ).SelectMany( a => a.BusinessImport.Addresses ).DistinctBy( a => new { a.GroupLocationTypeValueId, a.Street1, a.Street2, a.City, a.County, a.State } ).ToList();

                foreach ( var address in familyAddresses )
                {
                    GroupLocation groupLocation = new GroupLocation();
                    groupLocation.GroupLocationTypeValueId = address.GroupLocationTypeValueId;
                    groupLocation.GroupId = familyRecord.Key;
                    groupLocation.IsMailingLocation = address.IsMailingLocation;
                    groupLocation.IsMappedLocation = address.IsMappedLocation;
                    groupLocation.CreatedDateTime = locationCreatedDateTimeStart;
                    groupLocation.ModifiedDateTime = locationCreatedDateTimeStart;

                    Location location = new Location();

                    location.Street1 = address.Street1.Left( 100 );
                    location.Street2 = address.Street2.Left( 100 );
                    location.City = address.City.Left( 50 );
                    location.County = address.County.Left( 50 );
                    location.State = address.State.Left( 50 );
                    location.Country = address.Country.Left( 50 );
                    location.PostalCode = address.PostalCode.Left( 50 );
                    location.CreatedDateTime = locationCreatedDateTimeStart;
                    location.ModifiedDateTime = locationCreatedDateTimeStart;
                    if ( address.Latitude.HasValue && address.Longitude.HasValue )
                    {
                        location.SetLocationPointFromLatLong( address.Latitude.Value, address.Longitude.Value );
                    }

                    // give the Location a Guid, and store a reference to which Location is associated with the GroupLocation record. Then we'll match them up later and do the bulk insert
                    location.Guid = Guid.NewGuid();
                    groupLocation.Location = location;

                    groupLocationsToInsert.Add( groupLocation );
                    locationsToInsert.Add( location );
                }
            }

            rockContext.BulkInsert( locationsToInsert );

            var locationIdLookup = locationService.Queryable().Select( a => new { a.Id, a.Guid } ).ToList().ToDictionary( k => k.Guid, v => v.Id );
            foreach ( var groupLocation in groupLocationsToInsert )
            {
                groupLocation.LocationId = locationIdLookup[groupLocation.Location.Guid];
            }

            rockContext.BulkInsert( groupLocationsToInsert );

            // PhoneNumbers
            List<PhoneNumber> phoneNumbersToInsert = new List<PhoneNumber>();

            foreach ( var businessesIds in businessIdsForBusinessImport )
            {
                foreach ( var phoneNumberImport in businessesIds.BusinessImport.PhoneNumbers )
                {
                    var phoneNumberToInsert = new PhoneNumber();
                    phoneNumberToInsert.PersonId = businessesIds.BusinessId;
                    UpdatePhoneNumberFromPhoneNumberImport( phoneNumberImport, phoneNumberToInsert, importDateTime );

                    phoneNumbersToInsert.Add( phoneNumberToInsert );
                }
            }

            rockContext.BulkInsert( phoneNumbersToInsert );

            // Attribute Values
            var attributeValuesToInsert = new List<AttributeValue>();
            foreach ( var businessesIds in businessIdsForBusinessImport )
            {
                foreach ( var attributeValueImport in businessesIds.BusinessImport.AttributeValues )
                {
                    var attributeValue = new AttributeValue();

                    attributeValue.EntityId = businessesIds.BusinessId;
                    attributeValue.AttributeId = attributeValueImport.AttributeId;
                    attributeValue.Value = attributeValueImport.Value;
                    attributeValue.CreatedDateTime = businessesIds.BusinessImport.CreatedDateTime ?? importDateTime;
                    attributeValue.ModifiedDateTime = businessesIds.BusinessImport.ModifiedDateTime ?? importDateTime;
                    attributeValuesToInsert.Add( attributeValue );
                }
            }

            rockContext.BulkInsert( attributeValuesToInsert );

            if ( attributeValuesToInsert.Any() )
            {
                // manually update ValueAsDateTime since the tgrAttributeValue_InsertUpdate trigger won't fire during when using BulkInsert
                var rowsUpdated = rockContext.Database.ExecuteSqlCommand( @"
UPDATE [AttributeValue] SET ValueAsDateTime =
		CASE WHEN
			LEN(value) < 50 and
			ISNULL(value,'') != '' and
			ISNUMERIC([value]) = 0 THEN
				CASE WHEN [value] LIKE '____-__-__T%__:__:%' THEN
					ISNULL( TRY_CAST( TRY_CAST( LEFT([value],19) AS datetimeoffset ) as datetime) , TRY_CAST( value as datetime ))
				ELSE
					TRY_CAST( [value] as datetime )
				END
		END
        where (CASE WHEN
			LEN(value) < 50 and
			ISNULL(value,'') != '' and
			ISNUMERIC([value]) = 0 THEN
				CASE WHEN [value] LIKE '____-__-__T%__:__:%' THEN
					ISNULL( TRY_CAST( TRY_CAST( LEFT([value],19) AS datetimeoffset ) as datetime) , TRY_CAST( value as datetime ))
				ELSE
					TRY_CAST( [value] as datetime )
				END
		END) is not null
        and ValueAsDateTime is null
" );

                Debug.WriteLine( "ValueAsDateTime RowsUpdated: " + rowsUpdated.ToString() );
            }

            // since we bypassed Rock SaveChanges when Inserting Person records, sweep thru and ensure the AgeClassification, PrimaryFamily, and GivingLeaderId is set
            PersonService.UpdatePersonAgeClassificationAll( rockContext );
            PersonService.UpdatePrimaryFamilyAll( rockContext );
            PersonService.UpdateGivingLeaderIdAll( rockContext );

            stopwatchTotal.Stop();
            if ( businessesToInsert.Any() || groupMemberRecordsToInsertList.Any() || familiesToInsert.Any() )
            {
                sbStats.AppendLine( $"Imported {businessesToInsert.Count} People and {familiesToInsert.Count} Families [{stopwatchTotal.ElapsedMilliseconds}ms] and {groupMemberRecordsToInsertList.Count} family members" );
            }
            else
            {
                sbStats.AppendLine( $"No People were imported [{stopwatchTotal.ElapsedMilliseconds}ms]" );
            }

            var responseText = sbStats.ToString();

            return responseText;
        }

        /// <summary>
        /// Updates the person properties from person import.
        /// </summary>
        /// <param name="personImport">The person import.</param>
        /// <param name="person">The person.</param>
        /// <param name="foreignSystemKey">The foreign system key.</param>
        private void UpdateBusinessPropertiesFromPersonImport( PersonImport businessImport, Person business, string foreignSystemKey )
        {
            business.RecordTypeValueId = businessImport.RecordTypeValueId ?? _recordTypeBusinessId;
            business.RecordStatusValueId = businessImport.RecordStatusValueId;
            business.RecordStatusLastModifiedDateTime = businessImport.RecordStatusLastModifiedDateTime;
            business.RecordStatusReasonValueId = businessImport.RecordStatusReasonValueId;
            business.ConnectionStatusValueId = businessImport.ConnectionStatusValueId;
            business.ReviewReasonValueId = businessImport.ReviewReasonValueId;
            business.IsDeceased = businessImport.IsDeceased;

            business.LastName = businessImport.LastName.FixCase().Left( 50 );
            business.Gender = ( Gender ) businessImport.Gender;

            business.Email = businessImport.Email.Left( 75 );

            if ( !business.Email.IsValidEmail() )
            {
                business.Email = null;
            }

            business.IsEmailActive = businessImport.IsEmailActive;
            business.EmailNote = businessImport.EmailNote.Left( 250 );
            business.EmailPreference = ( EmailPreference ) businessImport.EmailPreference;
            business.InactiveReasonNote = businessImport.InactiveReasonNote.Left( 1000 );
            business.CreatedDateTime = businessImport.CreatedDateTime;
            business.ModifiedDateTime = businessImport.ModifiedDateTime;
            business.SystemNote = businessImport.Note;
            business.ForeignId = businessImport.PersonForeignId;
            business.ForeignKey = foreignSystemKey;
        }

        /// <summary>
        /// Updates the person from person import and returns whether there were any changes to the person record
        /// </summary>
        /// <param name="lookupPerson">The lookup person.</param>
        /// <param name="personImport">The person import.</param>
        /// <param name="attributeValuesLookup">The attribute values lookup.</param>
        /// <param name="familiesLookup">The families lookup.</param>
        /// <param name="foreignSystemKey">The foreign system key.</param>
        /// <param name="importDateTime">The import date time.</param>
        /// <returns></returns>
        private bool UpdateBusinessFromPersonImport( Person lookupBusiness, PersonImport businessImport, Dictionary<int, List<AttributeValueCache>> attributeValuesLookup, Dictionary<int, Group> familiesLookup, string foreignSystemKey, DateTime importDateTime )
        {
            using ( var rockContextForBusinessUpdate = new RockContext() )
            {
                rockContextForBusinessUpdate.People.Attach( lookupBusiness );
                var business = lookupBusiness;

                // Add/Update PhoneNumbers
                UpdateBusinessPropertiesFromPersonImport( businessImport, business, foreignSystemKey );
                var phoneNumberService = new PhoneNumberService( rockContextForBusinessUpdate );
                var businessPhoneNumberList = business.PhoneNumbers.Select( a => new
                {
                    a.Id,
                    a.Number
                } ).ToList();

                foreach ( var phoneNumberImport in businessImport.PhoneNumbers )
                {
                    var hasPhoneNumber = businessPhoneNumberList.Any( a => a.Number == PhoneNumber.CleanNumber( phoneNumberImport.Number ) );
                    if ( !hasPhoneNumber )
                    {
                        var personPhoneNumber = new PhoneNumber();
                        personPhoneNumber.PersonId = business.Id;
                        UpdatePhoneNumberFromPhoneNumberImport( phoneNumberImport, personPhoneNumber, importDateTime );
                        phoneNumberService.Add( personPhoneNumber );
                    }
                }

                // Remove any phonenumbers that are no longer in the PersonImport.PhoneNumbers list
                foreach ( var phone in businessPhoneNumberList.Where( a => !businessImport.PhoneNumbers.Any( x => PhoneNumber.CleanNumber( x.Number ) == a.Number ) ) )
                {
                    var personPhoneNumber = phoneNumberService.Get( phone.Id );
                    if ( personPhoneNumber != null )
                    {
                        phoneNumberService.Delete( personPhoneNumber );
                    }
                }

                var businessAttributesUpdated = false;
                if ( businessImport.AttributeValues.Any() )
                {
                    var attributeValues = attributeValuesLookup.GetValueOrNull( business.Id );

                    foreach ( AttributeValueImport attributeValueImport in businessImport.AttributeValues )
                    {
                        var currentValue = attributeValues?.FirstOrDefault( a => a.AttributeId == attributeValueImport.AttributeId );

                        if ( ( currentValue == null ) || ( currentValue.Value != attributeValueImport.Value ) )
                        {
                            if ( business.Attributes == null )
                            {
                                business.LoadAttributes( rockContextForBusinessUpdate );
                            }

                            var attributeCache = AttributeCache.Get( attributeValueImport.AttributeId );
                            if ( business.AttributeValues[attributeCache.Key].Value != attributeValueImport.Value )
                            {
                                business.SetAttributeValue( attributeCache.Key, attributeValueImport.Value );
                                businessAttributesUpdated = true;
                            }
                        }
                    }
                }

                // update Addresses
                var addressesUpdated = false;
                if ( businessImport.Addresses.Any() )
                {
                    var primaryFamily = familiesLookup.GetValueOrNull( businessImport.FamilyForeignId ?? 0 );

                    if ( primaryFamily != null )
                    {
                        var groupLocationService = new GroupLocationService( rockContextForBusinessUpdate );
                        var primaryFamilyGroupLocations = groupLocationService.Queryable().Where( a => a.GroupId == primaryFamily.Id ).Include( a => a.Location ).AsNoTracking().ToList();
                        foreach ( var businessAddressImport in businessImport.Addresses )
                        {
                            bool addressAlreadyExistsExactMatch = primaryFamilyGroupLocations.Where( a =>
                                 a.GroupLocationTypeValueId == businessAddressImport.GroupLocationTypeValueId
                                 && (
                                    a.Location.Street1 == businessAddressImport.Street1
                                    && a.Location.Street2 == businessAddressImport.Street2
                                    && a.Location.City == businessAddressImport.City
                                    && a.Location.County == businessAddressImport.County
                                    && a.Location.State == businessAddressImport.State
                                    && a.Location.Country == businessAddressImport.Country
                                    && a.Location.PostalCode == businessAddressImport.PostalCode
                                 ) ).Any();

                            if ( !addressAlreadyExistsExactMatch )
                            {
                                var locationService = new LocationService( rockContextForBusinessUpdate );

                                Location location = locationService.Get( businessAddressImport.Street1, businessAddressImport.Street2, businessAddressImport.City, businessAddressImport.State, businessAddressImport.PostalCode, businessAddressImport.Country, false );

                                if ( !primaryFamilyGroupLocations.Where( a => a.GroupLocationTypeValueId == businessAddressImport.GroupLocationTypeValueId && a.LocationId == location.Id ).Any() )
                                {
                                    var groupLocation = new GroupLocation();
                                    groupLocation.GroupId = primaryFamily.Id;
                                    groupLocation.GroupLocationTypeValueId = businessAddressImport.GroupLocationTypeValueId;
                                    groupLocation.IsMailingLocation = businessAddressImport.IsMailingLocation;
                                    groupLocation.IsMappedLocation = businessAddressImport.IsMappedLocation;

                                    if ( location.GeoPoint == null && businessAddressImport.Latitude.HasValue && businessAddressImport.Longitude.HasValue )
                                    {
                                        location.SetLocationPointFromLatLong( businessAddressImport.Latitude.Value, businessAddressImport.Longitude.Value );
                                    }

                                    groupLocation.LocationId = location.Id;
                                    groupLocationService.Add( groupLocation );

                                    addressesUpdated = true;
                                }
                            }
                        }

                        // NOTE: Don't remove addresses that are part of family, but not included in the personImport.  It might be from another Person that is the same family which hasn't been included
                    }
                }

                if ( businessAttributesUpdated )
                {
                    business.SaveAttributeValues();
                }

                var updatedRecords = rockContextForBusinessUpdate.SaveChanges( true );

                return addressesUpdated || businessAttributesUpdated || updatedRecords > 0;
            }
        }

        #endregion BusinessImport

        #region PhotoImport

        /// <summary>
        /// Bulks the photo import.
        /// </summary>
        /// <param name="photoImports">The photo imports.</param>
        /// <param name="foreignSystemKey">The foreign system key.</param>
        /// <returns></returns>
        public string BulkPhotoImport( List<PhotoImport> photoImports, string foreignSystemKey )
        {
            Stopwatch stopwatchTotal = Stopwatch.StartNew();

            var rockContext = new RockContext();

            List<BinaryFile> binaryFilesToInsert = new List<BinaryFile>();
            Dictionary<PhotoImport.PhotoImportType, Dictionary<int, Guid>> photoTypeForeignIdBinaryFileGuidDictionary = new Dictionary<PhotoImport.PhotoImportType, Dictionary<int, Guid>>();
            foreach ( var photoImportType in Enum.GetValues( typeof( PhotoImport.PhotoImportType ) ).OfType<PhotoImport.PhotoImportType>() )
            {
                photoTypeForeignIdBinaryFileGuidDictionary.Add( photoImportType, new Dictionary<int, Guid>() );
            }

            var binaryFileService = new BinaryFileService( rockContext );

            HashSet<string> alreadyExists = new HashSet<string>( binaryFileService.Queryable().Where( a => a.ForeignKey != null && a.ForeignKey != "" ).Select( a => a.ForeignKey ).Distinct().ToList() );

            List<BinaryFileData> binaryFileDatasToInsert = new List<BinaryFileData>();

            var personFamilyBinaryFileType = new BinaryFileTypeService( rockContext ).Get( Rock.SystemGuid.BinaryFiletype.PERSON_IMAGE.AsGuid() );
            var financialTransactionBinaryFileType = new BinaryFileTypeService( rockContext ).Get( Rock.SystemGuid.BinaryFiletype.CONTRIBUTION_IMAGE.AsGuid() );

            bool useBulkInsertForPhotos = false;

            var importDateTime = RockDateTime.Now;

            foreach ( var photoImport in photoImports )
            {
                BinaryFileType binaryFileType;
                if ( photoImport.PhotoType == PhotoImport.PhotoImportType.FinancialTransaction )
                {
                    binaryFileType = financialTransactionBinaryFileType;
                }
                else
                {
                    binaryFileType = personFamilyBinaryFileType;
                }

                var binaryFileToInsert = new BinaryFile()
                {
                    FileName = photoImport.FileName,
                    MimeType = photoImport.MimeType,
                    BinaryFileTypeId = binaryFileType.Id,
                    CreatedDateTime = importDateTime,
                    ModifiedDateTime = importDateTime,
                    Guid = Guid.NewGuid()
                };

                if ( !useBulkInsertForPhotos )
                {
                    byte[] photoData = Convert.FromBase64String( photoImport.PhotoData );
                    binaryFileToInsert.FileSize = photoData.Length;
                    binaryFileToInsert.ContentStream = new MemoryStream( photoData );
                }

                binaryFileToInsert.SetStorageEntityTypeId( binaryFileType.StorageEntityTypeId );

                if ( photoImport.PhotoType == PhotoImport.PhotoImportType.Person )
                {
                    binaryFileToInsert.ForeignKey = $"PersonForeignId_{foreignSystemKey}_{photoImport.ForeignId}";
                }
                else if ( photoImport.PhotoType == PhotoImport.PhotoImportType.Family )
                {
                    binaryFileToInsert.ForeignKey = $"FamilyForeignId_{foreignSystemKey}_{photoImport.ForeignId}";
                }
                else if ( photoImport.PhotoType == PhotoImport.PhotoImportType.FinancialTransaction )
                {
                    binaryFileToInsert.ForeignKey = $"FinancialTransactionForeignId_{foreignSystemKey}_{photoImport.ForeignId}";
                }

                if ( !alreadyExists.Contains( binaryFileToInsert.ForeignKey ) )
                {
                    binaryFilesToInsert.Add( binaryFileToInsert );
                    photoTypeForeignIdBinaryFileGuidDictionary[photoImport.PhotoType].Add( photoImport.ForeignId, binaryFileToInsert.Guid );
                }
            }

            if ( !useBulkInsertForPhotos )
            {
                binaryFileService.AddRange( binaryFilesToInsert );
                rockContext.SaveChanges();
                foreach ( var binaryFile in binaryFilesToInsert )
                {
                    if ( binaryFile.ContentStream != null )
                    {
                        binaryFile.ContentStream.Dispose();
                    }
                }
            }
            else
            {
                rockContext.BulkInsert( binaryFilesToInsert );

                var binaryFileIdLookup = new BinaryFileService( rockContext ).Queryable().Select( a => new { a.Guid, a.Id } ).ToDictionary( k => k.Guid, v => v.Id );
                foreach ( var photoImport in photoImports )
                {
                    if ( photoTypeForeignIdBinaryFileGuidDictionary[photoImport.PhotoType].ContainsKey( photoImport.ForeignId ) )
                    {
                        Guid binaryFileGuid = photoTypeForeignIdBinaryFileGuidDictionary[photoImport.PhotoType][photoImport.ForeignId];
                        int binaryFileId = binaryFileIdLookup[binaryFileGuid];
                        var binaryFileDataToInsert = new BinaryFileData()
                        {
                            Id = binaryFileId,
                            Content = Convert.FromBase64String( photoImport.PhotoData ),
                            CreatedDateTime = importDateTime,
                            ModifiedDateTime = importDateTime
                        };

                        binaryFileDatasToInsert.Add( binaryFileDataToInsert );
                    }
                }

                rockContext.BulkInsert( binaryFileDatasToInsert );
            }

            // Update Person PhotoIds to the photos that were just Imported
            rockContext.Database.ExecuteSqlCommand( $@"UPDATE p
SET p.PhotoId = b.Id
FROM Person p
INNER JOIN BinaryFile b ON p.ForeignId = Replace(b.ForeignKey, 'PersonForeignId_{foreignSystemKey}_', '')
WHERE b.ForeignKey LIKE 'PersonForeignId_{foreignSystemKey}_%'
	AND p.PhotoId IS NULL" );

            // Update FamilyPhoto attribute for photos that were imported
            int? familyPhotoAttributeId = null;
            var groupEntityTypeId = EntityTypeCache.Get( SystemGuid.EntityType.GROUP.AsGuid() )?.Id;
            var familyGroupTypeId = GroupTypeCache.GetFamilyGroupType()?.Id;
            if ( groupEntityTypeId.HasValue && familyGroupTypeId.HasValue )
            {
                familyPhotoAttributeId = new AttributeService( rockContext )
                    .Get( groupEntityTypeId.Value, "GroupTypeId", familyGroupTypeId.Value.ToString(), "FamilyPhoto" )?.Id;
            }

            if ( familyPhotoAttributeId.HasValue )
            {
                rockContext.Database.ExecuteSqlCommand( $@"
DECLARE @AttributeId INT = {familyPhotoAttributeId.Value}

-- just in case the family photo was already saved but with No Photo
DELETE
FROM AttributeValue
WHERE (
		[Value] IS NULL
		OR [Value] = ''
		)
	AND AttributeId = @AttributeId

-- set the Photo for the Families
INSERT INTO AttributeValue (
	IsSystem
	,AttributeId
	,EntityId
	,[Value]
	,[Guid]
	)
SELECT 0
	,@AttributeId
	,g.Id
	,b.[Guid]
	,newid()
FROM [Group] g
INNER JOIN BinaryFile b ON g.ForeignId = Replace(b.ForeignKey, 'FamilyForeignId_{foreignSystemKey}_', '')
WHERE g.GroupTypeId = {familyGroupTypeId.Value}
	AND b.ForeignKey LIKE 'FamilyForeignId_{foreignSystemKey}_%'
	AND g.Id NOT IN (
		SELECT EntityId
		FROM AttributeValue
		WHERE AttributeId = @AttributeId
		)
" );
            }

            // Insert Financial Transaction Images (note: some transactions might have multiple images)
            rockContext.Database.ExecuteSqlCommand(
            $@"
INSERT INTO [FinancialTransactionImage]
    ([TransactionId]
    ,[BinaryFileId]
    ,[Guid]
    ,[Order]
)
select
    ft.Id [TransactionId],
    bf.Id[BinaryFileId],
    NEWID()[Guid],
    ROW_NUMBER() OVER (partition by bf.ForeignKey order by bf.[FileName] ) - 1 as [Order]
        FROM FinancialTransaction ft
        INNER JOIN BinaryFile bf ON ft.ForeignId = Replace( bf.ForeignKey, 'FinancialTransactionForeignId_{foreignSystemKey}_', '')
WHERE bf.ForeignKey LIKE 'FinancialTransactionForeignId_{foreignSystemKey}_%'
and ft.Id not in (select TransactionId from FinancialTransactionImage)" );

            stopwatchTotal.Stop();

            return GetResponseMessage( binaryFilesToInsert.Count, "Photo", stopwatchTotal.ElapsedMilliseconds );
        }

        #endregion PhotoImport

        #region ScheduleImport

        /// <summary>
        /// Bulks the schedule import.
        /// </summary>
        /// <param name="scheduleImports">The schedule imports.</param>
        /// <returns></returns>
        public string BulkScheduleImport( List<ScheduleImport> scheduleImports, string foreignSystemKey )
        {
            Stopwatch stopwatchTotal = Stopwatch.StartNew();

            RockContext rockContext = new RockContext();

            var qrySchedulesWithForeignIds = new ScheduleService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey );

            var scheduleAlreadyExistForeignIdHash = new HashSet<int>( qrySchedulesWithForeignIds.Select( a => a.ForeignId.Value ).ToList() );

            List<Schedule> schedulesToInsert = new List<Schedule>();
            var newScheduleImports = scheduleImports.Where( a => !scheduleAlreadyExistForeignIdHash.Contains( a.ScheduleForeignId ) ).ToList();

            int entityTypeIdSchedule = EntityTypeCache.GetId<Schedule>() ?? 0;
            var categoryService = new CategoryService( rockContext );
            string categoryName = "Imported Schedules";
            var scheduleCategory = categoryService.Queryable().Where( a => a.EntityTypeId == entityTypeIdSchedule && a.Name == categoryName ).FirstOrDefault();
            if ( scheduleCategory == null )
            {
                scheduleCategory = new Category
                {
                    EntityTypeId = entityTypeIdSchedule,
                    Name = categoryName
                };

                categoryService.Add( scheduleCategory );
                rockContext.SaveChanges();
            }

            var importDateTime = RockDateTime.Now;

            foreach ( var scheduleImport in newScheduleImports )
            {
                var schedule = new Schedule();
                schedule.ForeignId = scheduleImport.ScheduleForeignId;
                schedule.ForeignKey = foreignSystemKey;
                schedule.CategoryId = scheduleCategory.Id;
                if ( scheduleImport.Name.Length > 50 )
                {
                    schedule.Name = scheduleImport.Name.Left( 50 );
                    schedule.Description = scheduleImport.Name;
                }
                else
                {
                    schedule.Name = scheduleImport.Name;
                }

                schedule.CreatedDateTime = importDateTime;
                schedule.ModifiedDateTime = importDateTime;

                schedulesToInsert.Add( schedule );
            }

            rockContext.BulkInsert( schedulesToInsert );

            stopwatchTotal.Stop();
            return GetResponseMessage( schedulesToInsert.Count, "Schedules", stopwatchTotal.ElapsedMilliseconds );
        }

        #endregion ScheduleImport

        #region FinancialPledgeImport

        /// <summary>
        /// Bulks the financial pledge import.
        /// </summary>
        /// <param name="financialPledgeImports">The financial pledge imports.</param>
        /// <returns></returns>
        public string BulkFinancialPledgeImport( List<FinancialPledgeImport> financialPledgeImports, string foreignSystemKey )
        {
            Stopwatch stopwatchTotal = Stopwatch.StartNew();

            RockContext rockContext = new RockContext();

            var qryFinancialPledgesWithForeignIds = new FinancialPledgeService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey );

            var financialPledgeAlreadyExistForeignIdHash = new HashSet<int>( qryFinancialPledgesWithForeignIds.Select( a => a.ForeignId.Value ).ToList() );

            var personAliasIdLookup = new PersonAliasService( rockContext ).Queryable().Where( a => a.Person.ForeignId.HasValue && a.Person.ForeignKey == foreignSystemKey && a.PersonId == a.AliasPersonId )
                .Select( a => new { PersonAliasId = a.Id, PersonForeignId = a.Person.ForeignId } ).ToDictionary( k => k.PersonForeignId.Value, v => v.PersonAliasId );

            var financialAccountIdLookup = new FinancialAccountService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey )
                .Select( a => new { a.Id, a.ForeignId } )
                .ToList().ToDictionary( k => k.ForeignId.Value, v => v.Id );

            int groupTypeIdFamily = GroupTypeCache.GetFamilyGroupType().Id;
            var familyGroupIdLookup = new GroupService( rockContext ).Queryable().Where( a => a.GroupTypeId == groupTypeIdFamily && a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey )
                .Select( a => new { a.Id, a.ForeignId } )
                .ToList().ToDictionary( k => k.ForeignId.Value, v => v.Id );

            List<FinancialPledge> financialPledgesToInsert = new List<FinancialPledge>();
            var newFinancialPledgeImports = financialPledgeImports.Where( a => !financialPledgeAlreadyExistForeignIdHash.Contains( a.FinancialPledgeForeignId ) ).ToList();

            var importDateTime = RockDateTime.Now;

            foreach ( var financialPledgeImport in newFinancialPledgeImports )
            {
                var financialPledge = new FinancialPledge();
                financialPledge.ForeignId = financialPledgeImport.FinancialPledgeForeignId;
                financialPledge.ForeignKey = foreignSystemKey;
                financialPledge.PersonAliasId = personAliasIdLookup.GetValueOrNull( financialPledgeImport.PersonForeignId );

                if ( financialPledgeImport.FinancialAccountForeignId.HasValue )
                {
                    financialPledge.AccountId = financialAccountIdLookup.GetValueOrNull( financialPledgeImport.FinancialAccountForeignId.Value );
                }

                if ( financialPledgeImport.GroupForeignId.HasValue )
                {
                    financialPledge.GroupId = familyGroupIdLookup.GetValueOrNull( financialPledgeImport.GroupForeignId.Value );
                }

                financialPledge.TotalAmount = financialPledgeImport.TotalAmount;

                financialPledge.PledgeFrequencyValueId = financialPledgeImport.PledgeFrequencyValueId;
                financialPledge.StartDate = financialPledgeImport.StartDate;
                financialPledge.EndDate = financialPledgeImport.EndDate;

                financialPledge.CreatedDateTime = financialPledgeImport.CreatedDateTime ?? importDateTime;
                financialPledge.ModifiedDateTime = financialPledgeImport.ModifiedDateTime ?? importDateTime;

                financialPledgesToInsert.Add( financialPledge );
            }

            rockContext.BulkInsert( financialPledgesToInsert );

            stopwatchTotal.Stop();

            return GetResponseMessage( financialPledgesToInsert.Count, "Financial Pledges", stopwatchTotal.ElapsedMilliseconds );
        }

        #endregion FinancialPledgeImport

        #region NoteImport

        /// <summary>
        /// Bulks the note import.
        /// </summary>
        /// <param name="noteImports">The note imports.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="foreignSystemKey">The foreign system key.</param>
        /// <param name="groupEntityIsFamily">If this is a GroupEntity, is it a Family GroupType?</param>
        /// <returns></returns>
        public string BulkNoteImport( List<NoteImport> noteImports, int entityTypeId, string foreignSystemKey, bool? groupEntityIsFamily )
        {
            Stopwatch stopwatchTotal = Stopwatch.StartNew();

            var entityTypeCache = EntityTypeCache.Get( entityTypeId );
            var entityFriendlyName = entityTypeCache.FriendlyName;
            if ( entityTypeId == EntityTypeCache.GetId<Rock.Model.Group>().Value )
            {
                if ( groupEntityIsFamily.Value )
                {
                    entityFriendlyName = "Family";
                }
            }

            // first check for invalid NoteType or NoteType.EntityType
            var noteTypeList = noteImports.Select( a => a.NoteTypeId ).Distinct().ToList().Select( a => NoteTypeCache.Get( a ) ).ToList();
            if ( noteTypeList.Any( a => a == null ) )
            {
                return "WARNING: Unable to determine NoteType for one or more notes. No Notes imported.";
            }
            else if ( noteTypeList.Where( a => a != null ).Any( a => a.EntityTypeId != entityTypeId ) )
            {
                return "WARNING: NoteType for one or more notes is not for the specified entityTypeId. No Notes imported.";
            }

            RockContext rockContext = new RockContext();

            var qryNotesWithForeignIds = new NoteService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey && a.NoteType.EntityTypeId == entityTypeId );

            var noteAlreadyExistForeignIdHash = new HashSet<int>( qryNotesWithForeignIds.Select( a => a.ForeignId.Value ).ToList() );

            Dictionary<int, int> entityIdLookup;
            if ( entityTypeId == EntityTypeCache.GetId<Rock.Model.Group>().Value )
            {
                int groupTypeIdFamily = GroupTypeCache.GetFamilyGroupType().Id;
                if ( groupEntityIsFamily.Value == true )
                {
                    entityIdLookup = new GroupService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey && a.GroupTypeId == groupTypeIdFamily )
                        .Select( a => new { a.Id, a.ForeignId } )
                        .ToList().ToDictionary( k => k.ForeignId.Value, v => v.Id );
                }
                else
                {
                    entityIdLookup = new GroupService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey && a.GroupTypeId != groupTypeIdFamily )
                        .Select( a => new { a.Id, a.ForeignId } )
                        .ToList().ToDictionary( k => k.ForeignId.Value, v => v.Id );
                }
            }
            else
            {
                Type entityType = entityTypeCache.GetEntityType();
                var entityService = Reflection.GetServiceForEntityType( entityType, rockContext );
                MethodInfo queryableMethodInfo = entityService.GetType().GetMethod( "Queryable", new Type[] { } );
                IQueryable<IEntity> entityQuery = queryableMethodInfo.Invoke( entityService, null ) as IQueryable<IEntity>;

                entityIdLookup = entityQuery.Where( a => a.ForeignId.HasValue && a.ForeignKey == foreignSystemKey )
                    .Select( a => new { a.Id, a.ForeignId } )
                    .ToList().ToDictionary( k => k.ForeignId.Value, v => v.Id );
            }

            var personAliasIdLookup = new PersonAliasService( rockContext ).Queryable().Where( a => a.Person.ForeignId.HasValue && a.Person.ForeignKey == foreignSystemKey && a.PersonId == a.AliasPersonId )
                .Select( a => new { PersonAliasId = a.Id, PersonForeignId = a.Person.ForeignId } ).ToDictionary( k => k.PersonForeignId.Value, v => v.PersonAliasId );

            List<Note> notesToInsert = new List<Note>();
            var newNoteImports = noteImports.Where( a => !noteAlreadyExistForeignIdHash.Contains( a.NoteForeignId ) ).ToList();

            int noteImportErrors = 0;

            var importDateTime = RockDateTime.Now;

            foreach ( var noteImport in newNoteImports )
            {
                var note = new Note();
                note.ForeignId = noteImport.NoteForeignId;
                note.ForeignKey = foreignSystemKey;
                note.EntityId = entityIdLookup.GetValueOrNull( noteImport.EntityForeignId );
                note.NoteTypeId = noteImport.NoteTypeId;
                note.Caption = noteImport.Caption ?? string.Empty;
                if ( note.Caption.Length > 200 )
                {
                    note.Caption = note.Caption.Left( 200 );
                }

                note.IsAlert = noteImport.IsAlert;
                note.IsPrivateNote = noteImport.IsPrivateNote;
                note.Text = noteImport.Text;
                note.CreatedDateTime = noteImport.DateTime ?? importDateTime;
                note.ModifiedDateTime = noteImport.DateTime ?? importDateTime;
                if ( noteImport.CreatedByPersonForeignId.HasValue )
                {
                    note.CreatedByPersonAliasId = personAliasIdLookup.GetValueOrNull( noteImport.CreatedByPersonForeignId.Value );
                }

                notesToInsert.Add( note );
            }

            rockContext.BulkInsert( notesToInsert );

            stopwatchTotal.Stop();
            string responseText = string.Empty;
            if ( noteImportErrors > 0 )
            {
                responseText += $"WARNING: Unable to import {noteImportErrors} notes due to invalid NoteType or NoteType EntityType mismatch.\n";
            }

            responseText += GetResponseMessage( notesToInsert.Count, $"{entityFriendlyName} Notes", stopwatchTotal.ElapsedMilliseconds );

            return responseText;
        }

        #endregion NoteImport
    }

    public class ImportOccurrence
    {
        public int Id { get; set; }
        public int? GroupId { get; set; }
        public int? LocationId { get; set; }
        public int? ScheduleId { get; set; }
        public DateTime OccurrenceDate { get; set; }
    }
}