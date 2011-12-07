using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Serialization.Json;
using System.Xml;
using System.Xml.Linq;

namespace Rock.Custom.CCV.ClientTestApp
{
    public partial class Form1 : Form
    {
        const string APIKEY = "dtKey";

        public Form1()
        {
            InitializeComponent();
        }

        private void btnGo_Click( object sender, EventArgs e )
        {
            try
            {
                Rock.DataTransferObjects.Crm.Address address = SendRequest( "Standardize" );
                if ( address != null )
                {
                    tbStreet1.Text = address.Street1;
                    tbStreet2.Text = address.Street2;
                    tbCity.Text = address.City;
                    tbState.Text = address.State;
                    tbZip.Text = address.Zip;

                    tbStandardized.Text = string.Format( "{0}: {1}", address.StandardizeService, address.StandardizeResult );
                }

                address = SendRequest( "Geocode" );
                if ( address != null )
                    tbGeocoded.Text = string.Format( "{0}[{3}]: {1} {2}", address.GeocodeService, address.Latitude, address.Longitude, address.GeocodeResult );
            }
            catch ( System.Exception ex )
            {
                MessageBox.Show( ex.Message, "Exception" );
            }
        }

        private Rock.DataTransferObjects.Crm.Address SendRequest( string service )
        {
            Rock.DataTransferObjects.Crm.Address address = new Rock.DataTransferObjects.Crm.Address();
            address.Street1 = tbStreet1.Text;
            address.Street2 = tbStreet2.Text;
            address.City = tbCity.Text;
            address.State = tbState.Text;
            address.Zip = tbZip.Text;

            WebClient proxy = new System.Net.WebClient();
            proxy.Headers["Content-type"] = "application/json";
            MemoryStream ms = new MemoryStream();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer( typeof( Rock.DataTransferObjects.Crm.Address ) );
            serializer.WriteObject(ms, address);

            try
            {
                byte[] data = proxy.UploadData( string.Format( "http://localhost:56627/Crm/Address/{0}/{1}", service, APIKEY ),
                    "PUT", ms.ToArray() );
                Stream stream = new MemoryStream( data );
                return serializer.ReadObject( stream ) as Rock.DataTransferObjects.Crm.Address;
            }
            catch ( WebException ex )
            {
                //string response = (ex.Response
                using ( Stream data = ex.Response.GetResponseStream() )
                {
                    string text = new StreamReader( data ).ReadToEnd();

                    MessageBox.Show( string.Format( "Response: {0}\n{1}",
                        ( ( System.Net.HttpWebResponse )ex.Response ).StatusDescription, text ), service + " Error" );
                }

                return null;
            }
        }
    }

}
