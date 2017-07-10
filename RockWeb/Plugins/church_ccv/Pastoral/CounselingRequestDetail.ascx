<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CounselingRequestDetail.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Pastoral.CounselingRequestDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <asp:HiddenField ID="hfCounselingRequestId" runat="server" />
            <div class="panel-heading">
                <h1 class="panel-title pull-left"><i class="fa fa-paste"></i> Counseling Request</h1>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">
                <asp:ValidationSummary ID="valValidation" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                
                <div class="">
                    <div class="row">
                        <div class="col-md-3">
                            <Rock:DatePicker ID="dpRequestDate" runat="server" Label="Request Date" SourceTypeName="church.ccv.Pastoral.Model.CareRequest, church.ccv.Pastoral" PropertyName="RequestDateTime" />
                        </div>
                        <div class="col-md-3">
                            <Rock:RockDropDownList ID="ddlWorker" runat="server" Label="Worker" SourceTypeName="church.ccv.Pastoral.Model.CareRequest, church.ccv.Pastoral" PropertyName="WorkerPersonAlias" />
                        </div>
                        <div class="col-md-3">
                            <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" />
                        </div>
                    </div>
                </div>
                
                <Rock:PanelWidget ID="wpRequestor" runat="server" Title="Requestor" Expanded="true" CssClass="margin-t-md">
                <div class="row">
                    <div class="col-md-8">
                        <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" OnSelectPerson="ppPerson_SelectPerson" SourceTypeName="church.ccv.Pastoral.Model.CareRequest, church.ccv.Pastoral" PropertyName="RequestedByPersonAlias" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-4">
                        <Rock:DataTextBox ID="dtbFirstName" runat="server" Label="First Name" SourceTypeName="church.ccv.Pastoral.Model.CareRequest, church.ccv.Pastoral" PropertyName="FirstName" />
                    </div>
                    <div class="col-md-4">
                        <Rock:DataTextBox ID="dtbLastName" runat="server" Label="Last Name" SourceTypeName="church.ccv.Pastoral.Model.CareRequest, church.ccv.Pastoral" PropertyName="LastName" />
                    </div>
                    <div class="col-md-4">
                        <Rock:RockDropDownList ID="ddlConnectionStatus" runat="server" Label="Connection Status" SourceTypeName="church.ccv.Pastoral.Model.CareRequest, church.ccv.Pastoral" PropertyName="ConnectionStatusValue" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-4">
                        <Rock:EmailBox ID="ebEmail" runat="server" Label="Email" SourceTypeName="church.ccv.Pastoral.Model.CareRequest, church.ccv.Pastoral" PropertyName="Email" />
                    </div>
                    <div class="col-md-4">
                        <Rock:LocationAddressPicker ID="lapAddress" runat="server" Label="Address" SourceTypeName="church.ccv.Pastoral.Model.CareRequest, church.ccv.Pastoral" PropertyName="Location" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-4">
                        <Rock:PhoneNumberBox ID="pnbHomePhone" runat="server" Label="Home Phone" SourceTypeName="church.ccv.Pastoral.Model.CareRequest, church.ccv.Pastoral" PropertyName="HomePhone" />
                    </div>
                    <div class="col-md-4">
                        <Rock:PhoneNumberBox ID="pnbCellPhone" runat="server" Label="Cell Phone" SourceTypeName="church.ccv.Pastoral.Model.CareRequest, church.ccv.Pastoral" PropertyName="CellPhone" />
                    </div>
                    <div class="col-md-4">
                        <Rock:PhoneNumberBox ID="pnbWorkPhone" runat="server" Label="Work Phone" SourceTypeName="church.ccv.Pastoral.Model.CareRequest, church.ccv.Pastoral" PropertyName="WorkPhone" />
                    </div>
                </div>
                </Rock:PanelWidget>

                <Rock:PanelWidget ID="pwRequest" runat="server" Title="Request Details" Expanded="true">
                    <Rock:DataTextBox ID="dtbRequestText" runat="server" Label="Description of Request" TextMode="MultiLine" Rows="4" SourceTypeName="church.ccv.Pastoral.Model.CareRequest, church.ccv.Pastoral" PropertyName="RequestText" />
                
                    <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>

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
                            <Rock:DataTextBox ID="dtbSummary" runat="server" Label="Result Summary" TextMode="MultiLine" Rows="3" SourceTypeName="church.ccv.Pastoral.Model.CareRequest, church.ccv.Pastoral" PropertyName="ResultSummary" />
                        </div>
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="dtbProvidedNextSteps" runat="server" Label="Provided Next Steps" TextMode="MultiLine" Rows="3" SourceTypeName="church.ccv.Pastoral.Model.CareRequest, church.ccv.Pastoral" PropertyName="ResultSummary" />
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
                <asp:LinkButton ID="lbPrint" runat="server" Text="<i class='fa fa-print'></i>" CssClass="btn btn-sm btn-default pull-right" OnClick="lbPrint_Click" Visible="false" />
            </div>

        </asp:Panel>

        <Rock:ModalDialog ID="mdAddResult" runat="server" ScrollbarEnabled="false" SaveButtonText="Add" OnSaveClick="btnAddResults_Click" Title="Counseling Request Result" ValidationGroup="valResult">
            <Content>
                <asp:ValidationSummary ID="valResultsSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="valResult" />
                <asp:HiddenField ID="hfInfoGuid" runat="server" />
                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlResultType" runat="server" Label="Result Type" ValidationGroup="valResult" SourceTypeName="church.ccv.Pastoral.Model.CareRequest, church.ccv.Pastoral" PropertyName="ResultTypeValue" />
                    </div>
                    <div class="col-md-6">
                        <Rock:CurrencyBox ID="dtbAmount" runat="server" Label="Amount" ValidationGroup="valResult" SourceTypeName="church.ccv.Pastoral.Model.CareRequest, church.ccv.Pastoral" PropertyName="Amount" />
                    </div>
                </div>

                <Rock:DataTextBox ID="dtbResultSummary" runat="server" Label="Details" ValidationGroup="valResult" SourceTypeName="church.ccv.Pastoral.Model.CareRequest, church.ccv.Pastoral" TextMode="MultiLine" Rows="3" PropertyName="ResultSummary" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ConfirmPageUnload ID="confirmExit" runat="server" ConfirmationMessage="Changes have been made to this counseling request that have not yet been saved." Enabled="false" />

    </ContentTemplate>
</asp:UpdatePanel>
