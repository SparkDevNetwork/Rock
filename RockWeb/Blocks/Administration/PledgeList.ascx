<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PledgeList.ascx.cs" Inherits="RockWeb.Blocks.Administration.PledgeList" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server"/>
        <Rock:GridFilter ID="rFilter" runat="server">
            <Rock:PersonPicker ID="ppPerson" runat="server"/>
        </Rock:GridFilter>
        <Rock:Grid ID="gPledges" runat="server" AllowSorting="True" OnRowDataBound="gPledges_RowDataBound" OnRowSelected="gPledges_Edit">
            <Columns>
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>