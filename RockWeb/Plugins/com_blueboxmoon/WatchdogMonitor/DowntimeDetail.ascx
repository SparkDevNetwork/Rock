<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DowntimeDetail.ascx.cs" Inherits="RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor.DowntimeDetail" %>
<%@ Register Namespace="com.blueboxmoon.WatchdogMonitor.Web.UI.Controls" Assembly="com.blueboxmoon.WatchdogMonitor" TagPrefix="WM" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />


        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lName" runat="server" />
                </h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlInactive" runat="server" Text="Inactive" LabelType="Default" />
                </div>
            </div>

            <div class="panel-body">
                <fieldset>
                    <div class="row">
                        <div class="col-md-6">
                            <dl>
                                <dt>Schedule</dt>
                                <dd><asp:Literal ID="lSchedule" runat="server" /></dd>
                            </dl>
                        </div>
                    </div>

                    <dl>
                        <dt>Description</dt>
                        <dd><asp:Literal ID="lDescription" runat="server" /></dd>
                    </dl>

                    <div class="row">
                        <div class="col-md-6">
                            <dl>
                                <dt>Devices</dt>
                                <dd><asp:Literal ID="lDevices" runat="server" /></dd>
                            </dl>
                        </div>

                        <div class="col-md-6">
                            <dl>
                                <dt>Device Groups</dt>
                                <dd>
                                    <asp:Literal ID="lDeviceGroups" runat="server" />
                                </dd>
                            </dl>
                        </div>
                    </div>
                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="lbEdit" runat="server" CssClass="btn btn-primary" Text="Edit" OnClick="lbEdit_Click" />
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlEdit" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">Edit Downtime</h1>
            </div>

            <div class="panel-body">
                <asp:ValidationSummary ID="vsEdit" runat="server" ValidationGroup="Edit" HeaderText="Please correct the following:" />
                <Rock:NotificationBox ID="nbEditError" runat="server" NotificationBoxType="Danger" />
                <asp:HiddenField ID="hfEditId" runat="server" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbEditName" runat="server" Label="Name" MaxLength="50" Required="true" ValidationGroup="Edit" />
                        <Rock:RockDropDownList ID="ddlEditSchedule" runat="server" Label="Schedule" ValidationGroup="Edit" Required="true" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbEditIsActive" runat="server" Label="Active" ValidationGroup="Edit" />
                    </div>
                </div>

                <Rock:RockTextBox ID="tbEditDescription" runat="server" Label="Description" ValidationGroup="Edit" TextMode="MultiLine" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockControlWrapper ID="rcwDevices" runat="server" Label="Device">
                            <asp:Repeater ID="rpDevices" runat="server" OnItemCommand="rpDevices_ItemCommand">
                                <ItemTemplate>
                                    <div class="control-static"><%# Eval( "Name" ) %> <asp:LinkButton ID="lbDeleteDevice" runat="server" CssClass="btn btn-link" CommandArgument='<%# Eval( "Id" ) %>' CommandName="Delete" CausesValidation="false"><i class="fa fa-times"></i></asp:LinkButton></div>
                                </ItemTemplate>
                            </asp:Repeater>

                            <Rock:RockDropDownList ID="ddlDevices" runat="server" OnSelectedIndexChanged="ddlDevices_SelectedIndexChanged" AutoPostBack="true" CausesValidation="false" />
                        </Rock:RockControlWrapper>
                    </div>

                    <div class="col-md-6">
                        <Rock:RockControlWrapper ID="rcwDeviceGroups" runat="server" Label="Device Groups">
                            <asp:Repeater ID="rpDeviceGroups" runat="server" OnItemCommand="rpDeviceGroups_ItemCommand">
                                <ItemTemplate>
                                    <div class="control-static"><%# Eval( "Name" ) %> <asp:LinkButton ID="lbDeleteDeviceGroup" runat="server" CssClass="btn btn-link" CommandArgument='<%# Eval( "Id" ) %>' CommandName="Delete" CausesValidation="false"><i class="fa fa-times"></i></asp:LinkButton></div>
                                </ItemTemplate>
                            </asp:Repeater>

                            <Rock:RockDropDownList ID="ddlDeviceGroups" runat="server" OnSelectedIndexChanged="ddlDeviceGroups_SelectedIndexChanged" AutoPostBack="true" CausesValidation="false" />
                        </Rock:RockControlWrapper>
                    </div>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" ValidationGroup="Edit" />
                    <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="lbCancel_Click" CausesValidation="false" />
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
