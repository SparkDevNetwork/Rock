<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TagList.ascx.cs" Inherits="RockWeb.Blocks.Administration.TagList" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <Rock:GridFilter ID="rFilter" runat="server" >
            <Rock:LabeledDropDownList ID="ddlEntityType" runat="server" LabelText="Entity Type" />
            <Rock:LabeledRadioButtonList ID="rblScope" runat="server" LabelText="Scope" RepeatDirection="Horizontal" 
                AutoPostBack="true" OnSelectedIndexChanged="rblScope_SelectedIndexChanged">
                <asp:ListItem Value="Public" Text="Public" Selected="True" />
                <asp:ListItem Value="Private" Text="Private" />
            </Rock:LabeledRadioButtonList>
            <Rock:PersonPicker ID="ppOwner" runat="server" LabelText="Owner"  />
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
