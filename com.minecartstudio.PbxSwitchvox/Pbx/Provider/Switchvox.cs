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
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Rock.Attribute;
using Rock.Pbx;
using Rock.Model;
using Rock.Data;
using Rock;
using System.Collections.Generic;
using Rock.Web.Cache;
using Rock.SystemKey;

namespace com.minecartstudio.PbxSwitchvox.Pbx.Provider
{
    /// <summary>
    /// Text PBX Provider
    /// </summary>
    /// <seealso cref="Rock.UniversalSearch.IndexComponent" />
    [Description( "Switchvox PBX Provider." )]
    [Export( typeof( PbxComponent ) )]
    [ExportMetadata( "ComponentName", "Switchvox" )]

    [TextField( "Server URL", "The URL of the PBX node (http://myserver:80)", true, key: "ServerUrl", order: 0 )]
    [TextField( "Username", "The username to use to connect with.", true, order: 1 )]
    [TextField( "Password", "The password to use to connect with.", true, order: 2 )]
    [CodeEditorField( "Phone Extension Template", "Lava template to use to get the extension from the internal phone. This helps translate the full phone number to just the internal extension (e.g. (602) 555-2345 to 2345). The phone number will be passed into the template as the variable 'PhoneNumber'.", Rock.Web.UI.Controls.CodeEditorMode.Lava, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 200, false, "{{ PhoneNumber | Right:4 }}", order: 4)]
    [CodeEditorField( "Origination Rules Template", "Lava template that will be applied to both the source and destination number when originating a call. Lava variables include {{ PhoneNumber }}.", Rock.Web.UI.Controls.CodeEditorMode.Lava, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 400, false, "{{ PhoneNumber }}", order: 4 )]
    [TextField( "Default Origination Extension", "When originating calls between two phones this is the default extension to use for outgoing call rules and call api settings should be used when placing the calls. This is only used if we don't know the person to use as the source of the call (when we're only passed two phone numbers).", true, order: 6 )]
    public class Switchvox : PbxComponent
    {
        /// <summary>
        /// Gets a value indicating whether [supports origination].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [supports origination]; otherwise, <c>false</c>.
        /// </value>
        public override bool SupportsOrigination
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Originates the specified from phone.
        /// </summary>
        /// <param name="fromPhone">From phone.</param>
        /// <param name="toPhone">To phone.</param>
        /// <param name="callerId">The caller identifier.</param>
        /// <returns></returns>
        public override bool Originate( string fromPhone, string toPhone, string callerId, out string message )
        {
            message = string.Empty;
            var accountId = GetAttributeValue( "DefaultOriginationExtension" );

            // run the numbers through the calling rules
            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "PhoneNumber", fromPhone );

            var ruleTemplate = GetAttributeValue( "OriginationRulesTemplate" );

            fromPhone = ruleTemplate.ResolveMergeFields( mergeFields ).Trim();

            mergeFields.AddOrReplace( "PhoneNumber", toPhone );
            toPhone = ruleTemplate.ResolveMergeFields( mergeFields ).Trim();

            // if this is an internal phone use it as the accountId
            if (fromPhone.Length < 7 )
            {
                accountId = fromPhone;
            }

            return OrginateSwitchvox( fromPhone, toPhone, accountId, callerId, out message );
        }

        /// <summary>
        /// Originates the specified from person.
        /// </summary>
        /// <param name="fromPerson">From person.</param>
        /// <param name="toPhone">To phone.</param>
        /// <param name="callerId">The caller identifier.</param>
        /// <returns></returns>
        public override bool Originate( Person fromPerson, string toPhone, string callerId, out string message )
        {
            message = string.Empty;

            var preferredOriginateCallSource = PersonService.GetUserPreference( fromPerson, UserPreference.ORIGINATE_CALL_SOURCE ).AsIntegerOrNull();
            if ( !preferredOriginateCallSource.HasValue )
            {
                preferredOriginateCallSource = this.GetAttributeValue( "InternalPhoneType" ).AsIntegerOrNull();
            }

            if ( !preferredOriginateCallSource.HasValue )
            {
                message = "Could not determine the phone to use to originate this call.";
                return false;
            }

            // get phone
            var phoneNumber = new PhoneNumberService( new RockContext() ).Queryable()
                                    .Where( p => p.PersonId == fromPerson.Id && p.NumberTypeValueId == preferredOriginateCallSource.Value )
                                    .FirstOrDefault();

            if (phoneNumber == null )
            {
                var phoneType = DefinedValueCache.Read( preferredOriginateCallSource.Value );
                message = string.Format("There is no {0} phone number configured.", phoneType.Value.ToLower());
                return false;
            }

            return Originate( phoneNumber.Number, toPhone, callerId, out message );
        }

        /// <summary>
        /// Downloads the CDR.
        /// </summary>
        /// <param name="startDateTime">The start date time.</param>
        /// <returns></returns>
        public override string DownloadCdr(DateTime? startDateTime = null)
        {
            var utilityRockContext = new RockContext();

            var recordsProcessed = 0;

            if (startDateTime == null )
            {
                startDateTime = new DateTime( 2000, 1, 1 );
            }

            try
            {
                XDocument result = XMLRequest( new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ),
                    new XElement( "request",
                        new XAttribute( "method", "switchvox.extensions.search" ),
                        new XElement( "parameters",
                            new XElement( "items_per_page", "1000" ),
                            new XElement( "extension_types",
                                new XElement( "extension_type", "sip" )
                            )
                        )
                    )
                ) );

                if ( result != null )
                {
                    XElement xAccountIDs = new XElement( "account_ids" );
                    foreach ( XElement xExtension in result.Descendants( "extension" ) )
                        xAccountIDs.Add(
                            new XElement( "account_id", xExtension.Attributes( "account_id" ).First().Value )
                        );

                    int pageNumber = 0;
                    int totalPages = 0;

                    XDocument cdrResult;

                    var interactionComponentId = Rock.Web.Cache.InteractionComponentCache.Read( SystemGuid.InteractionComponent.PBX_SWITCHVOX ).Id;
                    var relatedEntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( "Rock.Model.PersonAlias" ).Id;

                    // get list of current interaction foreign keys for the same timefrae to ensure we don't get duplicates
                    var startDate = startDateTime.Value.Date;
                    var currentInteractions = new InteractionService( utilityRockContext ).Queryable()
                                                    .Where( i =>
                                                        i.InteractionComponentId == interactionComponentId
                                                        && i.InteractionDateTime >= startDate )
                                                    .Select( i => i.ForeignKey ).ToList();

                    var internalPhoneTypeId = GetAttributeValue( "InternalPhoneType" ).AsInteger();

                    // get list of the iternal extensions an who is tied to them
                    var extensionList = new PhoneNumberService( utilityRockContext ).Queryable()
                                            .Where( p => p.NumberTypeValueId == internalPhoneTypeId )
                                            .Select( p => new ExtensionMap { Number = p.Number, Extension = p.Number, PersonAliasId = p.Person.Aliases.FirstOrDefault().Id } )
                                            .ToList();

                    // run lava template over the extension to translate the full number into an extension
                    var translationTemplate = this.GetAttributeValue( "PhoneExtensionTemplate" );
                    foreach ( var extension in extensionList )
                    {
                        var mergeFields = new Dictionary<string, object>();
                        mergeFields.Add( "PhoneNumber", extension.Number );
                        extension.Extension = translationTemplate.ResolveMergeFields( mergeFields );
                    }

                    do
                    {
                        cdrResult = GetCalls( ++pageNumber, xAccountIDs, startDateTime.Value );

                        if ( cdrResult != null )
                        {
                            totalPages = Int32.Parse( cdrResult.Root.Element( "result" ).Element( "calls" ).Attributes( "total_pages" ).First().Value );

                            foreach ( XElement xCall in cdrResult.Descendants( "call" ) )
                            {
                                string foreignKey = xCall.Attributes( "id" ).First().Value;

                                var cdrRecord = new CdrRecord();

                                // determine direction
                                switch ( xCall.Attributes( "origination" ).First().Value )
                                {
                                    case "incoming":
                                        cdrRecord.Direction = CdrDirection.Incoming;
                                        break;
                                    case "outgoing":
                                        cdrRecord.Direction = CdrDirection.Outgoing;
                                        break;
                                    default:
                                        cdrRecord.Direction = CdrDirection.Unknown;
                                        break;
                                }

                                cdrRecord.Source = xCall.Attributes( "from_number" ).First().Value;
                                cdrRecord.Destination = xCall.Attributes( "to_number" ).First().Value;
                                cdrRecord.CallerId = xCall.Attributes( "from_name" ).First().Value;
                                cdrRecord.StartDateTime = DateTime.Parse( xCall.Attributes( "start_time" ).First().Value );
                                cdrRecord.Duration = Int32.Parse( xCall.Attributes( "total_duration" ).First().Value );
                                cdrRecord.EndDateTime = cdrRecord.StartDateTime.Value.AddSeconds( cdrRecord.Duration );
                                cdrRecord.RecordKey = xCall.Attributes( "id" ).First().Value;

                                if ( !currentInteractions.Contains( cdrRecord.RecordKey ) )
                                {
                                    var rockContext = new RockContext();
                                    var interactionService = new InteractionService( rockContext );

                                    var cdrInteraction = new Interaction();
                                    interactionService.Add( cdrInteraction );

                                    cdrInteraction.Operation = cdrRecord.Direction.ToString();

                                    var personService = new PersonService( rockContext );

                                    int? sourcePersonAliasId = null;
                                    int? destinationPersonAliasId = null;

                                    // try to phone number to a person
                                    // first use the extension map (since one of them should be using an internal number)
                                    // then consider all numbers in the system
                                    sourcePersonAliasId = extensionList.Where( e => e.Number == cdrRecord.Source || e.Extension == cdrRecord.Source ).Select( e => e.PersonAliasId ).FirstOrDefault();
                                    if ( !sourcePersonAliasId.HasValue )
                                    {
                                        sourcePersonAliasId = personService.GetByPhonePartial( cdrRecord.Source ).FirstOrDefault()?.PrimaryAliasId;
                                    }

                                    destinationPersonAliasId = extensionList.Where( e => e.Number == cdrRecord.Destination || e.Extension == cdrRecord.Destination ).Select( e => e.PersonAliasId ).FirstOrDefault();
                                    if ( !destinationPersonAliasId.HasValue )
                                    {
                                        destinationPersonAliasId = personService.GetByPhonePartial( cdrRecord.Destination ).FirstOrDefault()?.PrimaryAliasId;
                                    }

                                    cdrInteraction.InteractionData = cdrRecord.ToJson();
                                    cdrInteraction.InteractionComponentId = interactionComponentId;

                                    // depending on the call direction set the entity id and related entity id
                                    // the entity id should always be the internal person
                                    if ( cdrRecord.Direction == CdrDirection.Incoming )
                                    {
                                        cdrInteraction.EntityId = destinationPersonAliasId;
                                        cdrInteraction.RelatedEntityId = sourcePersonAliasId;
                                    }
                                    else
                                    {
                                        cdrInteraction.EntityId = sourcePersonAliasId;
                                        cdrInteraction.RelatedEntityId = destinationPersonAliasId;
                                    }

                                    cdrInteraction.RelatedEntityTypeId = relatedEntityTypeId;
                                    cdrInteraction.ForeignKey = cdrRecord.RecordKey;

                                    if ( cdrRecord.StartDateTime.HasValue )
                                    {
                                        cdrInteraction.InteractionDateTime = cdrRecord.StartDateTime.Value;
                                        rockContext.SaveChanges();

                                        recordsProcessed++;
                                    }
                                }
                            }
                        }
                    } while ( cdrResult != null && totalPages > pageNumber );
                }

                return string.Format( "Switchvox: Processed {0} records.", recordsProcessed );
            }
            catch( Exception ex )
            {
                return string.Format( "Switchvox: Experienced and error: {0}.", ex.Message );
            }
        }

        #region Private Methods

        private bool OrginateSwitchvox( string sourcePhone, string destinationPhone, string accountId, string callerId, out string message )
        {
            message = string.Empty;

            XDocument result = XMLRequest( new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ),
                    new XElement( "request",
                        new XAttribute( "method", "switchvox.call" ),
                        new XElement( "parameters",
                            new XElement( "ignore_user_api_settings", "1" ),
                            new XElement( "dial_first", sourcePhone ),
                            new XElement( "dial_second", destinationPhone ),
                            new XElement( "dial_as_account_id", accountId ),
                            new XElement( "caller_id_name", callerId ),
                            new XElement( "timeout_second_call", "120" )
                        )
                    )
                ) );

            return result != null;
        }

        private XDocument GetCalls( int pageNumber, XElement xAccountIDs, DateTime startDateTime )
        {
            XDocument xRequest = new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ),
                new XElement( "request",
                    new XAttribute( "method", "switchvox.callLogs.search" ),
                    new XElement( "parameters",
                        new XElement( "items_per_page", "1000" ),
                        new XElement( "page_number", pageNumber.ToString() ),
                        new XElement( "start_date", string.Format( "{0:yyyy-MM-dd HH:mm:ss}", startDateTime ) ),
                        new XElement( "end_date", string.Format( "{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now ) ),
                        xAccountIDs
                    )
                )
            );
        
            return XMLRequest( xRequest );
        }

        private XDocument XMLRequest( XDocument request )
        {
            string stringData = request.Declaration.ToString() + request.ToString( SaveOptions.DisableFormatting );
            byte[] postData = ASCIIEncoding.ASCII.GetBytes( stringData );

            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                NetworkCredential nc = new NetworkCredential();
                nc.UserName = GetAttributeValue( "Username" );
                nc.Password = GetAttributeValue( "Password" );

                WebRequest webRequest = WebRequest.Create( string.Format( "{0}/xml", GetAttributeValue( "ServerUrl" ) ) );
                webRequest.ContentLength = postData.Length;
                webRequest.ContentType = "application/xml";
                webRequest.Method = "POST";
                webRequest.Credentials = nc;

                Stream requestStream = webRequest.GetRequestStream();
                requestStream.Write( postData, 0, postData.Length );
                requestStream.Close();

                HttpWebResponse response = ( HttpWebResponse ) webRequest.GetResponse();
                XDocument xResponse = GetResponse( response.GetResponseStream(), response.ContentType );

                XElement xError = xResponse.Descendants( "error" ).FirstOrDefault();
                if ( xError != null )
                    throw new Exception( string.Format( "Switchvox Error [{0}]: {1}",
                        xError.Attributes( "code" ).First().Value,
                        xError.Attributes( "message" ).First().Value ) );

                return xResponse;
            }
            catch ( WebException webException )
            {
                string sResponse = string.Empty;

                if ( webException.Response != null )
                    sResponse = GetResponse( webException.Response.GetResponseStream() );

                throw new Exception( webException.Status.ToString() + ": " +
                    webException.Message + sResponse.SanitizeHtml(), webException );
            }
        }

        private string GetResponse( Stream responseStream )
        {
            Stream receiveStream = responseStream;
            Encoding encode = System.Text.Encoding.GetEncoding( "utf-8" );
            StreamReader readStream = new StreamReader( receiveStream, encode );

            StringBuilder sb = new StringBuilder();
            Char[] read = new Char[8192];
            int count = 0;
            do
            {
                count = readStream.Read( read, 0, 8192 );
                String str = new String( read, 0, count );
                sb.Append( str );
            }
            while ( count > 0 );

            return sb.ToString();
        }

        private XDocument GetResponse( Stream responseStream, string contentType )
        {
            Stream receiveStream = responseStream;
            Encoding encode = System.Text.Encoding.GetEncoding( "utf-8" );
            StreamReader readStream = new StreamReader( receiveStream, encode );

            StringBuilder sb = new StringBuilder();
            Char[] read = new Char[8192];
            int count = 0;
            do
            {
                count = readStream.Read( read, 0, 8192 );
                String str = new String( read, 0, count );
                sb.Append( str );
            }
            while ( count > 0 );

            string HTMLResponse = sb.ToString();

            if ( contentType.ToLower().Contains( "xml" ) &&
                HTMLResponse.Trim().Length > 0 )
                return XDocument.Parse( HTMLResponse );
            else
                return null;
        }

        #endregion

        #region Related Objects
        /// <summary>
        /// Class to use for extension mapping
        /// </summary>
        private class ExtensionMap
        {
            /// <summary>
            /// Gets or sets the number.
            /// </summary>
            /// <value>
            /// The number.
            /// </value>
            public string Number { get; set; }

            /// <summary>
            /// Gets or sets the extension.
            /// </summary>
            /// <value>
            /// The extension.
            /// </value>
            public string Extension { get; set; }

            /// <summary>
            /// Gets or sets the person alias identifier.
            /// </summary>
            /// <value>
            /// The person alias identifier.
            /// </value>
            public int? PersonAliasId { get; set; }
        }
        #endregion

    }
}


