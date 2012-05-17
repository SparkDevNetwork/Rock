<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Blocks.ascx.cs" Inherits="RockWeb.Blocks.Administration.Blocks" %>

<asp:UpdatePanel ID="upBlocks" runat="server">
<ContentTemplate>

    <script type="text/javascript">
        Sys.Application.add_load(function () {

            $('td.grid-icon-cell.delete a').click(function () {
                return confirm('Are you sure you want to delete this block?');
            });

        });
    </script>

    <asp:Literal ID="lScript" runat="server">

    </asp:Literal>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert-message block-massage error"/>

    <asp:PlaceHolder ID="phList" runat="server">
        
        <Rock:Grid ID="gBlocks" runat="server" EmptyDataText="No Blocks Found" AllowSorting="true" >
            <Columns>
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <asp:BoundField HeaderText="Path" DataField="Path" SortExpression="Path" />
                <asp:BoundField HeaderText="Description" DataField="Description" />
                <Rock:BoolField DataField="System" HeaderText="System" />
                <Rock:EditField OnClick="gBlocks_Edit" />
                <Rock:DeleteField OnClick="gBlocks_Delete" />
            </Columns>
        </Rock:Grid>

    </asp:PlaceHolder>

    <Rock:ModalDialog ID="mdDetails" runat="server">
    <Content>
        <asp:HiddenField ID="hfBlockId" runat="server" />
        <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert-message block-message error"/>
        <fieldset>
            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.CMS.Block, Rock" PropertyName="Name" />
            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.CMS.Block, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
            <Rock:DataTextBox ID="tbPath" runat="server" SourceTypeName="Rock.CMS.Block, Rock" PropertyName="Path" />
        </fieldset>
    </Content>
    </Rock:ModalDialog>

</ContentTemplate>
</asp:UpdatePanel>

