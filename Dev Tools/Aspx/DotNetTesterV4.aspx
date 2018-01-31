<%@ Page Language="C#" Debug="true" %>

<%@ Import Namespace="System.Net.Sockets" %>
<%@ Import Namespace="System.Security.AccessControl" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Net" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="System.Text" %>
<%@ Import Namespace="Microsoft.Win32" %>
<%@ Import Namespace="System.Web.Hosting" %>
<%@ Import Namespace="System.Web.Caching" %>

<script language="CS" runat="server">

    /// <summary>
    /// The purpose of this page is check the version of .Net someone is using
    /// to make sure they are running 4.5.2.
    /// </summary>
    #region Contstants or Properties
    protected readonly string dotNetVersionRequired = "4.5.2";
    protected readonly string rockLogoIco = "http://storage.rockrms.com/install/rock-chms.ico";
    protected readonly string rockStyles = "http://storage.rockrms.com/install/install.css";
    #endregion

    public void Page_Load( object sender, EventArgs e )
    {
        // check asp.net version
        string checkResults = string.Empty;

        if ( !CheckDotNetVersion( out checkResults ) )
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

        lOutput.Text += string.Format( "<h3>As Reported via System.Environment.Version</h3><pre>{0}</pre>(Note: this is not always very accurate.)", System.Environment.Version.ToString() );

        lOutput.Text += "<h3>Installed versions:</h3><pre>";
        GetVersionFromRegistry();
        lOutput.Text += "</pre>";

        lOutput.Text += "<h3>Update History:</h3><pre>";
        GetUpdateHistory();
        lOutput.Text += "</pre>";

        lOutput.Text += "<h3>App Pool Details:</h3><pre>";
        lOutput.Text += GetAppPool();
        lOutput.Text += "</pre>";
    }

    public string GetAppPool()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append( HostingEnvironment.ApplicationID + "<br/>" );
        sb.Append( HostingEnvironment.ApplicationPhysicalPath + "<br/>" );
        sb.Append( HostingEnvironment.ApplicationVirtualPath + "<br/>" );
        sb.Append( HostingEnvironment.SiteName + "<br/>" );

        return sb.ToString();
    }

    public void GetUpdateHistory()
    {
        StringBuilder sb = new StringBuilder();
        using ( RegistryKey baseKey = RegistryKey.OpenBaseKey( RegistryHive.LocalMachine, RegistryView.Registry32 ).OpenSubKey( @"SOFTWARE\Microsoft\Updates" ) )
        {
            foreach ( string baseKeyName in baseKey.GetSubKeyNames() )
            {
                if ( baseKeyName.Contains( ".NET Framework" ) || baseKeyName.StartsWith( "KB" ) || baseKeyName.Contains( ".NETFramework" ) )
                {
                    using ( RegistryKey updateKey = baseKey.OpenSubKey( baseKeyName ) )
                    {
                        string name = ( string ) updateKey.GetValue( "PackageName", "" );
                        sb.AppendLine( baseKeyName + "  " + name );
                        foreach ( string kbKeyName in updateKey.GetSubKeyNames() )
                        {
                            using ( RegistryKey kbKey = updateKey.OpenSubKey( kbKeyName ) )
                            {
                                name = ( string ) kbKey.GetValue( "PackageName", "" );
                                sb.AppendLine( "  " + kbKeyName + "  " + name );

                                if ( kbKey.SubKeyCount > 0 )
                                {
                                    foreach ( string sbKeyName in kbKey.GetSubKeyNames() )
                                    {
                                        using ( RegistryKey sbSubKey = kbKey.OpenSubKey( sbKeyName ) )
                                        {
                                            name = ( string ) sbSubKey.GetValue( "PackageName", "" );
                                            if ( name == "" )
                                                name = ( string ) sbSubKey.GetValue( "Description", "" );
                                            sb.AppendLine( "    " + sbKeyName + "  " + name );
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

    public void GetVersionFromRegistry()
    {
        StringBuilder sb = new StringBuilder();

        // Opens the registry key for the .NET Framework entry. 
        using ( RegistryKey ndpKey =
            RegistryKey.OpenRemoteBaseKey( RegistryHive.LocalMachine, "" ).
            OpenSubKey( @"SOFTWARE\Microsoft\NET Framework Setup\NDP\" ) )
        {
            // As an alternative, if you know the computers you will query are running .NET Framework 4.5  
            // or later, you can use: 
            // using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,  
            // RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
            foreach ( string versionKeyName in ndpKey.GetSubKeyNames() )
            {
                if ( versionKeyName.StartsWith( "v" ) )
                {
                    RegistryKey versionKey = ndpKey.OpenSubKey( versionKeyName );
                    string name = ( string ) versionKey.GetValue( "Version", "" );
                    string sp = versionKey.GetValue( "SP", "" ).ToString();
                    string install = versionKey.GetValue( "Install", "" ).ToString();
                    if ( install == "" ) //no install info, must be later.
                        sb.AppendLine( versionKeyName + "  " + name );
                    else
                    {
                        if ( sp != "" && install == "1" )
                        {
                            sb.AppendLine( versionKeyName + "  " + name + "  SP" + sp );
                        }
                    }

                    if ( name != "" )
                    {
                        continue;
                    }

                    foreach ( string subKeyName in versionKey.GetSubKeyNames() )
                    {
                        RegistryKey subKey = versionKey.OpenSubKey( subKeyName );
                        name = ( string ) subKey.GetValue( "Version", "" );
                        if ( name != "" )
                            sp = subKey.GetValue( "SP", "" ).ToString();
                        install = subKey.GetValue( "Install", "" ).ToString();
                        if ( install == "" ) //no install info, must be later.
                            sb.AppendLine( versionKeyName + "  " + name );
                        else
                        {
                            if ( sp != "" && install == "1" )
                            {
                                sb.AppendLine( "  " + subKeyName + "  " + name + "  SP" + sp );
                            }
                            else if ( install == "1" )
                            {
                                sb.AppendLine( "  " + subKeyName + "  " + name );
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
                <h1>
                    <asp:Literal ID="lTitle" Text="" runat="server" /></h1>

                <asp:Label ID="lOutput" runat="server" />

            </div>
        </div>
    </form>
</body>
</html>

<script language="CS" runat="server">

    #region  Private Helper Methods

    /// <summary>
    /// This is the old way we used to check the hosting .Net version, but
    /// we call the new method Get45PlusFromRegistry() in here now too.
    /// </summary>
    /// <param name="errorDetails"></param>
    /// <returns>true if the hosting environment has the correct version of .Net; false otherwise</returns>
    private bool CheckDotNetVersion( out string errorDetails )
    {
        bool checksPassed = false;
        errorDetails = string.Empty;
        string version = System.Environment.Version.ToString();

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
        else if ( System.Environment.Version.Major == 4 && System.Environment.Version.Build == 30319 )
        {
            // Once we get to 4.5 Microsoft recommends we test via the Registry...
            CheckResult result = Get45PlusFromRegistry();
            checksPassed = result.Pass;
            version = result.Version;
        }

        if ( checksPassed )
        {
            errorDetails += String.Format( "Great! Rock needs .Net version '{0}' and you have version '{1}'.", dotNetVersionRequired, version );
        }
        else
        {
            errorDetails = "The server does not have the correct .Net Framework runtime.  You have .Net version '" + version + "' and Rock RMS requires '" + dotNetVersionRequired + "'.";
        }

        return checksPassed;
    }

    /// <summary>
    /// Suggested approach to check which version of the .Net framework is intalled when using version 4.5 or later
    /// as per https://msdn.microsoft.com/en-us/library/hh925568(v=vs.110).aspx.
    /// </summary>
    /// <returns>a string containing the human readable version of the .Net framework</returns>
    private static CheckResult Get45PlusFromRegistry()
    {
        const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";
        using ( RegistryKey ndpKey = RegistryKey.OpenBaseKey( RegistryHive.LocalMachine, RegistryView.Registry32 ).OpenSubKey( subkey ) )
        {
            if ( ndpKey != null && ndpKey.GetValue( "Release" ) != null )
            {
                return CheckFor45PlusVersion( ( int ) ndpKey.GetValue( "Release" ) );
            }
            else
            {
                CheckResult result = new CheckResult();
                result.Pass = false;
                result.Version = ".NET Framework Version 4.5 or later is not detected.";
                return result;
            }
        }
    }

    /// <summary>
    /// Helper method from: https://msdn.microsoft.com/en-us/library/hh925568(v=vs.110).aspx.
    /// Checking the version using >= will enable forward compatibility.
    /// </summary>
    /// <param name="releaseKey"></param>
    /// <returns></returns>
    private static CheckResult CheckFor45PlusVersion( int releaseKey )
    {
        CheckResult result = new CheckResult();
        result.Pass = true;
        if ( releaseKey >= 394802 )
        {
            result.Version = "4.6.2 or later";
        }
        else if ( releaseKey >= 394254 )
        {
            result.Version = "4.6.1";
        }
        else if ( releaseKey >= 393295 )
        {
            result.Version = "4.6";
        }
        else if ( ( releaseKey >= 379893 ) )
        {
            result.Version = "4.5.2";
        }
        else if ( ( releaseKey >= 378675 ) )
        {
            result.Pass = false;
            result.Version = "4.5.1";
        }
        else if ( ( releaseKey >= 378389 ) )
        {
            result.Pass = false;
            result.Version = "4.5";
        }
        else
        {
            // This code should never execute. A non-null release key should mean
            // that 4.5 or later is installed.
            result.Pass = false;
            result.Version = "No 4.5 or later version detected";
        }

        return result;
    }
    #endregion

    public class CheckResult
    {
        public bool Pass;
        public string Version;
    }
</script>
