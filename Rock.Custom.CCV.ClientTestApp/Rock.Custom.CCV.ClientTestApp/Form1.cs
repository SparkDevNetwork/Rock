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
        public Form1()
        {
            InitializeComponent();
        }

        private void btnGo_Click( object sender, EventArgs e )
        {
            string ns = "http://schemas.datacontract.org/2004/07/Rock.Api.Crm.Address";

            string Request = string.Format(
                "<AddressStub xmlns=\"{0}\"><City>{1}</City><State>{2}</State><Street1>{3}</Street1><Street2>{4}</Street2><Zip>{5}</Zip></AddressStub>",
                ns, tbCity.Text, tbState.Text, tbStreet1.Text, tbStreet2.Text, tbZip.Text );

            HttpWebRequest req = WebRequest.Create("http://localhost:6229/RockWeb/api/Crm/Address/Geocode") as HttpWebRequest;
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
            {
                XDocument xdoc = XDocument.Parse( Response );
                if ( xdoc != null )
                {
                    string street1 = xdoc.Root.Element( string.Format( "{{{0}}}Street1", ns ) ).Value;
                    string street2 = xdoc.Root.Element( string.Format( "{{{0}}}Street2", ns ) ).Value;
                    string city = xdoc.Root.Element( string.Format( "{{{0}}}City", ns ) ).Value;
                    string state = xdoc.Root.Element( string.Format( "{{{0}}}State", ns ) ).Value;
                    string zip = xdoc.Root.Element( string.Format( "{{{0}}}Zip", ns ) ).Value;
                    string geocodeService = xdoc.Root.Element( string.Format( "{{{0}}}GeocodeService", ns ) ).Value;
                    string geocodeResult = xdoc.Root.Element( string.Format( "{{{0}}}GeocodeResult", ns ) ).Value;
                    string standardizeService = xdoc.Root.Element( string.Format( "{{{0}}}StandardizeService", ns ) ).Value;
                    string standardizeResult = xdoc.Root.Element( string.Format( "{{{0}}}StandardizeResult", ns ) ).Value;
                    string latitude = xdoc.Root.Element( string.Format( "{{{0}}}Latitude", ns ) ).Value;
                    string longitude = xdoc.Root.Element( string.Format( "{{{0}}}Longitude", ns ) ).Value;

                    lblStandardized.Text = string.Format( "{0}: {1} {2} {3}, {4} {5}", standardizeService, street1, street2, city, state, zip  );
                    lblGeocoded.Text = string.Format( "{0}: {1} {2}", geocodeService, latitude, longitude );
                }
            }
        }

        private void tbStreet1_TextChanged( object sender, EventArgs e )
        {

        }
    }
}
