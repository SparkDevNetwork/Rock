<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TagsByLetter.ascx.cs" Inherits="RockWeb.Blocks.Core.TagsByLetter" %>

<asp:UpdatePanel ID="upnlTagCloud" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-tag"></i> Tag List</h1>
                </div>
                <div class="panel-body">
                     <div class="nav nav-pills margin-b-md">
                        <li class='<%= personalTagsCss %>'><asp:LinkButton id="lbPersonalTags" runat="server" OnClick="lbPersonalTags_Click" Text="Personal Tags" CssClass="active"></asp:LinkButton></li>
                        <li class="<%= publicTagsCss %>"><asp:LinkButton id="lbPublicTags" runat="server" OnClick="lbPublicTags_Click" Text="Organizational Tags"></asp:LinkButton></li>
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
