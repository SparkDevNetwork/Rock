using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Install.Utilities;

namespace InstallerWeb
{
    public partial class Tester : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            string exceptionLog = Server.MapPath( "~/App_Data/Logs/RockExceptions.csv" );
            if ( File.Exists( exceptionLog ) )
            {
                StringBuilder exceptionDetails = new StringBuilder();
                exceptionDetails.Append( "<ul class='list-unstyled'>" );

                var reader = new StreamReader( File.OpenRead( exceptionLog ) );
                while ( !reader.EndOfStream )
                {
                    var line = reader.ReadLine();
                    var values = line.Split( ',' );

                    exceptionDetails.Append( String.Format( @"<li><i class='fa fa-exclamation-triangle fail'></i> {0} - {1}</li>", values[0], values[2] ) );
                }

                exceptionDetails.Append( "</ul>" );
                exceptionDetails.Append( "<div class='alert alert-warning'>Depending on your exception you may need to start over with a fresh install.</div>" );

                //ProcessPageException( exceptionDetails.ToString() );
                lMessages.Text = exceptionDetails.ToString();
            }
        }

        protected void btnExecute_Click(object sender, EventArgs e)
        {
            string errorMessage = string.Empty;

            bool result = EnvironmentChecks.CheckSqlServer(txtServer.Text, txtUsername.Text, txtPassword.Text, txtDatabase.Text, out errorMessage);

            lMessages.Text = result.ToString() + " - " + errorMessage;
        }
    }
}