<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BulkPersonInactivator.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Core.BulkPersonInactivator" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbConfigurationWarning" runat="server" NotificationBoxType="Warning" Visible="false" Text="A Query or DataView need to specified in block settings." />
        
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h4 class="panel-title pull-left">Bulk Person Inactivator</h4>
            </div>
            <div class="panel-body">
                <Rock:RockLiteral ID="lDataViewName" runat="server" Label="DataView" />
                <Rock:RockLiteral ID="lRecordCount" runat="server" Label="Record Count" />

                
                <asp:LinkButton ID="btnInactivateRecords" runat="server" CssClass="btn btn-primary" Text="Inactivate Records" OnClick="btnInactivateRecords_Click" />
            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
