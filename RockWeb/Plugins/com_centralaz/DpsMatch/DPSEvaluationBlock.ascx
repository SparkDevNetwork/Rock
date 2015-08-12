<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DPSEvaluationBlock.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.DpsMatch.DPSEvaluationBlock" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
            </div>
            <div class="panel-body">
                <asp:HiddenField ID="hfSelectedColumn" runat="server" />

                <div class="grid">
                    <Rock:Grid ID="gValues" runat="server" AllowSorting="false" EmptyDataText="No Results" />
                </div>

                <div class="actions pull-right">
                    <asp:LinkButton ID="lbNext" runat="server" Text="Next" CssClass="btn btn-primary" OnClick="lbNext_Click" />
                </div>

                <Rock:NotificationBox ID="nbComplete" runat="server" Visible="false" />

            </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
