<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GatewayList.ascx.cs" Inherits="RockWeb.Blocks.Finance.GatewayList" %>

<asp:UpdatePanel ID="pnlGatewayListUpdatePanel" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        <Rock:NotificationBox
            ID="nbInactiveWarning"
            runat="server"
            NotificationBoxType="Warning"
            Visible="false"
            Heading="Inactive Gateways"
            Text="Inactive gateways will not be selectable when configuring new configurations, but will continue to process payments where used. Consider blanking out the configured values if you would like to ensure that inactive gateways no longer work." >
        </Rock:NotificationBox>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-credit-card"></i> Gateway List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="rGridGateway" runat="server"  RowItemText="Gateway" OnRowSelected="rGridGateway_Edit" TooltipField="Description" AllowSorting="true">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <asp:TemplateField HeaderText="Gateway Type" SortExpression="EntityType.Name">
                                <ItemTemplate><%# GetComponentDisplayName( Eval( "EntityType") )%></ItemTemplate>
                            </asp:TemplateField>
                            <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                            <Rock:DeleteField OnClick="rGridGateway_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
