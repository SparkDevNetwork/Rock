<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RockControlGalleryMini.ascx.cs" Inherits="RockWeb.Blocks.Examples.RockControlGalleryMini" %>
<script type="text/javascript">
    function pageLoad() {
        prettyPrint();
    }
</script>
<style>
    .rlink {
        font-size: 16px;
        margin-left: -16px;
        outline: none;
    }

    .anchor {
        outline: none;
    }
</style>
<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <asp:ValidationSummary ID="vs" runat="server" />

            <a id="HtmlEditor"></a>
            <h2 runat="server">Rock:HtmlEditor</h2>
            <div runat="server" class="r-example">
                <Rock:HtmlEditor ID="htmlEditorFull" runat="server" Label="HtmlEditor" Toolbar="Full" Required="true" DocumentFolderRoot="External Site/Sports Ministry"/>
            </div>

            <h2 runat="server">Rock:HtmlEditor</h2>
            <div runat="server" class="r-example">
                <Rock:HtmlEditor ID="htmlEditorLight" runat="server" Label="HtmlEditor" Toolbar="Light" Required="true"/>
            </div>

            <asp:Button runat="server" ID="btnOK" CausesValidation="true" Text="Save" />

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

