<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SystemInfo.ascx.cs" Inherits="RockWeb.Blocks.Administration.SystemInfo" %>

<script language="javascript">

    $(document).ready(function() {
        $('#show-cache-details').click(function(){
            $('#cache-details').toggle('slow');
        });
    });

</script>

<h3>Memory Cache Info</h3>
<div>
    <asp:Literal ID="lCacheOverview" runat="server"></asp:Literal>
</div>
<a id="show-cache-details" href="#">Show Cache Objects</a>
<div id="cache-details" style="display:none">
    <asp:Literal ID="lCacheObjects" runat="server"></asp:Literal>
</div>
