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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Security.BackgroundCheck
{
    /// <summary>
    /// Protect My Ministry Background Check 
    /// Note: This component requires 
    /// </summary>
    [Description( "Protect My Ministry Background Check" )]
    [Export( typeof( BackgroundCheckComponent ) )]
    [ExportMetadata( "ComponentName", "Protect My Ministry" )]

    [TextField( "User Name", "Protect My Ministry User Name", true, "", "", 0 )]
    [EncryptedTextField( "Password", "Protect My Ministry Password", true, "", "", 1, null, true )]
    [BooleanField( "Test Mode", "Should requests be sent in 'test' mode?", false, "", 2 )]
    [UrlLinkField( "Request URL", "The Protect My Ministry URL to send requests to.", true, "https://services.priorityresearch.com/webservice/default.cfm", "", 3 )]
    [UrlLinkField( "Return URL", "The Web Hook URL for Protect My Ministry to send results to (e.g. 'http://www.mysite.com/Webhooks/ProtectMyMinistry.ashx').", true, "", "", 4 )]
    public class ProtectMyMinistry : BackgroundCheckComponent
    {
        private HttpStatusCode _HTTPStatusCode;

        /// <summary>
        /// Sends a background request to Protect My Ministry
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="workflow">The Workflow initiating the request.</param>
        /// <param name="personAttribute">The person attribute.</param>
        /// <param name="ssnAttribute">The SSN attribute.</param>
        /// <param name="requestTypeAttribute">The request type attribute.</param>
        /// <param name="billingCodeAttribute">The billing code attribute.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns>
        /// True/False value of whether the request was successfully sent or not
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <remarks>
        /// Note: If the associated workflow type does not have attributes with the following keys, they
        /// will automatically be added to the workflow type configuration in order to store the results
        /// of the PMM background check request
        ///     ReportStatus:           The status returned by PMM
        ///     ReportLink:             The location of the background report on PMM server
        ///     ReportRecommendation:   PMM's recomendataion
        ///     Report (BinaryFile):    The downloaded background report
        /// </remarks>
        public override bool SendRequest( RockContext rockContext, Model.Workflow workflow,
            AttributeCache personAttribute, AttributeCache ssnAttribute, AttributeCache requestTypeAttribute,
            AttributeCache billingCodeAttribute, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            try
            {
                // Check to make sure workflow is not null
                if ( workflow == null )
                {
                    errorMessages.Add( "The 'ProtectMyMinistry' requires a valid workflow." );
                    return false;
                }

                // Get the person that the request is for
                Person person = null;
                if ( personAttribute != null )
                {
                    Guid? personAliasGuid = workflow.GetAttributeValue( personAttribute.Key ).AsGuidOrNull();
                    if ( personAliasGuid.HasValue )
                    {
                        person = new PersonAliasService( rockContext ).Queryable()
                            .Where( p => p.Guid.Equals( personAliasGuid.Value ) )
                            .Select( p => p.Person )
                            .FirstOrDefault();
                    }
                }

                if ( person == null )
                {
                    errorMessages.Add( "The 'ProtectMyMinistry' background check requires the workflow to have a 'Person' attribute that contains the person who the background check is for." );
                    return false;
                }

                string password = Encryption.DecryptString( GetAttributeValue( "Password" ) );

                XElement rootElement = new XElement( "OrderXML",
                    new XElement( "Method", "SEND ORDER" ),
                    new XElement( "Authentication",
                        new XElement( "Username", GetAttributeValue( "UserName" ) ),
                        new XElement( "Password", password )
                    )
                );

                if ( GetAttributeValue( "TestMode" ).AsBoolean() )
                {
                    rootElement.Add( new XElement( "TestMode", "YES" ) );
                }

                rootElement.Add( new XElement( "ReturnResultURL", GetAttributeValue( "ReturnURL" ) ) );

                XElement orderElement = new XElement( "Order" );
                rootElement.Add( orderElement );

                if ( billingCodeAttribute != null )
                {
                    Guid? campusGuid = workflow.GetAttributeValue( billingCodeAttribute.Key ).AsGuidOrNull();
                    if ( campusGuid.HasValue )
                    {
                        var campus = CampusCache.Read( campusGuid.Value );
                        if ( campus != null )
                        {
                            orderElement.Add( new XElement( "BillingReferenceCode", campus.Name ) );
                        }
                    }
                }

                XElement subjectElement = new XElement( "Subject",
                    new XElement( "FirstName", person.FirstName ),
                    new XElement( "MiddleName", person.MiddleName ),
                    new XElement( "LastName", person.LastName )
                );
                orderElement.Add( subjectElement );

                if ( person.BirthDate.HasValue )
                {
                    subjectElement.Add( new XElement( "DOB", person.BirthDate.Value.ToString( "MM/dd/yyyy" ) ) );
                }

                if ( ssnAttribute != null )
                {
                    string ssn = Encryption.DecryptString( workflow.GetAttributeValue( ssnAttribute.Key ) ).AsNumeric();
                    if ( !string.IsNullOrWhiteSpace( ssn ) && ssn.Length == 9 )
                    {
                        subjectElement.Add( new XElement( "SSN", ssn.Insert( 5, "-" ).Insert( 3, "-" ) ) );
                    }
                }

                if ( person.Gender == Gender.Male )
                {
                    subjectElement.Add( new XElement( "Gender", "Male" ) );
                }
                if ( person.Gender == Gender.Female )
                {
                    subjectElement.Add( new XElement( "Gender", "Female" ) );
                }

                var homelocation = person.GetHomeLocation();
                if ( homelocation != null)
                {
                    subjectElement.Add( new XElement( "CurrentAddress",
                        new XElement( "StreetAddress", homelocation.Street1 ),
                        new XElement( "City", homelocation.City ),
                        new XElement( "State", homelocation.State ),
                        new XElement( "Zipcode", homelocation.PostalCode )
                    ) );
                }

                XElement aliasesElement = new XElement( "Aliases" );
                if ( person.NickName != person.FirstName )
                {
                    aliasesElement.Add( new XElement( "Alias", new XElement( "FirstName", person.NickName ) ) );
                }
                // foreach ( string lastName in [previous last names] ) 
                //{
                //    aliasesElement.Add( new XElement( "Alias", new XElement( "LastName", lastName ) ) );
                //}
                if ( aliasesElement.HasElements )
                {
                    subjectElement.Add( aliasesElement );
                }

                string packageType = requestTypeAttribute != null ? workflow.GetAttributeValue( requestTypeAttribute.Key ) : string.Empty;
                if ( string.IsNullOrWhiteSpace(packageType) )
                {
                    packageType = "Basic";
                }
                orderElement.Add( new XElement( "PackageServiceCode", packageType,
                    new XAttribute( "OrderId", workflow.Id.ToString() ) ) );
                orderElement.Add( new XElement( "OrderDetail",
                    new XAttribute( "OrderId", workflow.Id.ToString() ),
                    new XAttribute( "ServiceCode", "combo" ) ) );

                XDocument xdoc = new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), rootElement );

                XDocument xResult = PostToWebService( xdoc, GetAttributeValue("RequestURL") );

                if ( _HTTPStatusCode == HttpStatusCode.OK )
                {
                    if ( xResult.Root.Descendants().Count() > 0 )
                    {
                        SaveResults( xResult, workflow, rockContext );
                    }
                    return true;
                }
                else
                {
                    errorMessages.Add( "Invalid HttpStatusCode: " + _HTTPStatusCode.ToString() );
                }

                return false;

            }
            catch( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );
                errorMessages.Add( ex.Message );
                return false;
            }
        }

        XDocument PostToWebService( XDocument data, string requestUrl )
        {
            string stringData = "REQUEST=" + data.Declaration.ToString() + data.ToString( SaveOptions.DisableFormatting );
            byte[] postData = ASCIIEncoding.ASCII.GetBytes( stringData );

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create( requestUrl );
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            request.ContentLength = postData.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write( postData, 0, postData.Length );
            requestStream.Close();

            try
            {
                HttpWebResponse response = ( HttpWebResponse )request.GetResponse();
                return GetResponse( response.GetResponseStream(), response.ContentType, response.StatusCode );
            }
            catch (WebException webException)
            {
                string message = GetResponseMessage(webException.Response.GetResponseStream());
                throw new Exception(webException.Message + " - " + message);
            }
        }

        private XDocument GetResponse(Stream responseStream, string contentType, HttpStatusCode statusCode)
        {
            _HTTPStatusCode = statusCode;

            Stream receiveStream = responseStream;
            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
            StreamReader readStream = new StreamReader(receiveStream, encode);

            StringBuilder sb = new StringBuilder();
            Char[] read = new Char[8192];
            int count = 0;
            do
            {
                count = readStream.Read(read, 0, 8192);
                String str = new String(read, 0, count);
                sb.Append(str);
            }
            while (count > 0);

            string HTMLResponse = sb.ToString();

            if (HTMLResponse.Trim().Length > 0 && HTMLResponse.Contains("<?xml"))
                return XDocument.Parse(HTMLResponse);
            else
                return null;
        }

        private string GetResponseMessage( Stream responseStream )
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

        /// <summary>
        /// Saves the results.
        /// </summary>
        /// <param name="xResult">The x result.</param>
        /// <param name="workflow">The workflow.</param>
        /// <param name="rockContext">The rock context.</param>
        public static void SaveResults( XDocument xResult, Rock.Model.Workflow workflow, RockContext rockContext )
        {
            var xOrderDetail = xResult.Descendants( "OrderDetail" ).FirstOrDefault();
            if ( xOrderDetail != null )
            {
                string status = ( from o in xOrderDetail.Descendants( "Status" ) select o.Value ).FirstOrDefault();
                if ( !string.IsNullOrWhiteSpace( status ) )
                {
                    // Request has been completed

                    // Save the status
                    SaveAttributeValue( workflow, "ReportStatus", status == "NO RECORD" ? "Pass" : "Review",
                        FieldTypeCache.Read( Rock.SystemGuid.FieldType.SINGLE_SELECT.AsGuid() ), rockContext,
                        new Dictionary<string, string> { { "fieldtype", "ddl" }, { "values", "Pass,Fail,Review" } } );

                    // Save the report link 
                    string reportLink = ( from o in xResult.Descendants( "ReportLink" ) select o.Value ).FirstOrDefault();
                    SaveAttributeValue( workflow, "ReportLink", reportLink, 
                        FieldTypeCache.Read( Rock.SystemGuid.FieldType.URL_LINK.AsGuid() ), rockContext );

                    // Save the recommendation 
                    string recommendation = ( from o in xResult.Descendants( "Recommendation" ) select o.Value ).FirstOrDefault();
                    SaveAttributeValue( workflow, "ReportRecommendation", recommendation,
                        FieldTypeCache.Read( Rock.SystemGuid.FieldType.TEXT.AsGuid() ), rockContext,
                        new Dictionary<string, string> { { "ispassword", "false" } } );

                    // Save the report
                    Guid? binaryFileGuid = SaveFile( workflow.Attributes["Report"], reportLink, workflow.Id.ToString() + ".pdf" );
                    if ( binaryFileGuid.HasValue )
                    {
                        SaveAttributeValue( workflow, "Report", binaryFileGuid.Value.ToString(),
                            FieldTypeCache.Read( Rock.SystemGuid.FieldType.BINARY_FILE.AsGuid() ), rockContext,
                            new Dictionary<string, string> { { "binaryFileType", "" } } );
                    }

                }
            }
        }

        private static void SaveAttributeValue( Rock.Model.Workflow workflow, string key, string value, 
            FieldTypeCache fieldType, RockContext rockContext, Dictionary<string, string> qualifiers = null )
        {
            if (workflow.Attributes.ContainsKey(key))
            {
                workflow.SetAttributeValue( key, value );
            }
            else
            {
                // If workflow attribute doesn't exist, create it 
                // ( should only happen first time a background check is processed for given workflow type)
                var attribute = new Rock.Model.Attribute();
                attribute.EntityTypeId = workflow.WorkflowType.TypeId;
                attribute.EntityTypeQualifierColumn = "WorkflowTypeId";
                attribute.EntityTypeQualifierValue = workflow.WorkflowTypeId.ToString();
                attribute.Name = key.SplitCase();
                attribute.Key = key;
                attribute.FieldTypeId = fieldType.Id;

                if ( qualifiers != null )
                {
                    foreach( var keyVal in qualifiers )
                    {
                        var qualifier = new Rock.Model.AttributeQualifier();
                        qualifier.Key = keyVal.Key;
                        qualifier.Value = keyVal.Value;
                        attribute.AttributeQualifiers.Add( qualifier );
                    }
                }

                // Set the value for this action's instance to the current time
                var attributeValue = new Rock.Model.AttributeValue();
                attributeValue.Attribute = attribute;
                attributeValue.EntityId = workflow.Id;
                attributeValue.Value = value;
                new AttributeValueService( rockContext ).Add( attributeValue );
            }

        }

        private static Guid? SaveFile( AttributeCache binaryFileAttribute, string url, string fileName )
        {
            // get BinaryFileType info
            if ( binaryFileAttribute != null &&
                binaryFileAttribute.QualifierValues != null &&
                binaryFileAttribute.QualifierValues.ContainsKey( "binaryFileType" ) )
            {
                Guid? fileTypeGuid = binaryFileAttribute.QualifierValues["binaryFileType"].Value.AsGuidOrNull();
                if ( fileTypeGuid.HasValue )
                {
                    RockContext rockContext = new RockContext();
                    BinaryFileType binaryFileType = new BinaryFileTypeService( rockContext ).Get( fileTypeGuid.Value );

                    if ( binaryFileType != null )
                    {
                        byte[] data = null;

                        using ( WebClient wc = new WebClient() )
                        {
                            data = wc.DownloadData( url );
                        }

                        BinaryFile binaryFile = new BinaryFile();
                        binaryFile.Guid = Guid.NewGuid();
                        binaryFile.IsTemporary = true;
                        binaryFile.BinaryFileTypeId = binaryFileType.Id;
                        binaryFile.MimeType = "application/pdf";
                        binaryFile.FileName = fileName;
                        binaryFile.ContentStream = new MemoryStream( data );

                        var binaryFileService = new BinaryFileService( rockContext );
                        binaryFileService.Add( binaryFile );

                        rockContext.SaveChanges();

                        return binaryFile.Guid;
                    }
                }
            }

            return null;
        }

    }
}