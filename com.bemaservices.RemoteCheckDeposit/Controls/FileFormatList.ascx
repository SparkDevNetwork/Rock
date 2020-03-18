<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FileFormatList.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.RemoteCheckDeposit.FileFormatList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />

        <asp:Panel ID="pnlFileFormatList" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-file-o"></i> File Format List</h1>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />

                    <Rock:Grid ID="gFileFormat" runat="server" RowItemText="File Format" AllowSorting="true" OnRowSelected="gFileFormat_RowSelected" TooltipField="Description">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <asp:TemplateField HeaderText="File Format Type" SortExpression="EntityType.Name">
                                <ItemTemplate><%# GetComponentName( Eval( "EntityType" ) ) %></ItemTemplate>
                            </asp:TemplateField>
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                            <Rock:DeleteField OnClick="gFileFormat_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
