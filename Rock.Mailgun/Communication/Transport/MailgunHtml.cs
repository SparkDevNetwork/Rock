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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;

namespace Rock.Mailgun.Communication.Transport
{
    public class MailGunHtml
    {
        public IRestResponse response { get; set; }
        private Uri BASE_URL = new Uri("https://api.mailgun.net/v3");
        private const string API_KEY = "apikeyguidhere";

        public void Send(MailGunEmail mailGunEmail)
        {
            RestRequest restRequest = new RestRequest();
            restRequest.Resource = "{messages}";
            restRequest.Method = Method.POST;

            foreach( var parameter in mailGunEmail.ToTupleList())
            {
                if( !string.IsNullOrWhiteSpace(parameter.Item2) )
                {
                    restRequest.AddParameter( parameter.Item1, parameter.Item2 );
                }
            }

            RestClient restClient = new RestClient
            {
                BaseUrl = BASE_URL,
                Authenticator = new HttpBasicAuthenticator( "api", API_KEY )
            };

            restClient.Execute( restRequest );
        }
    }

    // TODO: put this in a different file
    public class MailGunEmail
    {

        public string From { get; set; }
        public string To { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
        public string Html { get; set; }
        public string Attachemnt { get; set; }
        public string InLineImage { get; set; }
        public List<string> Tags { get; set; }
        public bool Dkim { get; set; }
        public DateTime DeliveryTime { get; set; }
        public bool TestMode { get; set; }
        public bool Tracking { get; set; }
        public TrackingClicksOptions TrackingClicks { get; set; }
        public bool TrackingOpens { get; set; }
        public bool RequireTls { get; set; }
        public bool SkipVerification { get; set; }
        //public List<string> CustomHeaders { get; set; }
        //public List<string> CustomJsonData { get; set; }

        public List<Tuple<string, string>> ToTupleList()
        {
            var list = new List<Tuple<string, string>>();
            list.Add( Tuple.Create( "from", From ) );
            list.Add( Tuple.Create( "to", To ) );
            list.Add( Tuple.Create( "cc", Cc ) );
            list.Add( Tuple.Create( "bcc", Bcc ) );
            list.Add( Tuple.Create( "subject", Subject ) );
            list.Add( Tuple.Create( "text", Text ) );
            list.Add( Tuple.Create( "html", Html ) );
            list.Add( Tuple.Create( "attachment", Attachemnt ) );
            list.Add( Tuple.Create( "inline", InLineImage ) );
            list.Add( Tuple.Create( "o:dkim", Dkim.ToYesNo() ) );
            list.Add( Tuple.Create( "o:deliverytime", DeliveryTime.ToUniversalTime().ToString() ) );
            list.Add( Tuple.Create( "o:testmode", TestMode.ToYesNo() ) );
            list.Add( Tuple.Create( "o:tracking", Tracking.ToYesNo() ) );
            list.Add( Tuple.Create( "o:tracking-clicks", TrackingClicks.ToString() ) );
            list.Add( Tuple.Create( "o:tracking-opens", TrackingOpens.ToYesNo() ) );
            list.Add( Tuple.Create( "o:require-tls", RequireTls.ToYesNo() ) );
            list.Add( Tuple.Create( "o:skip-verification", SkipVerification.ToYesNo() ) );

            foreach ( string tag in Tags )
            {
                list.Add( Tuple.Create( "o:tag", tag ) );
            }

            return list;
        }
    }

    public enum TrackingClicksOptions
    {
        yes,
        no,
        htmlonly
    }

}
