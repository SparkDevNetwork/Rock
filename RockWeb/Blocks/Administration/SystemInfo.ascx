<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SystemInfo.ascx.cs" Inherits="RockWeb.Blocks.Administration.SystemInfo" %>

<script type="text/javascript">

    function pageLoad() {

        $('#show-cache-objects').off('click').on('click', function () {
            $('#cache-objects').toggle(0, function () {
                Rock.controls.modal.updateSize();
            });
        });

        $('#show-routes').off('click').on('click', function () {
            $('#routes').toggle(0, function () {
                Rock.controls.modal.updateSize();
            });
        });

        if ($('div.alert.alert-success').length > 0) {
    	        window.setTimeout("fadeAndClear()", 5000);
        }

        $(".js-restart").off('click').on("click", function () {
            Rock.dialogs.confirm('Are you sure you want to restart Rock?', function (result) {
                if (result) {
                    bootbox.alert("The Rock application will be restarted. You will need to reload this page to continue.")
                    __doPostBack('<%= btnRestart.UniqueID %>', '');
                }
            });
        });

    }

    function fadeAndClear() {
    	$('div.alert.alert-success').animate({ opacity: 0 }, 2000 );
    }

</script>

<ul class="nav nav-pills margin-b-md" >
    <li id="tabVersion" runat="server" class="active">
        <asp:LinkButton ID="lnkVersionInfo" runat="server" Text="Version Info" OnClick="lbTab_Click" class="show-pill" pill="version-info" />
    </li>
    <li id="tabDiagnostics" runat="server">
        <asp:LinkButton ID="lnkDiagnostics" runat="server" Text="Diagnostics" OnClick="lbTab_Click" class="show-pill" pill="diagnostics-tab" />
    </li>
</ul>

<div class="tabContent" >
    <asp:Panel ID="pnlVersionTab" runat="server" CssClass="tab-panel">

        <p><strong>Rock Version: </strong>
            <asp:Literal ID="lRockVersion" runat="server"></asp:Literal></p>

        <p><strong>Client Culture Setting: </strong>
            <asp:Literal ID="lClientCulture" runat="server"></asp:Literal></p>

        <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Success" Title="Success" Visible="false" Text=""></Rock:NotificationBox>

        <div ID="divActions" runat="server" class="actions margin-t-xl">
            <Rock:BootstrapButton runat="server" ID="btnFlushCache" CssClass="btn btn-primary" Text="Clear Cache" OnClick="btnClearCache_Click" DataLoadingText="Clearing..." ToolTip="Flushes all cached items from the Rock cache (e.g. Pages, BlockTypes, Blocks, Attributes, etc." />
            <a href="#" class="btn btn-link js-restart" title="Restarts the Application.">Restart Rock</a>
            <asp:Button runat="server" ID="btnRestart" OnClick="btnRestart_Click" CssClass="hidden" />
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlDiagnosticsTab" runat="server" CssClass="tab-panel">

        <h4>Details</h4>
        <p>
            <strong>Database:</strong><br />
            <asp:Literal ID="lDatabase" runat="server"></asp:Literal>
        </p>

        <p>
            <strong>Lava Engine:</strong><br />
            <asp:Literal ID="lLavaEngine" runat="server"></asp:Literal>
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
            <strong>Rock Application Start Time:</strong><br />
            <asp:Literal ID="lRockApplicationStartTime" runat="server" />
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
            <div><asp:Literal ID="lTransactionQueue" runat="server"></asp:Literal></div>
            <asp:LinkButton ID="btnDrainQueue" runat="server" CssClass="btn btn-default btn-xs" Text="Drain Queue" ToolTip="Drain Queue now instead of waiting for the scheduled drain." OnClick="btnDrainQueue_Click" />
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

        <h4>Cache</h4>
        <asp:Panel ID="pnlShowCacheStatistics" runat="server" Visible="false">
            <div id="cache-details">
                <asp:Literal ID="lCacheOverview" runat="server"></asp:Literal>
            </div>

            <p><a id="show-cache-objects" href="#">Show Cache Statistics</a></p>
            <div id="cache-objects" style="display:none">
                <p><asp:Literal ID="lCacheObjects" runat="server"></asp:Literal></p>
            </div>
        </asp:Panel>
        <asp:Panel ID="pnlHideCacheStatistics" runat="server">
            <p><a id="cache-disabled" target="_top" href="/cachemanager">Cache Statistics (disabled)</a></p>
        </asp:Panel>

        <div>
            <h4>Threads</h4>
            <asp:Literal ID="lThreads" runat="server"></asp:Literal>
        </div>

        <asp:LinkButton runat="server" ID="btnDumpDiagnostics" CssClass="btn btn-action margin-t-lg" OnClick="btnDumpDiagnostics_Click" ToolTip="Generates a diagnostics file for sharing with others.">
            <i class="fa fa-download"></i> Download Diagnostics File
        </asp:LinkButton>

    </asp:Panel>
</div>