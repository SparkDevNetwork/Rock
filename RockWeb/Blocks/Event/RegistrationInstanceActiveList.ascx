<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationInstanceActiveList.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationInstanceActiveList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div id="pnlInstances" runat="server">
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-file-o"></i> Active Registration Instance List</h1>
                </div>
                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:Grid ID="gRegInstances" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gRegInstances_Edit" RowItemText="Instance" >
                            <Columns>
                                <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                                <Rock:DateField DataField="StartDateTime" HeaderText="Start Date" SortExpression="StartDateTime" />
                                <Rock:DateField DataField="EndDateTime" HeaderText="End Date" SortExpression="EndDateTime" />
                                <Rock:RockBoundField DataField="Details" HeaderText="Details" SortExpression="Details" />
                                <Rock:RockBoundField DataField="Registrants" HeaderText="Registrants" SortExpression="Registrants" />
                                <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
