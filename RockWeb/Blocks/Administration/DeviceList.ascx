<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DeviceList.ascx.cs" Inherits="RockWeb.Blocks.Administration.DeviceList" %>

<asp:UpdatePanel ID="upDevice" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <Rock:GridFilter ID="fDevice" runat="server">
            <Rock:RockTextBox ID="tbName" runat="server" Label="Name" />
            <Rock:LabeledDropDownList ID="ddlDeviceType" runat="server" Label="Device Type" />
            <Rock:RockTextBox ID="tbIPAddress" runat="server" Label="IP Address" />
            <Rock:LabeledDropDownList ID="ddlPrintTo" runat="server" Label="Print To" />
            <Rock:LabeledDropDownList ID="ddlPrinter" runat="server" Label="Printer" DataTextField="Name" DataValueField="Id" />
            <Rock:LabeledDropDownList ID="ddlPrintFrom" runat="server" Label="Print From" />
        </Rock:GridFilter>
        
        <Rock:Grid ID="gDevice" runat="server" AllowSorting="true" OnRowSelected="gDevice_Edit">
            <Columns>
                <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                <asp:BoundField DataField="DeviceTypeName" HeaderText="Device Type" SortExpression="DeviceTypeName" />
                <asp:BoundField DataField="IPAddress" HeaderText="IP Address" SortExpression="IPAddress"/>
                <Rock:EnumField DataField="PrintToOverride" HeaderText="Print To" SortExpression="PrintToOverride" />
                <Rock:EnumField DataField="PrintFrom" HeaderText="Print From" SortExpression="PrintFrom" />
                <asp:BoundField DataField="PrinterDeviceName" HeaderText="Printer" SortExpression="PrinterDeviceName" />
                <Rock:DeleteField OnClick="gDevice_Delete" />
            </Columns>
        </Rock:Grid>

    </ContentTemplate>
</asp:UpdatePanel>
