<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DeviceList.ascx.cs" Inherits="RockWeb.Blocks.Core.DeviceList" %>

<asp:UpdatePanel ID="upDevice" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-desktop"></i> Device List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="fDevice" runat="server">
                        <Rock:RockTextBox ID="tbName" runat="server" Label="Name" />
                        <Rock:DefinedValuePicker ID="dvpDeviceType" runat="server" Label="Device Type" />
                        <Rock:RockTextBox ID="tbIPAddress" runat="server" Label="IP Address" />
                        <Rock:RockDropDownList ID="ddlPrintTo" runat="server" Label="Print To" />
                        <Rock:RockDropDownList ID="ddlPrinter" runat="server" Label="Printer" DataTextField="Name" DataValueField="Id" EnhanceForLongLists="true" />
                        <Rock:RockDropDownList ID="ddlPrintFrom" runat="server" Label="Print From" />
                        <Rock:RockDropDownList ID="ddlActiveFilter" runat="server" Label="Active Status">
                            <asp:ListItem Text="[All]" Value="all"></asp:ListItem>
                            <asp:ListItem Text="Active" Value="True"></asp:ListItem>
                            <asp:ListItem Text="Inactive" Value="False"></asp:ListItem>
                        </Rock:RockDropDownList>
                    </Rock:GridFilter>
        
                    <Rock:Grid ID="gDevice" runat="server" RowItemText="Device" AllowSorting="true" OnRowSelected="gDevice_Edit" OnRowDataBound="gDevice_RowDataBound">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="DeviceTypeName" HeaderText="Device Type" SortExpression="DeviceTypeName" />
                            <Rock:RockBoundField DataField="IPAddress" HeaderText="IP Address / Hostname" SortExpression="IPAddress"/>
                            <Rock:RockLiteralField ID="lPrintToOverride" HeaderText="Print To" SortExpression="PrintToOverride" />
                            <Rock:RockLiteralField ID="lPrintFrom" HeaderText="Print From" SortExpression="PrintFrom" />
                            <Rock:RockLiteralField ID="lPrinterDeviceName" HeaderText="Printer" SortExpression="PrinterDeviceName" />
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
