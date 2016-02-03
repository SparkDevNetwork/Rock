// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Linq;

using Rock.Data;
using Rock.Model;

using UAParser;

namespace Rock.Transactions
{
    /// <summary>
    /// Tracks when a page is viewed.
    /// </summary>
    public class PageViewTransaction : ITransaction
    {
        /// <summary>
        /// Gets or sets the Page Id.
        /// </summary>
        /// <value>
        /// Page Id.
        /// </value>
        public int? PageId { get; set; }

        /// <summary>
        /// Gets or sets the Site Id.
        /// </summary>
        /// <value>
        /// Site Id.
        /// </value>
        public int? SiteId { get; set; }

        /// <summary>
        /// Gets or sets the Person Id.
        /// </summary>
        /// <value>
        /// Person Id.
        /// </value>
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the DateTime the page was viewed.
        /// </summary>
        /// <value>
        /// Date Viewed.
        /// </value>
        public DateTime DateViewed { get; set; }

        /// <summary>
        /// Gets or sets the IP address that requested the page.
        /// </summary>
        /// <value>
        /// IP Address.
        /// </value>
        public string IPAddress { get; set; }

        /// <summary>
        /// Gets or sets the browser vendor and version.
        /// </summary>
        /// <value>
        /// IP Address.
        /// </value>
        public string UserAgent { get; set; }

        /// <summary>
        /// Gets or sets the session id.
        /// </summary>
        /// <value>
        /// Session Id.
        /// </value>
        public string SessionId { get; set; }

        /// <summary>
        /// Gets or sets the query string.
        /// </summary>
        /// <value>
        /// Query String.
        /// </value>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the page title.
        /// </summary>
        /// <value>
        /// Page Title.
        /// </value>
        public string PageTitle { get; set; }

        /// <summary>
        /// Execute method to write transaction to the database.
        /// </summary>
        public void Execute()
        {
            using ( var rockContext = new RockContext() )
            {
                PageViewService pageViewService = new PageViewService( rockContext );
                PageViewUserAgentService pageViewUserAgentService = new PageViewUserAgentService( rockContext );
                PageViewSessionService pageViewSessionService = new PageViewSessionService( rockContext );

                var userAgent = ( this.UserAgent ?? string.Empty ).Trim();

                // get user agent info
                var clientType = PageViewUserAgent.GetClientType( userAgent );

                Parser uaParser = Parser.GetDefault();
                ClientInfo client = uaParser.Parse( userAgent );
                var clientOs = client.OS.ToString();
                var clientBrowser = client.UserAgent.ToString();

                // lookup the pageViewUserAgent, and create it if it doesn't exist
                var pageViewUserAgent = pageViewUserAgentService.Queryable().Where( a => a.UserAgent == userAgent ).FirstOrDefault();
                if ( pageViewUserAgent == null)
                {
                    pageViewUserAgent = new PageViewUserAgent();
                    pageViewUserAgent.UserAgent = userAgent;
                    pageViewUserAgent.ClientType = clientType;
                    
                    pageViewUserAgent.OperatingSystem = clientOs;
                    pageViewUserAgent.Browser = clientBrowser;

                    pageViewUserAgentService.Add( pageViewUserAgent );
                    rockContext.SaveChanges();
                } else
                {
                    // check if the user agent properties need to be updated
                    if (clientType != pageViewUserAgent.ClientType || clientOs != pageViewUserAgent.OperatingSystem || clientBrowser != pageViewUserAgent.Browser )
                    {
                        pageViewUserAgent.ClientType = clientType;
                        pageViewUserAgent.OperatingSystem = clientOs;
                        pageViewUserAgent.Browser = clientBrowser;
                        rockContext.SaveChanges();
                    }
                }

                // lookup PageViewSession, and create it if it doesn't exist
                Guid sessionId = this.SessionId.AsGuid();
                int? pageViewSessionId = pageViewSessionService.Queryable().Where( a => a.PageViewUserAgentId == pageViewUserAgent.Id && a.SessionId == sessionId && a.IpAddress == this.IPAddress ).Select( a => (int?)a.Id ).FirstOrDefault();
                if ( !pageViewSessionId.HasValue )
                {
                    var pageViewSession = new PageViewSession();
                    pageViewSession.PageViewUserAgentId = pageViewUserAgent.Id;
                    pageViewSession.SessionId = sessionId;
                    pageViewSession.IpAddress = this.IPAddress;
                    pageViewSessionService.Add( pageViewSession );
                    rockContext.SaveChanges();
                    pageViewSessionId = pageViewSession.Id;
                }

                PageView pageView = new PageView();
                pageViewService.Add( pageView );

                pageView.PageId = this.PageId;
                pageView.SiteId = this.SiteId;
                pageView.Url = this.Url;
                pageView.DateTimeViewed = this.DateViewed;
                pageView.PersonAliasId = this.PersonAliasId;
                pageView.PageTitle = this.PageTitle;

                pageView.PageViewSessionId = pageViewSessionId.Value;

                rockContext.SaveChanges();
            }
        }
    }
}