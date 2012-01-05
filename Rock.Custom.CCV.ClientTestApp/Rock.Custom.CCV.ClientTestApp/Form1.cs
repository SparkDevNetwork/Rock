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
        const string APIKEY = "CcvRockApiKey";

        public Form1()
        {
            InitializeComponent();
        }

        private void btnGo_Click( object sender, EventArgs e )
        {
            try
            {
                Rock.CRM.DTO.Address address = SendRequest( "Standardize" );
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

        private Rock.CRM.DTO.Address SendRequest( string service )
        {
            Rock.CRM.DTO.Address address = new Rock.CRM.DTO.Address();
            address.Street1 = tbStreet1.Text;
            address.Street2 = tbStreet2.Text;
            address.City = tbCity.Text;
            address.State = tbState.Text;
            address.Zip = tbZip.Text;

            WebClient proxy = new System.Net.WebClient();
            proxy.Headers["Content-type"] = "application/json";
            MemoryStream ms = new MemoryStream();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer( typeof( Rock.CRM.DTO.Address ) );
            serializer.WriteObject(ms, address);

            try
            {
                byte[] data = proxy.UploadData( string.Format( "http://localhost:6229/RockWeb/REST/CRM/Address/{0}/{1}", service, APIKEY ),
                    "PUT", ms.ToArray() );
                Stream stream = new MemoryStream( data );
                return serializer.ReadObject( stream ) as Rock.CRM.DTO.Address;
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

        private void btnEncrypt_Click( object sender, EventArgs e )
        {
            tbEncryptionResult.Text = System.Web.HttpUtility.UrlEncode( Rock.Security.Encryption.EncryptString( tbEncryption.Text, "MyreallylongPassPhraseMyreallylongPassPhraseMyreallylongPassPhraseMyreallylongPassPhrase" ) );
        }

        private void btnDecrypt_Click( object sender, EventArgs e )
        {
            tbEncryptionResult.Text = Rock.Security.Encryption.DecryptString( System.Web.HttpUtility.UrlDecode( tbEncryption.Text ), "MyreallylongPassPhraseMyreallylongPassPhraseMyreallylongPassPhraseMyreallylongPassPhrase" );
        }

        private void btnAttrGo_Click( object sender, EventArgs e )
        {
            Rock.Core.DTO.Attribute attribute = new Core.DTO.Attribute();
            attribute.Key = tbAttrKey.Text;
            attribute.Name = tbAttrName.Text;
            attribute.Category = tbAttrCategory.Text;
            attribute.Description = tbAttrDescription.Text;
            attribute.DefaultValue = tbAttrValue.Text;
            attribute.Required = cbAttrRequired.Checked;

            WebClient proxy = new System.Net.WebClient();
            proxy.Headers["Content-type"] = "application/json";
            MemoryStream ms = new MemoryStream();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer( typeof( Rock.Core.DTO.Attribute ) );
            serializer.WriteObject( ms, attribute );

            try
            {
                byte[] data = proxy.UploadData( string.Format( "http://localhost:6229/RockWeb/REST/Core/Attribute/{0}", APIKEY ),
                    "POST", ms.ToArray() );
                Stream stream = new MemoryStream( data );
                Rock.Core.DTO.Attribute updatedAttribute = serializer.ReadObject( stream ) as Rock.Core.DTO.Attribute;

                MessageBox.Show( string.Format( "Attribute Created.  ID: {0}", updatedAttribute.Id ), "Created" );
            }
            catch ( WebException ex )
            {
                //string response = (ex.Response
                using ( Stream data = ex.Response.GetResponseStream() )
                {
                    string text = new StreamReader( data ).ReadToEnd();

                    MessageBox.Show( string.Format( "Response: {0}\n{1}",
                        ( ( System.Net.HttpWebResponse )ex.Response ).StatusDescription, text ), "Error" );
                }
            }
        }
    }

}
