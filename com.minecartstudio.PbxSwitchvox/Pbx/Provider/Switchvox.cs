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
    [CustomDropdownListField("Internal Phone Type", "The phone type to that is connected to the PBX.", @"  SELECT 
	dv.[Value] AS [Text],
	dv.[Id] AS [Value]
FROM 
	[DefinedValue] dv
	INNER JOIN [DefinedType] dt ON dt.[Id] = dv.[DefinedTypeId]
WHERE dt.[Guid] = '8345DD45-73C6-4F5E-BEBD-B77FC83F18FD'", true, order: 3)]
    [CodeEditorField("Phone Extenstion Template", "Lava template to use to get the extension from the internal phone. This helps translate the full phone number to just the internal extension (e.g. (602) 555-2345 to 2345). The phone number will be passed into the template as the variable 'PhoneNumber'.", Rock.Web.UI.Controls.CodeEditorMode.Lava, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 200, false, "{{ PhoneNumber | Right:4 }}", order: 4)]
    public class Switchvox : PbxComponent
    {
        public override bool Originate( string fromPhone, string toPhone, string callerId )
        {
            return true;
        }

        public override int DownloadCdr(DateTime? startDateTime = null)
        {
            var recordsProcessed = 0;

            if (startDateTime == null )
            {
                startDateTime = new DateTime( 2000, 1, 1 );
            }

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

                var interactionComponentId = Rock.Web.Cache.InteractionComponentCache.Read(SystemGuid.InteractionComponent.PBX_SWITCHVOX).Id;
                var relatedEntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( "Rock.Model.PersonAlias" ).Id;

                // get list of current interaction foreign keys for the same timefrae to ensure we don't get duplicates
                var startDate = startDateTime.Value.Date;
                var currentInteractions = new InteractionService( new RockContext() ).Queryable()
                                                .Where( i => 
                                                    i.InteractionComponentId == interactionComponentId 
                                                    && i.InteractionDateTime >= startDate )
                                                .Select( i => i.ForeignKey ).ToList();

                do
                {
                    cdrResult = GetCalls( ++pageNumber, xAccountIDs, startDateTime.Value );

                    if ( cdrResult != null )
                    {
                        totalPages = Int32.Parse( cdrResult.Root.Element( "result" ).Element( "calls" ).Attributes( "total_pages" ).First().Value );

                        foreach ( XElement xCall in cdrResult.Descendants( "call" ) )
                        {
                            try
                            {
                                string foreignKey = xCall.Attributes( "id" ).First().Value;

                                var cdrRecord = new CdrRecord();

                                var rockContext = new RockContext();
                                var interactionService = new InteractionService( rockContext );

                                var cdrInteraction = new Interaction();
                                interactionService.Add( cdrInteraction );

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
                                cdrInteraction.Operation = cdrRecord.Direction.ToString();

                                cdrRecord.Source = xCall.Attributes( "from_number" ).First().Value;
                                cdrRecord.Destination = xCall.Attributes( "to_number" ).First().Value;
                                cdrRecord.CallerId = xCall.Attributes( "from_name" ).First().Value;
                                cdrRecord.StartDateTime = DateTime.Parse( xCall.Attributes( "start_time" ).First().Value );
                                cdrRecord.Duration = Int32.Parse( xCall.Attributes( "total_duration" ).First().Value );
                                cdrRecord.EndDateTime = cdrRecord.StartDateTime.Value.AddSeconds( cdrRecord.Duration );
                                cdrRecord.RecordKey = xCall.Attributes( "id" ).First().Value;

                                if ( !currentInteractions.Contains( cdrRecord.RecordKey ) )
                                {
                                    var personService = new PersonService( rockContext );
                                    var sourcePerson = personService.GetByPhonePartial( cdrRecord.Source ).FirstOrDefault();
                                    var destinationPerson = personService.GetByPhonePartial( cdrRecord.Destination ).FirstOrDefault();

                                    cdrInteraction.InteractionData = cdrRecord.ToJson();
                                    cdrInteraction.InteractionComponentId = interactionComponentId;

                                    // depending on the call direction set the entity id and related entity id
                                    // the entity id should always be the internal person
                                    if ( cdrRecord.Direction == CdrDirection.Incoming )
                                    {
                                        cdrInteraction.EntityId = destinationPerson?.PrimaryAliasId;
                                        cdrInteraction.RelatedEntityId = sourcePerson?.PrimaryAliasId;
                                    }
                                    else
                                    {
                                        cdrInteraction.EntityId = sourcePerson?.PrimaryAliasId;
                                        cdrInteraction.RelatedEntityId = destinationPerson?.PrimaryAliasId;
                                    }

                                    cdrInteraction.RelatedEntityTypeId = relatedEntityTypeId;
                                    cdrInteraction.ForeignKey = cdrRecord.RecordKey;

                                    rockContext.SaveChanges();

                                    recordsProcessed++;
                                }
                            }
                            catch { }
                        }
                    }
                } while ( cdrResult != null && totalPages > pageNumber );
            }

            return recordsProcessed;
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
    }
}


