<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DeviceGroupDetail.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor.DeviceGroupDetail" %>
<%@ Register Namespace="com.blueboxmoon.WatchdogMonitor.Web.UI.Controls" Assembly="com.blueboxmoon.WatchdogMonitor" TagPrefix="WM" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-list-ul"></i>
                    <asp:Literal ID="lName" runat="server" />
                </h1>
            </div>

            <div class="panel-body">
                <dl>
                    <dt>Description</dt>
                    <dd><asp:Literal ID="lDescription" runat="server" /></dd>
                </dl>

                <div class="actions">
                    <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="lbEdit_Click" />
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlDevices" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-desktop"></i> Devices</h1>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gDevice" runat="server" RowItemText="Device" OnGridRebind="gDevice_GridRebind">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:DeleteField OnClick="gDevice_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlEdit" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-list-ul"></i>
                    <asp:Literal ID="lEditTitle" runat="server" />
                </h1>
            </div>

            <div class="panel-body">
                <asp:ValidationSummary ID="vsEdit" runat="server" CssClass="alert alert-danger" HeaderText="Please correct the following:" />
                <asp:HiddenField ID="hfEditId" runat="server" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbEditName" runat="server" Label="Name" Required="true" MaxLength="100" ValidationGroup="Edit" />
                    </div>
                </div>

                <Rock:RockTextBox ID="tbEditDescription" runat="server" Label="Description" TextMode="MultiLine" ValidationGroup="Edit" />

                <div class="actions">
                    <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                    <asp:LinkButton ID="lbCancelSave" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="lbCancelSave_Click" />
                </div>
            </div>
        </asp:Panel>

        <Rock:ModalDialog ID="mdlAdd" runat="server" Title="Add Device" OnSaveClick="mdlAdd_SaveClick" ValidationGroup="Add">
            <Content>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlAddDevice" runat="server" Label="Device" ValidationGroup="Add" Required="true" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
