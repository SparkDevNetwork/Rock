<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BlockTypes.ascx.cs" Inherits="RockWeb.Blocks.Administration.BlockTypes" %>

<asp:UpdatePanel ID="upPanel" runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert-message block-message error"/>

    <Rock:Grid ID="rGrid" runat="server" EmptyDataText="No Block Types Found" AllowSorting="true" >
        <Columns>
            <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
            <asp:BoundField HeaderText="Path" DataField="Path" SortExpression="Path" />
            <asp:BoundField HeaderText="Description" DataField="Description" />
            <Rock:BoolField DataField="IsSystem" HeaderText="System" />
            <Rock:EditField OnClick="rGrid_Edit" />
            <Rock:DeleteField OnClick="rGrid_Delete" />
        </Columns>
    </Rock:Grid>

    <Rock:ModalDialog ID="modalDetails" runat="server">
    <Content>
        <asp:HiddenField ID="hfId" runat="server" />
        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert-message block-message error"/>
        <fieldset>
            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Cms.BlockType, Rock" PropertyName="Name" />
            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Cms.BlockType, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
            <Rock:DataTextBox ID="tbPath" runat="server" SourceTypeName="Rock.Cms.BlockType, Rock" PropertyName="Path" />
        </fieldset>
    </Content>
    </Rock:ModalDialog>

</ContentTemplate>
</asp:UpdatePanel>

