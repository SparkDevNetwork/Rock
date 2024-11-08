<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BenevolenceRequestDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.BenevolenceRequestDetailView" %>
<style>
    .person-image {
        background-color: #fafafa;
        background-size: cover;
        border: none;
        border-radius: 50%;
        background-size: cover;
        border: 1px solid #dfe0e1;
    }

    .person-image-small {
        position: relative;
        box-sizing: border-box;
        display: inline-flex;
        align-items: center;
        justify-content: center;
        width: 24px;
        height: 24px;
        margin-bottom: 4px;
        vertical-align: top;
        background: center/cover #cbd4db;
        border-radius: 50%;
        box-shadow: inset 0 0 0 1px rgba(0,0,0,0.07)
    }

    .request-date {
        font-size: 14px;
        font-weight: 400;
        color: #737475;
        letter-spacing: 0.32px;
    }

    .related-documents {
        text-overflow: ellipsis;
        overflow: hidden;
    }
</style>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <!-- Edit Panel -->
        <asp:Panel ID="pnlEditDetail" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title pull-left"><i class="fa fa-paste"></i>Benevolence Request</h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlEditStatus" runat="server" LabelType="Default" Text="Pending" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdEditAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">
                <asp:ValidationSummary ID="valEditValidation" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                <div class="">
                    <div class="row">
                        <div class="col-md-3">
                            <Rock:DatePicker ID="dpEditRequestDate" runat="server" Label="Request Date" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="RequestDateTime" />
                        </div>
                        <div class="col-md-3">
                            <Rock:RockDropDownList ID="ddlEditRequestType" runat="server" AutoPostBack="true" Label="Request Type" SourceTypeName="Rock.Model.BenevolenceType, Rock" PropertyName="Name" Required="true" OnSelectedIndexChanged="ddlEditRequestType_SelectedIndexChanged" />
                        </div>
                        <div class="col-md-3">
                            <Rock:DefinedValuePicker ID="dvpEditRequestStatus" runat="server" Label="Request Status" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="RequestStatusValueId" Required="true" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-3">
                            <Rock:PersonPicker ID="ppEditCaseWorker" runat="server" Label="Assigned To" Visible="false" />
                            <Rock:RockDropDownList ID="ddlEditCaseWorker" runat="server" Label="Assigned To" EnhanceForLongLists="true" />
                        </div>
                        <div class="col-md-3">
                            <Rock:CampusPicker ID="cpEditCampus" runat="server" Label="Campus" />
                        </div>
                    </div>
                </div>

                <Rock:PanelWidget ID="wpEditRequestor" runat="server" Title="Requestor" Expanded="true" CssClass="margin-t-md">
                    <div class="row">
                        <div class="col-md-4">
                            <div class="row">
                                <div class="col-md-6 pr-md-0">
                                    <Rock:PersonPicker ID="ppEditPerson" ClientIDMode="Static" runat="server" Label="Person" OnSelectPerson="ppPerson_SelectPerson" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" FormGroupCssClass="" PropertyName="RequestedByPersonAlias" />
                                </div>
                                <div class="col-md-4 p-md-0 pt-md-4">
                                    <asp:LinkButton ID="lbEditCreatePerson" ClientIDMode="Static" runat="server" Text="Create Record From Fields" CssClass="btn btn-sm btn-link" OnClick="lbEditCreatePerson_Click"></asp:LinkButton>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-4">
                            <Rock:DataTextBox ID="dtbEditFirstName" runat="server" Label="First Name" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="FirstName" />
                        </div>
                        <div class="col-md-4">
                            <Rock:DataTextBox ID="dtbEditLastName" runat="server" Label="Last Name" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="LastName" />
                        </div>
                        <div class="col-md-4">
                            <Rock:DefinedValuePicker ID="dvpEditConnectionStatus" runat="server" Label="Connection Status" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="ConnectionStatusValue" Required="true" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-4">
                            <Rock:EmailBox ID="ebEditEmail" runat="server" Label="Email" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="Email" />
                        </div>
                        <div class="col-md-4">
                            <Rock:LocationAddressPicker ID="lapEditAddress" runat="server" Label="Address" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="Location" />
                        </div>
                        <div class="col-md-4">
                            <Rock:DataTextBox ID="dtbEditGovernmentId" runat="server" Label="Government ID" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="GovernmentId" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-4">
                            <Rock:PhoneNumberBox ID="pnbEditHomePhone" runat="server" Label="Home Phone" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="HomePhone" />
                        </div>
                        <div class="col-md-4">
                            <Rock:PhoneNumberBox ID="pnbEditCellPhone" runat="server" Label="Cell Phone" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="CellPhone" />
                        </div>
                        <div class="col-md-4">
                            <Rock:PhoneNumberBox ID="pnbEditWorkPhone" runat="server" Label="Work Phone" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="WorkPhone" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-4">
                            <Rock:RacePicker ID="rpRace" runat="server" />
                        </div>
                        <div class="col-md-4">
                            <Rock:EthnicityPicker ID="epEthnicity" runat="server" />
                        </div>
                    </div>
                </Rock:PanelWidget>

                <Rock:PanelWidget ID="pwEditRequest" runat="server" Title="Request Details" Expanded="true">
                    <Rock:DataTextBox ID="dtbEditRequestText" runat="server" Label="Description of Request" TextMode="MultiLine" Rows="4" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="RequestText" />
                    <Rock:AttributeValuesContainer ID="avcAttributes" runat="server" NumberOfColumns="2" />

                    <Rock:RockControlWrapper ID="rcwEditDocuments" runat="server" Label="Related Documents">
                        <asp:DataList ID="dlEditDocuments" runat="server" CellPadding="4" RepeatDirection="Horizontal" RepeatColumns="4">
                            <ItemTemplate>
                                <div class="margin-r-sm margin-b-sm">
                                    <Rock:FileUploader ID="fuEditDoc" BinaryFileId='<%# Container.DataItem %>' runat="server" OnFileUploaded="fuEditDoc_FileUploaded" OnFileRemoved="fuEditDoc_FileRemoved" />
                                </div>
                            </ItemTemplate>
                        </asp:DataList>
                    </Rock:RockControlWrapper>
                </Rock:PanelWidget>



                <Rock:PanelWidget ID="pwEditResults" runat="server" Title="Results" Expanded="true">
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="dtbEditSummary" runat="server" Label="Result Summary" TextMode="MultiLine" Rows="3" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="ResultSummary" />
                        </div>
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="dtbEditProvidedNextSteps" runat="server" Label="Next Steps Provided" TextMode="MultiLine" Rows="3" SourceTypeName="Rock.Model.BenevolenceRequest, Rock" PropertyName="ResultSummary" />
                        </div>
                    </div>
                </Rock:PanelWidget>

                <div class="actions">
                    <asp:LinkButton ID="lbEditSave" runat="server" data-shortcut-key="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="lbEditSave_Click" />
                    <asp:LinkButton ID="lbEditCancel" runat="server" data-shortcut-key="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbEditCancel_Click" />
                </div>
            </div>
            <Rock:ConfirmPageUnload ID="confirmEditExit" runat="server" ConfirmationMessage="Changes have been made to this benevolence request that have not yet been saved." Enabled="false" />
        </asp:Panel>
        <!-- End Edit Panel -->

        <!-- View Panel -->
        <asp:Panel ID="pnlViewDetail" CssClass="panel panel-block" runat="server">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-plug"></i>Benevolence Request</h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlViewBenevolenceType" runat="server" LabelType="Type" />
                    <Rock:HighlightLabel ID="hlViewCampus" runat="server" LabelType="Campus" />

                    <!-- LabelType specified in  BenevolenceRequestDetail.ascx.cs -->
                    <Rock:HighlightLabel ID="hlViewStatus" runat="server" />
                </div>
            </div>
            <div class="panel-body">
                <!-- Person & Request Information  -->
                <div class="row">
                    <div class="col-xs-12 col-md-7">
                        <div class="row">
                            <div class="col-xs-6 col-sm-4 col-lg-2">
                                <asp:Image CssClass="person-image img-responsive" ID="imgViewRequestor" runat="server" />
                            </div>
                            <div class="col-xs-12 col-sm-8 col-lg-10">
                                <h3>
                                    <asp:Literal ID="lName" runat="server" />
                                </h3>
                                <Rock:BadgeListControl ID="blViewStatus" runat="server" />
                                <asp:LinkButton ID="lbViewProfile" runat="server" OnClick="lbViewProfile_Click"></asp:LinkButton>
                                <asp:Literal ID="lViewNotLinkedProfile" runat="server"></asp:Literal>
                            </div>
                        </div>
                    </div>
                    <div class="col-xs-12 col-md-3">
                        <h6 class="mt-0 static-control-label">Assigned To</h6>
                        <asp:Image CssClass="person-image-small float-left" ID="imgViewAssignedTo" runat="server" />
                        <asp:Literal ID="lViewAssignedTo" runat="server"></asp:Literal>
                    </div>
                    <div class="col-xs-12 col-md-2">
                        <Rock:RockLiteral ID="lViewRequestDate" runat="server" Label="Request Date" />
                    </div>
                </div>
                <!-- Contact Information -->
                <div class="row mt-3">
                    <div class="col-md-12">
                        <h5>Contact Information</h5>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-4">
                        <div class="email" style="padding-bottom: 10px">
                            <asp:Literal ID="lViewEmail" runat="server" />
                        </div>
                        <ul class="list-unstyled phonenumbers">
                            <asp:Repeater ID="rptViewPhones" runat="server">
                                <ItemTemplate>
                                    <li data-value="<%# Eval("Number") %>"><%# FormatPhoneNumber( (bool)Eval("IsUnlisted"), Eval("CountryCode"), Eval("Number"), (int?)Eval("NumberTypeValueId") ?? 0, (bool)Eval("IsMessagingEnabled") ) %></li>
                                </ItemTemplate>
                            </asp:Repeater>
                        </ul>
                    </div>
                    <div class="col-md-3">
                        <div class="address">
                            <Rock:RockLiteral ID="lViewAddress" Label="Address" runat="server" />
                        </div>
                    </div>
                    <div class="col-md-5">
                        <Rock:RockLiteral ID="lViewGovernmentId" Label="Government Id" Visible="false" runat="server" />
                    </div>
                </div>
                <!-- Workflows -->
                <asp:UpdatePanel ID="upViewWorkflows" runat="server">
                    <ContentTemplate>
                        <div class="row mt-3">
                            <div class="col-md-12">
                                <h5>Available Workflows</h5>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:ModalAlert ID="mdViewWorkflowLaunched" runat="server" />
                                <asp:Repeater ID="rptViewRequestWorkflows" runat="server">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="lbLaunchRequestWorkflow" runat="server" CssClass="btn btn-default btn-xs" CommandArgument='<%# Eval("Id") %>' CommandName="LaunchWorkflow">
                                        <i class="<%# Eval("WorkflowType.IconCssClass") %>"></i>&nbsp;<%# Eval("WorkflowType.Name") %>
                                        </asp:LinkButton>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
                <asp:Literal ID="lViewBenevolenceTypeLava" runat="server"></asp:Literal>

                <!-- Details -->
                <hr />
                <div class="row mt-3">
                    <div class="col-md-12">
                        <h5>Request Details</h5>
                        <Rock:RockLiteral ID="lViewBenevolenceTypeDescription" Label="Description of Request" runat="server" />
                    </div>
                </div>
                <div id="divViewAttributes" class="row mt-3" runat="server">
                    <div class="col-md-12">
                        <Rock:AttributeValuesContainer ID="avcViewBenevolenceTypeAttributes" runat="server" ShowCategoryLabel="false" />
                    </div>
                </div>
                <div id="divViewRelatedDocs" runat="server">
                    <div class="row">
                        <div class="col-md-12">
                            <h6>Related Documents</h6>
                        </div>
                    </div>
                    <div class="row">
                        <asp:Repeater ID="rptViewBenevolenceDocuments" runat="server">
                            <ItemTemplate>
                                <div class="col-md-3 related-documents">
                                    <asp:HyperLink ID="lnkViewUploadedFile" runat="server" Target="_blank" rel="noopener noreferrer" CssClass="btn-link"></asp:HyperLink>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </div>
                <!-- Summary -->
                <hr />
                <div class="row mt-3">
                    <div class="col-md-12">
                        <h5>Summary of Results</h5>
                    </div>
                    <div class="col-md-6">
                        <Rock:RockLiteral ID="lViewSummaryResults" Label="Results Summary" runat="server" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockLiteral ID="lViewSummaryNextSteps" Label="Next Steps Provided" runat="server" />
                    </div>
                </div>
                <asp:Panel ID="pnlResults" runat="server" class="row mt-3">
                    <div class="col-md-12">
                        <Rock:Grid ID="gViewResults" runat="server" DisplayType="Light" AllowSorting="false" ShowActionRow="true" RowItemText="Result" AllowPaging="false"
                            OnRowSelected="gViewResults_RowSelected">
                            <Columns>
                                <Rock:RockBoundField DataField="ResultTypeValue.Value" HeaderText="Result Type" SortExpression="ResultType" />
                                <Rock:CurrencyField DataField="Amount" HeaderText="Amount" SortExpression="Amount" />
                                <Rock:RockBoundField DataField="ResultSummary" HeaderText="Details" SortExpression="Details" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </asp:Panel>
                <div class="actions">
                    <asp:LinkButton ID="lbViewEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="lbViewEdit_Click" />
                    <asp:LinkButton ID="lbViewCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="lbViewCancel_Click" />
                    <asp:LinkButton ID="lbViewPrint" runat="server" Text="<i class='fa fa-print'></i>" CssClass="btn btn-sm btn-default btn-square pull-right" OnClick="lbViewPrint_Click" />
                </div>
            </div>

            <!-- Modals -->
            <Rock:ModalDialog ID="mdViewAddResult" runat="server" ScrollbarEnabled="false" SaveButtonText="Save" OnSaveClick="btnAddResult_SaveClick" Title="Benevolence Request Result"
                ValidationGroup="valResult">
                <Content>
                    <asp:ValidationSummary ID="valViewResultsSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="valViewResult" />
                    <asp:HiddenField ID="hfInfoGuid" runat="server" />
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DefinedValuePicker ID="dvpResultType" runat="server" Label="Result Type" ValidationGroup="valViewResult" SourceTypeName="Rock.Model.BenevolenceResult, Rock" PropertyName="ResultTypeValue" />
                        </div>
                        <div class="col-md-6">
                            <Rock:CurrencyBox ID="dtbAmount" runat="server" Label="Amount" ValidationGroup="valViewResult" SourceTypeName="Rock.Model.BenevolenceResult, Rock" PropertyName="Amount" />
                        </div>
                    </div>

                    <Rock:DataTextBox ID="dtbResultSummary" runat="server" Label="Details" ValidationGroup="valViewResult" SourceTypeName="Rock.Model.BenevolenceResult, Rock" TextMode="MultiLine" Rows="3" PropertyName="ResultSummary" />

                    <Rock:DynamicPlaceholder ID="phViewResultAttributes" runat="server" />
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>
        <!-- End View Panel -->
    </ContentTemplate>
</asp:UpdatePanel>
