<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountList.ascx.cs" Inherits="RockWeb.Plugins.tech_triumph.WistiaIntegration.AccountList" %>

<asp:UpdatePanel ID="upSettings" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-video-camera"></i> Accounts</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:ModalAlert id="mdGridWarning" runat="server" />
                    <Rock:Grid id="gAccount" runat="server" allowsorting="true" tooltipfield="Name" CssClass="js-grid-accounts" OnRowSelected="gAccount_Edit" OnRowDataBound="gAccount_RowDataBound">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:DateTimeField DataField="LastRefreshDateTime" NullDisplayText="No refresh has occured" ItemStyle-HorizontalAlign="Left" HeaderText="Last Refresh Date" SortExpression="LastRefreshDateTime" />
                            <asp:TemplateField HeaderText="Media Files" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right">
                                <ItemTemplate>
                                    <asp:Literal ID="lMediaFileCount" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <Rock:BoolField DataField="IsActive" HeaderText="Is Active" />
                            <Rock:DeleteField OnClick="gAccount_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
