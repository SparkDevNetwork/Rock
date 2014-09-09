<%@ control language="C#" autoeventwireup="true" inherits="RockWeb.Blocks.Cms.MarketingCampaignAdDetail, RockWeb" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <!-- Ad Details Controls -->
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-certificate"></i> <asp:Literal ID="lActionTitleAd" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <asp:ValidationSummary ID="ValidationSummary2" runat="server" CssClass="alert alert-danger" />
                <asp:HiddenField ID="hfMarketingCampaignId" runat="server" />
                <asp:HiddenField ID="hfMarketingCampaignAdId" runat="server" />

                <!-- Approval -->
                <asp:UpdatePanel ID="upnlApproval" runat="server">
                    <ContentTemplate>

                        <div class="alert alert-action">

                            <asp:Label ID="lblApprovalStatus" runat="server" />

                            <asp:Label ID="lblApprovalStatusPerson" runat="server" />

                            <div class="pull-right">
                                <asp:LinkButton ID="lbApprove" runat="server" OnClick="lbApprove_Click" CssClass="btn btn-primary btn-xs" Text="Approve" />
                                <asp:LinkButton ID="lbDeny" runat="server" OnClick="lbDeny_Click" CssClass="btn btn-xs btn-link" Text="Deny" />
                            </div>

                            <asp:HiddenField ID="hfApprovalStatusPersonId" runat="server" />
                            <asp:HiddenField ID="hfApprovalStatus" runat="server" />
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>

                <fieldset>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataDropDownList ID="ddlMarketingCampaignAdType" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.MarketingCampaignAdType, Rock" PropertyName="Name"
                                Label="Ad Type" AutoPostBack="true" OnSelectedIndexChanged="ddlMarketingCampaignAdType_SelectedIndexChanged" />

                            <Rock:DateRangePicker ID="drpAdDateRange" runat="server" Label="Date Range" Required="true" />
                            <Rock:DatePicker ID="dpAdSingleDate" runat="server" SourceTypeName="Rock.Model.MarketingCampaignAd, Rock" PropertyName="StartDate" Label="Date" Required="true" />
                        </div>

                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbUrl" runat="server" SourceTypeName="Rock.Model.MarketingCampaignAd, Rock" PropertyName="Url" Label="Web Address" />
                            <Rock:DataTextBox ID="tbPriority" runat="server" SourceTypeName="Rock.Model.MarketingCampaignAd, Rock" PropertyName="Priority" Label="Priority" />
                        </div>
                    </div>

                    <div class="attributes">
                        <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click"></asp:LinkButton>
                        <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" CausesValidation="false"></asp:LinkButton>
                    </div>
                </fieldset>

            </div>

            
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
