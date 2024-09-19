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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Newtonsoft.Json;
using System.Net;
using RestSharp;
using Rock.Security;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;
using System.Data.Entity;

namespace org.crossingchurch.HubSpotIntegration.Jobs
{
    /// <summary>
    /// Job to supply hubspot contacts that already have rock_person_ids with other info.
    /// </summary>
    [DisplayName( "HubSpot Integration: Update Records" )]
    [Description( "This job only updates HubSpot contacts with a valid Rock ID with additional info from Rock." )]
    [DisallowConcurrentExecution]

    [TextField( "AttributeKey", "The attribute key for the global attribute that contains the HubSpot API Key. The attribute must be encrypted.", true, "HubSpotAPIKeyGlobal" )]
    [TextField( "Business Unit", "HubSpot Business Unit value", true, "0" )]
    [DefinedValueField( "Contribution Transaction Type",
        AllowMultiple = false,
        AllowAddingNewValues = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE
    )]
    [BooleanField( "Include TMBT", defaultValue: false )]
    [AccountField( "Financial Account", "If syncing a total amount given which fund should we sync from" )]
    [TextField( "ENewsGuid", "ENews Subscriber Data View Guid", true, "e4f1db79-63c7-41ca-ab45-6ed6b16feb0e" )]
    [TextField( "DefinedTypeId", "Environment Defined Type Id", true, "527" )]
    public class HubSpotIntegrationPatching : RockJob
    {
        private string Key { get; set; }
        private List<HubSpotContactResult> Contacts { get; set; }
        private int RequestCount { get; set; }

        /// <summary>
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public HubSpotIntegrationPatching()
        {
        }

        /// <summary>
        /// Job that updates contacts in HubSpot with info from Rock.
        /// </summary>
        public override void Execute()
        {
            // Get HubSpot API Token
            string attrKey = GetAttributeValue( "AttributeKey" );
            string attrValue = GlobalAttributesCache.Get().GetValue( attrKey );
            Key = Encryption.DecryptString( attrValue );
            if ( string.IsNullOrEmpty( Key ) )
            {
                throw new ArgumentException( "Parameter 'Key' was Empty." );
            }

            var current_id = 0;

            PersonService personService = new PersonService( new RockContext() );

            // Get HubSpot Properties in Rock Information Group
            // This will allow us to add properties temporarily to the sync and then not continue to have them forever
            var propClient = new RestClient( "https://api.hubapi.com/crm/v3/properties/contacts?properties=name,label,createdUserId,groupName,options,fieldType" );
            propClient.Timeout = -1;
            var propRequest = new RestRequest( Method.GET );
            propRequest.AddHeader( "Authorization", $"Bearer {Key}" );
            IRestResponse propResponse = propClient.Execute( propRequest );
            // WriteToLog( "Is propResponse Null? : " + propResponse.ToString() );
            var props = new List<HubSpotProperty>();
            var tmbtProps = new List<HubSpotProperty>();
            var propsQry = JsonConvert.DeserializeObject<HubSpotPropertyQueryResult>( propResponse.Content );
            props = propsQry.results;
            // WriteToLog( "Is propsQry Null? : " + propsQry.IsNull().ToString() );
            // WriteToLog( "Is propsQry.results Null? : " + propsQry.results.IsNull().ToString() );
            // WriteToLog( "Is props Null? : " + props.IsNull().ToString() );

            // Filter to props in Rock Information Group (and Contact information group) 
            props = props.Where( p => p.groupName == "rock_information" || p.groupName == "contactinformation" ).ToList();

            // Save a list of the ones that are Rock attributes
            using ( RockContext context = new RockContext() )
            {
                Guid transactionTypeGuid = GetAttributeValue( "ContributionTransactionType" ).AsGuid();
                var transactionTypeDefinedValue = new DefinedValueService( context ).Get( transactionTypeGuid );
                int transactionTypeValueId = transactionTypeDefinedValue.Id;

                // Create a List of all contacts from HubSpot
                Contacts = new List<HubSpotContactResult>();
                RequestCount = 0;

                // Get all HubSpot Contacts that have a Rock Person Id
                GetContacts( "https://api.hubapi.com/crm/v3/objects/contacts?limit=100&properties=rock_person_id" );
                // Debug.WriteLine( "Contacts returned: " + Contacts.Count() );
                // WriteToLog( string.Format( "Total Contacts to Match: {0}", Contacts.Count() ) );
            
                PersonAliasService pa_svc = new PersonAliasService( context );
                FinancialTransactionService ft_svc = new FinancialTransactionService( context );
                AttributeValueService av_svc = new AttributeValueService( context );
                var dataViewService = new DataViewService( context );
                var enewsDV = dataViewService.Get( GetAttributeValue( "ENewsGuid" ) ); // ENews DataView Id: 2882, Guid: e4f1db79-63c7-41ca-ab45-6ed6b16feb0e
                if ( enewsDV == null )
                {
                    throw new ArgumentException( "Could not find eNews Dataview." );
                }

                // ENews from DataView To List
                var qry = enewsDV.GetQuery();
                var eNewsData = qry.ToList();
                HashSet<string> eNewsEmails = new HashSet<string>();
                // string eNewsEmailList = "";
                foreach ( var row in eNewsData )
                {
                    string colVal = string.IsNullOrEmpty( row.GetPropertyValue( "Email" ).ToString() ) == false ? row.GetPropertyValue( "Email" ).ToString().ToLower() : "";
                    if ( colVal != "" )
                    {
                        eNewsEmails.Add( colVal );
                    }
                }
                // Debug.WriteLine( string.Format( "{0} Emails on ENews List.",eNewsData.Count ) );

                HashSet<string> HubSpotPersonEmails = new HashSet<string>();
                foreach ( var contact in Contacts )
                {
                    if ( string.IsNullOrEmpty( contact.properties.email ) == false )
                    {
                        HubSpotPersonEmails.Add( contact.properties.email.ToString().ToLower() );
                    }
                }

                // WriteToLog( string.Format( "Total Contacts: {0}", contacts.Count() ) );
                for ( var i = 0; i < Contacts.Count(); i++ )
                {
                    // Stopwatch watch = new Stopwatch();
                    // watch.Start();
                    Person person = personService.Get( Contacts[i].properties.rock_person_id );

                    // For Testing
                    // WriteToLog( string.Format( "{1}i: {0}{1}", i, Environment.NewLine ) );
                    // WriteToLog( string.Format( "    After SQL: {0}{1}", watch.ElapsedMilliseconds, Environment.NewLine ) );

                    // If person is null, that means that we have a person in HubSpot w/ a personId that no longer exists in rock
                    // This implies that a merge occurred and we need to look at the alias table to figure out what the new Id is and update it
                    // After updating the ID, we need to find the person object and handle patching

                    // Setup for patching
                    // Look up hubspot defined type
                    DefinedTypeService definedTypeService = new DefinedTypeService( context );
                    DefinedType HubSpotDefinedType = definedTypeService.Get( GetAttributeValue( "DefinedTypeId" ) ); // as of 7/8/24 Dev is 528, Train is 527
                    AttributeValueService attributeValueService = new AttributeValueService( context );
                    AttributeService attributeService = new AttributeService( context );

                    // Schedule HubSpot update if 1:1 match
                    if ( person != null )
                    {
                        current_id = person.Id;
                        var url = $"https://api.hubapi.com/crm/v3/objects/contacts/{Contacts[i].id}";
                        // Debug.WriteLine( "Contact Count: " + i+1 + " of " + Contacts.Count() );
                        // Debug.WriteLine( "URL: " + url );
                        var properties = new Dictionary<string, string>();

                        foreach ( DefinedValue HubSpotSyncDefinedValue in HubSpotDefinedType.DefinedValues )
                        {
                            HubSpotSyncDefinedValue.LoadAttributes();
                            Dictionary<string, AttributeValueCache> dvAttributes = HubSpotSyncDefinedValue.AttributeValues;
                            string propertyOrAttribute;
                            string HubSpotKey;
                            string type;
                            string key;
                            var value = "";
                            try
                            {
                                propertyOrAttribute = dvAttributes.GetValueOrNull( "IsPropertyOrAttribute" ).Value;
                                HubSpotKey = dvAttributes.GetValueOrNull( "HubSpotAttributeKey" ).Value;
                                type = dvAttributes.GetValueOrNull( "Type" ).Value;
                                key = HubSpotSyncDefinedValue.Value;
                            }
                            catch
                            {
                                throw new Exception( "Missing Defined Value Property or Attribute." );
                            }

                            // Get Person property or attribute
                            if ( propertyOrAttribute == "Property" ) // is Person property
                            {
                                value = person.GetPropertyValue( key )?.ToString() ?? "";
                            }
                            else // is Person attribute
                            {
                                var attributeQry = attributeService.Queryable().Where( a => a.EntityTypeId == 15 && a.Key == key ).AsNoTracking();
                                // Debug.WriteLine( "Type/Key: " + " " + type + " / " + HubSpotKey );
                                var aVal = attributeValueService.Queryable().Where( av => av.EntityId == current_id )
                                    .Join( attributeQry, av => av.AttributeId,
                                    a => a.Id, ( av, a ) => av ).Select( av => av.Value )
                                    .AsNoTracking().FirstOrDefault();
                                value = aVal ?? "";
                            }

                            // Set date values to HubSpot required format
                            if ( type == "Date" )
                            {
                                if ( string.IsNullOrEmpty( value ) == false )
                                {
                                    if ( value.AsDateTime() != null )
                                    {
                                        value = ConvertDate( value.AsDateTime() );
                                    }
                                }
                            }

                            // Patch it!
                            // Debug.WriteLine( "Patching: " + HubSpotKey + " " + value );
                            properties[HubSpotKey] = value;

                        }
                    
                        // Handle name changes
                        properties["firstname"] = person.NickName;
                        properties["lastname"] = person.LastName;

                        // Handle Phone
                        PhoneNumber mobile = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == 12 );
                        if ( mobile != null && mobile.IsUnlisted == false && mobile.IsMessagingEnabled )
                        {
                            properties["phone"] = mobile.NumberFormatted;
                        }
                        else
                        {
                            properties["phone"] = "";
                        }

                        // Handle Email
                        bool verifiedEmail = false;
                        var personEmail = string.IsNullOrEmpty( person.Email ) == false ? person.Email.ToLower() : "";
                        if ( personEmail != "" )
                        {
                            try
                            {
                                var addr = new System.Net.Mail.MailAddress( personEmail );
                                verifiedEmail = addr.Address == personEmail.Trim();
                            }
                            catch
                            {
                                verifiedEmail = false;
                            }
                        }

                        if ( person.CanReceiveEmail( true ) && HubSpotPersonEmails.Contains( personEmail ) == false && verifiedEmail == true )
                        {
                            properties["email"] = personEmail;
                        }
                        else
                        {
                            properties["email"] = "";
                        }

                        // eNews Subscriber true or false
                        string eNewsSub = ( person.Email != "" && person.Email != null && eNewsEmails.Contains( person.Email.ToLower() ) ) ? "true" : "false";
                        properties["enews_subscriber"] = eNewsSub;
                        // Debug.WriteLine( "Patching: eNews: " + eNewsSub + " | " + person.Email );

                        // Discpleship Step Path step completed date gathering
                        Dictionary<int, string> stepsPath = new Dictionary<int, string>()
                        {
                            { 26, "baptism" },
                            { 27, "life_groups" },
                            { 29, "first_time_serving" }
                        };
                        foreach ( KeyValuePair<int, string> kvp in stepsPath )
                        {
                            var stepService = new StepService( context );
                            var stepResult = stepService.Queryable()
                                .Where( s => s.StepTypeId == kvp.Key && s.PersonAlias.PersonId == person.Id )
                                .Select( s => s.CompletedDateTime )
                                .OrderBy( s => s )
                                .FirstOrDefault();

                            if ( stepResult != null )
                            {
                                properties[kvp.Value] = ConvertDate( stepResult );
                            }
                            else
                            {
                                properties[kvp.Value] = "";
                            }
                        }

                        try
                        {
                            // Update the Contact in HubSpot
                            MakeRequest( current_id, url, properties, 0 );
                            // WriteToLog( string.Format( "    After Request: {0}", watch.ElapsedMilliseconds ) );
                        }
                        catch ( Exception e )
                        {
                            ExceptionLogService.LogException( new Exception( $"HubSpot Sync Error{Environment.NewLine}{e}{Environment.NewLine}Current Id: {current_id}{Environment.NewLine}Exception from Job:{Environment.NewLine}{e.Message}{Environment.NewLine} - Record Count:{i}" ) );
                        }
                    }
                    // WriteToLog( string.Format( "    End of iteration: {0}", watch.ElapsedMilliseconds ) );
                    // watch.Stop();
                    // WriteToLog( string.Format( i + " of " + Contacts.Count() + " Contacts Patched." ) );
                    // WriteToLog( string.Format( "Last Person: " + person.FullName + "( " + person.Id + " )" ) );

                    // Output Job Status
                    var resultMsg = ( $"{ i } of { Contacts.Count()+1 } Contacts Patched." );
                    UpdateLastStatusMessage( resultMsg.ToString() );
                }
            }
        }
        private void MakeRequest( int current_id, string url, Dictionary<string, string> properties, int attempt )
        {
            // Update the HubSpot Contact
            try
            {
                // For Testing Write to Log File
                // WriteToLog( string.Format( "{0}     ID: {1}{2}PROPS:{2}{3}", RockDateTime.Now.ToString( "HH:mm:ss.ffffff" ), current_id, Environment.NewLine, JsonConvert.SerializeObject( properties ) ) );

                var client = new RestClient( url );
                client.Timeout = -1;
                var request = new RestRequest( Method.PATCH );
                request.AddHeader( "accept", "application/json" );
                request.AddHeader( "content-type", "application/json" );
                request.AddHeader( "Authorization", $"Bearer {Key}" );
                request.AddParameter( "application/json", $"{{\"properties\": {{ {String.Join( ",", properties.Select( p => $"\"{p.Key}\": \"{p.Value}\"" ) )} }} }}", ParameterType.RequestBody );
                IRestResponse response = client.Execute( request );
                if ( ( int )response.StatusCode == 429 )
                {
                    if ( attempt < 3 )
                    {
                        Thread.Sleep( 9000 );
                        MakeRequest( current_id, url, properties, attempt + 1 );
                    }
                }
                if ( response.StatusCode != HttpStatusCode.OK )
                {
                    throw new Exception( response.Content );
                }
            }
            catch ( Exception e )
            {
                var json = $"{{\"properties\": {JsonConvert.SerializeObject( properties )} }}";
                ExceptionLogService.LogException( new Exception( $"HubSpot Sync Error{Environment.NewLine}{e}{Environment.NewLine}Current Id: {current_id}{Environment.NewLine}Exception from Request:{Environment.NewLine}{e.Message}{Environment.NewLine}Request:{Environment.NewLine}{json}{Environment.NewLine}" ) );
            }
        }
        private void WriteToLog( string message )
        {
            string logFile = System.Web.Hosting.HostingEnvironment.MapPath( "~/App_Data/Logs/HubSpotPatchLog.txt" );
            using ( System.IO.FileStream fs = new System.IO.FileStream( logFile, System.IO.FileMode.Append, System.IO.FileAccess.Write ) )
            {
                using ( System.IO.StreamWriter sw = new System.IO.StreamWriter( fs ) )
                {
                    sw.WriteLine( message );
                }
            }
        }
        private void GetContacts( string url )
        {
            RequestCount++;
            var contactClient = new RestClient( url );
            contactClient.Timeout = -1;
            var contactRequest = new RestRequest( Method.GET );
            contactRequest.AddHeader( "Authorization", $"Bearer {Key}" );
            IRestResponse contactResponse = contactClient.Execute( contactRequest );
            var contactResults = JsonConvert.DeserializeObject<HubSpotContactQueryResult>( contactResponse.Content );
            Contacts.AddRange( contactResults.results.Where( c => c.properties.rock_person_id != null && c.properties.rock_person_id != "" ).ToList() );
            if ( contactResults.paging != null && contactResults.paging.next != null && !String.IsNullOrEmpty( contactResults.paging.next.link ) && RequestCount < 2000 )
            {
                GetContacts( contactResults.paging.next.link );
            }
        }
        private string ConvertDate( DateTime? date )
        {
            if ( date.HasValue )
            {
                DateTime today = RockDateTime.Now;
                if ( today.Year - date.Value.Year < 1000 && today.Year - date.Value.Year > -1000 )
                {
                    var d = date.Value.Date.Subtract( new DateTime( 1970, 1, 1 ) ).TotalSeconds * 1000;
                    return d.ToString();
                }
            }
            return "";
        }

        public class HubSpotContactProperties
        {
            public string createdate { get; set; }
            public string email { get; set; }
            public string firstname { get; set; }
            public string lastname { get; set; }
            public string lastmodifieddate { get; set; }
            public string _phone { get; set; }
            public string phone
            {
                get
                {
                    return String.IsNullOrEmpty( _phone ) == false ? _phone.Replace( " ", "" ).Replace( "(", "" ).Replace( ")", "" ).Replace( "-", "" ) : "";
                }
                set
                {
                    _phone = value;
                }
            }
            public string rock_person_id { get; set; }

            public string rock_record_status { get; set; }

            public string has_potential_rock_match { get; set; }

            public override string ToString()
            {
                return "CreatedDate: " + createdate + "; LastModifiedDate: " + lastmodifieddate + "; First: " + firstname + "; Last:" + lastname + "; Email: " + email + "; Phone: " + phone + "; RockId: " + rock_person_id + "; RockStatus: " + rock_record_status + "; RockPotentialMatch: " + has_potential_rock_match;
            }
        }

        [DebuggerDisplay( "Label: {label}, FieldType: {fieldType}" )]
        public class HubSpotProperty
        {
            public string name { get; set; }
            public string label { get; set; }
            public string fieldType { get; set; }
            public string groupName { get; set; }
        }

        public class HubSpotPropertyQueryResult
        {
            public List<HubSpotProperty> results { get; set; }
        }

        [DebuggerDisplay( "Id: {id}, Email: {properties.email}" )]
        public class HubSpotContactResult
        {
            public string id { get; set; }
            public HubSpotContactProperties properties { get; set; }
            public string archived { get; set; }

            public override string ToString()
            {
                return "Id: " + id + "; Properties: " + properties.ToString();
            }
        }

        public class HubSpotResultPaging
        {
            public HubSpotResultPagingNext next { get; set; }
        }

        public class HubSpotResultPagingNext
        {
            public string link { get; set; }
        }

        public class HubSpotContactQueryResult
        {
            public List<HubSpotContactResult> results { get; set; }
            public HubSpotResultPaging paging { get; set; }

            public override string ToString()
            {
                string ret = "";
                foreach ( var item in results )
                {
                    ret += "; " + item.ToString();
                }
                return ret;

            }
        }
    }
}
