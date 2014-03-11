using System;
using System.Collections.Generic;
using System.Linq;
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

            
        }

        protected void btnExecute_Click(object sender, EventArgs e)
        {
            string errorMessage = string.Empty;

            bool result = EnvironmentChecks.CheckSqlServer(txtServer.Text, txtUsername.Text, txtPassword.Text, txtDatabase.Text, out errorMessage);

            lMessages.Text = result.ToString() + " - " + errorMessage;
        }
    }
}