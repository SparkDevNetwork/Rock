<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MarketingCampaignAdDetail.ascx.cs" Inherits="RockWeb.Blocks.Cms.MarketingCampaignAdDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        <!-- Ad Details Controls -->
        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            
            <div class="banner">
                <h1><asp:Literal ID="lActionTitleAd" runat="server" /></h1>
            </div>
            
            <asp:ValidationSummary ID="ValidationSummary2" runat="server" CssClass="alert alert-danger" />
            <asp:HiddenField ID="hfMarketingCampaignId" runat="server" />
            <asp:HiddenField ID="hfMarketingCampaignAdId" runat="server" />
            
            <asp:UpdatePanel ID="upAdApproval" runat="server">
                <ContentTemplate>

                    <div class="alert alert-action">

                        <asp:Literal ID="ltMarketingCampaignAdStatus" runat="server" />

                        <asp:Label ID="lblMarketingCampaignAdStatusPerson" runat="server" />

                        <div class="pull-right">    
                            <asp:LinkButton ID="btnApproveAd" runat="server" OnClick="btnApproveAd_Click" CssClass="btn btn-primary btn-xs" Text="Approve" />
                            <asp:LinkButton ID="btnDenyAd" runat="server" OnClick="btnDenyAd_Click" CssClass="btn btn-xs btn-link" Text="Deny" />
                        </div>

                        <asp:HiddenField ID="hfMarketingCampaignAdStatusPersonId" runat="server" />
                        <asp:HiddenField ID="hfMarketingCampaignAdStatus" runat="server" />
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>

            <fieldset>
                
                <div class="row">
                    <div class="col-md-6">
                        <Rock:DataDropDownList ID="ddlMarketingCampaignAdType" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.MarketingCampaignAdType, Rock" PropertyName="Name"
                            Label="Ad Type" AutoPostBack="true" OnSelectedIndexChanged="ddlMarketingCampaignAdType_SelectedIndexChanged" />
                        
                        <Rock:DatePicker ID="tbAdDateRangeStartDate" runat="server" SourceTypeName="Rock.Model.MarketingCampaignAd, Rock" PropertyName="StartDate" Label="Start Date" Required="true" />
                        <Rock:DatePicker ID="tbAdDateRangeEndDate" runat="server" SourceTypeName="Rock.Model.MarketingCampaignAd, Rock" PropertyName="EndDate" Label="End Date" Required="true" />
                    </div>

                    <div class="col-md-6">
                        <Rock:DataTextBox ID="tbUrl" runat="server" SourceTypeName="Rock.Model.MarketingCampaignAd, Rock" PropertyName="Url" Label="Url" />
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
        </asp:Panel>
        <script type="text/javascript">
            // change approval status to pending if any values are changed
            Sys.Application.add_load(function () {
                $("#<%=upDetail.ClientID%> :input").change(function () {
                    $(".MarketingCampaignAdStatus").removeClass('alert-success alert-danger').addClass('alert-info');
                    $(".MarketingCampaignAdStatus").text('Pending Approval');

                    $('#<%=hfMarketingCampaignAdStatus.ClientID%>').val("1");
                    $('#<%=hfMarketingCampaignAdStatusPersonId.ClientID%>').val("");
                    $("#<%=lblMarketingCampaignAdStatusPerson.ClientID %>").hide();
                });
            })
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
