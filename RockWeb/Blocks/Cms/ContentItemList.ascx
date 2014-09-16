<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentItemList.ascx.cs" Inherits="RockWeb.Blocks.Cms.ContentItemList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>
        <div id="pnlContentItems" runat="server">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-certificate "></i> Content Item List</h1>
                </div>
                <div class="panel-body">

                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="rFilter" runat="server" >
                            <%-- Approval Status, Priority Range, Ad Type, Date Range --%>
                            <Rock:RockDropDownList ID="ddlContentType" runat="server" Label="Content Type" />
                            <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                            <Rock:DateRangePicker ID="pDateRange" runat="server" Label="Date Range" />
                            <Rock:NumberRangeEditor ID="pPriorityRange" runat="server" Label="Priority Range" />
                        </Rock:GridFilter>
                    
                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                        <Rock:Grid ID="gContentItems" runat="server" DisplayType="Full" OnRowSelected="gContentItems_Edit" AllowSorting="true">
                            <Columns>
                                <asp:BoundField DataField="Title" HeaderText="Title" SortExpression="Title" />
                                <asp:BoundField DataField="ContentType" HeaderText="Content Type" SortExpression="ContentType.Name" />
                                <Rock:DateTimeField DataField="StartDateTime" HeaderText="Start" SortExpression="StartDateTime" />
                                <Rock:DateTimeField DataField="ExpireDateTime" HeaderText="Expire" SortExpression="ExpireDateTime" />
                                <asp:BoundField DataField="Priority" HeaderText="Priority" SortExpression="Priority" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                                <Rock:EnumField DataField="Status" HeaderText="Status" SortExpression="Status" />
                                <asp:BoundField DataField="Approver" HeaderText="Approver" SortExpression="Approver" />
                                <Rock:DeleteField OnClick="gContentItems_Delete" />
                            </Columns> 
                        </Rock:Grid>
                    </div>

                </div>

            </div>
        </div>
        
    </ContentTemplate>
</asp:UpdatePanel>
