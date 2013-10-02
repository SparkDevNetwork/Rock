<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TagList.ascx.cs" Inherits="RockWeb.Blocks.Administration.TagList" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <Rock:GridFilter ID="rFilter" runat="server" >
            <Rock:RockDropDownList ID="ddlEntityType" runat="server" Label="Entity Type" />
            <Rock:RockRadioButtonList ID="rblScope" runat="server" Label="Scope" RepeatDirection="Horizontal" 
                AutoPostBack="true" OnSelectedIndexChanged="rblScope_SelectedIndexChanged">
                <asp:ListItem Value="Public" Text="Public" Selected="True" />
                <asp:ListItem Value="Private" Text="Private" />
            </Rock:RockRadioButtonList>
            <Rock:PersonPicker ID="ppOwner" runat="server" Label="Owner"  />
        </Rock:GridFilter>
        <Rock:Grid ID="rGrid" runat="server" RowItemText="Tag" OnRowSelected="rGrid_Edit">
            <Columns>
                <Rock:ReorderField />
                <asp:BoundField DataField="Id" HeaderText="Id" Visible="false" />
                <asp:BoundField DataField="Name" HeaderText="Name" />
                <asp:BoundField DataField="Owner" HeaderText="Owner" />
                <Rock:DeleteField OnClick="rGrid_Delete" />
            </Columns>
        </Rock:Grid>

    </ContentTemplate>
</asp:UpdatePanel>
