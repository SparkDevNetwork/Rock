using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using Rock.Slingshot.Model;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Slingshot
{
    /// <summary>
    /// 
    /// </summary>
    public static class BulkImportHelper
    {
        /// <summary>
        /// Bulks the attendance import.
        /// </summary>
        /// <param name="attendanceImports">The attendance imports.</param>
        /// <returns></returns>
        public static string BulkAttendanceImport( List<AttendanceImport> attendanceImports )
        {
            Stopwatch stopwatchTotal = Stopwatch.StartNew();
            Stopwatch stopwatch = Stopwatch.StartNew();

            RockContext rockContext = new RockContext();
            StringBuilder sbStats = new StringBuilder();

            int groupTypeIdFamily = GroupTypeCache.GetFamilyGroupType().Id;

            var groupIdLookup = new GroupService( rockContext ).Queryable().Where( a => a.GroupTypeId != groupTypeIdFamily && a.ForeignId.HasValue )
                .Select( a => new { a.Id, a.ForeignId } ).ToDictionary( k => k.ForeignId.Value, v => v.Id );

            var locationIdLookup = new LocationService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue )
                .Select( a => new { a.Id, a.ForeignId } ).ToDictionary( k => k.ForeignId.Value, v => v.Id );

            var scheduleIdLookup = new ScheduleService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue )
                .Select( a => new { a.Id, a.ForeignId } ).ToDictionary( k => k.ForeignId.Value, v => v.Id );

            // Get the primary alias id lookup for each person foreign id
            var personAliasIdLookup = new PersonAliasService( rockContext ).Queryable().Where( a => a.Person.ForeignId.HasValue && a.PersonId == a.AliasPersonId )
                .Select( a => new { PersonAliasId = a.Id, PersonForeignId = a.Person.ForeignId } ).ToDictionary( k => k.PersonForeignId.Value, v => v.PersonAliasId );

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Prepare Lookups for Attendance Insert" );
            stopwatch.Restart();

            var attendancesToInsert = new List<Attendance>( attendanceImports.Count );
            foreach ( var attendanceImport in attendanceImports )
            {
                var attendance = new Attendance();

                // NOTE: attendanceImport doesn't have to have an AttendanceForeignId and probably won't have one
                if ( attendanceImport.AttendanceForeignId.HasValue )
                {
                    attendance.ForeignId = attendanceImport.AttendanceForeignId;
                }

                attendance.CampusId = attendanceImport.CampusId;
                attendance.StartDateTime = attendanceImport.StartDateTime;
                attendance.EndDateTime = attendanceImport.EndDateTime;

                if ( attendanceImport.GroupForeignId.HasValue )
                {
                    attendance.GroupId = groupIdLookup.GetValueOrNull( attendanceImport.GroupForeignId.Value );
                }

                if ( attendanceImport.LocationForeignId.HasValue )
                {
                    attendance.LocationId = locationIdLookup.GetValueOrNull( attendanceImport.LocationForeignId.Value );
                }

                if ( attendanceImport.ScheduleForeignId.HasValue )
                {
                    attendance.ScheduleId = scheduleIdLookup.GetValueOrNull( attendanceImport.ScheduleForeignId.Value );
                }

                attendance.PersonAliasId = personAliasIdLookup.GetValueOrNull( attendanceImport.PersonForeignId );
                attendance.Note = attendanceImport.Note;
                attendance.DidAttend = true;

                attendancesToInsert.Add( attendance );
            }

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Prepare Attendance Insert List" );
            stopwatch.Restart();

            var groupIds = attendancesToInsert.Select( a => a.GroupId ).Distinct().ToList();
            var allGroupIds = new GroupService( rockContext ).Queryable().Select( a => a.Id ).ToList();
            var missing = groupIds.Where( a => !a.HasValue || !allGroupIds.Contains( a.Value ) );

            rockContext.BulkInsert( attendancesToInsert );

            sbStats.AppendLine( $"[{stopwatchTotal.Elapsed.TotalMilliseconds}ms] Insert {attendanceImports.Count} Attendance records" );
            var responseText = sbStats.ToString();

            return responseText;
        }

        /// <summary>
        /// Bulks the financial account import.
        /// </summary>
        /// <param name="financialAccountImports">The financial account imports.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public static string BulkFinancialAccountImport( List<FinancialAccountImport> financialAccountImports )
        {
            Stopwatch stopwatchTotal = Stopwatch.StartNew();

            RockContext rockContext = new RockContext();

            var qryFinancialAccountsWithForeignIds = new FinancialAccountService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue );

            var financialAccountAlreadyExistForeignIdHash = new HashSet<int>( qryFinancialAccountsWithForeignIds.Select( a => a.ForeignId.Value ).ToList() );

            List<FinancialAccount> financialAccountsToInsert = new List<FinancialAccount>();
            var newFinancialAccountImports = financialAccountImports.Where( a => !financialAccountAlreadyExistForeignIdHash.Contains( a.FinancialAccountForeignId ) ).ToList();

            foreach ( var financialAccountImport in newFinancialAccountImports )
            {
                var financialAccount = new FinancialAccount();
                financialAccount.ForeignId = financialAccountImport.FinancialAccountForeignId;
                if ( financialAccountImport.Name.Length > 50 )
                {
                    financialAccount.Name = financialAccountImport.Name.Truncate( 50 );
                    financialAccount.Description = financialAccountImport.Name;
                }
                else
                {
                    financialAccount.Name = financialAccountImport.Name;
                }

                financialAccount.CampusId = financialAccountImport.CampusId;
                financialAccount.IsTaxDeductible = financialAccountImport.IsTaxDeductible;

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
            var responseText = $"[{stopwatchTotal.Elapsed.TotalMilliseconds}ms] Insert {financialAccountsToInsert.Count} Financial Accounts";

            return responseText;
        }

        /// <summary>
        /// Bulks the financial batch import.
        /// </summary>
        /// <param name="financialBatchImports">The financial batch imports.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public static string BulkFinancialBatchImport( List<FinancialBatchImport> financialBatchImports )
        {
            Stopwatch stopwatchTotal = Stopwatch.StartNew();

            RockContext rockContext = new RockContext();

            var qryFinancialBatchsWithForeignIds = new FinancialBatchService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue );

            var financialBatchAlreadyExistForeignIdHash = new HashSet<int>( qryFinancialBatchsWithForeignIds.Select( a => a.ForeignId.Value ).ToList() );

            List<FinancialBatch> financialBatchsToInsert = new List<FinancialBatch>();
            var newFinancialBatchImports = financialBatchImports.Where( a => !financialBatchAlreadyExistForeignIdHash.Contains( a.FinancialBatchForeignId ) ).ToList();

            // Get the primary alias id lookup for each person foreign id
            var personAliasIdLookup = new PersonAliasService( rockContext ).Queryable().Where( a => a.Person.ForeignId.HasValue && a.PersonId == a.AliasPersonId )
                .Select( a => new { PersonAliasId = a.Id, PersonForeignId = a.Person.ForeignId } ).ToDictionary( k => k.PersonForeignId.Value, v => v.PersonAliasId );

            foreach ( var financialBatchImport in newFinancialBatchImports )
            {
                var financialBatch = new FinancialBatch();
                financialBatch.ForeignId = financialBatchImport.FinancialBatchForeignId;
                if ( financialBatchImport.Name.Length > 50 )
                {
                    financialBatch.Name = financialBatchImport.Name.Truncate( 50 );
                }
                else
                {
                    financialBatch.Name = financialBatchImport.Name;
                }

                financialBatch.CampusId = financialBatchImport.CampusId;
                financialBatch.ControlAmount = financialBatchImport.ControlAmount;

                financialBatch.CreatedDateTime = financialBatchImport.CreatedDateTime;
                financialBatch.BatchEndDateTime = financialBatchImport.EndDate;

                financialBatch.ModifiedDateTime = financialBatchImport.ModifiedDateTime;
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
            var responseText = $"[{stopwatchTotal.Elapsed.TotalMilliseconds}ms] Insert {financialBatchsToInsert.Count} Financial Batches";

            return responseText;
        }

        /// <summary>
        /// Bulks the financial transaction import.
        /// </summary>
        /// <param name="financialTransactionImports">The financial transaction imports.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public static string BulkFinancialTransactionImport( List<FinancialTransactionImport> financialTransactionImports )
        {
            Stopwatch stopwatchTotal = Stopwatch.StartNew();

            RockContext rockContext = new RockContext();

            var qryFinancialTransactionsWithForeignIds = new FinancialTransactionService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue );

            var financialTransactionAlreadyExistForeignIdHash = new HashSet<int>( qryFinancialTransactionsWithForeignIds.Select( a => a.ForeignId.Value ).ToList() );

            var newFinancialTransactionImports = financialTransactionImports.Where( a => !financialTransactionAlreadyExistForeignIdHash.Contains( a.FinancialTransactionForeignId ) ).ToList();

            // Get the primary alias id lookup for each person foreign id
            var personAliasIdLookup = new PersonAliasService( rockContext ).Queryable().Where( a => a.Person.ForeignId.HasValue && a.PersonId == a.AliasPersonId )
                .Select( a => new { PersonAliasId = a.Id, PersonForeignId = a.Person.ForeignId } ).ToDictionary( k => k.PersonForeignId.Value, v => v.PersonAliasId );

            var batchIdLookup = new FinancialBatchService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue )
                .Select( a => new { a.Id, a.ForeignId } ).ToDictionary( k => k.ForeignId.Value, v => v.Id );

            var accountIdLookup = new FinancialAccountService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue )
                .Select( a => new { a.Id, a.ForeignId } ).ToDictionary( k => k.ForeignId.Value, v => v.Id );

            // Insert FinancialPaymentDetail for all the transactions first
            List<FinancialPaymentDetail> financialPaymentDetailToInsert = new List<FinancialPaymentDetail>( newFinancialTransactionImports.Count );
            foreach ( var financialTransactionImport in newFinancialTransactionImports )
            {
                var financialPaymentDetail = new FinancialPaymentDetail();
                financialPaymentDetail.CurrencyTypeValueId = financialTransactionImport.CurrencyTypeValueId;
                financialPaymentDetail.ForeignId = financialTransactionImport.FinancialTransactionForeignId;
                financialPaymentDetailToInsert.Add( financialPaymentDetail );
            }

            rockContext.BulkInsert( financialPaymentDetailToInsert );

            var financialPaymentDetailLookup = new FinancialPaymentDetailService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue )
                .Select( a => new { a.Id, a.ForeignId } ).ToDictionary( k => k.ForeignId.Value, v => v.Id );

            // Prepare and Insert FinancialTransactions
            List<FinancialTransaction> financialTransactionsToInsert = new List<FinancialTransaction>();
            foreach ( var financialTransactionImport in newFinancialTransactionImports )
            {
                var financialTransaction = new FinancialTransaction();
                financialTransaction.ForeignId = financialTransactionImport.FinancialTransactionForeignId;

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
                financialTransaction.CreatedDateTime = financialTransactionImport.CreatedDateTime;
                financialTransaction.ModifiedDateTime = financialTransactionImport.ModifiedDateTime;

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

            rockContext.BulkInsert( financialTransactionsToInsert );

            var financialTransactionIdLookup = new FinancialTransactionService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue )
                .Select( a => new { a.Id, a.ForeignId } )
                .ToList().ToDictionary( k => k.ForeignId.Value, v => v.Id );

            var financialAccountIdLookup = new FinancialAccountService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue )
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
                    financialTransactionDetail.Amount = financialTransactionDetailImport.Amount;
                    financialTransactionDetail.AccountId = financialAccountIdLookup[financialTransactionDetailImport.FinancialAccountForeignId.Value];
                    financialTransactionDetail.Summary = financialTransactionDetailImport.Summary;
                    financialTransactionDetail.CreatedDateTime = financialTransactionDetailImport.CreatedDateTime;
                    financialTransactionDetail.ModifiedDateTime = financialTransactionDetailImport.ModifiedDateTime;

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

            rockContext.BulkInsert( financialTransactionDetailsToInsert );

            stopwatchTotal.Stop();
            var responseText = $"[{stopwatchTotal.Elapsed.TotalMilliseconds}ms] Insert {financialTransactionsToInsert.Count} Financial Transactions";

            return responseText;
        }

        /// <summary>
        /// Bulks the group import.
        /// </summary>
        /// <param name="groupImports">The group imports.</param>
        /// <returns></returns>
        public static string BulkGroupImport( List<GroupImport> groupImports )
        {
            Stopwatch stopwatchTotal = Stopwatch.StartNew();
            Stopwatch stopwatch = Stopwatch.StartNew();

            RockContext rockContext = new RockContext();
            StringBuilder sbStats = new StringBuilder();

            var groupsAlreadyExistLookupQry = new GroupService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue ).Select( a => new
            {
                GroupForeignId = a.ForeignId.Value,
                GroupTypeId = a.GroupTypeId
            } );

            var newGroupImports = groupImports.Where( a => !groupsAlreadyExistLookupQry.ToList().Any( x => x.GroupForeignId == a.GroupForeignId && x.GroupTypeId == a.GroupTypeId ) ).ToList();

            var importedGroupTypeRoleNames = groupImports.GroupBy( a => a.GroupTypeId ).Select( a => new
            {
                GroupTypeId = a.Key,
                RoleNames = a.SelectMany( x => x.GroupMemberImports ).Select( x => x.RoleName ).Distinct().ToList()
            } );

            // Create any missing roles on the GroupType
            var groupTypeRolesToInsert = new List<GroupTypeRole>();

            foreach ( var importedGroupTypeRoleName in importedGroupTypeRoleNames )
            {
                var groupTypeCache = GroupTypeCache.Read( importedGroupTypeRoleName.GroupTypeId, rockContext );
                foreach ( var roleName in importedGroupTypeRoleName.RoleNames )
                {
                    if ( !groupTypeCache.Roles.Any( a => a.Name.Equals( roleName, StringComparison.OrdinalIgnoreCase ) ) )
                    {
                        var groupTypeRole = new GroupTypeRole();
                        groupTypeRole.GroupTypeId = groupTypeCache.Id;
                        groupTypeRole.Name = roleName.Truncate( 100 );
                        groupTypeRolesToInsert.Add( groupTypeRole );
                    }
                }
            }

            var updatedGroupTypes = groupTypeRolesToInsert.Select( a => a.GroupTypeId.Value ).Distinct().ToList();
            updatedGroupTypes.ForEach( id => GroupTypeCache.Flush( id ) );

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Updated {groupTypeRolesToInsert.Count} GroupType Roles" );
            stopwatch.Restart();

            if ( groupTypeRolesToInsert.Any() )
            {
                rockContext.BulkInsert( groupTypeRolesToInsert );
            }

            List<Group> groupsToInsert = new List<Group>( newGroupImports.Count );

            foreach ( var groupImport in newGroupImports )
            {
                var group = new Group();
                group.ForeignId = groupImport.GroupForeignId;
                group.GroupTypeId = groupImport.GroupTypeId;
                if ( groupImport.Name.Length > 100 )
                {
                    group.Name = groupImport.Name.Truncate( 100 );
                    group.Description = groupImport.Name;
                }
                else
                {
                    group.Name = groupImport.Name;
                }

                group.Order = groupImport.Order;
                group.CampusId = groupImport.CampusId;

                groupsToInsert.Add( group );
            }

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Prepare {groupsToInsert.Count} Groups" );
            stopwatch.Restart();

            rockContext.BulkInsert( groupsToInsert );

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Insert {groupsToInsert.Count} Groups" );
            stopwatch.Restart();

            // Get lookups for Group and Person so that we can populate the ParentGroups and GroupMembers
            var qryGroupTypeGroupLookup = new GroupService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue ).Select( a => new
            {
                Group = a,
                GroupForeignId = a.ForeignId.Value,
                GroupTypeId = a.GroupTypeId
            } );

            Dictionary<int, Dictionary<int, Group>> groupTypeGroupLookup = qryGroupTypeGroupLookup.GroupBy( a => a.GroupTypeId ).ToDictionary( k => k.Key, v => v.ToDictionary( k1 => k1.GroupForeignId, v1 => v1.Group ) );

            var personIdLookup = new PersonService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue )
                .Select( a => new { a.Id, ForeignId = a.ForeignId.Value } ).ToDictionary( k => k.ForeignId, v => v.Id );

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Prepare Lookups for Group Members " );
            stopwatch.Restart();

            // populate GroupMembers from the new groups that we added
            List<GroupMember> groupMembersToInsert = new List<GroupMember>();
            var groupMemberImports = newGroupImports.SelectMany( a => a.GroupMemberImports ).ToList();
            foreach ( var groupWithMembers in newGroupImports.Where( a => a.GroupMemberImports.Any() ) )
            {
                var groupTypeRoleLookup = GroupTypeCache.Read( groupWithMembers.GroupTypeId ).Roles.ToDictionary( k => k.Name, v => v.Id );
                foreach ( var groupMemberImport in groupWithMembers.GroupMemberImports )
                {
                    var groupMember = new GroupMember();
                    groupMember.GroupId = groupTypeGroupLookup[groupWithMembers.GroupTypeId][groupWithMembers.GroupForeignId].Id;
                    groupMember.GroupRoleId = groupTypeRoleLookup[groupMemberImport.RoleName];
                    groupMember.PersonId = personIdLookup[groupMemberImport.PersonForeignId];
                    groupMembersToInsert.Add( groupMember );
                }
            }

            rockContext.BulkInsert( groupMembersToInsert );

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Insert {groupMembersToInsert.Count} Group Members " );
            stopwatch.Restart();

            var groupsUpdated = false;
            var groupImportsWithParentGroup = newGroupImports.Where( a => a.ParentGroupForeignId.HasValue ).ToList();

            int groupTypeIdFamily = GroupTypeCache.GetFamilyGroupType().Id;
            var parentGroupLookup = qryGroupTypeGroupLookup.Where( a => a.GroupTypeId != groupTypeIdFamily ).Select( a => new
            {
                GroupId = a.Group.Id,
                a.GroupForeignId
            } ).ToDictionary( k => k.GroupForeignId, v => v.GroupId );

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

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Update {groupImportsWithParentGroup.Count} Group's Parent Group " );
            stopwatch.Restart();


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
                GroupTypeCache.Flush( groupTypeId );
            }

            stopwatchTotal.Stop();

            sbStats.AppendLine( $"[{stopwatchTotal.Elapsed.TotalMilliseconds}ms] Insert {newGroupImports.Count} Groups and {groupMembersToInsert.Count} Group Members" );
            var responseText = sbStats.ToString();

            return responseText;
        }

        /// <summary>
        /// Bulks the location import.
        /// </summary>
        /// <param name="locationImports">The location imports.</param>
        /// <returns></returns>
        public static string BulkLocationImport( List<LocationImport> locationImports )
        {
            Stopwatch stopwatchTotal = Stopwatch.StartNew();

            RockContext rockContext = new RockContext();

            var qryLocationsWithForeignIds = new LocationService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue );

            var locationsAlreadyExistForeignIdHash = new HashSet<int>( qryLocationsWithForeignIds.Select( a => a.ForeignId.Value ).ToList() );

            List<Location> locationsToInsert = new List<Location>();
            var newLocationImports = locationImports.Where( a => !locationsAlreadyExistForeignIdHash.Contains( a.LocationForeignId ) ).ToList();

            foreach ( var locationImport in newLocationImports )
            {
                var location = new Location();
                location.ForeignId = locationImport.LocationForeignId;
                location.LocationTypeValueId = locationImport.LocationTypeValueId;

                location.Street1 = locationImport.Street1.Truncate( 50 );
                location.Street2 = locationImport.Street2.Truncate( 50 );
                location.City = locationImport.City;
                location.County = locationImport.County;
                location.State = locationImport.State;
                location.Country = locationImport.Country;
                location.PostalCode = locationImport.PostalCode;

                location.Name = locationImport.Name.Truncate( 100 );
                location.IsActive = locationImport.IsActive;
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
            var responseText = $"[{stopwatchTotal.Elapsed.TotalMilliseconds}ms] Insert {newLocationImports.Count} Locations";

            return responseText;
        }

        /// <summary>
        /// Bulks the import.
        /// </summary>
        /// <param name="personImports">The person imports.</param>
        /// <returns></returns>
        public static string BulkPersonImport( List<PersonImport> personImports )
        {
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

            StringBuilder sbStats = new StringBuilder();

            Dictionary<int, Group> familiesLookup = groupService.Queryable().AsNoTracking().Where( a => a.GroupTypeId == familyGroupTypeId && a.ForeignId.HasValue )
                .ToList().ToDictionary( k => k.ForeignId.Value, v => v );

            Dictionary<int, Person> personLookup = qryAllPersons.AsNoTracking().Where( a => a.ForeignId.HasValue )
                .ToList().ToDictionary( k => k.ForeignId.Value, v => v );

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Get {familiesLookup.Count} family and {personLookup.Count} person lookups" );
            stopwatch.Restart();
            string defaultPhoneCountryCode = PhoneNumber.DefaultCountryCode();

            int nextNewFamilyForeignId = familiesLookup.Any() ? familiesLookup.Max( a => a.Key ) : 0;
            if ( personImports.Any() )
            {
                nextNewFamilyForeignId = Math.Max( nextNewFamilyForeignId, personImports.Where( a => a.FamilyForeignId.HasValue ).Max( a => a.FamilyForeignId.Value ) );
            }

            foreach ( var personImport in personImports )
            {
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
                    person.RecordTypeValueId = personImport.RecordTypeValueId;
                    person.RecordStatusValueId = personImport.RecordStatusValueId;
                    person.RecordStatusLastModifiedDateTime = personImport.RecordStatusLastModifiedDateTime;
                    person.RecordStatusReasonValueId = personImport.RecordStatusReasonValueId;
                    person.ConnectionStatusValueId = personImport.ConnectionStatusValueId;
                    person.ReviewReasonValueId = personImport.ReviewReasonValueId;
                    person.IsDeceased = personImport.IsDeceased;
                    person.TitleValueId = personImport.TitleValueId;
                    person.FirstName = personImport.FirstName.FixCase();
                    person.NickName = personImport.NickName.FixCase();

                    if ( string.IsNullOrWhiteSpace( person.NickName ) )
                    {
                        person.NickName = person.FirstName;
                    }

                    if ( string.IsNullOrWhiteSpace( person.FirstName ) )
                    {
                        person.FirstName = person.NickName;
                    }

                    person.LastName = personImport.LastName.FixCase();
                    person.SuffixValueId = personImport.SuffixValueId;
                    person.BirthDay = personImport.BirthDay;
                    person.BirthMonth = personImport.BirthMonth;
                    person.BirthYear = personImport.BirthYear;
                    person.Gender = ( Gender ) personImport.Gender;
                    person.MaritalStatusValueId = personImport.MaritalStatusValueId;
                    person.AnniversaryDate = personImport.AnniversaryDate;
                    person.GraduationYear = personImport.GraduationYear;
                    person.Email = personImport.Email;
                    person.IsEmailActive = personImport.IsEmailActive;
                    person.EmailNote = personImport.EmailNote;
                    person.EmailPreference = ( EmailPreference ) personImport.EmailPreference;
                    person.InactiveReasonNote = personImport.InactiveReasonNote;
                    person.ConnectionStatusValueId = personImport.ConnectionStatusValueId;
                    person.ForeignId = personImport.PersonForeignId;
                    personLookup.Add( personImport.PersonForeignId, person );
                }
            }

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Build Import Lists" );
            stopwatch.Restart();

            double buildImportListsMS = stopwatch.Elapsed.TotalMilliseconds;
            stopwatch.Restart();
            bool useSqlBulkCopy = true;
            List<int> insertedPersonForeignIds = new List<int>();

            // insert all the [Group] records
            var familiesToInsert = familiesLookup.Where( a => a.Value.Id == 0 ).Select( a => a.Value ).ToList();

            // insert all the [Person] records.
            // NOTE: we are only inserting the [Person] record, not the PersonAlias or GroupMember records yet
            var personsToInsert = personLookup.Where( a => a.Value.Id == 0 ).Select( a => a.Value ).ToList();

            rockContext.BulkInsert( familiesToInsert, useSqlBulkCopy );

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Insert {familiesToInsert.Count} Families" );
            stopwatch.Restart();

            // lookup GroupId from Group.ForeignId
            var familyIdLookup = groupService.Queryable().AsNoTracking().Where( a => a.GroupTypeId == familyGroupTypeId && a.ForeignId.HasValue )
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

            try
            {
                rockContext.BulkInsert( personsToInsert, useSqlBulkCopy );

                // TODO: Figure out a good way to handle database errors since SqlBulkCopy doesn't tell you which record failed.  Maybe do it like this where we catch the exception, rollback to a EF AddRange, and then report which record(s) had the problem
            }
            catch
            {
                try
                {
                    // do it the EF AddRange which is slower, but it will help us determine which record fails
                    rockContext.BulkInsert( personsToInsert, false );
                }
                catch ( System.Data.Entity.Infrastructure.DbUpdateException dex )
                {
                    // nbResults.Text = string.Empty;
                    foreach ( var entry in dex.Entries )
                    {
                        var errorRecord = entry.Entity as IEntity;
                    }
                }
            }

            insertedPersonForeignIds = personsToInsert.Select( a => a.ForeignId.Value ).ToList();

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Insert {personsToInsert.Count} Person records" );
            stopwatch.Restart();

            // Make sure everybody has a PersonAlias
            PersonAliasService personAliasService = new PersonAliasService( rockContext );
            var personAliasServiceQry = personAliasService.Queryable();
            List<PersonAlias> personAliasesToInsert = qryAllPersons.Where( p => p.ForeignId.HasValue && !p.Aliases.Any() && !personAliasServiceQry.Any( pa => pa.AliasPersonId == p.Id ) )
                .Select( x => new { x.Id, x.Guid } )
                .ToList()
                .Select( person => new PersonAlias { AliasPersonId = person.Id, AliasPersonGuid = person.Guid, PersonId = person.Id } ).ToList();

            rockContext.BulkInsert( personAliasesToInsert, useSqlBulkCopy );
            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Insert {personAliasesToInsert.Count} Person Aliases" );
            stopwatch.Restart();

            // get the person Ids along with the PersonImport and GroupMember record
            var personsIdsForPersonImport = from p in qryAllPersons.AsNoTracking().Where( a => a.ForeignId.HasValue ).Select( a => new { a.Id, a.ForeignId } ).ToList()
                                            join pi in personImports on p.ForeignId equals pi.PersonForeignId
                                            join f in groupService.Queryable().Where( a => a.ForeignId.HasValue ).Select( a => new { a.Id, a.ForeignId } ).ToList() on pi.FamilyForeignId equals f.ForeignId
                                            join gm in groupMemberService.Queryable( true ).Select( a => new { a.Id, a.PersonId } ) on p.Id equals gm.PersonId into gmj
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
                                                    GroupMemberStatus = GroupMemberStatus.Active
                                                };

            var groupMemberRecordsToInsertList = groupMemberRecordsToInsertQry.ToList();
            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Prepare {groupMemberRecordsToInsertList.Count()} Family Members for insert" );
            stopwatch.Restart();

            rockContext.BulkInsert( groupMemberRecordsToInsertList, useSqlBulkCopy );
            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Insert {groupMemberRecordsToInsertList.Count()} Family Members" );
            stopwatch.Restart();

            List<Location> locationsToInsert = new List<Location>();
            List<GroupLocation> groupLocationsToInsert = new List<GroupLocation>();

            var locationCreatedDateTimeStart = RockDateTime.Now;

            // NOTE: TODO To test the "Foriegn Key Issue" , don't narrow it down to just person records that we inserted
            foreach ( var familyRecord in personsIdsForPersonImport.GroupBy( a => a.FamilyId ) )
            {
                // get the distinct addresses for each family in our import
                var familyAddresses = familyRecord.Where( a => a.PersonImport?.Addresses != null ).SelectMany( a => a.PersonImport.Addresses ).DistinctBy( a => new { a.Street1, a.Street2, a.City, a.County, a.State, a.Country, a.PostalCode } );

                foreach ( var address in familyAddresses )
                {
                    GroupLocation groupLocation = new GroupLocation();
                    groupLocation.GroupLocationTypeValueId = address.GroupLocationTypeValueId;
                    groupLocation.GroupId = familyRecord.Key;
                    groupLocation.IsMailingLocation = address.IsMailingLocation;
                    groupLocation.IsMappedLocation = address.IsMappedLocation;

                    Location location = new Location();

                    location.Street1 = address.Street1;
                    location.Street2 = address.Street2;
                    location.City = address.City;
                    location.County = address.County;
                    location.State = address.State;
                    location.Country = address.Country;
                    location.PostalCode = address.PostalCode;
                    location.CreatedDateTime = locationCreatedDateTimeStart;
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

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Prepare {locationsToInsert.Count} Location and Group Location records" );
            stopwatch.Restart();
            rockContext.BulkInsert( locationsToInsert );

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Insert {locationsToInsert.Count} Location records" );
            stopwatch.Restart();

            var locationIdLookup = locationService.Queryable().Select( a => new { a.Id, a.Guid } ).ToList().ToDictionary( k => k.Guid, v => v.Id );
            foreach ( var groupLocation in groupLocationsToInsert )
            {
                groupLocation.LocationId = locationIdLookup[groupLocation.Location.Guid];
            }

            rockContext.BulkInsert( groupLocationsToInsert );

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Insert {groupLocationsToInsert.Count} Group Location records" );
            stopwatch.Restart();

            // PhoneNumbers
            List<PhoneNumber> phoneNumbersToInsert = new List<PhoneNumber>();

            foreach ( var personsIds in personsIdsForPersonImport )
            {
                foreach ( var phoneNumberImport in personsIds.PersonImport.PhoneNumbers )
                {
                    var phoneNumberToInsert = new PhoneNumber();

                    phoneNumberToInsert.PersonId = personsIds.PersonId;
                    phoneNumberToInsert.NumberTypeValueId = phoneNumberImport.NumberTypeValueId;
                    phoneNumberToInsert.CountryCode = defaultPhoneCountryCode;
                    phoneNumberToInsert.Number = PhoneNumber.CleanNumber( phoneNumberImport.Number );
                    phoneNumberToInsert.NumberFormatted = PhoneNumber.FormattedNumber( phoneNumberToInsert.CountryCode, phoneNumberToInsert.Number );
                    phoneNumberToInsert.Extension = phoneNumberImport.Extension;
                    phoneNumberToInsert.IsMessagingEnabled = phoneNumberImport.IsMessagingEnabled;
                    phoneNumberToInsert.IsUnlisted = phoneNumberImport.IsUnlisted;

                    phoneNumbersToInsert.Add( phoneNumberToInsert );
                }
            }

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Prepare {phoneNumbersToInsert.Count} Phone records" );
            stopwatch.Restart();

            rockContext.BulkInsert( phoneNumbersToInsert );

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Insert {phoneNumbersToInsert.Count} Phone records" );
            stopwatch.Restart();

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

                    attributeValuesToInsert.Add( attributeValue );
                }
            }

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Prepare {attributeValuesToInsert.Count} Attribute Values" );
            stopwatch.Restart();

            rockContext.BulkInsert( attributeValuesToInsert );

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Insert {attributeValuesToInsert.Count} Attribute Values" );
            stopwatch.Restart();

            stopwatchTotal.Stop();
            sbStats.AppendLine( $"[{stopwatchTotal.Elapsed.TotalMilliseconds}ms] Total Person" );

            // TODO: Rebuild all indexes on the effected tables to fix bogus "Foreign Key violation" issue
            var responseText = sbStats.ToString();

            return responseText;
        }

        /// <summary>
        /// Bulks the photo import.
        /// </summary>
        /// <param name="photoImports">The photo imports.</param>
        /// <returns></returns>
        public static string BulkPhotoImport( List<PhotoImport> photoImports )
        {
            Stopwatch stopwatchTotal = Stopwatch.StartNew();

            var rockContext = new RockContext();

            List<BinaryFile> binaryFilesToInsert = new List<BinaryFile>();
            Dictionary<PhotoImport.PhotoImportType, Dictionary<int, Guid>> photoTypeForeignIdBinaryFileGuidDictionary = new Dictionary<PhotoImport.PhotoImportType, Dictionary<int, Guid>>();
            photoTypeForeignIdBinaryFileGuidDictionary.Add( PhotoImport.PhotoImportType.Person, new Dictionary<int, Guid>() );
            photoTypeForeignIdBinaryFileGuidDictionary.Add( PhotoImport.PhotoImportType.Family, new Dictionary<int, Guid>() );
            var binaryFileService = new BinaryFileService( rockContext );

            HashSet<string> alreadyExists = new HashSet<string>( binaryFileService.Queryable().Where( a => a.ForeignKey != null && a.ForeignKey != "" ).Select( a => a.ForeignKey ).Distinct().ToList() );

            List<BinaryFileData> binaryFileDatasToInsert = new List<BinaryFileData>();

            var binaryFileType = new BinaryFileTypeService( rockContext ).Get( Rock.SystemGuid.BinaryFiletype.PERSON_IMAGE.AsGuid() );

            bool useBulkInsertForPhotos = false;

            foreach ( var photoImport in photoImports )
            {
                var binaryFileToInsert = new BinaryFile()
                {
                    FileName = photoImport.FileName,
                    MimeType = photoImport.MimeType,
                    BinaryFileTypeId = binaryFileType.Id,
                    Guid = Guid.NewGuid()
                };

                if ( !useBulkInsertForPhotos )
                {
                    binaryFileToInsert.ContentStream = new MemoryStream( Convert.FromBase64String( photoImport.PhotoData ) );
                }

                binaryFileToInsert.SetStorageEntityTypeId( binaryFileType.StorageEntityTypeId );

                if ( photoImport.PhotoType == PhotoImport.PhotoImportType.Person )
                {
                    binaryFileToInsert.ForeignKey = $"PersonForeignId_{photoImport.ForeignId}";
                }
                else if ( photoImport.PhotoType == PhotoImport.PhotoImportType.Family )
                {
                    binaryFileToInsert.ForeignKey = $"FamilyForeignId_{photoImport.ForeignId}";
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
                            Content = Convert.FromBase64String( photoImport.PhotoData )
                        };

                        binaryFileDatasToInsert.Add( binaryFileDataToInsert );
                    }
                }

                rockContext.BulkInsert( binaryFileDatasToInsert );
            }

            // Update Person PhotoIds to the photos that were just Imported
            rockContext.Database.ExecuteSqlCommand( @"UPDATE p
SET p.PhotoId = b.Id
FROM Person p
INNER JOIN BinaryFile b ON p.ForeignId = Replace(b.ForeignKey, 'PersonForeignId_', '')
WHERE b.ForeignKey LIKE 'PersonForeignId_%'
	AND p.PhotoId IS NULL" );

            // Update FamilyPhoto attribute for photos that were imported
            var familyGroupType = GroupTypeCache.GetFamilyGroupType();
            var familyPhotoAttribute = familyGroupType.Attributes.GetValueOrNull( "FamilyPhoto" );
            if ( familyPhotoAttribute != null )
            {
                rockContext.Database.ExecuteSqlCommand( $@"
DECLARE @AttributeId INT = {familyPhotoAttribute.Id}

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
INNER JOIN BinaryFile b ON g.ForeignId = Replace(b.ForeignKey, 'FamilyForeignId_', '')
WHERE g.GroupTypeId = {familyGroupType.Id}
	AND b.ForeignKey LIKE 'FamilyForeignId_%'
	AND g.Id NOT IN (
		SELECT EntityId
		FROM AttributeValue
		WHERE AttributeId = @AttributeId
		)
" );
            }

            stopwatchTotal.Stop();
            string importType = useBulkInsertForPhotos ? "Bulk" : string.Empty;

            var responseText = $"[{stopwatchTotal.Elapsed.TotalMilliseconds}ms] {importType} Insert {binaryFilesToInsert.Count} Binary File records";

            return responseText;
        }

        /// <summary>
        /// Bulks the schedule import.
        /// </summary>
        /// <param name="scheduleImports">The schedule imports.</param>
        /// <returns></returns>
        public static string BulkScheduleImport( List<ScheduleImport> scheduleImports )
        {
            Stopwatch stopwatchTotal = Stopwatch.StartNew();

            RockContext rockContext = new RockContext();

            var qrySchedulesWithForeignIds = new ScheduleService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue );

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

            foreach ( var scheduleImport in newScheduleImports )
            {
                var schedule = new Schedule();
                schedule.ForeignId = scheduleImport.ScheduleForeignId;
                schedule.CategoryId = scheduleCategory.Id;
                if ( scheduleImport.Name.Length > 50 )
                {
                    schedule.Name = scheduleImport.Name.Truncate( 50 );
                    schedule.Description = scheduleImport.Name;
                }
                else
                {
                    schedule.Name = scheduleImport.Name;
                }

                schedulesToInsert.Add( schedule );
            }

            rockContext.BulkInsert( schedulesToInsert );

            stopwatchTotal.Stop();
            var responseText = $"[{stopwatchTotal.Elapsed.TotalMilliseconds}ms] Bulk Insert {schedulesToInsert.Count} Schedules";

            return responseText;
        }
    }
}
