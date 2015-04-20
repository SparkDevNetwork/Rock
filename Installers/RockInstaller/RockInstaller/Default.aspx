<%@ Page Language="C#"%>
<%@ Import Namespace="System.Net"  %>
<%@ Import Namespace="System.IO"  %>

<script language="CS" runat="server">


    void Page_Load( object sender, EventArgs e )
    {
        
    }
 </script>


<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Rock Installer</title>
    <link rel='stylesheet' href='//fonts.googleapis.com/css?family=Open+Sans:400,600,700' type='text/css' />
    <link rel="stylesheet" href="//netdna.bootstrapcdn.com/bootstrap/3.0.0/css/bootstrap.min.css" />
    <link href="//netdna.bootstrapcdn.com/font-awesome/4.0.3/css/font-awesome.css" rel="stylesheet" />

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
