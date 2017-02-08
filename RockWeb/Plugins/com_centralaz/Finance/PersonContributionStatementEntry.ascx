<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonContributionStatementEntry.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Finance.PersonContributionStatementEntry" %>

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
                    <asp:Literal ID="lActionTitle" runat="server" Text="Create Merge Document" /></h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbNotification" runat="server" NotificationBoxType="Success" Text="Merge document submitted for processing. You will receive an email once the statements have been generated." Visible="false" />
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" Required="true" IncludeBusinesses="true" />

                        <Rock:SlidingDateRangePicker ID="drpDates" runat="server" Label="Dates" SlidingDateRangeMode="DateRange" Required="true" />
                        <br />
                        <Rock:MergeTemplatePicker ID="mtpMergeTemplate" runat="server" Label="Merge Template" Required="true" />
                    </div>

                    <div class="col-md-6">
                        <Rock:AccountPicker ID="apAccount1" runat="server" Label="Account 1" />
                        <Rock:AccountPicker ID="apAccount2" runat="server" Label="Account 2" />
                        <Rock:AccountPicker ID="apAccount3" runat="server" Label="Account 3" />
                        <Rock:AccountPicker ID="apAccount4" runat="server" Label="Account 4" />
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
