<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TrackList.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.TrackList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" runat="server">
            <h4>Tracks</h4>

            <asp:HiddenField ID="hfPeriodId" runat="server" />
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

            <Rock:Grid ID="gList" runat="server" AllowSorting="false" OnRowSelected="gList_Edit" DataKeyNames="Id" TooltipField="Description">
                <Columns>
                    <Rock:ReorderField />
                    <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                    <Rock:DeleteField OnClick="gList_Delete" />
                </Columns>
            </Rock:Grid>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
