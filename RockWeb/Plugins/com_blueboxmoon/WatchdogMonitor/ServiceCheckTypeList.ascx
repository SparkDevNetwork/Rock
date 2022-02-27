<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ServiceCheckTypeList.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor.ServiceCheckTypeList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />

        <asp:Panel ID="pnlServiceCheckTypeList" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-list-ul"></i> Service Check Types</h1>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gServiceCheckType" runat="server" AllowSorting="true" OnRowSelected="gServiceCheckType_RowSelected" OnRowDataBound="gServiceCheckType_RowDataBound" RowItemText="Service Check Type" TooltipField="Description">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="Provider" HeaderText="Provider" SortExpression="Provider" />
                            <Rock:RockBoundField DataField="CheckInterval" HeaderText="Interval" SortExpression="CheckInterval" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                            <Rock:BoolField DataField="IsSystem" HeaderText="System" SortExpression="IsSystem" />
                            <Rock:DeleteField OnClick="gServiceCheckType_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>

        <Rock:ModalDialog ID="mdlEdit" runat="server" Title="Service Check Type Details" ValidationGroup="Edit" OnSaveClick="mdlEdit_SaveClick">
            <Content>
                <asp:ValidationSummary ID="vsEdit" runat="server" CssClass="alert alert-danger" HeaderText="Please correct the following:" ValidationGroup="Edit" />
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <asp:HiddenField ID="hfEditId" runat="server" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbEditName" runat="server" Label="Name" Required="true" MaxLength="100" ValidationGroup="Edit" />
                        <Rock:ComponentPicker ID="cpEditProvider" runat="server" Label="Provider" Required="true" ValidationGroup="Edit" ContainerType="com.blueboxmoon.WatchdogMonitor.ServiceCheckContainer, com.blueboxmoon.WatchdogMonitor" AutoPostBack="true" OnSelectedIndexChanged="cpEditProvider_SelectedIndexChanged" CausesValidation="false" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbEditIsActive" runat="server" Label="Is Active" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:NumberBox ID="nbEditCheckInterval" runat="server" Label="Check Interval" Required="true" ValidationGroup="Edit" NumberType="Integer" MinimumValue="1" />
                    </div>

                    <div class="col-md-6">
                        <Rock:NumberBox ID="nbEditRecheckInterval" runat="server" Label="Recheck Interval" Required="true" ValidationGroup="Edit" NumberType="Integer" MinimumValue="1" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:NumberBox ID="nbEditRecheckCount" runat="server" Label="Recheck Count" Required="true" ValidationGroup="Edit" NumberType="Integer" MinimumValue="0" />
                    </div>

                    <div class="col-md-6">
                    </div>
                </div>

                <Rock:RockTextBox ID="tbEditDescription" runat="server" Label="Description" TextMode="MultiLine" />

                <asp:PlaceHolder ID="phEditAttributes" runat="server" />
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
