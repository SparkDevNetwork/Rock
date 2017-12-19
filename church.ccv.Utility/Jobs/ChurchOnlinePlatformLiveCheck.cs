using System;
using System.Net;
using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace church.ccv.Utility
{
    /// <summary>
    /// Checks Church Online Platform current event api if the event is live. Sets specified global attribute to true or false based on response.
    /// </summary>
    [TextField("Church Online Platform API Url", "The Church Online Platform API Url", true)]
    [TextField("Live Stream Global Attribute Key","The key of the global attribute used to store if live stream is active",true)]

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

            // Load Job Datamap for job settings
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            // Get the Global Attribute used for live stream
            AttributeService attributeService = new AttributeService( rockContext );
            Rock.Model.Attribute liveStream = attributeService.GetGlobalAttribute( dataMap.GetString( "LiveStreamGlobalAttributeKey" ) );

            if (liveStream != null )
            {
                // Get the value of liveStream attribute
                AttributeValueService attributeValueService = new AttributeValueService( rockContext );
                AttributeValue liveStreamValue = attributeValueService.GetByAttributeIdAndEntityId( liveStream.Id , null );

                if ( liveStream != null)
                {
                    // Get API Url from settings
                    string apiUrl = dataMap.GetString( "ChurchOnlinePlatformAPIUrl" );
                    
                    if ( apiUrl != null )
                    {
                        // Make API call to Church Online Platform
                        string apiResponse = null;

                        using ( WebClient webClient = new WebClient() )
                        {
                            try
                            {
                                apiResponse = webClient.DownloadString( apiUrl );
                            }
                            catch ( WebException ex )
                            {
                                //context.Result = String.Format( "API did not respond. Error code: {0}<br>Response: {1}", ex.Response, apiResponse );
                                throw new Exception( String.Format( "API did not respond. Error: {0} - {1}", ex.Status, ex.Message ) );
                            }
                        }

                        // If response contains "isLive":true then update global attribute to true, else set attribute to false
                        if ( apiResponse.Contains( "\"isLive\":true" ) )
                        {
                            liveStreamValue.Value = "True";
                        }
                        else
                        {
                            liveStreamValue.Value = "False";
                        }

                        // Save Changes flush global attributes cache
                        rockContext.SaveChanges();
                        GlobalAttributesCache.Flush();

                        // Display job execution result
                        context.Result = string.Format( "{0} Global Attribute was changed to {1}", liveStream.Name, liveStreamValue.Value );                     
                    }
                    else
                    {
                        // Display error
                        //context.Result = String.Format( "Error loading Church Online Platform API url" );
                        throw new Exception( "Error loading Church Online Platform API url" );
                    }
                }
                else
                {
                    // Display error
                    //context.Result = string.Format( "Error loading value of Global Attribute" );
                    throw new Exception( "Error loading value of Global Attribute" );
                }
            }
            else
            {
                // Display error
                //context.Result = string.Format( "Error loading Global Attribute" );
                throw new Exception( "Error Loading Global Attribute" );
            }
        }
    }
}
