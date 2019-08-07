<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SystemInfo.ascx.cs" Inherits="RockWeb.Blocks.Administration.SystemInfo" %>

<script type="text/javascript">

    function pageLoad() {

        $('#show-cache-objects').click(function () {
            $('#cache-objects').toggle('slow', function () {
                Rock.controls.modal.updateSize();
            });
        });

        $('#show-routes').click(function () {
            $('#routes').toggle('slow', function () {
                Rock.controls.modal.updateSize();
            });
        });

        $('a.show-pill').click(function () {
    	    $('ul.nav-pills > li').attr('class', '');
    	    $(this).parent().attr('class', 'active');
    	    $('div.tabContent > div').hide('slow');
    	    $('#' + $(this).attr('pill')).show('slow', function () {
    	        Rock.controls.modal.updateSize();
    	    });
        });

        if ($('div.alert.alert-success').length > 0) {
    	        window.setTimeout("fadeAndClear()", 5000);
        }
    }

    function fadeAndClear() {
    	$('div.alert.alert-success').animate({ opacity: 0 }, 2000 );
    }


</script>

<ul class="nav nav-pills margin-b-md" >
    <li class='active'><a pill="version-info" class="show-pill" href="#">Version Info</a></li>
    <li><a pill="diagnostics-tab" class="show-pill" href="#">Diagnostics</a></li>
</ul>

<div class="tabContent" >

    <div id="version-info">

        <p><strong>Rock Version: </strong>
            <asp:Literal ID="lRockVersion" runat="server"></asp:Literal></p>

        <p><strong>Client Culture Setting: </strong>
            <asp:Literal ID="lClientCulture" runat="server"></asp:Literal></p>
        
        <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Success" Title="Success" Visible="false" Text=""></Rock:NotificationBox>

        <div class="actions margin-t-xl">
            <Rock:BootstrapButton runat="server" ID="btnFlushCache" CssClass="btn btn-primary" Text="Clear Cache" OnClick="btnClearCache_Click" DataLoadingText="Clearing..." ToolTip="Flushes all cached items from the Rock cache (e.g. Pages, BlockTypes, Blocks, Attributes, etc." />
            <a href="#" Class="btn btn-link js-restart" title="Restarts the Application.">Restart Rock</a>
            <asp:Button runat="server" ID="btnRestart" OnClick="btnRestart_Click" CssClass="hidden" />
        </div>
    </div>

    <div id="diagnostics-tab" style="display:none">
        
        <h4>Details</h4>
        <p>
           <strong>Database:</strong><br />
           <asp:Literal ID="lDatabase" runat="server"></asp:Literal>
        </p>
        
        <p>
            <strong>System Date Time:</strong><br />
            <asp:Literal ID="lSystemDateTime" runat="server" />
        </p>

        <p>
            <strong>Rock Time:</strong><br /> 
            <asp:Literal ID="lRockTime" runat="server" />
        </p>

        <p>
            <strong>Process Start Time:</strong><br /> 
            <asp:Literal ID="lProcessStartTime" runat="server" />
        </p>

        <p>
            <strong>Executing Location:</strong><br />
             <asp:Literal ID="lExecLocation" runat="server"></asp:Literal>
        </p>

        <p>
            <strong>Last Migration(s):</strong><br />
             <asp:Literal ID="lLastMigrations" runat="server"></asp:Literal>
        </p>

        <div>
            <h4>Transaction Queue</h4>
            <asp:Literal ID="lTransactionQueue" runat="server"></asp:Literal>
        </div>

        <div>
            <h4>Routes</h4>
            <p><a id="show-routes" href="#">Show Routes</a></p>
            <div id="routes" style="display:none">
                <p>
                    <asp:Literal ID="lRoutes" runat="server"></asp:Literal>
                </p>
            </div>
        </div>


        <%--<div class="row">
            <div class="col-md-6">
                <h4>Transaction Queue</h4>
                <asp:Literal ID="lTransactionQueue" runat="server"></asp:Literal>
            </div>
            <div class="col-md-6">
                <h4>Routes</h4>
                <p><a id="show-routes" href="#">Show Routes</a></p>
                <div id="routes" style="display:none">
                    <p>
                    <asp:Literal ID="lRoutes" runat="server"></asp:Literal>
                    </p>
                </div>
            </div>
        </div>--%>

        <h4>Cache</h4>
        <div id="cache-details">
            <asp:Literal ID="lCacheOverview" runat="server"></asp:Literal>
        </div>

        <asp:Literal ID="lFalseCacheHits" runat="server"></asp:Literal>

        <p><a id="show-cache-objects" href="#">Show Cache Statistics</a></p>
        <div id="cache-objects" style="display:none">
            <p><asp:Literal ID="lCacheObjects" runat="server"></asp:Literal></p>
        </div>

        <div>
            <h4>Threads</h4>
            <asp:Literal ID="lThreads" runat="server"></asp:Literal>
        </div>

        <asp:LinkButton runat="server" ID="btnDumpDiagnostics" CssClass="btn btn-action margin-t-lg" OnClick="btnDumpDiagnostics_Click" ToolTip="Generates a diagnostics file for sharing with others.">
            <i class="fa fa-download"></i> Download Diagnostics File
        </asp:LinkButton>

    </div>

    <script>
        $(".js-restart").on("click", function () {
            Rock.dialogs.confirm('Are you sure you want to restart Rock?', function (result) {
                if (result) {
                    bootbox.alert("The Rock application will be restarted. You will need to reload this page to continue.")
                    __doPostBack('<%= btnRestart.UniqueID %>', '');
                }
            });
        });
    </script>

</div>

