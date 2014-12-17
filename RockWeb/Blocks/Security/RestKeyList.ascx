<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RestKeyList.ascx.cs" Inherits="RockWeb.Blocks.Security.RestKeyList" %>

<asp:UpdatePanel ID="upnlRestKeys" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <asp:Panel ID="pnlRestKeyList" CssClass="panel panel-block" runat="server">
            
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-key"></i> REST Key List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gRestKeyList" runat="server" EmptyDataText="No Rest Keys Found" AllowSorting="true" OnRowDataBound="gRestKeyList_RowDataBound" ShowConfirmDeleteDialog="true" OnRowSelected="gRestKeyList_RowSelected">
                        <Columns>
                            <Rock:RockBoundField DataField="LastName" HeaderText="Name" SortExpression="LastName" />
                            <Rock:RockTemplateField>
                                <HeaderTemplate>Description</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblDescription" runat="server" />
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateField>
                                <HeaderTemplate>Key</HeaderTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="lblKey" runat="server" />
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:DeleteField OnClick="gRestKeyList_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
