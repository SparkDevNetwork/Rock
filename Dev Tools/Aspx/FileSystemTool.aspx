<%@ Page Language="C#" AutoEventWireup="true" CodeFile="FileSystemTool.aspx.cs" Inherits="FileSystemTool" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=10" />
    <meta charset="utf-8">
    <title>Rock Tester FileSystem Tool</title>

    <script src="Scripts/modernizr.js"></script>
    <script src="https://code.jquery.com/jquery-1.11.1.min.js"></script>
    <script src="Scripts/bootstrap.min.js"></script>

    <script type="text/javascript">
        $(function () {
            $(".btn").on('click', function () {
                var $btn = $(this).button('loading')
            });
        });
    </script>
    <link rel="stylesheet" href="Themes/Stark/Styles/bootstrap.css" />
    <link rel="stylesheet" href="Themes/Stark/Styles/theme.css" />

    <!-- Set the viewport width to device width for mobile -->
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
</head>

<body>
    <form id="form1" runat="server">

        <div class="container">
            <div class="header">

                <h3 class="text-muted">Rock RMS Tester Tools</h3>
            </div>

            <div class="jumbotron">
                <h1>FileSystem Tool</h1>
                <p class="lead">
                    Use this to assist you with making a filesystem backup and restore when no
                    FileManager utility is provided by the Hosting Provider.
                </p>
                <p>
                    Backup folder: ~/<%= _backupFolder %>
                </p>
                <asp:Panel ID="pnlAlert" runat="server" CssClass="alert" Visible="false">
                    <asp:Literal id="lMessage" runat="server" ></asp:Literal>
                </asp:Panel>
                <p>
                    <asp:LinkButton ID="btnBackupFiles" runat="server" OnClick="btnBackupFiles_Click" Text="Backup Filesystem" CssClass="btn btn-lg btn-success" data-loading-text="<i class='fa fa-cog fa-spin'></i> backing up..." />
                    <asp:LinkButton ID="btnRestoreFiles" runat="server" OnClick="btnRestoreFiles_Click" Text="Restore Filesystem" CssClass="btn btn-lg btn-warning" data-loading-text="<i class='fa fa-cog fa-spin'></i> restoring..."/>
                </p>
            </div>

            <footer class="footer">
                <p>Copyright &copy; 2014 by Spark Development Network, Inc.</p>
            </footer>

        </div>
        <!-- /container -->

    </form>
</body>
</html>
