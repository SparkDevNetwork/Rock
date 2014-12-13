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
                        <Rock:RockDropDownList ID="ddlDeviceType" runat="server" Label="Device Type" />
                        <Rock:RockTextBox ID="tbIPAddress" runat="server" Label="IP Address" />
                        <Rock:RockDropDownList ID="ddlPrintTo" runat="server" Label="Print To" />
                        <Rock:RockDropDownList ID="ddlPrinter" runat="server" Label="Printer" DataTextField="Name" DataValueField="Id" />
                        <Rock:RockDropDownList ID="ddlPrintFrom" runat="server" Label="Print From" />
                    </Rock:GridFilter>
        
                    <Rock:Grid ID="gDevice" runat="server" AllowSorting="true" OnRowSelected="gDevice_Edit">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="DeviceTypeName" HeaderText="Device Type" SortExpression="DeviceTypeName" />
                            <Rock:RockBoundField DataField="IPAddress" HeaderText="IP Address" SortExpression="IPAddress"/>
                            <Rock:EnumField DataField="PrintToOverride" HeaderText="Print To" SortExpression="PrintToOverride" />
                            <Rock:EnumField DataField="PrintFrom" HeaderText="Print From" SortExpression="PrintFrom" />
                            <Rock:RockBoundField DataField="PrinterDeviceName" HeaderText="Printer" SortExpression="PrinterDeviceName" />
                            <Rock:DeleteField OnClick="gDevice_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
