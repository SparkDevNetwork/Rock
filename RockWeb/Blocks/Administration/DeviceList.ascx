<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DeviceList.ascx.cs" Inherits="RockWeb.Blocks.Administration.DeviceList" %>

<asp:UpdatePanel ID="upDevice" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <Rock:GridFilter ID="fDevice" runat="server">
            <Rock:LabeledTextBox ID="tbName" runat="server" LabelText="Name" />
            <Rock:LabeledDropDownList ID="ddlDeviceType" runat="server" LabelText="Device Type" />
            <Rock:LabeledTextBox ID="tbIPAddress" runat="server" LabelText="IP Address" />
            <Rock:LabeledDropDownList ID="ddlPrintTo" runat="server" LabelText="Print To" />
            <Rock:LabeledDropDownList ID="ddlPrinter" runat="server" LabelText="Printer" DataTextField="Name" DataValueField="Id" />
            <Rock:LabeledDropDownList ID="ddlPrintFrom" runat="server" LabelText="Print From" />
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
