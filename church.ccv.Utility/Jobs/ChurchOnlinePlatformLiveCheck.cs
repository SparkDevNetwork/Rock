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

            string isStreamLive = "False";
            
            // Load Job Datamap for job settings
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            
            // Get global attribute
            AttributeService attributeService = new AttributeService( rockContext );
            Rock.Model.Attribute globalAttribute = attributeService.GetGlobalAttribute( dataMap.GetString( "LiveStreamGlobalAttributeKey" ) );

            if(globalAttribute != null )
            {
                // Get the value of the Global Attribute
                string globalAttributeValue = GlobalAttributesCache.Read().GetValue( globalAttribute.Key );

                if ( globalAttributeValue != null )
                {
                    // Get API Url
                    string apiUrl = dataMap.GetString( "ChurchOnlinePlatformAPIUrl" );

                    if ( apiUrl != null )
                    {
                        // Make API call
                        string apiResponse = null;

                        using ( WebClient webClient = new WebClient() )
                        {
                            try
                            {
                                apiResponse = webClient.DownloadString( apiUrl );
                            }
                            catch ( WebException ex )
                            {
                                // Call failed, process error - save global attribute
                                HandleError( globalAttribute.Key, isStreamLive, String.Format( "API did not respond. Error: {0} - {1}", ex.Status, ex.Message) );
                            }
                        }

                        // If apiResponse contains "isLive":true then set isStreamLive to true
                        if ( apiResponse.Contains( "\"isLive\":true" ) )
                        {
                            isStreamLive = "True";
                        }

                        // Save the attribute value
                        GlobalAttributesCache.Read().SetValue( globalAttribute.Key, isStreamLive, true );

                        // Update job execution result
                        context.Result = string.Format( "{0} Global Attribute was changed to {1}", globalAttribute.Name, isStreamLive );
                    }
                    else
                    {
                        // Process error - Save global attribute
                        HandleError( globalAttribute.Key, isStreamLive, "Error loading Church Online Platform API url" );
                    }
                }
                else
                {
                    // Process error - Save global attribute
                    HandleError( globalAttribute.Key, isStreamLive, "Error loading value of the Global Attribute" );
                }
            }
            else
            {
                // Throw error - dont save any changes
                throw new Exception( "Error getting Global Attribute" );
            }
        }
      
        private void HandleError( string attributeKey, string attributeValue, string message )
        {
            // Save Value of attribute
            GlobalAttributesCache.Read().SetValue( attributeKey, attributeValue, true );

            // Throw exception
            throw new Exception( message );
        }
    }
}
