<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PotluckDinnerList.ascx.cs" Inherits="com.mychurch.Blocks.PotluckDinnerList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfPotluckDinnerId" runat="server" />
        <Rock:Grid ID="gPotluckDinner" runat="server" DataKeyNames="Id">
            <Columns>
                <asp:BoundField DataField="Name" HeaderText="Dinner Name" />
                <asp:BoundField DataField="PotluckDishes.Count" HeaderText="Dishes Count" />
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>

