using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace Rock.Custom.CCV.ClientTestApp
{
    public partial class Form1 : Form
    {
        readonly string ns = "http://schemas.datacontract.org/2004/07/Rock.Address";

        public Form1()
        {
            InitializeComponent();
        }

        private void btnGo_Click( object sender, EventArgs e )
        {
            try
            {
                XDocument xdoc = SendRequest( "Standardize" );
                if ( xdoc != null )
                {
                    string street1 = xdoc.Root.Element( string.Format( "{{{0}}}Street1", ns ) ).Value;
                    string street2 = xdoc.Root.Element( string.Format( "{{{0}}}Street2", ns ) ).Value;
                    string city = xdoc.Root.Element( string.Format( "{{{0}}}City", ns ) ).Value;
                    string state = xdoc.Root.Element( string.Format( "{{{0}}}State", ns ) ).Value;
                    string zip = xdoc.Root.Element( string.Format( "{{{0}}}Zip", ns ) ).Value;

                    string standardizeService = xdoc.Root.Element( string.Format( "{{{0}}}StandardizeService", ns ) ).Value;
                    string standardizeResult = xdoc.Root.Element( string.Format( "{{{0}}}StandardizeResult", ns ) ).Value;

                    tbStreet1.Text = street1;
                    tbStreet2.Text = street2;
                    tbCity.Text = city;
                    tbState.Text = state;
                    tbZip.Text = zip;

                    lblStandardized.Text = string.Format( "{0}[{6}]: {1} {2} {3}, {4} {5}", standardizeService, street1, street2, city, state, zip, standardizeResult );
                }


                xdoc = SendRequest( "Geocode" );
                if ( xdoc != null )
                {
                    string latitude = xdoc.Root.Element( string.Format( "{{{0}}}Latitude", ns ) ).Value;
                    string longitude = xdoc.Root.Element( string.Format( "{{{0}}}Longitude", ns ) ).Value;

                    string geocodeService = xdoc.Root.Element( string.Format( "{{{0}}}GeocodeService", ns ) ).Value;
                    string geocodeResult = xdoc.Root.Element( string.Format( "{{{0}}}GeocodeResult", ns ) ).Value;

                    lblGeocoded.Text = string.Format( "{0}[{3}]: {1} {2}", geocodeService, latitude, longitude, geocodeResult );
                }
            }
            catch ( System.Exception ex )
            {
                MessageBox.Show( ex.Message, "Exception" );
            }
        }

        private XDocument SendRequest( string action )
        {
            string Request = string.Format(
                "<AddressStub xmlns=\"{0}\"><City>{1}</City><State>{2}</State><Street1>{3}</Street1><Street2>{4}</Street2><Zip>{5}</Zip></AddressStub>",
                ns, tbCity.Text, tbState.Text, tbStreet1.Text, tbStreet2.Text, tbZip.Text );

            HttpWebRequest req = WebRequest.Create(string.Format("http://localhost:6229/RockWeb/api/Crm/Address/{0}", action)) as HttpWebRequest;
            req.KeepAlive = false;
            req.Method = "PUT";

            UTF8Encoding encoding = new UTF8Encoding();
            byte[] buffer = encoding.GetBytes(Request);
            req.ContentLength = buffer.Length;
            req.ContentType = "text/xml";
            Stream PostData = req.GetRequestStream();
            PostData.Write(buffer, 0, buffer.Length);
            PostData.Close();

            HttpWebResponse resp = req.GetResponse() as HttpWebResponse;

            Encoding enc = Encoding.GetEncoding(1252);
            StreamReader loResponseStream = new StreamReader(resp.GetResponseStream(), enc);
            string Response = loResponseStream.ReadToEnd();
            loResponseStream.Close();

            if ( Response.Trim().Length > 0 )
                return XDocument.Parse( Response );

            return null;
        }
    }
}
