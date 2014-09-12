<%@ control language="C#" autoeventwireup="true" inherits="RockWeb.Blocks.Core.TagReport, RockWeb" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server">

            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-check-square"></i> <asp:Literal ID="lTaggedTitle" runat="server" /></h1>
                </div>
                <div class="panel-body">

                    <div class="grid grid-panel">
                        <Rock:Grid ID="gReport" runat="server" AllowSorting="true" EmptyDataText="No Results" OnRowSelected="gReport_RowSelected" />
                    </div>

                </div>
            </div>

            

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
