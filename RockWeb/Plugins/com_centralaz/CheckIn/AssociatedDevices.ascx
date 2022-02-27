<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AssociatedDevices.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.CheckIn.AssociatedDevices" %>

<asp:UpdatePanel ID="upDevice" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">
            <asp:Panel ID="pnlReadOnly" runat="server" Visible="true">
                <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                <div class="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title"><i class="fa fa-desktop"></i> Associated Devices</h1>
                        <div class="pull-right">
                            <asp:LinkButton ID="lbEdit" runat="server" CssClass=" pull-right" CausesValidation="false" OnClick="lbEdit_Click"><i class="fa fa-gear"></i></asp:LinkButton>
                        </div>
                    </div>
                    <div class="panel-body">

                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="fDevice" runat="server">
                                <Rock:RockTextBox ID="tbName" runat="server" Label="Name" />
                                <Rock:RockDropDownList ID="ddlDeviceType" runat="server" Label="Device Type" />
                                <Rock:RockTextBox ID="tbIPAddress" runat="server" Label="IP Address" />
                                <Rock:RockDropDownList ID="ddlPrintTo" runat="server" Label="Print To" />
                                <Rock:RockDropDownList ID="ddlPrinter" runat="server" Label="Printer" DataTextField="Name" DataValueField="Id" />
                                <Rock:RockDropDownList ID="ddlPrintFrom" runat="server" Label="Print From" />
                            </Rock:GridFilter>

                            <Rock:Grid ID="gDevice" runat="server" AllowSorting="true">
                                <Columns>
                                    <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                                    <Rock:RockBoundField DataField="DeviceTypeName" HeaderText="Device Type" SortExpression="DeviceTypeName" />
                                    <Rock:RockBoundField DataField="IPAddress" HeaderText="IP Address" SortExpression="IPAddress" />
                                    <Rock:EnumField DataField="PrintToOverride" HeaderText="Print To" SortExpression="PrintToOverride" />
                                    <Rock:EnumField DataField="PrintFrom" HeaderText="Print From" SortExpression="PrintFrom" />
                                    <Rock:RockBoundField DataField="PrinterDeviceName" HeaderText="Printer" SortExpression="PrinterDeviceName" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </div>
            </asp:Panel>
            <asp:Panel ID="pnlEdit" runat="server" Visible="false">
                <div class="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title"><i class="fa fa-desktop"></i> Associated Devices</h1>
                    </div>
                    <div class="panel-body">
                        <asp:Repeater ID="rptDeviceTypes" runat="server" OnItemDataBound="rptDeviceTypes_ItemDataBound">
                            <ItemTemplate>
                                <Rock:RockCheckBoxList ID="cblDevices" runat="server" RepeatDirection="Horizontal" DataTextField="Name" DataValueField="Id" />
                            </ItemTemplate>
                        </asp:Repeater>
                        </br>
                    <asp:LinkButton ID="lbSave" runat="server" OnClick="lbSave_Click" CssClass="btn btn-primary" Text="Save" />
                        <asp:LinkButton ID="lbCancel" runat="server" OnClick="lbCancel_Click" Text="Cancel" />
                    </div>
                </div>
            </asp:Panel>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
