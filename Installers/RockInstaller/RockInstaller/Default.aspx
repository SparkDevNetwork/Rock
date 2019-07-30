<%@ Page Language="C#"%>
<%@ Import Namespace="System.Net"  %>
<%@ Import Namespace="System.IO"  %>

<script language="CS" runat="server">


    void Page_Load( object sender, EventArgs e )
    {

    }
 </script>


<!DOCTYPE html>

<html lang="en">
<head runat="server">
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">
    <title>Rock Installer</title>
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css" integrity="sha384-BVYiiSIFeK1dGmJRAkycuHAHRg32OmUcww7on3RYdg4Va+PmSTsz/K68vbdEjh4u" crossorigin="anonymous">
    <link rel="stylesheet" href="https://use.fontawesome.com/releases/v5.3.1/css/all.css" integrity="sha384-mzrmE5qonljUremFsqc01SB46JvROS7bZs3IO2EmfFsd15uHvIt+Y8vEf7N7fWAU" crossorigin="anonymous">

    <link href="/Styles/rock-installer.css" rel="stylesheet" />

    <script src="//code.jquery.com/jquery-1.9.0.min.js"></script>
    <script src="Scripts/rock-install.js"></script>
</head>
<body>
    <form id="form1" runat="server">
        <div id="content">
	        <h1>Rock RMS</h1>

            <div id="content-box">

                <h1>Rock Install Test Harness</h1>

                <div class="form-group">
					<label class="control-label" for="inputEmail">Version To Run</label>
					<asp:TextBox ID="txtVersion" runat="server" CssClass="required-field form-control" Text="2_7_0"></asp:TextBox>
				</div>

				<div class="form-group">
					<label class="control-label" for="cbRunDebug">Run in Debug (highly recommended)</label>
					<asp:CheckBox ID="cbRunDebug" runat="server" Checked="true" />
				</div>

                <a href="#" id="btnStart" class="btn btn-primary">Start</a>

                <script>
                    $('body').on('click', '#btnStart', function (e) {
                        window.location = 'Start.aspx?Version=' + $('#txtVersion').val() + '&Debug=' + $('#cbRunDebug').is(':checked')
                    });
                </script>
            </div>
        </div>
    </form>
</body>

</html>
