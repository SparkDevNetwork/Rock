<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BenevolenceRequestDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.BenevolenceRequestDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <asp:HiddenField ID="hfBenevolenceRequestId" runat="server" />
            <div class="panel-heading">
                <h1 class="panel-title pull-left"><i class="fa fa-paste"></i> Benevolence Request</h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlStatus" runat="server" LabelType="Default" Text="Pending" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">
                <asp:ValidationSummary ID="valValidation" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                
                <div class="">
                    <div class="row">
                        <div class="col-md-3">
                            <Rock:DatePicker ID="dpRequestDate" runat="server" Label="Request Date" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="RequestDateTime" />
                        </div>
                        <div class="col-md-3">
                            <Rock:PersonPicker ID="ppCaseWorker" runat="server" Label="Case Worker" Visible="false" />
                            <Rock:RockDropDownList ID="ddlCaseWorker" runat="server" Label="Case Worker" />
                        </div>
                        <div class="col-md-3">
                            <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" />
                        </div>
                        <div class="col-md-3">
                            <Rock:RockDropDownList ID="ddlRequestStatus" runat="server" Label="Request Status" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="RequestStatusValue" />
                        </div>
                    </div>
                </div>
                
                <Rock:PanelWidget ID="wpRequestor" runat="server" Title="Requestor" Expanded="true" CssClass="margin-t-md">
                <div class="row">
                    <div class="col-md-8">
                        <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" OnSelectPerson="ppPerson_SelectPerson" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="RequestedByPersonAlias" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-4">
                        <Rock:DataTextBox ID="dtbFirstName" runat="server" Label="First Name" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="FirstName" />
                    </div>
                    <div class="col-md-4">
                        <Rock:DataTextBox ID="dtbLastName" runat="server" Label="Last Name" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="LastName" />
                    </div>
                    <div class="col-md-4">
                        <Rock:RockDropDownList ID="ddlConnectionStatus" runat="server" Label="Connection Status" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="ConnectionStatusValue" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-4">
                        <Rock:EmailBox ID="ebEmail" runat="server" Label="Email" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="Email" />
                    </div>
                    <div class="col-md-4">
                        <Rock:LocationAddressPicker ID="lapAddress" runat="server" Label="Address" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="Location" />
                    </div>
                    <div class="col-md-4">
                        <Rock:DataTextBox ID="dtbGovernmentId" runat="server" Label="Government ID" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="GovernmentId" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-4">
                        <Rock:PhoneNumberBox ID="pnbHomePhone" runat="server" Label="Home Phone" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="HomePhone" />
                    </div>
                    <div class="col-md-4">
                        <Rock:PhoneNumberBox ID="pnbCellPhone" runat="server" Label="Cell Phone" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="CellPhone" />
                    </div>
                    <div class="col-md-4">
                        <Rock:PhoneNumberBox ID="pnbWorkPhone" runat="server" Label="Work Phone" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="WorkPhone" />
                    </div>
                </div>
                </Rock:PanelWidget>

                <Rock:PanelWidget ID="pwRequest" runat="server" Title="Request Details" Expanded="true">
                    <Rock:DataTextBox ID="dtbRequestText" runat="server" Label="Description of Request" TextMode="MultiLine" Rows="4" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="RequestText" />
                
                    <Rock:DynamicPlaceHolder ID="phAttributes" runat="server" />

                    <Rock:RockControlWrapper ID="rcwDocuments" runat="server" Label="Related Documents">
                        <asp:DataList ID="dlDocuments" runat="server" CellPadding="4" RepeatDirection="Horizontal" RepeatColumns="4" >
                            <ItemTemplate>
                                <div class="margin-r-sm margin-b-sm">
                                    <Rock:FileUploader ID="fileUpDoc" BinaryFileId='<%# Container.DataItem %>' runat="server" OnFileUploaded="fileUpDoc_FileUploaded" OnFileRemoved="fileUpDoc_FileRemoved" /> 
                                </div>
                            </ItemTemplate>
                        </asp:DataList>
                    </Rock:RockControlWrapper>
                </Rock:PanelWidget>

                

                <Rock:PanelWidget ID="pwResults" runat="server" Title="Results" Expanded="true">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="dtbSummary" runat="server" Label="Result Summary" TextMode="MultiLine" Rows="3" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="ResultSummary" />
                        </div>
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="dtbProvidedNextSteps" runat="server" Label="Provided Next Steps" TextMode="MultiLine" Rows="3" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="ResultSummary" />
                        </div>
                    </div>
                
                    <Rock:Grid ID="gResults" runat="server" DisplayType="Light" AllowSorting="true" ShowActionRow="true" RowItemText="Result" AllowPaging="false" OnRowSelected="gResults_RowSelected">
                        <Columns>
                            <Rock:RockBoundField DataField="ResultTypeName" HeaderText="Result Type" SortExpression="ResultType" />
                            <Rock:CurrencyField DataField="Amount" HeaderText="Amount" SortExpression="Amount" />
                            <Rock:RockBoundField DataField="ResultSummary" HeaderText="Details" SortExpression="Details" />
                            <Rock:DeleteField OnClick="gResults_DeleteClick" />
                        </Columns>
                    </Rock:Grid>
                </Rock:PanelWidget>

                <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                <asp:LinkButton ID="lbPrint" runat="server" Text="<i class='fa fa-print'></i>" CssClass="btn btn-sm btn-default pull-right" OnClick="lbPrint_Click" />
            </div>

        </asp:Panel>

        <Rock:ModalDialog ID="mdAddResult" runat="server" ScrollbarEnabled="false" SaveButtonText="Add" OnSaveClick="btnAddResults_Click" Title="Benevolence Request Result" ValidationGroup="valResult">
            <Content>
                <asp:ValidationSummary ID="valResultsSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="valResult" />
                <asp:HiddenField ID="hfInfoGuid" runat="server" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlResultType" runat="server" Label="Result Type" ValidationGroup="valResult" SourceTypeName="Rock.Model.BenevolenceResult, Rock" PropertyName="ResultTypeValue" />
                    </div>
                    <div class="col-md-6">
                        <Rock:CurrencyBox ID="dtbAmount" runat="server" Label="Amount" ValidationGroup="valResult" SourceTypeName="Rock.Model.BenevolenceResult, Rock" PropertyName="Amount" />
                    </div>
                </div>

                <Rock:DataTextBox ID="dtbResultSummary" runat="server" Label="Details" ValidationGroup="valResult" SourceTypeName="Rock.Model.BenevolenceResult, Rock" TextMode="MultiLine" Rows="3" PropertyName="ResultSummary" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ConfirmPageUnload ID="confirmExit" runat="server" ConfirmationMessage="Changes have been made to this benevolence request that have not yet been saved." Enabled="false" />

    </ContentTemplate>
</asp:UpdatePanel>
