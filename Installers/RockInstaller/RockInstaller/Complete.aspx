<%@ Page Language="C#"  %>
<%@ Import Namespace="System.IO"  %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Rock Installer</title>
    <link rel='stylesheet' href='//fonts.googleapis.com/css?family=Open+Sans:300,400,600,700' type='text/css' />
    <link rel="stylesheet" href="//netdna.bootstrapcdn.com/bootstrap/3.0.0/css/bootstrap.min.css" />
    <link href="//netdna.bootstrapcdn.com/font-awesome/4.1.0/css/font-awesome.min.css" rel="stylesheet" />
    
    <style type="text/css">

        body {
            background-color: #dbd5cb;
            border-top: 24px solid #282526;
        }

    </style>

    <script src="//code.jquery.com/jquery-1.9.0.min.js"></script>

    <script src="<%=String.Format("{0}Scripts/rock-install.js", storageUrl) %>"></script>
    <link href="<%=String.Format("{0}Styles/rock-installer.css", storageUrl) %>" rel="stylesheet" />
    <link rel="shortcut icon" href="<%=String.Format("{0}Images/favicon.ico", storageUrl) %>" />

</head>
<body>
    <form id="form1" runat="server">
        <div id="content">
	        <h1 id="logo">Rock RMS</h1>

                <div id="content-box">
                    <!-- welcome panel -->
                    <div id="pnlComplete">

                            <asp:Panel ID="pnlSuccess" runat="server" Visible="true">
                                <div class="content-narrow">
                                    <img src="<%=storageUrl %>Images/laptop.png" />
                                
                                    <h1>Success</h1>

                                    <div class="success-message">
                                        <p>
                                            Rock RMS has been successfully installed on your server. All that's
                                            left is to login and get started.
                                        </p>

                                        <a href="#" class="btn btn-primary start-up"><i class="fa fa-lightbulb-o "></i> Flip the Switch</a>
                                    </div>

                                    <div id="waiting-message" class="alert alert-info" style="display: none;">
                                        <strong>Something To Keep In Mind...</strong>
                                        <p>
                                            Rock can take a minute or two to intially start-up when the server has been shutdown. Once started 
                                            though, pages will load quickly.
                                        </p>
                                        <span class="blink label label-info" style="margin-top: 6px;">Rock Loading...</span>
                                    </div>

                                </div>
                            </asp:Panel>

                            <asp:Panel ID="pnlError" runat="server" Visible="false">
                                <img src="<%=storageUrl %>Images/laptop.png" />
                                
                                <h1>An Error Occurred Moving Rock Into Place</h1>

                                <asp:Literal ID="lErrorMessage" runat="server" />
                            </asp:Panel>

                    </div>
                </div>

        </div>

        <script>

            $('.start-up').click(function () {
                $('.success-message').slideUp();
                $('#waiting-message').slideDown();
                window.location = './';
                return false;
            });

        </script>

    </form>
</body>

<script language="CS" runat="server">
    

    const string baseStorageUrl = "//storage.rockrms.com/install/";
    const string baseVersion = "2_0_0";

    string storageUrl = string.Empty;

    bool isDebug = false;
    string serverPath = string.Empty; 
   
    void Page_Init( object sender, EventArgs e )
    {
        bool cleanupSuccessful = true;
        string errorMessage = string.Empty;
        
        serverPath = serverPath = (System.Web.HttpContext.Current == null)
                    ? System.Web.Hosting.HostingEnvironment.MapPath( "~/" )
                    : System.Web.HttpContext.Current.Server.MapPath( "~/" );

        if ( Request["Version"] != null )
        {
            storageUrl = String.Format( "{0}{1}/", baseStorageUrl, Request["Version"] );
        }
        else
        {
            storageUrl = String.Format( "{0}{1}/", baseStorageUrl, baseVersion );
        }
        
        if ( Request["Debug"] != null )
        {
            isDebug = Convert.ToBoolean( Request["Debug"] );
        }
        
        try
        {
            // remove installer data files
            File.Delete( serverPath + @"\rock-install-latest.zip" );
            File.Delete( serverPath + @"\sql-config.sql" );
            File.Delete( serverPath + @"\sql-install.sql" );
            File.Delete( serverPath + @"\sql-latest.zip" );

            if ( !isDebug )
            {
                // process the bin folder
                System.Threading.Thread cleanup = new System.Threading.Thread( new System.Threading.ThreadStart( CleanUpInstall ) );
                cleanup.IsBackground = true;
                cleanup.Start();
            }
            else
            {
                File.Delete( serverPath + @"\webconfig.xml" );
            }
            
        }
        catch ( Exception ex )
        {
            cleanupSuccessful = false;
            errorMessage = ex.Message;
        }
            
        if (!cleanupSuccessful) {
            pnlSuccess.Visible = false;
            pnlError.Visible = true;

            lErrorMessage.Text = String.Format( "<div class='alert alert-danger'><strong>Error Details</strong> {0}</div>", errorMessage );
        }
    }

    private void CleanUpInstall()
    {
        // remove installer files
        File.Delete( serverPath + @"Start.aspx" );
        File.Delete( serverPath + @"Install.aspx" );

        DeleteDirectory( serverPath + @"\bin" );

        // move the rock application into place
        if ( DirectoryContentsMove( serverPath + @"\rock", serverPath ) )
        {
            // set permissions (fix for Arvixe)
            SetInheritPermissions( serverPath, true );
            
            // delete this page
            File.Delete( serverPath + @"Complete.aspx" );

            // delete a web.config if it already exists in the root, this is not the rock one
            File.Copy( serverPath + @"\webconfig.xml", serverPath + @"\web.config", true );

            // delete web config template
            File.Delete( serverPath + @"\webconfig.xml" );

            // delete rock install directory
            Directory.Delete( serverPath + @"\rock", true );
        }
    }

    private void DeleteDirectory( string target_dir )
    {
        string[] files = Directory.GetFiles( target_dir );
        string[] dirs = Directory.GetDirectories( target_dir );

        foreach ( string file in files )
        {
            File.SetAttributes( file, FileAttributes.Normal );
            File.Delete( file );
        }

        foreach ( string dir in dirs )
        {
            DeleteDirectory( dir );
        }

        Directory.Delete( target_dir, false );
    }

    private bool DirectoryContentsMove( string sourceDirName, string destDirName )
    {
        // this will move the contents of a folder into the contents of another
        // if a folder already exists in the directory with the same name it
        // will be deleted. ONLY USE THIS IF THIS USE CASE WORKS FOR YOU!!!

        try
        {
            DirectoryInfo sourceDirectory = new DirectoryInfo( sourceDirName );
            DirectoryInfo[] sourceChildDirectories = sourceDirectory.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if ( !sourceDirectory.Exists )
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName );
            }

            // If the destination directory does not exist, create it.
            if ( !Directory.Exists( destDirName ) )
            {
                Directory.CreateDirectory( destDirName );
            }

            // move child directories
            foreach ( DirectoryInfo dir in sourceChildDirectories )
            {
                string destChildDirectory = Path.Combine( destDirName, dir.Name );
                if ( Directory.Exists( destChildDirectory ) )
                {
                    DeleteDirectory( destChildDirectory );
                }
                dir.MoveTo( destChildDirectory );
            }

            // move child files
            FileInfo[] files = sourceDirectory.GetFiles();

            foreach ( FileInfo file in files )
            {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine( destDirName, file.Name );

                // check if file exists in dest if so delete
                if ( File.Exists( temppath ) )
                {
                    File.Delete( temppath );
                }

                // move the file.
                file.MoveTo( temppath );
            }
        }
        
        catch (Exception ex)
        {
            LogException( ex );
            return false;
        }
        
        return true;
    }

    private void SetInheritPermissions( string sourceDirName, bool isRoot )
    {
        string logPath = Path.Combine( serverPath, "InstallLog.txt" );

        if ( isRoot )
        {
            File.AppendAllText( logPath, String.Format( "Start: {0}{1}", DateTime.UtcNow.ToString( "yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture ), Environment.NewLine ) );
        }

        File.AppendAllText( logPath, String.Format( "Method SetInheritPermissions Called On: {0}{1}", sourceDirName, Environment.NewLine ) );
        
        try
        {
            DirectoryInfo sourceDirectory = new DirectoryInfo( sourceDirName );
            DirectoryInfo[] sourceChildDirectories = sourceDirectory.GetDirectories();
            FileInfo[] files = sourceDirectory.GetFiles();

            File.AppendAllText( logPath, String.Format( "Directory Count: {0}{1}", sourceChildDirectories.Count().ToString(), Environment.NewLine ) );
            File.AppendAllText( logPath, String.Format( "File Count: {0}{1}", files.Count().ToString(), Environment.NewLine ) );

            // set directory permission if not root
            if ( !isRoot )
            {
                File.AppendAllText( logPath, String.Format( "Attempting to set permissions on root directory {0}{1}", sourceDirName, Environment.NewLine ) );
                var directorySecurity = sourceDirectory.GetAccessControl();
                directorySecurity.SetAccessRuleProtection( false, false );
                sourceDirectory.SetAccessControl( directorySecurity );
            }
            
            // process subdirectories
            foreach ( DirectoryInfo dir in sourceChildDirectories )
            {
                File.AppendAllText( logPath, String.Format( "Calling set permissions on {0}{1}", dir.FullName, Environment.NewLine ) );
                SetInheritPermissions( dir.FullName + @"\", false );
            }
            
            // process files 
            foreach ( FileInfo file in files )
            {
                File.AppendAllText( logPath, String.Format( "Attempting to set permissions on file {0}{1}", file.FullName, Environment.NewLine ) );
                var fileSecurity = file.GetAccessControl();
                fileSecurity.SetAccessRuleProtection( false, false );
                file.SetAccessControl( fileSecurity );
            }

            if ( isRoot )
            {
                File.AppendAllText( logPath, String.Format( "End: {0}{1}", DateTime.UtcNow.ToString( "yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture ), Environment.NewLine ) );
            }
            
        }
        catch ( Exception ex )
        {
            File.AppendAllText( logPath, String.Format( "Error: {0}{1}", ex.Message, Environment.NewLine ) );
        }
    }
    
    private void LogException(Exception ex)
    {
        string directory = AppDomain.CurrentDomain.BaseDirectory;
        string filePath = Path.Combine( directory, "InstallExceptions.csv" );
        string when = DateTime.Now.ToString();
        while ( ex != null )
        {
            File.AppendAllText( filePath, string.Format( "{0},{1},\"{2}\"\r\n", when, ex.GetType(), ex.Message ) );
            ex = ex.InnerException;
        }
    }

</script>

</html>
