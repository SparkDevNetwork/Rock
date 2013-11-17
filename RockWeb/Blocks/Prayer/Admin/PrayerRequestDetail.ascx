<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PrayerRequestDetail.ascx.cs" Inherits="RockWeb.Blocks.Prayer.PrayerRequestDetail" %>

<asp:UpdatePanel ID="upPrayerRequests" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfPrayerRequestId" runat="server" />

            <div class="banner">
                <h1><asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>
            
            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-danger" />

            <!-- Edit -->
            <div id="pnlEditDetails" runat="server">

                <fieldset>
                
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbFirstName" runat="server" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="FirstName" />
                            <Rock:DataTextBox ID="tbLastName" runat="server" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="LastName" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbIsActive" Text="Active " CssClass="checkbox inline" runat="server" />
                            <Rock:RockCheckBox ID="cbApproved" Text="Approved " CssClass="checkbox inline" runat="server" />
                            <Rock:HighlightLabel ID="hlFlaggedMessage" IconCssClass="fa fa-flag" LabelType="warning" runat="server" Visible="false" ToolTip="re-approve the request to clear the flags" />
                            <Rock:RockCheckBox ID="cbIsPublic" Text="Public " CssClass="checkbox inline" runat="server" />
                            <Rock:RockCheckBox ID="cbIsUrgent" Text="Urgent " CssClass="checkbox inline" runat="server" />
                            <Rock:RockCheckBox ID="cbAllowComments" Text="Allow Comments " CssClass="checkbox inline" runat="server" />
                            <asp:Label ID="lblApprovedByPerson" runat="server" CssClass="muted text-muted" Visible="false" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:CategoryPicker ID="cpCategory" runat="server" Label="Category" Required="true" EntityTypeName="Rock.Model.PrayerRequest"/>
                            <Rock:DataTextBox ID="tbText" runat="server" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="Text" Label="Request" CssClass="field span12" TextMode="MultiLine" Rows="4"/>
                            <Rock:DataTextBox ID="tbAnswer" runat="server" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="Answer" Label="Answer" CssClass="field span12" TextMode="MultiLine" Rows="4"/>
                        </div>
                    </div>

                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

            <!-- Read only -->
            <fieldset id="fieldsetViewDetails" runat="server">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div class="pull-right">
                    <Rock:HighlightLabel ID="hlFlaggedMessageRO" LabelType="warning" runat="server" Visible="false" IconCssClass="fa fa-flag" ToolTip="To clear the flags you'll have to re-approve this request." />
                    <Rock:HighlightLabel ID="hlStatus" runat="server" LabelType="Warning" Text="Unapproved"  Visible="false" />
                    <Rock:HighlightLabel ID="hlUrgent" LabelType="Danger" runat="server" Visible="false" IconCssClass="fa fa-exclamation-circle" Text="Urgent" />
                    <Rock:Badge ID="bPrayerCount" runat="server"></Rock:Badge>
                </div>

                <div class="row">
                    <asp:Literal ID="lblMainDetails" runat="server" />
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-mini" OnClick="btnEdit_Click" />
                    <asp:LinkButton ID="btnCancelView" runat="server" Text="Back" CssClass="btn btn-mini" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </fieldset>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
