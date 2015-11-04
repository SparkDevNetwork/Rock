
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace com.centralaz.Prayer.Jobs
{
    [LinkedPage( "Request Update Page", "The page that the link directs the user to." )]
    [DisallowConcurrentExecution]
    public class SendRequestUpdateEmail : IJob
    {

        public SendRequestUpdateEmail()
        {
        }


        public void Execute( IJobExecutionContext context )
        {
            var rockContext = new RockContext();
            var prayerRequestService = new PrayerRequestService( rockContext );
            Guid? systemEmailGuid = com.centralaz.Prayer.SystemGuid.SystemEmail.PRAYER_REQUEST_UPDATE_TEMPLATE.AsGuidOrNull();

            JobDataMap dataMap = context.JobDetail.JobDataMap;
            Guid? updatePageGuid = dataMap.GetString( "RequestUpdatePage" ).AsGuidOrNull();
            if ( updatePageGuid != null )
            {
                var pageId = ( new PageService( new RockContext() ).Get( updatePageGuid.Value ) ).Id;
                if ( pageId != null )
                {
                    SystemEmailService emailService = new SystemEmailService( rockContext );

                    SystemEmail systemEmail = null;
                    if ( systemEmailGuid.HasValue )
                    {
                        systemEmail = emailService.Get( systemEmailGuid.Value );
                    }

                    if ( systemEmail == null )
                    {
                        // no email specified, so nothing to do
                        return;
                    }

                    // Get all prayer requests that expire today
                    var prayerRequestQry = prayerRequestService.Queryable().Where( pr => pr.ExpirationDate != null && pr.ExpirationDate.Value.Date == DateTime.Now.Date );

                    var recipients = new List<RecipientData>();

                    var prayerRequestList = prayerRequestQry.AsNoTracking().ToList();
                    foreach ( var prayerRequest in prayerRequestList )
                    {
                        var mergeFields = new Dictionary<string, object>();
                        mergeFields.Add( "PrayerRequest", prayerRequest );

                        Byte[] b = System.Text.Encoding.UTF8.GetBytes( prayerRequest.Email );
                        string encodedEmail = Convert.ToBase64String( b );

                        String url = VirtualPathUtility.ToAbsolute( String.Format( "~/page/{0}?Guid={1}&Key={1}", pageId, prayerRequest.Guid, encodedEmail ) );

                        mergeFields.Add( "MagicUrl", string.Format( "" ) );

                        var globalAttributeFields = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( null );
                        globalAttributeFields.ToList().ForEach( d => mergeFields.Add( d.Key, d.Value ) );

                        recipients.Add( new RecipientData( prayerRequest.Email, mergeFields ) );
                    }

                    var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "ExternalApplicationRoot" );
                    Email.Send( systemEmail.Guid, recipients, appRoot );
                }
            }
        }
    }
}
