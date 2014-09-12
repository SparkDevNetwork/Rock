<%@ control language="C#" autoeventwireup="true" inherits="RockWeb.Blocks.Cms.MarketingCampaignAdTypeDetail, RockWeb" %>

<asp:UpdatePanel ID="upMarketingCampaignAdType" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">

            <asp:HiddenField ID="hfMarketingCampaignAdTypeId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-lightbulb-o"></i> <asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <fieldset>

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <div class="row-fluid">
                        <div class="span6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.MarketingCampaignAdType, Rock" PropertyName="Name" />
                            <Rock:RockDropDownList ID="ddlDateRangeType" runat="server" Label="Date Range Type" />
                        </div>
                        <div class="span6">
                            <div class="grid">
                                <Rock:Grid ID="gMarketingCampaignAdAttributeTypes" runat="server" AllowPaging="false" DisplayType="Light">
                                    <Columns>
                                        <asp:BoundField DataField="Name" HeaderText="Attribute Types" />
                                        <Rock:EditField OnClick="gMarketingCampaignAdAttributeType_Edit" />
                                        <Rock:DeleteField OnClick="gMarketingCampaignAdAttributeType_Delete" />
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

        </asp:Panel>

        <asp:Panel ID="pnlAdTypeAttribute" CssClass="panel panel-block" runat="server" Visible="false">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-list-ul"></i> Ad Type Attributes</h1>
            </div>
            <div class="panel-body">
                <Rock:AttributeEditor ID="edtAdTypeAttributes" runat="server" OnSaveClick="btnSaveAttribute_Click" OnCancelClick="btnCancelAttribute_Click" ValidationGroup="Attribute" />
            </div>

        </asp:Panel>

        <Rock:NotificationBox ID="nbWarning" runat="server" Title="Warning" NotificationBoxType="Warning" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
