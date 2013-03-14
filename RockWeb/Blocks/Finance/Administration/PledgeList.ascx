<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PledgeList.ascx.cs" Inherits="RockWeb.Blocks.Finance.Administration.PledgeList" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server"/>
        <Rock:GridFilter ID="rFilter" runat="server">
            <%--<Rock:PersonPicker ID="ppPerson" runat="server"/>--%>
        </Rock:GridFilter>
        <Rock:Grid ID="gPledges" ItemType="Rock.Model.Pledge" runat="server" AllowSorting="True" SelectMethod="GetPledges">
            <Columns>
                <asp:BoundField DataField="Amount" HeaderText="Amount" SortExpression="Amount"/>
                <Rock:DateField DataField="StartDate" HeaderText="Starts" SortExpression="StartDate"/>
                <Rock:DateField DataField="EndDate" HeaderText="Ends" SortExpression="EndDate"/>
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>