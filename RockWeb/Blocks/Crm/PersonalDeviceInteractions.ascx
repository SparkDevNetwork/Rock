<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonalDeviceInteractions.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonalDeviceInteractions" %>

<asp:UpdatePanel ID="upPersonBadge" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-mobile"></i>
                    <asp:Label ID="lblHeading" runat="server" />
                </h1>
            </div>
            <div class="panel-body">
                <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                <asp:HiddenField ID="hfPersonalDevice" runat="server" />
                <Rock:NotificationBox ID="nbError" runat="server" NotificationBoxType="Danger" Visible="false"></Rock:NotificationBox>

                <div class="grid grid-panel">
                    <Rock:Grid ID="gInteractions" runat="server" AllowSorting="true">
                        <Columns>
                            <Rock:RockBoundField DataField="InteractionDateTime" HeaderText="Date / Time" SortExpression="InteractionDateTime" />
                            <Rock:RockBoundField DataField="Operation" HeaderText="Interaction Type" SortExpression="Operation" />
                            <Rock:RockBoundField DataField="InteractionSummary" HeaderText="Details" SortExpression="InteractionSummary" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
