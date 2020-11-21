<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StepBulkEntry.ascx.cs" Inherits="RockWeb.Blocks.Steps.StepBulkEntry" %>

<Rock:NotificationBox ID="nbBlockError" runat="server" Visible="false" NotificationBoxType="Danger" />

<asp:UpdatePanel ID="pnlStepBulkEntry" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-truck"></i>
                    Step Bulk Entry
                </h1>
            </div>

            <div class="panel-body">

                <asp:ValidationSummary ID="vsValidation" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="BulkEntry" />
                <Rock:NotificationBox ID="nbNotificationBox" runat="server" Visible="false" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:StepProgramPicker Label="Step Program" runat="server" ID="sppProgramPicker" OnSelectedIndexChanged="sppProgramPicker_SelectedIndexChanged" AutoPostBack="true"></Rock:StepProgramPicker>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:StepTypePicker Label="Step Type" runat="server" Required="true" ValidationGroup="BulkEntry" ID="stpStepTypePicker" OnSelectedIndexChanged="stpStepTypePicker_SelectedIndexChanged" AutoPostBack="true"></Rock:StepTypePicker>
                    </div>
                    <div class="col-md-3 col-sm-6">
                        <Rock:DatePicker Label="Start Date" ID="dpStartDate" Required="true" ValidationGroup="BulkEntry" runat="server"></Rock:DatePicker>
                    </div>
                    <div class="col-md-3 col-sm-6">
                        <Rock:DatePicker Label="End Date" ID="dpEndDate" runat="server"></Rock:DatePicker>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:StepStatusPicker Label="Status" runat="server" Required="true" ValidationGroup="BulkEntry" ID="sspStatusPicker"></Rock:StepStatusPicker>
                    </div>
                    <div class="col-md-6">
                        <Rock:CampusPicker Label="Campus" runat="server" ValidationGroup="BulkEntry" ID="cpCampus"></Rock:CampusPicker>
                    </div>
                </div>

                <Rock:AttributeValuesContainer ID="avcBulkAttributes" runat="server" NumberOfColumns="2" />

                <hr />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:PersonPicker ID="ppPersonPicker" Label="Person" runat="server" Required="true" ValidationGroup="BulkEntry" OnSelectPerson="ppPersonPicker_SelectPerson" />
                        <div id="divPersonName" runat="server">
                            <label>Person</label>
                            <p>
                                <asp:Literal ID="lPersonName" runat="server" />
                            </p>
                        </div>
                    </div>
                </div>

                <Rock:NotificationBox ID="nbNonBulkNotificationBox" runat="server" Visible="false" />

                <Rock:AttributeValuesContainer ID="avcNonBulkAttributes" runat="server" NumberOfColumns="2" />

                <div class="actions">
                    <Rock:BootstrapButton ID="btnPrevious" runat="server" Enabled="false" Visible="false" Text="Previous Person" CssClass="btn btn-default" OnClick="btnPrevious_Click" DataLoadingText="Loading..." />
                    <Rock:BootstrapButton ID="btnNext" runat="server" Enabled="false" Visible="false" Text="Next Person" CssClass="btn btn-primary" OnClick="btnNext_Click" DataLoadingText="Loading..." ValidationGroup="BulkEntry" />
                    <span>
                        <asp:Literal ID="lSetStatus" runat="server" /></span>

                    <Rock:BootstrapButton ID="btnSave" runat="server" Enabled="false" AccessKey="s" ToolTip="Alt+s" Text="Add Step" CssClass="btn btn-primary" OnClick="btnSave_Click" DataLoadingText="Loading..." ValidationGroup="BulkEntry" />
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
