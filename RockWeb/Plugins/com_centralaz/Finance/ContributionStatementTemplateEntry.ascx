<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContributionStatementTemplateEntry.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Finance.ContributionStatementTemplateEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <Triggers>
        <asp:PostBackTrigger ControlID="btnMerge" />
    </Triggers>
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

        <asp:Panel ID="pnlEntry" runat="server" CssClass="panel panel-block">

            <asp:HiddenField ID="hfEntitySetId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-files-o"></i>
                    <asp:Literal ID="lActionTitle" runat="server" Text="Generate Statements and Letters" /></h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbNotification" runat="server" NotificationBoxType="Success" Text="Merge document submitted for processing. You will receive an email once the statements have been generated." Visible="false" />
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                <h3>Contribution Filters</h3>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockCheckBoxList ID="cblCampus" runat="server" Label="Campuses" DataTextField="Name" DataValueField="Id" RepeatDirection="Horizontal" />

                        <Rock:SlidingDateRangePicker ID="drpDates" runat="server" Label="Dates" SlidingDateRangeMode="DateRange" Required="true" />
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:CurrencyBox ID="nbMinimumAmount" runat="server" Label="Minimum Contribution Amount" NumberType="Currency" MinimumValue="0" Text="25.00" />
                            </div>
                        </div>
                        <br />
                    </div>

                    <div class="col-md-6">
                        <Rock:AccountPicker ID="apAccount1" runat="server" Label="Account 1" />
                        <Rock:AccountPicker ID="apAccount2" runat="server" Label="Account 2" />
                        <Rock:AccountPicker ID="apAccount3" runat="server" Label="Account 3" />
                        <Rock:AccountPicker ID="apAccount4" runat="server" Label="Account 4" />
                    </div>
                </div>
                <h3>Statement Template</h3>
                <div class="row">
                    <div class="col-md-3">
                        <Rock:MergeTemplatePicker ID="mtpStatement" runat="server" Label="Statement Template" Required="false" />
                    </div>
                    <div class="col-md-3">
                        <Rock:NumberBox ID="nbChapterSize" runat="server" Label="Chapter Size" Help="Number of records in a chapter" NumberType="Integer" MinimumValue="1" MaximumValue="1000" Text="300" />
                    </div>
                </div>
                <h3>Letter Template</h3>
                <div class="row">
                    <div class="col-md-3">
                        <Rock:MergeTemplatePicker ID="mtpLetter" runat="server" Label="Letter Template" Required="false" />
                    </div>
                </div>

                <Rock:NotificationBox ID="nbMergeError" runat="server" NotificationBoxType="Warning" Visible="false" CssClass="js-merge-error" />

                <div class="actions">
                    <asp:LinkButton ID="btnMerge" runat="server" Text="Merge" CssClass="btn btn-primary" OnClientClick="$('.js-merge-error').hide()" OnClick="btnMerge_Click" />
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
