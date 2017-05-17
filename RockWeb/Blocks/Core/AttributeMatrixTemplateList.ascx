<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AttributeMatrixTemplateList.ascx.cs" Inherits="RockWeb.Blocks.Core.AttributeMatrixTemplateList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-list-alt"></i>&nbsp;Attribute Matrix Template List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" AllowSorting="true" OnRowSelected="gList_RowSelected" RowItemText="Matrix Template">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                            <Rock:DeleteField OnClick="gList_DeleteClick" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
