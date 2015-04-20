<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentChannelItemList.ascx.cs" Inherits="RockWeb.Blocks.Cms.ContentChannelItemList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        
        <asp:panel ID="pnlContent" runat="server" CssClass="panel panel-block">
            
            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lIcon" runat="server" />
                    <asp:Literal ID="lContentChannel" runat="server" /> Items
                </h1>
            </div>
            
            <div class="panel-body">
                <div class="grid grid-panel">
                    
                    <Rock:GridFilter ID="gfFilter" runat="server">
                        <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                        <Rock:DateRangePicker ID="drpDateRange" runat="server" Label="Date Range" />
                        <Rock:RockTextBox ID="tbTitle" runat="server" Label="Title" />
                    </Rock:GridFilter>

                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />

                    <Rock:Grid ID="gItems" runat="server" EmptyDataText="No Items Found" RowItemText="Item" AllowSorting="true" OnRowSelected="gItems_Edit">
                        <Columns>
                            <Rock:RockBoundField DataField="Title" HeaderText="Title" SortExpression="Title" />
                            <Rock:DateTimeField DataField="StartDateTime" HeaderText="Start" SortExpression="StartDateTime" />
                            <Rock:DateTimeField DataField="ExpireDateTime" HeaderText="Expire" SortExpression="ExpireDateTime" />
                            <Rock:RockBoundField DataField="Priority" HeaderText="Priority" SortExpression="Priority" DataFormatString="{0:N0}" ItemStyle-HorizontalAlign="Right" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

        </asp:panel>

    </ContentTemplate>
</asp:UpdatePanel>
