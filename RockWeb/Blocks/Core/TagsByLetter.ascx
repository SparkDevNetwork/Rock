<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TagsByLetter.ascx.cs" Inherits="RockWeb.Blocks.Core.TagsByLetter" %>

<asp:UpdatePanel ID="upnlTagCloud" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-tag"></i>Tag List</h1>
                <div class="form-inline pull-right">
                    <Rock:RockDropDownList ID="ddlEntityType" runat="server" Label="Type" CssClass="input-width-lg input-sm" AutoPostBack="true" OnSelectedIndexChanged="ddlEntityType_SelectedIndexChanged" />
                </div>
            </div>
            <div class="panel-body">
                <div class="nav nav-pills margin-b-md">
                    <li class='<%= personalTagsCss %>'>
                        <asp:LinkButton ID="lbPersonalTags" runat="server" OnClick="lbPersonalTags_Click" Text="Personal Tags" CssClass="active"></asp:LinkButton></li>
                    <li class="<%= publicTagsCss %>">
                        <asp:LinkButton ID="lbPublicTags" runat="server" OnClick="lbPublicTags_Click" Text="Organizational Tags"></asp:LinkButton></li>
                    <Rock:Toggle ID="tglStatus" runat="server" CssClass="pull-right" OffText="Active" ActiveButtonCssClass="btn-success" ButtonSizeCssClass="btn-xs" OnText="All" AutoPostBack="true" OnCheckedChanged="tgl_CheckedChanged" />
                </div>

                <asp:Literal ID="lLetters" runat="server"></asp:Literal>

                <asp:Literal ID="lTagList" runat="server"></asp:Literal>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>

<script>
    // fade-in effect for the panel
    function FadePanelIn() {
        $("[id$='upnlTagCloud']").rockFadeIn();
    }
    $(document).ready(function () { FadePanelIn(); });
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(FadePanelIn);

</script>
