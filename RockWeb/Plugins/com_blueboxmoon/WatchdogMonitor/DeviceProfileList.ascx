<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DeviceProfileList.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor.DeviceProfileList" %>
<%@ Register Namespace="com.blueboxmoon.WatchdogMonitor.Web.UI.Controls" Assembly="com.blueboxmoon.WatchdogMonitor" TagPrefix="WD" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDeviceProfileList" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-list-ul"></i> Device Profiles</h1>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gDeviceProfile" runat="server" AllowSorting="true" OnRowSelected="gDeviceProfile_RowSelected" OnRowDataBound="gDeviceProfile_RowDataBound" RowItemText="Device Profile" TooltipField="Description">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="HostServiceCheckType.Name" HeaderText="Host Check" SortExpression="HostServiceCheckType.Name" />
                            <Rock:RockBoundField DataField="CheckSchedule.Name" HeaderText="Schedule" SortExpression="CheckSchedule.Name" />
                            <Rock:DeleteField OnClick="gDeviceProfile_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>

        <Rock:ModalDialog ID="mdlEdit" runat="server" Title="Device Profile Details" ValidationGroup="Edit" OnSaveClick="mdlEdit_SaveClick">
            <Content>
                <asp:ValidationSummary ID="vsEdit" runat="server" ValidationGroup="Edit" HeaderText="Please correct the following:" />
                <Rock:NotificationBox ID="nbEditError" runat="server" NotificationBoxType="Danger" />
                <asp:HiddenField ID="hfEditId" runat="server" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbEditName" runat="server" Label="Name" MaxLength="100" Required="true" ValidationGroup="Edit" />
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlEditHostServiceCheckType" runat="server" Label="Host Service Check" Help="This service check will be used to determine if the host device is considered up or down." ValidationGroup="Edit" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbEditIconCssClass" runat="server" Label="Icon CSS Class" ValidationGroup="Edit" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlEditCheckSchedule" runat="server" Label="Check Schedule" Help="Devices will only be monitored during this schedule." ValidationGroup="Edit" />
                    </div>
                    <div class="col-md-6">
                    </div>
                </div>

                <Rock:RockTextBox ID="tbEditDescription" runat="server" Label="Description" ValidationGroup="Edit" TextMode="MultiLine" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlEditInheritedProfile" runat="server" Label="Inherited Profile" Help="Devices will also inherit SNMP and service check settings from the selected profile." ValidationGroup="Edit" CausesValidation="false" OnSelectedIndexChanged="ddlEditInheritedProfile_SelectedIndexChanged" AutoPostBack="true" />
                    </div>
                </div>

                <ul class="nav nav-pills margin-b-md">
                    <li id="liServiceChecks" runat="server" role="presentation">
                        <asp:LinkButton ID="lbServiceChecks" runat="server" Text="Service Checks" OnClick="lbServiceChecks_Click" />
                    </li>
                    <li id="liSnmp" runat="server" role="presentation">
                        <asp:LinkButton ID="lbSnmpSettings" runat="server" Text="SNMP Settings" OnClick="lbSnmpSettings_Click" />
                    </li>
                    <li id="liNrpe" runat="server" role="presentation">
                        <asp:LinkButton ID="lbNrpeSettings" runat="server" Text="NRPE Settings" OnClick="lbNrpeSettings_Click" />
                    </li>
                </ul>

                <asp:Panel ID="pnlServiceChecks" runat="server">
                    <Rock:Grid ID="gEditServiceChecks" runat="server" OnGridRebind="gEditServiceChecks_GridRebind" OnRowDataBound="gEditServiceChecks_RowDataBound">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                            <Rock:CheckBoxEditableField DataField="Enabled" HeaderText="Enabled" />
                            <asp:TemplateField HeaderText="Collector Override">
                                <ItemTemplate>
                                    <Rock:DefinedValuePicker ID="dvCollector" runat="server" Required="false" CssClass="input-sm" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <Rock:DeleteField OnClick="gEditServiceChecks_Delete" />
                        </Columns>
                    </Rock:Grid>

                    <asp:Panel ID="pnlEditInheritedServiceChecks" runat="server">
                        <h4>Inherited Service Checks</h4>

                        <div class="row">
                            <asp:Repeater ID="rptrEditInheritedServiceChecks" runat="server">
                                <ItemTemplate>
                                    <div class="col-md-4">
                                        <%# Eval( "Name" ) %>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </asp:Panel>
                </asp:Panel>

                <asp:Panel ID="pnlSnmpSettings" runat="server">
                    <Rock:RockCheckBox ID="cbEditOverrideSnmp" runat="server" Label="Override SNMP Settings" ValidationGroup="Edit" CausesValidation="false" OnCheckedChanged="cbEditOverrideSnmp_CheckedChanged" AutoPostBack="true" />
                    <WD:SnmpSettingsEditor ID="sseEditSnmpSettings" runat="server" />
                </asp:Panel>

                <asp:Panel ID="pnlNrpeSettings" runat="server">
                    <Rock:RockCheckBox ID="cbEditOverrideNrpe" runat="server" Label="Override NRPE Settings" ValidationGroup="Edit" CausesValidation="false" OnCheckedChanged="cbEditOverrideNrpe_CheckedChanged" AutoPostBack="true" />
                    <WD:NrpeSettingsEditor ID="nseEditNrpeSettings" runat="server" />
                </asp:Panel>
            </Content>
        </Rock:ModalDialog>

        <asp:LinkButton ID="lbEditAddCancel" runat="server" Text="Cancel" OnClick="lbEditAddCancel_Click" CssClass="hidden" />
        <Rock:ModalDialog ID="mdlEditAdd" runat="server" Title="Add Service Check" ValidationGroup="EditAdd" OnSaveClick="mdlEditAdd_SaveClick">
            <Content>
                <Rock:RockDropDownList ID="ddlEditAddType" runat="server" Label="Service Check Type" Required="true" ValidationGroup="EditAdd" />
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
