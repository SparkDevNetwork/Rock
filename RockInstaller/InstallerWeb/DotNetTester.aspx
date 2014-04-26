<%@ Page Language="C#"  %>
<%@ Import Namespace="System.Net.Sockets"  %>
<%@ Import Namespace="System.Security.AccessControl"  %>
<%@ Import Namespace="System.IO"  %>
<%@ Import Namespace="System.Net"  %>


<script language="CS" runat="server">
		
	//
	// The purpose of this page is check the version of .Net someone is using
    // to make sure they are running 4.5.1.
	//
	
	//
	// some constants
	//
    protected readonly string dotNetVersionRequired = "4.5.1";
    protected readonly string rockLogoIco = "http://storage.rockrms.com/install/rock-chms.ico";
    protected readonly string rockStyles = "http://storage.rockrms.com/install/install.css";

	//
	// page events
	//

    void Page_Load( object sender, EventArgs e )
    {
        // check asp.net version
        string checkResults = string.Empty;

        if ( ! CheckDotNetVersion( out checkResults ) )
        {
            var errorDetails = "<li><i class='fa fa-exclamation-triangle fail'></i> " + checkResults + " <a href='http://www.rockrms.com/Rock/LetsFixThis#IncorrectDotNETVersion' class='btn btn-info btn-xs'>Let's Fix It Together</a></li>";
            lTitle.Text = "Not Ready for McKinley 0.5";
            lOutput.Text = "<ul class='list-unstyled'>" + errorDetails + "</ul>";
        }
        else
        {
            lTitle.Text = "Rock McKinley 0.5 Ready";
            lOutput.Text = "<ul class='list-unstyled'>" + checkResults + "</ul>";
        }
    }
    
</script>
<!DOCTYPE html>
<html>
	<head>
		<title>Rock RMS .Net Checker</title>
		<link rel='stylesheet' href='http://fonts.googleapis.com/css?family=Open+Sans:400,600,700' type='text/css'>
        <link rel="stylesheet" href="//netdna.bootstrapcdn.com/bootstrap/3.0.0/css/bootstrap.min.css">
        <link href="//netdna.bootstrapcdn.com/font-awesome/4.0.3/css/font-awesome.css" rel="stylesheet">
        <link rel="stylesheet" href="<%=rockStyles %>">
        <link href="<%=rockLogoIco %>" rel="shortcut icon">
		<link href="<%=rockLogoIco %>" type="image/ico" rel="icon">

        <script src="http://code.jquery.com/jquery-1.9.0.min.js"></script>

	</head>
	<body>
		<form runat="server">
				<div id="content">
					<h1>Rock RMS</h1>
					
					<div id="content-box" class="group">
						<h1><asp:Literal ID="lTitle" Text="" runat="server" /></h1>
						
						<asp:Label ID="lOutput" runat="server" />

					</div>
				</div>
		</form>
	</body>
</html>



<script language="CS" runat="server">
	
	/* 
	<><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><>
		Private Helper Methods
	<><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><><> 
	*/
    
    private bool CheckDotNetVersion(out string errorDetails)
    {
        bool checksPassed = false;
        errorDetails = string.Empty;

        // check .net
        // ok this is not easy as .net 4.5.1 actually reports as 4.0.378675 or 4.0.378758 depending on how it was installed
        // http://en.wikipedia.org/wiki/List_of_.NET_Framework_versions
        if ( System.Environment.Version.Major > 4 )
        {
            // greater than 4 (i.e., 5.0)
            checksPassed = true;
        }
        else if ( System.Environment.Version.Major == 4 && System.Environment.Version.Build > 30319 )
        {
            // greater than 4.5 (i.e., 4.6, 4.7 etc.)
            checksPassed = true;
        }
        else if ( System.Environment.Version.Major == 4 && System.Environment.Version.Build == 30319 && System.Environment.Version.Revision >= 18408 )
        {
            // greater than 4.5.1
            checksPassed = true;
        }

        if ( checksPassed )
        {
            errorDetails += String.Format( "You have the correct version of .Net ({0}+).", dotNetVersionRequired );
        }
        else
        {
            errorDetails = "The server does not have the correct .Net runtime.  You have .Net version " + System.Environment.Version.Major.ToString() + "." + System.Environment.Version.ToString() + " the Rock RMS version requires " + dotNetVersionRequired + ".";
        }

        return checksPassed;
    }

</script>