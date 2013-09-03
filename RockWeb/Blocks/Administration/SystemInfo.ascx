<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SystemInfo.ascx.cs" Inherits="RockWeb.Blocks.Administration.SystemInfo" %>

<script language="javascript">

    $(document).ready(function () {

        $('#show-cache-objects').click(function () {
            $('#cache-objects').toggle('slow', function () {
                if ($('#modal-scroll-container').length) {
                    $('#modal-scroll-container').tinyscrollbar_update('relative');
                }
            });
        });

        $('a.show-pill').click(function () {

            $('ul.nav-pills > li').attr('class', '');
            $(this).parent().attr('class', 'active');
            $('div.tabContent > div').hide('slow');
            $('#' + $(this).attr('pill')).show('slow', function () {
                if ($('#modal-scroll-container').length) {
                    $('#modal-scroll-container').tinyscrollbar_update('relative');
                }
            });
        });
    });

</script>

<ul class="nav nav-pills" >
    <li class='active'><a pill="version-info" class="show-pill" href="#">Version Info</a></li>
    <li><a pill="memory-cache" class="show-pill" href="#">Memory Cache</a></li>
    <li><a pill="routes" class="show-pill" href="#">Routes</a></li>
</ul>

<div class="tabContent" >

    <div id="version-info">
        Rock Version:  <asp:Literal ID="lRockVersion" runat="server"></asp:Literal>
        <p>Executing Location: <asp:Literal ID="lExecLocation" runat="server"></asp:Literal></p>
    </div>

    <div id="memory-cache" style="display:none">
        
        <div id="cache-details">
            <asp:Literal ID="lCacheOverview" runat="server"></asp:Literal>
        </div>
        
        <a id="show-cache-objects" href="#">Show Cache Objects</a> 
        <asp:Button runat="server" ID="btnFlushCache" CssClass="btn" Text="Clear Cache" OnClick="btnClearCache_Click" ToolTip="Flushes Pages, BlockTypes, Blocks and Attributes from the Rock web cache." />

        <div id="cache-objects" style="display:none">
            <asp:Literal ID="lCacheObjects" runat="server"></asp:Literal>
        </div>

    </div>

    <div id="routes" style="display:none">
        <asp:Literal ID="lRoutes" runat="server"></asp:Literal>
    </div>

</div>

