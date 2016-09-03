<%@ Page Language="C#" Debug="true" %>
<%@ Import Namespace="System.Net.Sockets"  %>
<%@ Import Namespace="System.Security.AccessControl"  %>
<%@ Import Namespace="System.IO"  %>
<%@ Import Namespace="System.Net"  %>
<%@ Import Namespace="System"  %>
<%@ Import Namespace="System.Text"  %>
<%@ Import Namespace="Microsoft.Win32" %>
<%@ Import Namespace="System.Web.Hosting" %>
<%@ Import Namespace="System.Web.Caching" %>

<script language="CS" runat="server">
		
	//
	// The purpose of this page is check the version of .Net someone is using
	// to make sure they are running 4.5.2.
	//
	
	//
	// some constants
	//
    protected readonly string dotNetVersionRequired = "4.5.2";
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
            lTitle.Text = "Not Ready for McKinley 6.0";
            lOutput.Text = "<ul class='list-unstyled'>" + errorDetails + "</ul>";
        }
        else
        {
            lTitle.Text = "Rock McKinley 6.0 Ready";
            lOutput.Text = "<ul class='list-unstyled'>" + checkResults + "</ul>";
        }

        lOutput.Text += "<h3>Installed versions:</h3><pre>";
        GetVersionFromRegistry();
		lOutput.Text += "</pre>";
				        
        lOutput.Text += "<h3>Update History:</h3><pre>";
        GetUpdateHistory();
		lOutput.Text += "</pre>";

	    lOutput.Text +=  "<h3>App Pool Details:</h3><pre>";
        lOutput.Text += GetAppPool();
    	lOutput.Text += "</pre>";
    }

    public string GetAppPool()
    {

        StringBuilder sb = new StringBuilder();
        
//        using (ServerManager iisManager = new ServerManager())
//        {
            //SiteCollection sites = iisManager.Sites;
            //foreach (Site site in sites)
            //{
               //if (site.Name == HostingEnvironment.ApplicationHost.GetSiteName()) 
               //{

                 //iisManager.ApplicationPools[site.Applications["/"].ApplicationPoolName].Recycle();
                 //iisManager.ApplicationPools["/DefaultAppPool"].Recycle();
                 //sb.Append( site.Name + "<br/>" );
                // break;
               //}
            //}
//        }

    sb.Append( HostingEnvironment.ApplicationID + "<br/>");
    sb.Append( HostingEnvironment.ApplicationPhysicalPath + "<br/>" );
    sb.Append( HostingEnvironment.ApplicationVirtualPath + "<br/>" );
    sb.Append( HostingEnvironment.SiteName + "<br/>");

/*
    // in the app pool, you need to set the virtual memory limit to 800,000kb
    ObjectCache cache = MemoryCache.Default;
    var cachePolicy = new CacheItemPolicy();
    cachePolicy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds( 60 );

    for (int intCount = 1; intCount < 100000000; intCount++)
    {
        cache.Set( intCount.ToString(), cache, cachePolicy );
    }

    HostingEnvironment.InitiateShutdown();
*/
        return sb.ToString();
    }

    void GetUpdateHistory()
    {
    		StringBuilder sb = new StringBuilder();
        using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\Updates"))
        {
            foreach (string baseKeyName in baseKey.GetSubKeyNames())
            {
                if (baseKeyName.Contains(".NET Framework") || baseKeyName.StartsWith("KB") || baseKeyName.Contains(".NETFramework"))
                {
                    using (RegistryKey updateKey = baseKey.OpenSubKey(baseKeyName))
                    {
                        string name = (string)updateKey.GetValue("PackageName", "");
                        sb.AppendLine(baseKeyName + "  " + name );
                        foreach (string kbKeyName in updateKey.GetSubKeyNames())
                        {
                            using (RegistryKey kbKey = updateKey.OpenSubKey(kbKeyName))
                            {
                                name = (string)kbKey.GetValue("PackageName", "");
                                sb.AppendLine("  " + kbKeyName + "  " + name);

                                if (kbKey.SubKeyCount > 0)
                                {
                                    foreach (string sbKeyName in kbKey.GetSubKeyNames())
                                    {
                                        using (RegistryKey sbSubKey = kbKey.OpenSubKey(sbKeyName))
                                        {
                                            name = (string)sbSubKey.GetValue("PackageName", "");
                                            if (name == "")
                                                name = (string)sbSubKey.GetValue("Description", "");
                                            sb.AppendLine("    " + sbKeyName + "  " + name);

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
	    lOutput.Text += sb.ToString();
    }
    
void GetVersionFromRegistry()
{
    		StringBuilder sb = new StringBuilder();

     // Opens the registry key for the .NET Framework entry. 
        using (RegistryKey ndpKey = 
            RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, "").
            OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
        {
            // As an alternative, if you know the computers you will query are running .NET Framework 4.5  
            // or later, you can use: 
            // using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,  
            // RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
        foreach (string versionKeyName in ndpKey.GetSubKeyNames())
        {
            if (versionKeyName.StartsWith("v"))
            {
                RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);
                string name = (string)versionKey.GetValue("Version", "");
                string sp = versionKey.GetValue("SP", "").ToString();
                string install = versionKey.GetValue("Install", "").ToString();
                if (install == "") //no install info, must be later.
                    sb.AppendLine(versionKeyName + "  " + name);
                else
                {
                    if (sp != "" && install == "1")
                    {
                        sb.AppendLine(versionKeyName + "  " + name + "  SP" + sp);
                    }
                }
                if (name != "")
                {
                    continue;
                }
                foreach (string subKeyName in versionKey.GetSubKeyNames())
                {
                    RegistryKey subKey = versionKey.OpenSubKey(subKeyName);
                    name = (string)subKey.GetValue("Version", "");
                    if (name != "")
                        sp = subKey.GetValue("SP", "").ToString();
                    install = subKey.GetValue("Install", "").ToString();
                    if (install == "") //no install info, must be later.
                        sb.AppendLine(versionKeyName + "  " + name);
                    else
                    {
                        if (sp != "" && install == "1")
                        {
                            sb.AppendLine("  " + subKeyName + "  " + name + "  SP" + sp);
                        }
                        else if (install == "1")
                        {
                            sb.AppendLine("  " + subKeyName + "  " + name);
                        }
                    }
                }
            }
        }
    }
	lOutput.Text += sb.ToString();

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
        else if ( System.Environment.Version.Major == 4 && System.Environment.Version.Build == 30319 && System.Environment.Version.Revision >= 34000 )
        {
            // greater than 4.5.2
            checksPassed = true;
        }

        if ( checksPassed )
        {
            errorDetails += String.Format( "You have the correct version of .Net ({0}+).", dotNetVersionRequired );
        }
        else
        {
            errorDetails = "The server does not have the correct .NET Framework runtime.  You have .NET version " + System.Environment.Version.Major.ToString() + "." + System.Environment.Version.ToString() + " the Rock RMS version requires " + dotNetVersionRequired + ".";
        }

        return checksPassed;
    }

</script>