using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Quartz;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace com.bemaservices.MailChimp.Jobs
{
    [DefinedValueField( "0ED80CA8-987E-4A00-8CA5-56D0A4BDD629", "Audiences", "The Audiences whose members should by synced. Leave blank if you would like all audiences synced.", false, true, false, "", "", 0, "Audiences" )]
    [IntegerField( "Days Back to Sync Updates For", "Limit the sync to only Mailchimp and Rock members updated within the last X days. Leave blank to sync all members", false, Key = "DaysToSyncUpdates" )]
    [DisallowConcurrentExecution]
    public class MailChimpSync : IJob
    {
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            var accounts = DefinedTypeCache.Get( MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_ACCOUNTS.AsGuid() );

            var audienceGuids = dataMap.GetString( "Audiences" ).SplitDelimitedValues().AsGuidList();
            var daysToSyncUpdates = dataMap.GetString( "DaysToSyncUpdates" ).AsIntegerOrNull();
            foreach ( var account in accounts.DefinedValues )
            {
                Utility.MailChimpApi mailChimpApi = new Utility.MailChimpApi( account );
                var mailChimpLists = mailChimpApi.GetMailChimpLists();

                foreach ( var list in mailChimpLists )
                {
                    if ( !audienceGuids.Any() || audienceGuids.Contains( list.Guid ) )
                    {
                        mailChimpApi.SyncMembers( DefinedValueCache.Get( list.Guid ), daysToSyncUpdates );
                    }
                }
            }
        }
    }
}