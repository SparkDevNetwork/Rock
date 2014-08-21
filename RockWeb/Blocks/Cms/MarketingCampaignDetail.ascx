<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MarketingCampaignDetail.ascx.cs" Inherits="RockWeb.Blocks.Cms.MarketingCampaignDetail" %>

<asp:UpdatePanel ID="upMarketingCampaigns" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfMarketingCampaignId" runat="server" />

                <div id="pnlEditDetails" class="panel panel-block" runat="server">
                    <div class="panel-heading">
                        <h1 class="panel-title"><i class="fa fa-bullhorn"></i> <asp:Literal ID="lActionTitle" runat="server" /></h1>
                    </div>
                    <div class="panel-body">

                        <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                        <fieldset>
                    
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:DataTextBox ID="tbTitle" runat="server" SourceTypeName="Rock.Model.MarketingCampaign, Rock" PropertyName="Title" />
                                    <Rock:PersonPicker ID="ppContactPerson" runat="server" Label="Contact" OnSelectPerson="ppContactPerson_SelectPerson" />
                                    <Rock:DataTextBox ID="tbContactEmail" runat="server" SourceTypeName="Rock.Model.MarketingCampaign, Rock" PropertyName="ContactEmail" Label="Contact Email" />
                                    <Rock:DataTextBox ID="tbContactPhoneNumber" runat="server" SourceTypeName="Rock.Model.MarketingCampaign, Rock" PropertyName="ContactPhoneNumber" Label="Contact Phone" />
                                    <Rock:DataTextBox ID="tbContactFullName" runat="server" SourceTypeName="Rock.Model.MarketingCampaign, Rock" PropertyName="ContactFullName" Label="Contact Name" />
                                    <Rock:DataDropDownList ID="ddlEventGroup" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.Group, Rock" PropertyName="Name" Label="Linked Event" />
                                </div>
                                <div class="col-md-6">
                                    <Rock:CampusesPicker ID="cpCampuses" runat="server" />
                            
                                    <div class="grid">
                                        <Rock:Grid ID="gMarketingCampaignAudiencesPrimary" runat="server" DisplayType="Light">
                                            <Columns>
                                                <asp:BoundField DataField="Name" HeaderText="Primary Audience" />
                                                <Rock:DeleteField OnClick="gMarketingCampaignAudiences_Delete" />
                                            </Columns>
                                        </Rock:Grid>
                                    </div>
                                    <div class="grid">
                                        <Rock:Grid ID="gMarketingCampaignAudiencesSecondary" runat="server" DisplayType="Light">
                                            <Columns>
                                                <asp:BoundField DataField="Name" HeaderText="Secondary Audience" />
                                                <Rock:DeleteField OnClick="gMarketingCampaignAudiences_Delete" />
                                            </Columns>
                                        </Rock:Grid>
                                    </div>
                                </div>
                            </div>
                        </fieldset>

                        <div class="actions">
                            <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                            <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                        </div>
                    </div>
                </div>

                <fieldset id="fieldsetViewDetails" class="panel panel-block" runat="server">
                
                    <div class="panel-heading">
                        <h1 class="panel-title"><i class="fa fa-bullhorn"></i> <asp:Literal ID="lCampaignTitle" runat="server" /></h1>
                        <div class="panel-labels">
                            <asp:Literal ID="lCampusLabels" runat="server"></asp:Literal>
                        </div>
                    </div>
                    <div class="panel-body">

                        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                        <div class="row">
                            <div class="col-md-6">
                                <asp:Literal ID="lDetailsCol1" runat="server" />
                            </div>
                            <div class="col-md-6">
                                <asp:Literal ID="lDetailsCol2" runat="server" />
                            </div>
                        </div>
                        <div class="actions">
                            <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                        </div>
                    </div>

                </fieldset>

            

            

        </asp:Panel>

        <Rock:NotificationBox ID="nbWarning" runat="server" Title="Warning" NotificationBoxType="Warning" Visible="false" />

        <asp:Panel ID="pnlMarketingCampaignAudiencePicker" CssClass="panel panel-block" runat="server" Visible="false">
            
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-users"></i> Audience Selection</h1>
            </div>
            <div class="panel-body">

                <Rock:DataDropDownList ID="ddlMarketingCampaignAudiences" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.MarketingCampaignAudience, Rock"
                PropertyName="Name" Label="Select Audiences" />
                <asp:HiddenField ID="hfMarketingCampaignAudienceIsPrimary" runat="server" />
                <div class="actions">
                    <asp:LinkButton ID="btnAddMarketingCampaignAudience" runat="server" Text="Add" CssClass="btn btn-primary" OnClick="btnAddMarketingCampaignAudience_Click"></asp:LinkButton>
                    <asp:LinkButton ID="btnCancelAddMarketingCampaignAudience" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancelAddMarketingCampaignAudience_Click"></asp:LinkButton>
                </div>

            </div>
            
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
