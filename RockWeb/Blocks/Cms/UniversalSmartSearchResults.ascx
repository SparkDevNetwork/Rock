<%@ Control Language="C#" AutoEventWireup="true" CodeFile="UniversalSmartSearchResults.ascx.cs" Inherits="RockWeb.Blocks.Cms.UniversalSmartSearchResults" %>

<style>
    .model-cannavigate {
        cursor: pointer;
    }
</style>

<asp:UpdatePanel ID="upnlContent" runat="server" UpdateMode="Conditional">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-search"></i> Universal Search</h1>
            </div>
            <div class="panel-body padding-t-lg">

                <asp:Literal ID="lResults" runat="server" />
            </div>
        </div>

        <script>
            Sys.Application.add_load( function () {
                $(".model-cannavigate").click(function () {
                    window.document.location = $(this).data("href");
                });
            });
        </script>

    </ContentTemplate>

    

</asp:UpdatePanel>
