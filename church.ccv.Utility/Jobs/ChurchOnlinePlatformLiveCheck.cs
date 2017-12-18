using System;
using System.Net;
using Newtonsoft.Json.Linq;
using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace church.ccv.Utility
{
    /// <summary>
    /// Checks Church Online Platform current event api if the event is live. Sets CCVLive global attribute to true or false based on response.
    /// </summary>
    [TextField("Church Online Platform API Url", "The Church Online Platform API Url", true)]

    [DisallowConcurrentExecution]
    class ChurchOnlinePlatformLiveCheck : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public ChurchOnlinePlatformLiveCheck()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            RockContext rockContext = new RockContext();

            // Load Job Datamap
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            // Get the CCVLive Global Attribute
            AttributeService attributeService = new AttributeService( rockContext );
            Rock.Model.Attribute ccvLive = attributeService.GetGlobalAttribute( "CCVLive" );

            if (ccvLive != null )
            {
                // Get the attribute value of ccvLive
                AttributeValueService attributeValueService = new AttributeValueService( rockContext );
                AttributeValue ccvLiveValue = attributeValueService.GetByAttributeIdAndEntityId( ccvLive.Id, null );

                if ( ccvLiveValue != null)
                {
                    // Get API Url from settings
                    string apiUrl = dataMap.GetString( "ChurchOnlinePlatformAPIUrl" );
                    
                    // End job if no apiURL is loaded
                    if ( apiUrl == null )
                    {
                        context.Result = String.Format( "Error loading API url" );
                        return;
                    }

                    // Make API call to Church Online Platform
                    string response = null;
                  
                    using ( WebClient wc = new WebClient() )
                    {
                        try
                        {
                            response = wc.DownloadString( apiUrl );
                        }
                        catch ( WebException ex )
                        {
                            context.Result = String.Format( "API did not respond. Error code: {0}", ex.Response );
                            return;
                        }
                    }

                    // If response back was 200 and isLive:true then update global attribute to true, else set attribute to false
                    if ( response.Contains( "\"status\":200" ) && response.Contains( "\"isLive\":true" ) )
                    {
                        ccvLiveValue.Value = "True";
                    }
                    else
                    {
                        ccvLiveValue.Value = "False";
                    }
                    
                    // Save Changes
                    rockContext.SaveChanges();

                    // Display job execution status
                    context.Result = string.Format( "{0} Global Attribute was changed to {1}", ccvLive.Name, ccvLiveValue.Value );
                }
                else
                {
                    // Display error
                    context.Result = string.Format( "Error loading value of Global Attribute" );
                }
            }
            else
            {
                // Display error
                context.Result = string.Format( "Error loading Global Attribute" );
            }
        }
    }
}
