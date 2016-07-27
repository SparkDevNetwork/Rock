<%@ Page Language="C#" AutoEventWireup="true"   %>

<%@ Import Namespace="Rock.Data" %>
<%@ Import Namespace="Rock" %>
<%@ Import Namespace="Rock.Model" %>
<%@ Import Namespace="Rock.Security" %>
<%@ Import Namespace="Rock.Web.Cache" %>
<script runat="server">
    protected void Page_Load( object sender, EventArgs e )
    {
        if ( !IsPostBack )
        {
            RockContext rockContext = new RockContext();

            var adminGroupId = new GroupService( rockContext ).Get( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid() ).Id;

            var adminUsers = new GroupMemberService( rockContext ).Queryable().Where( gm => gm.GroupId == adminGroupId )
                                    .Select( gm => gm.Person )
                                    .ToList();

            rblAccounts.DataValueField = "Id";
            rblAccounts.DataTextField = "FullName";
            rblAccounts.DataSource = adminUsers;
            rblAccounts.DataBind();
        }

    }

    protected void btnCreateLogin_Click( object sender, EventArgs e )
    {
        try {
            var entityType = EntityTypeCache.Read("Rock.Security.Authentication.Database");

            RockContext rockContext = new RockContext();
            var service = new UserLoginService( rockContext );

            UserLogin userLogin = new UserLogin();
            service.Add( userLogin );

            int personId = rblAccounts.SelectedValue.AsInteger();

            if ( personId != 0 )
            {
                var component = AuthenticationContainer.GetComponent( entityType.Name );
                if ( component != null && component.ServiceType == AuthenticationServiceType.Internal )
                {
                    userLogin.PersonId = personId;
                    userLogin.UserName = tbUsername.Text;
                    userLogin.Password = component.EncodePassword( userLogin, tbPassword.Text ); ;
                    userLogin.EntityTypeId = entityType.Id;
                    userLogin.IsConfirmed = true;

                    rockContext.SaveChanges();

                    nbMessages.Text = "Login has been created.";
                } else
                {
                    nbMessages.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Warning;
                    nbMessages.Text = "Could Not Find Authenication Component";
                }
            }
            else
            {
                nbMessages.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Warning;
                nbMessages.Text = "No user was selected";
            }
        }
        catch(Exception ex )
        {
            nbMessages.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
            nbMessages.Text = "Error: " + ex.Message;
        }
    }
</script>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Rock Pill - Add Login</title>
    <link href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.6/css/bootstrap.min.css" rel="stylesheet" integrity="sha384-1q8mTJOASx8j1Au+a5WDVnPi2lkFfwwEAa8hDDdjZlpLegxhjVME1fgjWPGmkzs7" crossorigin="anonymous">
</head>
<body>
    <form id="form1" runat="server">
    <div class="container">
        <div class="row">
            <div class="col-md-offset-3 col-md-4 margin-t-xl">
                <h1>Select the Person to Add an Account To</h1>
                <Rock:RockRadioButtonList ID="rblAccounts" runat="server" />

                <Rock:RockTextBox ID="tbUsername" runat="server" Label="Username" Text="tempuser" />

                <Rock:RockTextBox ID="tbPassword" runat="server" Label="Password" />

                <asp:LinkButton ID="btnCreateLogin" runat="server" OnClick="btnCreateLogin_Click" CssClass="btn btn-primary margin-b-md" Text="Create User" />
                
                <div style="margin-top: 24px;">
                    <Rock:NotificationBox ID="nbMessages" NotificationBoxType="Info" runat="server"  />
                </div>
            </div>
        </div>
        
    </div>
    </form>
</body>
</html>
