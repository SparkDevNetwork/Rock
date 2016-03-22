<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PrayerResponse.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Prayer.PrayerResponse" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <div class="panel-body">
                <Rock:NotificationBox ID="nbWarning" runat="server" Visible="false" />

                <asp:Literal ID="lDescription" runat="server" />

                <Rock:DataTextBox ID="dtbAnswer" runat="server" Label="Answer" TextMode="MultiLine" Rows="3" MaxLength="10" ValidateRequestMode="Disabled" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="Text" Placeholder="My prayer was answered when..."></Rock:DataTextBox>
                <asp:LinkButton ID="lbSave" runat="server" AccessKey="s" Text="Save Answer" OnClick="lbSave_Click" CssClass="btn btn-primary" />
                <asp:LinkButton ID="lbExtend" runat="server" Text="Extend Request" OnClick="lbExtend_Click" CssClass="btn btn-default" CausesValidation="false" />

            </div>

        </asp:Panel>

        <asp:Panel ID="pnlResponse" runat="server" CssClass="panel panel-block">
            <div class="panel-body">
                <asp:Literal ID="lResponse" runat="server" />
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
