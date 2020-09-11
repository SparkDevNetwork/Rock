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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

using Newtonsoft.Json;
using Quartz;
using RestSharp;
using RestSharp.Authenticators;

namespace Rock.Jobs
{
    /// <summary>
    /// Compares phone numbers in your Rock database to Twilio's daily deactivated numbers list.
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisplayName( "Process Twilio Deactivated Numbers" )]
    [Description( "Compares phone numbers in your Rock database to Twilio's daily deactivated numbers list." )]

    [CustomRadioListField( "Deactivated Number Action",
        Description = "What would you like to do with any matched numbers?",
        ListSource = "Disable SMS,Delete Number",
        IsRequired = true,
        DefaultValue = "Disable SMS",
        Order = 0,
        Key = AttributeKey.MatchAction )]
    [TextField( "Account SID",
        Description = "Your Twilio Account SID (find at https://www.twilio.com/user/account)",
        IsRequired = true,
        Order = 1,
        Key = AttributeKey.TwilioAccountSid )]
    [TextField( "Auth Token",
        Description = "Your Twilio Account Token",
        IsRequired = true,
        Order = 2,
        Key = AttributeKey.TwilioAuthToken )]
    [DisallowConcurrentExecution]
    public class ProcessTwilioDeactivatedNumbers : IJob
    {
        /// <summary>
        /// Keys for DataMap Field Attributes.
        /// </summary>
        private static class AttributeKey
        {
            public const string MatchAction = "MatchAction";
            public const string TwilioAccountSid = "TwilioAccountSid";
            public const string TwilioAuthToken = "TwilioAuthToken";
        }

        /// <summary>
        /// How many numbers to process at a time
        /// </summary>
        private const int BATCH_SIZE = 500;

        /// <summary>
        /// Maximum number of days back to try for results before giving up
        /// </summary>
        private const int MAX_DAYS_BACK = 3;

        /// <summary>
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public ProcessTwilioDeactivatedNumbers()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var accountSid = dataMap.GetString( AttributeKey.TwilioAccountSid );
            var authToken = dataMap.GetString( AttributeKey.TwilioAuthToken );
            var matchAction = dataMap.GetString( AttributeKey.MatchAction );

            context.UpdateLastStatusMessage( "Connecting to Twilio API." );

            var client = new RestClient
            {
                BaseUrl = new Uri( "https://messaging.twilio.com/v1" ),
                Authenticator = new HttpBasicAuthenticator( accountSid, authToken ),
                FollowRedirects = false
            };

            var daysBack = 1;
            var downloadLink = string.Empty;

            while ( daysBack <= MAX_DAYS_BACK && downloadLink.IsNullOrWhiteSpace() )
            {
                downloadLink = GetDownloadLink( client, RockDateTime.Today.AddDays( -1 * daysBack ), context.UpdateLastStatusMessage );
                daysBack++;
            }

            if ( downloadLink.IsNullOrWhiteSpace() )
            {
                throw new JobExecutionException( $"Unable to get a deactivated number list from Twilio API for any of the past {daysBack} days." );
            }

            context.UpdateLastStatusMessage( "Downloading the deactivations list." );

            var deactivatedNumbers = GetDeactivatedNumbers( downloadLink );

            context.UpdateLastStatusMessage( "Download complete, beginning processing." );

            var totalNumbers = deactivatedNumbers.Count();
            var numbersProcessed = 0;
            var numbersUpdated = 0;

            using ( RockContext rockContext = new RockContext() )
            {
                var phoneNumberService = new PhoneNumberService( rockContext );

                var numbersToCheck = deactivatedNumbers.Take( BATCH_SIZE );
                while ( numbersToCheck.Any() )
                {
                    context.UpdateLastStatusMessage( string.Format(
                        "Processing {0}-{1} of {2} deactivated numbers.",
                        numbersProcessed,
                        numbersProcessed + BATCH_SIZE,
                        totalNumbers ) );

                    ProcessPhoneNumbers( phoneNumberService, numbersToCheck, matchAction, out int updated );

                    numbersUpdated += updated;
                    numbersProcessed += BATCH_SIZE;
                    numbersToCheck = deactivatedNumbers.Skip( numbersProcessed ).Take( BATCH_SIZE );
                }

                rockContext.SaveChanges();
            }

            context.UpdateLastStatusMessage( "Processing complete." );

            context.Result = string.Format(
                "{0} {1} deactivated {2}",
                matchAction == "Delete Number" ? "Deleted" : "Disabled",
                numbersUpdated,
                "number".PluralizeIf( numbersUpdated != 1 ) );
        }

        /// <summary>
        /// Using the provided <see cref="RestClient"/>, attempt to query the Twilio API
        /// for a download link for the list of deactivated numbers on the provided <see cref="RockDateTime"/>.
        /// </summary>
        /// <param name="client">The rest client</param>
        /// <param name="date">The date to download</param>
        /// <param name="updateLastStatusMessage">A reference to context.UpdateStatusMessage so we can log progress</param>
        /// <returns>If successful in getting a link, returns the link. Null otherwise.</returns>
        private string GetDownloadLink(RestClient client, DateTime date, Action<string> updateLastStatusMessage)
        {
            string downloadLink;
            var request = new RestRequest( "/Deactivations", Method.GET );
            request.AddParameter( "Date", date.ToString( "yyyy-MM-dd" ), ParameterType.QueryString );

            var response = client.Execute( request );

            if ( response.ResponseStatus != ResponseStatus.Completed )
            {
                throw new JobExecutionException( $"Twilio API error: {response.ErrorMessage}" );
            }

            if ( response.StatusCode == System.Net.HttpStatusCode.TemporaryRedirect )
            {
                var downloadResponse = JsonConvert.DeserializeObject<TwilioDeactivationsResponse>( response.Content );
                downloadLink = downloadResponse.DownloadLink;
                updateLastStatusMessage( "Got download link from Twilio API." );
                return downloadLink;
            }
            else if ( response.StatusCode == System.Net.HttpStatusCode.BadRequest )
            {
                var errorResponse = JsonConvert.DeserializeObject<TwilioErrorResponse>( response.Content );

                if ( errorResponse.Code == 30113 )
                {
                    updateLastStatusMessage( $"Twilio doesn't have data for {date:MMM. d} yet." );
                    return null;
                }
            }

            updateLastStatusMessage( "Unexpected response from Twilio API." );
            throw new JobExecutionException( string.Format( "Unexpected response from Twilio API: {0} => {1}", client.BuildUri( request ), response.Content ) );
        }

        /// <summary>
        /// Download the list of deactivated phone numbers from the provided URL.
        /// </summary>
        /// <param name="downloadLink">Link to the file to download</param>
        /// <returns>A queryable with 1 item per line of the file</returns>
        private IQueryable<string> GetDeactivatedNumbers(string downloadLink)
        {
            var uri = new Uri( downloadLink );
            var origin = uri.GetLeftPart( UriPartial.Authority );
            var client = new RestClient( origin );
            var request = new RestRequest( uri.PathAndQuery );

            byte[] fileResponse = client.DownloadData( request );

            if ( fileResponse.IsNull() || fileResponse.Length == 0 )
            {
                throw new JobExecutionException( "Error downloading the deactivated numbers list." );
            }

            var parsedList = Encoding.ASCII.GetString( fileResponse ).Split( '\n' ).AsQueryable<string>();

            if ( parsedList.Count() == 1 )
            {
                throw new JobExecutionException( "Error parsing the deactivated numbers list." );
            }

            return parsedList;
        }

        /// <summary>
        /// Using the provided <see cref="PhoneNumberService"/>, find any matching numbers
        /// in <paramref name="numbersToCheck"/>, and apply <paramref name="matchAction"/> to them.
        /// </summary>
        /// <param name="phoneNumberService">The phone number service</param>
        /// <param name="numbersToCheck">The numbers to check for</param>
        /// <param name="matchAction">What to do with any matches</param>
        /// <param name="updated">Out param for how many numbers were updated</param>
        private void ProcessPhoneNumbers(PhoneNumberService phoneNumberService, IQueryable<string> numbersToCheck, string matchAction, out int updated)
        {
            updated = 0;
            var matchingNumbers = phoneNumberService.Queryable().Where( pn => numbersToCheck.Contains( pn.FullNumber ) );

            if ( matchingNumbers.Any() )
            {
                if ( matchAction == "Delete Number" )
                {
                    var numbersToUpdate = matchingNumbers.ToList();

                    // Delete any matching numbers
                    phoneNumberService.DeleteRange( numbersToUpdate );
                    updated += numbersToUpdate.Count();
                }
                else
                {
                    var numbersToUpdate = matchingNumbers.Where( pn => pn.IsMessagingEnabled ).ToList();

                    // Disable messaging on any matching numbers
                    numbersToUpdate.ForEach( pn => pn.IsMessagingEnabled = false );
                    updated += numbersToUpdate.Count();
                }
            }
        }

        private class TwilioDeactivationsResponse
        {
            // Twilio's API returns a signed link to download a .txt file from S3
            [JsonProperty( PropertyName = "redirect_to", Required = Required.Always )]
            public string DownloadLink { get; set; }
        }

        private class TwilioErrorResponse
        {
            [JsonProperty( PropertyName = "code", Required = Required.Always )]
            public int Code { get; set; }

            [JsonProperty( PropertyName = "status" )]
            public int Status { get; set; }

            [JsonProperty( PropertyName = "message" )]
            public string Message { get; set; }

            [JsonProperty( PropertyName = "more_info" )]
            public string MoreInfo { get; set; }
        }
    }
}