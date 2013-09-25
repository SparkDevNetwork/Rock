<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MarketingCampaignAdDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.MarketingCampaignAdDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <!-- Ad Details Controls -->
        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            <asp:ValidationSummary ID="ValidationSummary2" runat="server" CssClass="alert alert-error" />
            <asp:HiddenField ID="hfMarketingCampaignId" runat="server" />
            <asp:HiddenField ID="hfMarketingCampaignAdId" runat="server" />
            <asp:UpdatePanel ID="upAdApproval" runat="server">
                <ContentTemplate>
                    <div class="well pull-right">
                        <Rock:RockLiteral ID="ltMarketingCampaignAdStatus" runat="server" Label="Approval Status" />
                        <asp:HiddenField ID="hfMarketingCampaignAdStatus" runat="server" />
                        <div class="controls">
                            <asp:Label ID="lblMarketingCampaignAdStatusPerson" runat="server" />
                        </div>
                        <asp:HiddenField ID="hfMarketingCampaignAdStatusPersonId" runat="server" />
                        <asp:LinkButton ID="btnApproveAd" runat="server" OnClick="btnApproveAd_Click" CssClass="btn btn-primary btn-mini" Text="Approve" />
                        <asp:LinkButton ID="btnDenyAd" runat="server" OnClick="btnDenyAd_Click" CssClass="btn btn-mini" Text="Deny" />
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
            <fieldset>
                <legend>
                    <asp:Literal ID="lActionTitleAd" runat="server" />
                </legend>
                <div class="row-fluid">
                    <div class="span6">
                        <Rock:DataDropDownList ID="ddlMarketingCampaignAdType" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.MarketingCampaignAdType, Rock" PropertyName="Name"
                            Label="Ad Type" AutoPostBack="true" OnSelectedIndexChanged="ddlMarketingCampaignAdType_SelectedIndexChanged" />
                        <Rock:DatePicker ID="tbAdDateRangeStartDate" runat="server" SourceTypeName="Rock.Model.MarketingCampaignAd, Rock" PropertyName="StartDate" Label="Start Date" Required="true" />
                        <Rock:DatePicker ID="tbAdDateRangeEndDate" runat="server" SourceTypeName="Rock.Model.MarketingCampaignAd, Rock" PropertyName="EndDate" Label="End Date" Required="true" />
                    </div>

                    <div class="span6">
                        <Rock:DataTextBox ID="tbUrl" runat="server" SourceTypeName="Rock.Model.MarketingCampaignAd, Rock" PropertyName="Url" Label="Url" />
                        <Rock:DataTextBox ID="tbPriority" runat="server" SourceTypeName="Rock.Model.MarketingCampaignAd, Rock" PropertyName="Priority" Label="Priority" />
                    </div>
                </div>

                <div class="attributes">
                    <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                </div>
                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click"></asp:LinkButton>
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" OnClick="btnCancel_Click" CausesValidation="false"></asp:LinkButton>
                </div>
            </fieldset>
        </asp:Panel>
        <script type="text/javascript">
            // change approval status to pending if any values are changed
            Sys.Application.add_load(function () {
                $("#<%=upDetail.ClientID%> :input").change(function () {
                    $(".MarketingCampaignAdStatus").removeClass('alert-success alert-error').addClass('alert-info');
                    $(".MarketingCampaignAdStatus").text('Pending Approval');

                    $('#<%=hfMarketingCampaignAdStatus.ClientID%>').val("1");
                    $('#<%=hfMarketingCampaignAdStatusPersonId.ClientID%>').val("");
                    $("#<%=lblMarketingCampaignAdStatusPerson.ClientID %>").hide();
                });
            })
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
