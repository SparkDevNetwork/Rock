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

    [DisallowConcurrentExecution]
    public class MailChimpSync: IJob
    {
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            var accounts = DefinedTypeCache.Get( MailChimp.SystemGuid.SystemDefinedTypes.MAIL_CHIMP_ACCOUNTS.AsGuid() );

            foreach( var account in accounts.DefinedValues )
            {
                Utility.MailChimpApi mailChimpApi = new Utility.MailChimpApi( account );
                var mailChimpLists = mailChimpApi.GetMailChimpLists();

                foreach( var list in mailChimpLists )
                {
                    mailChimpApi.SyncMembers( DefinedValueCache.Get( list.Guid ) );
                }
            }
        }
    }
}