using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Text;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;



namespace RockWeb.Plugins.org_newpointe.Tableau
{

    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName("Embed Tableau")]
    [Category("NewPointe Metrics")]
    [Description("Embed a Tableau Chart.")]
    [TextField("Tableau Server","Address of the Tableau Server",true,"","",1)]
    [TextField("Visualation Name","The name (address) of the visualation to display.",true,"","",2)]
    [TextField("Site Root","Path to the Root of your visulation",false,"","",3)]
    [IntegerField("Width","The width of the visulation.",true,900,"",4)]
    [IntegerField("Height", "The height of the visulation.", true, 1600, "", 5)]
    [BooleanField("Use Trusted Tickets","",true,"",6)]
    [TextField("Domain Prefix", "Domain used for Tableau Servers", false, "", "", 7)]
    [TextField("Tableau Server Username","Username used to log into tableau server if using Trusted Tickets and name is not found in Permitted User List.",false,"","",8)]
    [TextField("Permitted User List","List of users that have accounts on Tableau Server if using Trusted Tickets. Seperate with ;",false,"","",9)]


public partial class EmbedTableau : Rock.Web.UI.RockBlock

{
    public string responseFromServer;
    public string TableauServer;
    public string VisualationName;
    public string TrustedServerURL;
    public string JavaScriptLocation;
    public string TableauSiteRoot;
    public string UserName;
    public string DomainPrefix;
    public bool UseTrustedTickets;
    public string PermittedUserList;
    public string TableauServerUsername;
    public string Width;
    public string Height;

    protected void Page_Load(object sender, EventArgs e)
    {
        
        TableauServer = GetAttributeValue("TableauServer");
        TrustedServerURL = TableauServer + "/trusted/";
        JavaScriptLocation = TableauServer + "/javascripts/api/viz_v1.js";
        TableauSiteRoot = GetAttributeValue("SiteRoot");
        DomainPrefix = GetAttributeValue("DomainPrefix");
        UseTrustedTickets = bool.Parse(GetAttributeValue("UseTrustedTickets"));
        PermittedUserList = GetAttributeValue("PermittedUserList");
        TableauServerUsername = GetAttributeValue("TableauServerUsername");
        Width = GetAttributeValue("Width");
        Height = GetAttributeValue("Height");
        VisualationName = GetAttributeValue("VisualationName");


        if (string.IsNullOrWhiteSpace(TableauServer))
        {
            //show error
        }

        //Truncate Username (useful if using Active Directory full email addresses for Rock authentication)
        string LoggedInUser = CurrentUser.UserName;
        if (LoggedInUser.Contains('@'))
        {
            string[] test = LoggedInUser.Split('@');
            UserName = test[0];
        }
        else
        {
            UserName = LoggedInUser;
        }

        //Create Domain Prefix
        if (!string.IsNullOrWhiteSpace(DomainPrefix))
        {
            DomainPrefix = DomainPrefix + '\\';
        }

        //Check and see if the logged in Rock user is a Tableau user.  If so,
        //use their username. If not, use the default Tableau username.
        if (!string.IsNullOrWhiteSpace(PermittedUserList))
        {
            string[] UserListArray = PermittedUserList.Split(';');
            if (!UserListArray.Contains(UserName))
            {
                UserName = TableauServerUsername;
            }
        }


        //Get a Trusted Ticket from Tableau Server
        if (!string.IsNullOrWhiteSpace(TableauServer) && UseTrustedTickets == true)
        {
            // Create a request using a URL that can receive a post.
            WebRequest request = WebRequest.Create(TrustedServerURL);
            request.Method = "POST";

            // Create POST data and convert it to a byte array.
            string postData = "username=" + DomainPrefix + UserName;
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            // Set the ContentType property of the WebRequest.
            request.ContentType = "application/x-www-form-urlencoded";

            // Set the ContentLength property of the WebRequest.
            request.ContentLength = byteArray.Length;

            // Get the request stream.
            Stream dataStream = request.GetRequestStream();

            // Write the data to the request stream.
            dataStream.Write(byteArray, 0, byteArray.Length);

            // Close the Stream object.
            dataStream.Close();

            // Get the response.
            WebResponse response = request.GetResponse();

            // Get the stream containing content returned by the server.
            dataStream = response.GetResponseStream();

            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);

            // Read the content.
            responseFromServer = reader.ReadToEnd();

            // Clean up the streams.
            reader.Close();
            dataStream.Close();
            response.Close();
        }
    
    
    }

 

}
}