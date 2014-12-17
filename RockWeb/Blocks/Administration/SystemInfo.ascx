<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SystemInfo.ascx.cs" Inherits="RockWeb.Blocks.Administration.SystemInfo" %>

<script type="text/javascript">

    function pageLoad() {

        $('#show-cache-objects').click(function () {
            $('#cache-objects').toggle('slow', function () {
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

<ul class="nav nav-pills" >
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
            <asp:Button runat="server" ID="btnFlushCache" CssClass="btn btn-primary" Text="Clear Cache" OnClick="btnClearCache_Click" ToolTip="Flushes all cached items from the Rock cache (e.g. Pages, BlockTypes, Blocks, Attributes, etc." />
            <asp:Button runat="server" ID="btnRestart" CssClass="btn btn-link js-restart" Text="Restart Rock" OnClick="btnRestart_Click" ToolTip="Restarts the Application." />
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
            <%= DateTime.Now.ToString("G") + " " + DateTime.Now.ToString("zzz") %>
        </p>

        <p>
            <strong>Rock Time:</strong><br /> 
            <%= Rock.RockDateTime.Now.ToString("G") + " " + Rock.RockDateTime.OrgTimeZoneInfo.BaseUtcOffset %>
        </p>

        <p>
            <strong>Executing Location:</strong><br />
             <asp:Literal ID="lExecLocation" runat="server"></asp:Literal>
        </p>

        <h4>Cache</h4>
        <div id="cache-details">
            <asp:Literal ID="lCacheOverview" runat="server"></asp:Literal>
        </div>

<%-- This appears to have been disabled 9/19/2012:
    https://github.com/SparkDevNetwork/Rock/commit/f295069b2152d4b1ff93d44fa0d82fd2a2fb0d14#diff-357e7f0be3ea16b9658156b1ee1f8145L27    
    but this link was still here:         --%>
        <a id="show-cache-objects" href="#">Show Cache Objects</a>
        <div id="cache-objects" style="display:none">
            <asp:Literal ID="lCacheObjects" runat="server"></asp:Literal>
        </div>
        
        <h4>Routes</h4>
        <asp:Literal ID="lRoutes" runat="server"></asp:Literal>

        <asp:LinkButton runat="server" ID="btnDumpDiagnostics" CssClass="btn btn-action margin-t-lg" OnClick="btnDumpDiagnostics_Click" ToolTip="Generates a diagnostics file for sharing with others.">
            <i class="fa fa-download"></i> Download Diagnostics File
        </asp:LinkButton>

    </div>

    <script>
        $(".js-restart").on("click", function () {
            bootbox.alert("The Rock application will be restarted. You will need to reload this page to continue.")
        });
    </script>

</div>

