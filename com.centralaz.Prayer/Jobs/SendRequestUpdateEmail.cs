// <copyright>
// Copyright by Central Christian Church
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
using Rock.Web.UI;

namespace com.centralaz.Prayer.Jobs
{
    [LinkedPage( "Request Update Page", "The page that the link directs the user to.", true )]
    [SystemEmailField( "Request Expired Email", "The system email to send.", true )]
    [CategoryField( "Category", "The category of prayer request the email will be sent out to.", false, "Rock.Model.PrayerRequest", "", "", false, "4B2D88F5-6E45-4B4B-8776-11118C8E8269", "Category Selection", 2, "Category" )]
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
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            Guid? updatePageGuid = dataMap.GetString( "RequestUpdatePage" ).AsGuidOrNull();
            if ( updatePageGuid != null )
            {
                int? pageId = ( new PageService( new RockContext() ).Get( updatePageGuid.Value ) ).Id;
                if ( pageId != null )
                {
                    SystemEmailService emailService = new SystemEmailService( rockContext );

                    SystemEmail systemEmail = null;
                    Guid? systemEmailGuid = dataMap.GetString( "RequestExpiredEmail" ).AsGuidOrNull();

                    if ( systemEmailGuid != null )
                    {
                        systemEmail = emailService.Get( systemEmailGuid.Value );
                    }

                    if ( systemEmail == null )
                    {
                        // no email specified, so nothing to do
                        return;
                    }

                    Guid? categoryGuid = dataMap.GetString( "Category" ).AsGuidOrNull();
                    if ( categoryGuid != null )
                    {
                        var categoryId = CategoryCache.Read( categoryGuid.Value, rockContext ).Id;

                        // Get all prayer requests that expire today
                        var prayerRequestQry = prayerRequestService.GetByCategoryIds( categoryIds: new List<int> { categoryId }, onlyApproved: false, onlyUnexpired: false ).Where(
                            pr => pr.ExpirationDate != null &&
                                pr.ExpirationDate.Value.Day == DateTime.Now.Day &&
                                pr.ExpirationDate.Value.Month == DateTime.Now.Month &&
                                pr.ExpirationDate.Value.Year == DateTime.Now.Year
                                );

                        var recipients = new List<RecipientData>();

                        var prayerRequestList = prayerRequestQry.ToList();
                        foreach ( var prayerRequest in prayerRequestList )
                        {
                            if ( !String.IsNullOrWhiteSpace( prayerRequest.Email ) )
                            {
                                Byte[] b = System.Text.Encoding.UTF8.GetBytes( prayerRequest.Email );
                                string encodedEmail = Convert.ToBase64String( b );
                                String relativeUrl = String.Format( "page/{0}?Guid={1}&Key={2}", pageId, prayerRequest.Guid, encodedEmail );

                                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                                mergeFields.Add( "PrayerRequest", prayerRequest );
                                mergeFields.Add( "MagicUrl", relativeUrl );

                                recipients.Add( new RecipientData( prayerRequest.Email, mergeFields ) );
                            }
                        }

                        var appRoot = Rock.Web.Cache.GlobalAttributesCache.Read( rockContext ).GetValue( "ExternalApplicationRoot" );
                        Email.Send( systemEmail.Guid, recipients, appRoot );
                    }
                }
            }
        }
    }
}