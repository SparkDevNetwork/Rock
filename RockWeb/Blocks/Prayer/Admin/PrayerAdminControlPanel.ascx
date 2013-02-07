<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PrayerAdminControlPanel.ascx.cs" Inherits="RockWeb.Blocks.Prayer.PrayerAdminControlPanel" %>
                    <!-- <Rock:CustomCheckboxField DataField="IsApproved"  OnCheckChanged="gPrayerRequests_CheckChanged" HeaderText="Approval Status" SortExpression="IsApproved" /> -->

<asp:UpdatePanel ID="upPrayerRequests" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlLists" runat="server" Visible="true">
            <h3>Prayer Requests</h3>
            <Rock:GridFilter ID="rFilter" runat="server">
                <Rock:LabeledDropDownList ID="ddlCategoryFilter" runat="server" LabelText="Category" />
                <Rock:DateTimePicker ID="dtRequestEnteredDateRangeStartDate" runat="server" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="EnteredDate" LabelText="From date" />
                <Rock:DateTimePicker ID="dtRequestEnteredDateRangeEndDate" runat="server" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="EnteredDate" LabelText="To date" />
                <Rock:LabeledCheckBox ID="cbShowApproved" runat="server" LabelText="Show approved" />
            </Rock:GridFilter>
            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            <Rock:Grid ID="gPrayerRequests" runat="server" AllowSorting="true" OnRowSelected="gPrayerRequests_Edit" ShowActionExcelExport="false">
                <Columns>
                    <asp:BoundField DataField="FullName" HeaderText="Name" SortExpression="FirstName" />
                    <asp:BoundField DataField="Category.Name" HeaderText="Category" SortExpression="Category.Name" />
                    <Rock:DateField DataField="EnteredDate" HeaderText="Entered" SortExpression="EnteredDate"/>
                    <asp:BoundField DataField="Text" HeaderText="Request" SortExpression="Text" />
                    <Rock:CustomCheckboxField DataField="IsApproved" HeaderText="Approved? ccbf" SortExpression="IsApproved" OnCheckChanged="gPrayerRequests_CheckChanged" />
                    <Rock:ToggleField DataField="IsApproved" HeaderText="Approval Status" SortExpression="IsApproved" OnCheckedChanged="gPrayerRequests_CheckChanged" />
                    <asp:BoundField DataField="FlagCount" HeaderText="Flag Count" SortExpression="FlagCount" />
                    <Rock:DeleteField OnClick="gPrayerRequests_Delete"  />
                </Columns>
            </Rock:Grid>

            <h3>Prayer Team Comments/Responses</h3>
            <Rock:Grid ID="gPrayerComments" runat="server" AllowSorting="true" OnRowSelected="gPrayerComments_Edit" ShowActionExcelExport="false">
                <Columns>
                    <asp:BoundField DataField="Caption" HeaderText="From" SortExpression="Text" />
                    <asp:BoundField DataField="Text" HeaderText="Comment" SortExpression="Text" />
                    <Rock:DeleteField OnClick="gPrayerComments_Delete" />
                </Columns>
            </Rock:Grid>
        </asp:Panel>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            <asp:HiddenField ID="hfPrayerRequestId" runat="server" />
            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />
            <fieldset>
                <legend>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </legend>

                <asp:UpdatePanel ID="upRequestDetails" runat="server">
                    <ContentTemplate>
                        <div class="well pull-right">

                            <asp:Literal ID="lFlaggedMessage" runat="server"></asp:Literal>

                            <Rock:LabeledText ID="ltPrayerRequestApprovalStatus" runat="server" LabelText="Approval Status" />
                            <asp:HiddenField ID="hfPrayerRequestApprovalStatus" runat="server" />
                            <asp:HiddenField ID="hfPrayerRequestFlagCount" runat="server" />
                            <div class="controls">
                                <asp:Label ID="lblApprovedByPerson" runat="server" Visible="false" />
                            </div>
                            <asp:HiddenField ID="hfApprovedByPersonId" runat="server" />
                            <asp:LinkButton ID="btnApproveRequest" runat="server" OnClick="btnApproveRequest_Click" CssClass="btn btn-primary btn-mini" Text="<i class='icon-ok'></i> Approve" />
                            <asp:LinkButton ID="btnDenyRequest" runat="server" OnClick="btnDenyRequest_Click" CssClass="btn btn-mini" Text="<i class='icon-ban-circle'></i> Deny" />
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>

                <div class="row-fluid">
                    <div class="span6">
                        <Rock:DataTextBox ID="tbFirstName" runat="server" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="FirstName" />
                        <Rock:DataTextBox ID="tbLastName" runat="server" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="LastName" />
                        <Rock:DataDropDownList ID="ddlCategory" runat="server" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="Category" />
                    </div>
                </div>
                <div class="row-fluid">
                    <div class="span12">
                        <Rock:DataTextBox ID="tbText" runat="server" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="Text" LabelText="Request" CssClass="field span12" TextMode="MultiLine" Rows="4"/>
                    </div>
                </div>
            </fieldset>

            <div class="actions">
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
