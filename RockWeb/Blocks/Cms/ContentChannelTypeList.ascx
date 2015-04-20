<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentChannelTypeList.ascx.cs" Inherits="RockWeb.Blocks.Cms.ContentChannelTypeList" %>

<asp:UpdatePanel ID="upContentChannelType" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Danger" Visible="true" />        

        <asp:Panel ID="pnlContentChannelTypeList" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-lightbulb-o"></i> Content Type List</h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="gContentChannelType" runat="server" AllowSorting="true" OnRowSelected="gContentChannelType_Edit">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:BadgeField InfoMin="1" DataField="Channels" HeaderText="Channels" SortExpression="Channels" />
                            <Rock:DeleteField OnClick="gContentChannelType_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
