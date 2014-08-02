<%@ Page Language="C#"  %>
<%@ Import Namespace="System.IO"  %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Rock Installer</title>
    <link rel='stylesheet' href='//fonts.googleapis.com/css?family=Open+Sans:300,400,600,700' type='text/css' />
    <link rel="stylesheet" href="//netdna.bootstrapcdn.com/bootstrap/3.0.0/css/bootstrap.min.css" />
    <link href="//netdna.bootstrapcdn.com/font-awesome/4.1.0/css/font-awesome.min.css" rel="stylesheet" />
    
    <script src="//code.jquery.com/jquery-1.9.0.min.js"></script>

    <script src="<%=String.Format("{0}Scripts/rock-install.js", storageUrl) %>"></script>
    <link href="<%=String.Format("{0}Styles/rock-installer.css", storageUrl) %>" rel="stylesheet" />
    <link rel="shortcut icon" href="<%=String.Format("{0}Images/favicon.ico", storageUrl) %>" />

    <script>

        
    </script>

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

                                    <p>
                                        Rock RMS has been successfully installed on your server. All that's
                                        left is to login and get started.
                                    </p>

                                    <a href="./" class="btn btn-primary"><i class="fa fa-lightbulb-o "></i> Flip the Switch</a>
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

        </div>
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
                // remove installer files
                
                File.Delete( serverPath + @"Start.aspx" );
                File.Delete( serverPath + @"Install.aspx" );
                
                DeleteDirectory( serverPath + @"\bin" );

                // move the rock application into place
                DirectoryContentsMove( serverPath + @"\rock", serverPath );

                // delete rock install directory
                Directory.Delete( serverPath + @"\rock", true );

                // delete this page
                File.Delete( serverPath + @"Complete.aspx" );

                // delete a web.config if it already exists in the root, this is not the rock one
                if ( File.Exists( serverPath + @"\web.config" ) )
                {
                    File.Delete( serverPath + @"\web.config" );
                }
                
                // move the web.config into place
                File.Move( serverPath + @"\webconfig.xml", serverPath + @"\web.config" );
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
    
    private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs )
    {
        DirectoryInfo dir = new DirectoryInfo( sourceDirName );
        DirectoryInfo[] dirs = dir.GetDirectories();

        // If the source directory does not exist, throw an exception.
        if ( !dir.Exists )
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


        // Get the file contents of the directory to copy.
        FileInfo[] files = dir.GetFiles();

        foreach ( FileInfo file in files )
        {
            // Create the path to the new copy of the file.
            string temppath = Path.Combine( destDirName, file.Name );

            // Copy the file.
            file.CopyTo( temppath, false );
        }

        // If copySubDirs is true, copy the subdirectories.
        if ( copySubDirs )
        {

            foreach ( DirectoryInfo subdir in dirs )
            {
                // Create the subdirectory.
                string temppath = Path.Combine( destDirName, subdir.Name );

                // Copy the subdirectories.
                DirectoryCopy( subdir.FullName, temppath, copySubDirs );
            }
        }
    }

    private void DirectoryContentsMove( string sourceDirName, string destDirName )
    {
        // this will move the contents of a folder into the contents of another
        // if a folder already exists in the directory with the same name it
        // will be deleted. ONLY USE THIS IF THIS USE CASE WORKS FOR YOU!!!
        
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
        foreach ( var directory in sourceChildDirectories )
        {
            string destChildDirectory = Path.Combine( destDirName, directory.Name );
            if ( Directory.Exists( destChildDirectory ) )
            {
                DeleteDirectory( destChildDirectory );
            }
            Directory.Move( directory.FullName, destChildDirectory );
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
            
            // Copy the file.
            file.MoveTo( temppath );
        }
    }

</script>

</html>
