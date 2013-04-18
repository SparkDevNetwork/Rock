<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ExceptionList.ascx.cs" Inherits="RockWeb.Blocks.Administraton.ExceptionList" %>
<asp:UpdatePanel ID="upExceptionList" runat="server">
    <ContentTemplate>
        <Rock:GridFilter ID="fExceptionList" runat="server">
            <Rock:LabeledDropDownList ID="ddlSite" runat="server" LabelText="Site" />
            <Rock:PagePicker ID="ppPage" runat="server" LabelText="Page" />
            <Rock:PersonPicker ID="ppUser" runat="server" LabelText="User" />
            <Rock:LabeledTextBox ID="txtStatusCode" runat="server" LabelText="Status Code" />
            <Rock:DateTimePicker ID="dpStartDate" runat="server" LabelText="Start Date" />
            <Rock:DateTimePicker ID="dpEndDate" runat="server" LabelText="End Date" />
        </Rock:GridFilter>
        <Rock:Grid ID="gExceptionList" runat="server" AllowSorting="true" OnRowSelected="gExceptionList_RowSelected" EmptyDataText="No Exceptions Found">
            <Columns>
                <asp:BoundField DataField="LastExceptionDate" HeaderText="Last Date" DataFormatString="{0:d}" SortExpression="LastExceptionDate" />
                <asp:BoundField DataField="SiteName" HeaderText="Site Name" SortExpression="SiteName" />
                <asp:BoundField DataField="PageName" HeaderText="Page" SortExpression="PageName" />
                <asp:BoundField DataField="Description" HeaderText="Exception" SortExpression="Description" />
                <asp:BoundField DataField="TotalCount" HeaderText="Total Count" SortExpression="TotalCount" />
                <asp:TemplateField SortExpression="SubsetCount">
                    <HeaderTemplate>
                        <asp:Label ID="lblSubsetHeader" runat="server" />
                    </HeaderTemplate>
                    <ItemTemplate>
                        <%# Eval("SubsetCount") %>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>
