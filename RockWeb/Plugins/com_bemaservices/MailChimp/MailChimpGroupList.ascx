<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MailChimpGroupList.ascx.cs" Inherits="com.bemaservices.MailChimp.MailChimpGroupList" %>

<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server" CssClass="panel panel-block">
            <asp:HiddenField ID="hfDefinedTypeId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-envelope"></i>
                    MailChimp Groups
                </h1>
            </div>
            <div class="panel-body">

                <asp:Panel ID="pnlList" runat="server" Visible="false">

                    <asp:Panel ID="pnlValues" runat="server">
                        <Rock:ModalAlert ID="mdGridWarningValues" runat="server" />

                        <div class="grid grid-panel">
                            <Rock:Grid ID="gGroups" runat="server" AllowPaging="true" DisplayType="Full" OnRowSelected="gGroups_Edit" AllowSorting="False" TooltipField="Id">
                                <Columns>
                                    <Rock:RockBoundField DataField="Name" HeaderText="Group" />
                                </Columns>
                            </Rock:Grid>
                        </div>

                    </asp:Panel>

                </asp:Panel>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
