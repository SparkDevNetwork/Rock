<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PrayerRequestDetail.ascx.cs" Inherits="RockWeb.Blocks.Prayer.PrayerRequestDetail" %>

<asp:UpdatePanel ID="upPrayerRequests" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">
            <asp:HiddenField ID="hfPrayerRequestId" runat="server" />
            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />

            <!-- Read only -->
            <fieldset id="fieldsetViewDetails" runat="server">
                <legend>Prayer Request</legend>
                <div class="well">
                    <div class="row-fluid">
                        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    </div>

                     <div class="pull-right">
                            <asp:Literal ID="litFlaggedMessageRO" runat="server"></asp:Literal>
                            <asp:Literal ID="litStatus" runat="server"></asp:Literal>
                            <asp:Literal ID="litUrgent" runat="server"></asp:Literal>
                            <a href="#" data-toggle="tooltip" data-placement="top" data-trigger="hover focus" title="current prayer count" >
                                <asp:Literal ID="litPrayerCountRO" runat="server">0</asp:Literal></a>
                    </div>

                    <div class="row-fluid">
                        <dl>
                            <dt>Name</dt>
                            <dd><asp:Literal ID="litFullName" runat="server" /></dd>

                            <dt>Category</dt>
                            <dd><asp:Literal ID="litCategory" runat="server" /></dd>

                            <dt>Request</dt>
                            <dd><asp:Literal ID="litRequest" runat="server" /></dd>
                        </dl>

                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-mini" OnClick="btnEdit_Click" />
                        <asp:LinkButton ID="btnCancelView" runat="server" Text="Back" CssClass="btn btn-mini" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>
                </div>
            </fieldset>

            <!-- Edit -->
            <fieldset id="fieldsetEditDetails" runat="server">
                <legend>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </legend>

                <div class="well pull-right">
                    <dl>
                        <asp:Literal ID="lFlaggedMessage" runat="server"></asp:Literal>
                        <dt class="muted"><asp:Label ID="lblApprovedByPerson" runat="server" Visible="false" /></dt>
                        <dt><asp:CheckBox ID="cbIsActive" Text="Active " runat="server" /></dt>
                        <dt><asp:CheckBox ID="cbApproved" Text="Approved " runat="server" /></dt>
                        <dt><asp:CheckBox ID="cbIsPublic" Text="Public " runat="server" /></dt>
                        <dt><asp:CheckBox ID="cbIsUrgent" Text="Urgent " runat="server" /></dt>
                        <dt><asp:CheckBox ID="cbAllowComments" Text="Allow Comments " runat="server" /></dt>
                    </dl>
                </div>

                <div class="row-fluid">
                    <div class="span6">
                        <Rock:DataTextBox ID="tbFirstName" runat="server" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="FirstName" />
                        <Rock:DataTextBox ID="tbLastName" runat="server" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="LastName" />
                        <Rock:CategoryPicker ID="cpCategory" runat="server" Label="Category" Required="true" EntityTypeName="Rock.Model.PrayerRequest"/>
                    </div>
                </div>
                <div class="row-fluid">
                    <div class="span12">
                        <Rock:DataTextBox ID="tbText" runat="server" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="Text" Label="Request" CssClass="field span12" TextMode="MultiLine" Rows="4"/>
                    </div>
                </div>
                <div class="row-fluid">
                    <div class="span12">
                        <Rock:DataTextBox ID="tbAnswer" runat="server" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="Answer" Label="Answer" CssClass="field span12" TextMode="MultiLine" Rows="4"/>
                    </div>
                </div>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>
              
            </fieldset>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
