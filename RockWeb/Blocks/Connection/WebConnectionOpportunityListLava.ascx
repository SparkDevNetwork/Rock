<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WebConnectionOpportunityListLava.ascx.cs" Inherits="RockWeb.Blocks.Connection.WebConnectionOpportunityListLava" %>

<asp:UpdatePanel ID="upConnectionSelectLava" runat="server">
    <ContentTemplate>
        <!-- Content -->
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <div class="panel-body">
                <div class="row">
                    <div class="col-xs-12">
                        <h2>
                            <asp:Literal ID="lTitle" runat="server"></asp:Literal>
                        </h2>
                    </div>
                </div>
                <div class="row">
                    <div class="col-xs-12 pb-2">
                        <asp:LinkButton ID="lbOptions" runat="server" CssClass="text-muted text-semibold pull-right pr-1" OnClick="lbOptions_Click"><i class="fa fa-sliders"></i>&nbsp;&nbsp;Options</asp:LinkButton>
                    </div>
                </div>
                <div class="row">
                    <div class="col-xs-12">
                        <asp:Literal ID="lContent" runat="server"></asp:Literal>
                    </div>
                </div>
            </div>
        </asp:Panel>


        <Rock:ModalDialog ID="mdOptions" runat="server" Title="Options" SaveButtonText="Save" OnSaveClick="mdOptions_SaveClick">
            <Content>
                <div class="row">
                    <div class="col-xs-12">
                        <Rock:Switch
                            ID="swOnlyShowOpportunitiesWithRequestsForUser"
                            runat="server"
                            Checked="true"
                            FormGroupCssClass="custom-switch-centered hide-label-sm"
                            Text="Only Show Opportunities with Requests Assigned to Me" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
