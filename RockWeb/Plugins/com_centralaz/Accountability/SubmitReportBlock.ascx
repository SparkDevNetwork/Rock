<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SubmitReportBlock.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Accountability.SubmitReportBlock" %>
<asp:UpdatePanel ID="upnlContent" runat="server">

    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">

            <asp:HiddenField ID="hfGroupTypeId" runat="server" />

            <div class="panel panel-block">
                <div class="panel-body">
                    <h1>
                        <asp:Literal ID="lStatusMessage" runat="server" />

                        <span class="pull-right">
                            <asp:LinkButton ID="lbSubmitReport" runat="server" Text="Submit Report" CssClass="btn btn-lg btn-primary" OnClick="lbSubmit_Click" CausesValidation="false" />
                        </span>

                    </h1>
                </div>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
