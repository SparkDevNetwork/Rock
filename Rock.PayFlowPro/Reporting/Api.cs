using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Data;

namespace Rock.PayFlowPro.Reporting
{
    public class Api
    {
        public string User { get; set; }
        public string Vendor { get; set; }
        public string Partner { get; set; }
        public string Password { get; set; }
        public bool Test { get; set; }

        public Api( string user, string vendor, string partner, string password, bool test = false )
        {
            User = user;
            Vendor = vendor;
            Partner = partner;
            Password = password;
            Test = test;
        }

        public DataTable GetReport( string reportName, Dictionary<string, string> reportParameters, out string errorMessage )
        {
            errorMessage = string.Empty;

            // Request the report
            var response = SendRequest( new ReportRequest( reportName, reportParameters ).ToXmlElement(), out errorMessage );
            if ( response != null )
            {
                var reportResponse = new ReportResponse( response );

                // Request the Metadata
                response = SendRequest( new MetaDataRequest( reportResponse.ReportId ).ToXmlElement(), out errorMessage );
                if ( response != null )
                {
                    var metaData = new MetaDataResponse( response );
                    {
                        DataTable dt = new DataTable();
                        metaData.Columns.ForEach( c => dt.Columns.Add( c ) );

                        for ( int pageNum = 1; pageNum <= metaData.PageCount; pageNum++ )
                        {
                            // Get each page of the data
                            response = SendRequest( new DataRequest( reportResponse.ReportId, pageNum ).ToXmlElement(), out errorMessage );
                            if ( response != null )
                            {
                                var data = new DataResponse( response );
                                foreach ( var row in data.Rows )
                                {
                                    var dataRow = dt.NewRow();
                                    for ( int colNum = 0; colNum < metaData.ColumnCount; colNum++ )
                                    {
                                        dataRow[colNum] = row[colNum];
                                    }
                                    dt.Rows.Add(dataRow);
                                }
                            }
                        }

                        return dt;
                    }
                }
            }

            return null;
        }

        private XDocument SendRequest( XElement request, out string errorMessage )
        {
            errorMessage = string.Empty;

            var requestElement = GetRequestElement();
            requestElement.Add( request );
            XDocument xdocRequest = new XDocument( new XDeclaration( "1.0", "UTF-8", "yes" ), requestElement );

            XDocument response = null;

            byte[] postData = ASCIIEncoding.ASCII.GetBytes( xdocRequest.ToString() );

            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create( ReportingApiUrl() );
            webRequest.Method = "POST";
            webRequest.ContentType = "text/plain";
            webRequest.ContentLength = postData.Length;
            var requestStream = webRequest.GetRequestStream();
            requestStream.Write( postData, 0, postData.Length );
            requestStream.Close();

            using ( WebResponse webResponse = webRequest.GetResponse() )
            {
                var stream = webResponse.GetResponseStream();
                using ( XmlReader reader = XmlReader.Create( stream ) )
                {
                    response = XDocument.Load( reader );
                    var status = new RequestResponse(response);
                    if (status.Code != "100")
                    {
                        errorMessage = status.Message;
                        response = null;
                    }

                }
            }

            return response;
        }

        private string ReportingApiUrl()
        {
            if ( Test )
            {
                return "https://payments-reports.paypal.com/test-reportingengine";
            }
            else
            {
                return "https://payments-reports.paypal.com/reportingengine";
            }
        }

        private XElement GetRequestElement()
        {
            return new XElement( "reportingEngineRequest", GetAuthElement() );
        }

        private XElement GetAuthElement()
        {
            return new XElement( "authRequest",
                new XElement( "user", User ),
                new XElement( "vendor", Vendor ),
                new XElement( "partner", Partner ),
                new XElement( "password", Password )
            );
        }

    }
}
