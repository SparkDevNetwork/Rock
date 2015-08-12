<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BaptismLocationList.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Baptism.BaptismLocationList" %>

<asp:UpdatePanel ID="upnlGroupList" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlUpdate" runat="server" Visible="false">
            <div class="panel panel-block">
                <div class="panel-body">

                    <Rock:NotificationBox ID="nbExampleDanger" runat="server" Title="Danger" Text="Updates are required to the Baptism locations. Please click the update button." NotificationBoxType="Danger" />
                    <div class="panel-body pull-right">
                        <Rock:BootstrapButton ID="btnUpdate" runat="server" CssClass="btn btn-primary" OnClick="btnUpdate_Click" Text="Update" />
                    </div>
                </div>
            </div>
        </asp:Panel>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i runat="server" id="iIcon"></i>
                Baptism Locations
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gBaptismLocations" runat="server" RowItemText="Group" AllowSorting="true" OnRowSelected="gBaptismLocations_RowSelected">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="MemberCount" HeaderText="Members" SortExpression="MemberCount" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" />
                            <Rock:DeleteField OnClick="gBaptismLocations_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
