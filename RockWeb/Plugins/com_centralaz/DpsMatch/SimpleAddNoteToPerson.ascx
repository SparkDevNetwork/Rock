<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SimpleAddNoteToPerson.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.DpsMatch.SimpleAddNoteToPerson" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title text-danger"><i class="fa fa-exclamation-triangle text-danger"></i> Add Note</h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlblPersonName" runat="server" LabelType="Danger" Text="Label" />
                </div>
            </div>
            <div class="panel-body">
                <Rock:RockTextBox id="tbNoteText" runat="server" TextMode="MultiLine" Rows="3"></Rock:RockTextBox>
            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>