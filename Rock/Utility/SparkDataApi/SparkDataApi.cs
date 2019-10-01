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
using System.Net;
using System.Net.Http;
using System.Web.Http;

using RestSharp;

using Rock.Utility.Settings.SparkData;

namespace Rock.Utility.SparkDataApi
{
    /// <summary>
    ///API Calls to Spark Data server. 
    /// </summary>
    public class SparkDataApi
    {
        internal RestClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="SparkDataApi"/> class.
        /// </summary>
        public SparkDataApi()
        {
            _client = new RestClient( SparkDataConfig.SPARK_SERVER );
        }

        /// <summary>
        /// Spark Data account status
        /// </summary>
        public enum AccountStatus
        {
            /// <summary>
            /// The account is enabled and there is a valid credit card associated with the account
            /// </summary>
            EnabledCard,
            /// <summary>
            /// The account is enabled and there is no credit card associated with the account
            /// </summary>
            EnabledNoCard,
            /// <summary>
            /// The account is enabled and the credit card associated with the account have expired
            /// </summary>
            EnabledCardExpired,
            /// <summary>
            /// The account is enabled and the credit card associated with the account have no expire date
            /// </summary>
            EnabledCardNoExpirationDate,
            /// <summary>
            /// The account is disabled
            /// </summary>
            Disabled,
            /// <summary>
            /// The account have no name
            /// </summary>
            AccountNoName,
            /// <summary>
            /// The account was not found
            /// </summary>
            AccountNotFound,
            /// <summary>
            /// Invalid spark data key
            /// </summary>
            InvalidSparkDataKey
        }

        /// <summary>
        /// Checks if the account is valid on the Spark server.
        /// </summary>
        /// <param name="sparkDataApiKey">The spark data API key.</param>
        public AccountStatus CheckAccount( string sparkDataApiKey )
        {
            try
            {
                var request = new RestRequest( "api/SparkData/ValidateAccount", Method.GET )
                {
                    RequestFormat = DataFormat.Json
                };

                request.AddParameter( "sparkDataApiKey", sparkDataApiKey );
                var response = _client.Get<AccountStatus>( request );
                if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
                {
                    return response.Data;
                }
                else
                {
                    throw new HttpResponseException( new HttpResponseMessage( response.StatusCode )
                    {
                        Content = new StringContent( response.Content )
                    } );
                }
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Communication with Spark server failed: Could not authenticate Spark Data account. Possible cause is the Spark Server API server is down.", ex );
            }
        }

        /// <summary>
        /// Gets the price.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns>Price as string</returns>
        public string GetPrice( string service )
        {
            try
            {
                var request = new RestRequest( "api/SparkData/GetPrice", Method.GET )
                {
                    RequestFormat = DataFormat.Json
                };

                request.AddParameter( "service", service );
                IRestResponse response = _client.Execute( request );
                if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
                {
                    return response.Content.Trim('"');
                }
                else
                {
                    throw new HttpResponseException( new HttpResponseMessage( response.StatusCode )
                    {
                        Content = new StringContent( response.Content )
                    } );
                }
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Communication with Spark server failed: Could not get price of service. Possible cause is the Spark Server API server is down.", ex );
            }
        }

        #region NCOA

        /// <summary>
        /// Initiates the report on the Spark server.
        /// </summary>
        /// <param name="sparkDataApiKey">The spark data API key.</param>
        /// <param name="numberRecords">The number records.</param>
        /// <param name="personFullName">The person that initiated the request.</param>
        /// <returns>Return the organization name and transaction key.</returns>
        public GroupNameTransactionKey NcoaInitiateReport( string sparkDataApiKey, int? numberRecords, string personFullName = null )
        {
            try
            {
                string url;
                url = $"api/SparkData/Ncoa/InitiateReport/{sparkDataApiKey}/{numberRecords ?? 0}";

                var request = new RestRequest( url, Method.POST )
                {
                    RequestFormat = DataFormat.Json
                };

                request.AddHeader( "personFullName", personFullName.ToStringSafe() );

                var response = _client.Post<GroupNameTransactionKey>( request );
                if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
                {
                    return response.Data;
                }
                else
                {
                    throw new HttpResponseException( new HttpResponseMessage( response.StatusCode )
                    {
                        Content = new StringContent( response.Content )
                    } );
                }
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Communication with Spark server failed: Could not initiate the NCOA report. Possible cause is the Spark Server API server is down.", ex );
            }
        }

        /// <summary>
        /// Gets the credentials from the Spark server.
        /// </summary>
        /// <param name="sparkDataApiKey">The spark data API key.</param>
        /// <returns>The username and password</returns>
        public UsernamePassword NcoaGetCredentials( string sparkDataApiKey )
        {
            try
            {
                var request = new RestRequest( $"api/SparkData/Ncoa/GetCredentials/{sparkDataApiKey}", Method.GET )
                {
                    RequestFormat = DataFormat.Json
                };

                var response = _client.Get<UsernamePassword>( request );
                if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
                {
                    return response.Data;
                }
                else
                {
                    throw new HttpResponseException( new HttpResponseMessage( response.StatusCode )
                    {
                        Content = new StringContent( response.Content )
                    } );
                }
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Communication with Spark server failed: Could not get the NCOA credentials. Possible cause is the Spark Server API server is down.", ex );
            }
        }

        /// <summary>
        /// Sent Complete report message to Spark server.
        /// </summary>
        /// <param name="sparkDataApiKey">The spark data API key.</param>
        /// <param name="reportKey">The report key.</param>
        /// <param name="exportFileKey">The export file key.</param>
        /// <returns>Return true if successful</returns>
        public bool NcoaCompleteReport( string sparkDataApiKey, string reportKey, string exportFileKey )
        {
            try
            {
                var request = new RestRequest( $"api/SparkData/Ncoa/CompleteReport/{sparkDataApiKey}/{reportKey}/{exportFileKey}", Method.POST )
                {
                    RequestFormat = DataFormat.Json
                };

                IRestResponse response = _client.Execute( request );
                if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
                {
                    return response.Content.AsBoolean();
                }
                else
                {
                    throw new HttpResponseException( new HttpResponseMessage( response.StatusCode )
                    {
                        Content = new StringContent( response.Content )
                    } );
                }
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Communication with Spark server failed: Could not set Spark report to complete. Possible cause is the Spark Server API server is down.", ex );
            }
        }

        /// <summary>
        /// Inform Spark server that NCOA failed and the job will try again to get NCOA data.
        /// </summary>
        /// <param name="sparkDataApiKey">The spark data API key.</param>
        /// <param name="reportKey">The report key.</param>
        /// <returns>Return the organization name and transaction key.</returns>
        public GroupNameTransactionKey NcoaRetryReport( string sparkDataApiKey, string reportKey )
        {
            try
            {
                var request = new RestRequest( $"api/SparkData/Ncoa/RetryReport/{sparkDataApiKey}/{reportKey}", Method.POST )
                {
                    RequestFormat = DataFormat.Json
                };

                var response = _client.Post<GroupNameTransactionKey>( request );
                if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
                {
                    return response.Data;
                }
                else
                {
                    throw new HttpResponseException( new HttpResponseMessage( response.StatusCode )
                    {
                        Content = new StringContent( response.Content )
                    } );
                }
            }
            catch ( Exception ex )
            {
                throw new AggregateException( "Communication with Spark server failed: Could not set Spark initiate a retry. Possible cause is the Spark Server API server is down.", ex );
            }
        }

        #endregion
    }
}
