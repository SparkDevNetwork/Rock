<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentTypeList.ascx.cs" Inherits="RockWeb.Blocks.Cms.ContentTypeList" %>

<asp:UpdatePanel ID="upContentType" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />        

        <asp:Panel ID="pnlContentTypeList" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-lightbulb-o"></i> Content Type List</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gContentType" runat="server" AllowSorting="true" OnRowSelected="gContentType_Edit">
                        <Columns>
                            <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <asp:BoundField DataField="Channels" HeaderText="Channels" SortExpression="Channels" ItemStyle-HorizontalAlign="Right" />
                            <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                            <Rock:SecurityField TitleField="Name" />
                            <Rock:DeleteField OnClick="gContentType_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
