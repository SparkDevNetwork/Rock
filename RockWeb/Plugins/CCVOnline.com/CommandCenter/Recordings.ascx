<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Recordings.ascx.cs" Inherits="RockWeb.Plugins.CCVOnline.CommandCenter.Recordings" %>

<asp:UpdatePanel ID="upRecordings" runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlList" runat="server">
        
        <Rock:Grid ID="gRecordings" runat="server" EmptyDataText="No Recordings Found">
            <Columns>
                <asp:BoundField DataField="Date" HeaderText="Date" />
                <asp:BoundField HeaderText="Label" DataField="Label" />
                <Rock:EditField OnClick="gRecordings_Edit" />
                <Rock:DeleteField OnClick="gRecordings_Delete" />
            </Columns>
        </Rock:Grid>

    </asp:Panel>

    <asp:Panel ID="pnlDetails" runat="server" Visible="false">
    
        <asp:HiddenField ID="hfRecordingId" runat="server" />

        <asp:ValidationSummary runat="server" CssClass="failureNotification"/>

        <fieldset>
            <legend><asp:Literal ID="lAction" runat="server"></asp:Literal> Recording</legend>
            <Rock:DataTextBox ID="tbDate" runat="server" SourceTypeName="Rock.Custom.CCVOnline.CommandCenter.Recording, Rock.Custom.CCVOnline" PropertyName="Date" />
            <Rock:DataTextBox ID="tbLabel" runat="server" SourceTypeName="Rock.Custom.CCVOnline.CommandCenter.Recording, Rock.Custom.CCVOnline" PropertyName="Label" />
        </fieldset>

        <div class="actions">
            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn primary" onclick="btnSave_Click" />
            <asp:LinkButton id="btnCancel" runat="server" Text="Cancel" CssClass="btn secondary" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>

    </asp:Panel>

    <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

</ContentTemplate>
</asp:UpdatePanel>

