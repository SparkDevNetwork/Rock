<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MailChimpAudienceList.ascx.cs" Inherits="com.bemaservices.MailChimp.MailChimpAudienceList" %>

<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server" CssClass="panel panel-block">
            <asp:HiddenField ID="hfDefinedTypeId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-envelope"></i>
                    Audiences
                </h1>
            </div>
            <div class="panel-body">

                <asp:Panel ID="pnlList" runat="server" Visible="false">

                    <asp:Panel ID="pnlValues" runat="server">
                        <Rock:ModalAlert ID="mdGridWarningValues" runat="server" />

                        <div class="grid grid-panel">
                            <Rock:Grid ID="gDefinedValues" runat="server" AllowPaging="true" EmptyDataText="No MailChimp Audiences Found" DisplayType="Full" OnRowSelected="gDefinedValues_Edit" AllowSorting="False" TooltipField="Id">
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockBoundField DataField="Value" HeaderText="Audience" />
                                    <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                </Columns>
                            </Rock:Grid>
                        </div>

                    </asp:Panel>

                </asp:Panel>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
