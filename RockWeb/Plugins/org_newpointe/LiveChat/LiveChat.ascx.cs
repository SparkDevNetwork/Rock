using Rock.Web.UI;
using System;
using Rock.Model;
using System.ComponentModel;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.IO;
using System.Configuration;

namespace RockWeb.Plugins.org_newpointe.LiveChat
{
    [DisplayName("LiveChat")]
    [Category("NewPointe.org Web Blocks")]
    [Description("This is the Live Chat")]
    public partial class LiveChat : RockBlock
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            
            Person person = CurrentPerson;
            
            if (person != null)
            {
                pnlPrivatePrayer.Visible = true;
                string name = person.FullName.Replace(" ", "");
                string guid = person.Guid.ToString().Replace("-", "");
                string uid = name + guid;
                hdnDisplayName.Value = person.FullName;
                hdnPerson.Value = person.Id.ToString();
                hdnRoom.Value = name + guid;
                hdnEmail.Value = person.Email;
                hdnPhoto.Value = person.PhotoUrl;
                hdnDate.Value = DateTime.Now.Date.ToShortDateString().Replace("/", "");

                var baseDate = new DateTime(1970, 1, 1);
                var startDate = (DateTime.Now.ToUniversalTime() - baseDate).TotalSeconds;
                var endDate = ((DateTime.Now.AddHours(1)).ToUniversalTime() - baseDate).TotalSeconds;

                var payload = new Dictionary<string, object>()
            {
                {"aud", ConfigurationManager.AppSettings["FirebaseAud"].ToString() },
                {"iat", Math.Truncate(startDate) },
                {"exp", Math.Truncate(endDate) },
                {"iss", ConfigurationManager.AppSettings["FirebaseIss"].ToString() },
                {"sub", ConfigurationManager.AppSettings["FirebaseIss"].ToString() },
                {"user_id", uid },
                {"scope", "https://www.googleapis.com/auth/identitytoolkit" }
            };

                var header = new Dictionary<string, object>()
            {
                {"alg", "RS256" },
                {"kid", ConfigurationManager.AppSettings["FirebaseKid"].ToString() },
                {"typ", "JWT" }
            };

                string filePath = Server.MapPath(ConfigurationManager.AppSettings["CertFile"]);

                RSACryptoServiceProvider cert = new X509Certificate2(filePath, ConfigurationManager.AppSettings["CertPassword"], X509KeyStorageFlags.Exportable).PrivateKey as RSACryptoServiceProvider;

                System.Security.Cryptography.RSACryptoServiceProvider rsa = new System.Security.Cryptography.RSACryptoServiceProvider();
                rsa.ImportCspBlob(cert.ExportCspBlob(true));

//                string token = Jose.JWT.Encode(payload, rsa, Jose.JwsAlgorithm.RS256, extraHeaders: header);

//                hdnToken.Value = token;
            }
            else
            {
                // add in logic for person not logged in
            }
        }
    }
}