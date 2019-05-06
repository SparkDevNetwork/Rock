<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationInstanceDetail.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationInstanceDetail" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('.js-follow-status').tooltip();
    });
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="wizard">
            <div class="wizard-item complete">
                <asp:LinkButton ID="lbWizardTemplate" runat="server" OnClick="lbTemplate_Click" CausesValidation="false">
                    <%-- Placeholder needed for bug. See: http://stackoverflow.com/questions/5539327/inner-image-and-text-of-asplinkbutton-disappears-after-postback--%>
                    <asp:PlaceHolder runat="server">
                        <div class="wizard-item-icon">
                            <i class="fa fa-fw fa-clipboard"></i>
                        </div>
                        <div class="wizard-item-label">
                            <asp:Literal ID="lWizardTemplateName" runat="server" />
                        </div>
                    </asp:PlaceHolder>
                </asp:LinkButton>
            </div>

            <div class="wizard-item active">
                <div class="wizard-item-icon">
                    <i class="fa fa-fw fa-file-o"></i>
                </div>
                <div class="wizard-item-label">
                    <asp:Literal ID="lWizardInstanceName" runat="server" />
                </div>
            </div>

            <div class="wizard-item">
                <div class="wizard-item-icon">
                    <i class="fa fa-fw fa-group"></i>
                </div>
                <div class="wizard-item-label">
                    Registration
                </div>
            </div>

            <div class="wizard-item">
                <div class="wizard-item-icon">
                    <i class="fa fa-fw fa-user"></i>
                </div>
                <div class="wizard-item-label">
                    Registrant
                </div>
            </div>
        </div>

        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfRegistrationInstanceId" runat="server" />
            <asp:HiddenField ID="hfRegistrationTemplateId" runat="server" />

            <div class="panel panel-block">

                <div class="panel-heading panel-follow clearfix">
                    <h1 class="panel-title">
                        <i class="fa fa-file-o"></i>
                        <asp:Literal ID="lReadOnlyTitle" runat="server" />
                    </h1>
                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                        <Rock:HighlightLabel ID="hlType" runat="server" LabelType="Type" />
                    </div>
                    <asp:Panel runat="server" ID="pnlFollowing" CssClass="panel-follow-status js-follow-status" data-toggle="tooltip" data-placement="top" title="Click to Follow"></asp:Panel>
                </div>
                <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
                <div class="panel-body">

                    <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />

                    <div id="pnlEditDetails" runat="server">

                        <Rock:RegistrationInstanceEditor ID="rieDetails" runat="server" />

                        <div class="actions">
                            <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                            <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                        </div>
                    </div>

                    <fieldset id="fieldsetViewDetails" runat="server">
                        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockLiteral ID="lName" runat="server" Label="Name" />
                                <Rock:RockLiteral ID="lMaxAttendees" runat="server" Label="Maximum Attendees" />
                                <Rock:RockLiteral ID="lWorkflowType" runat="server" Label="Registration Workflow" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockLiteral ID="lCost" runat="server" Label="Cost" />
                                <Rock:RockLiteral ID="lMinimumInitialPayment" runat="server" Label="Minimum Initial Payment" />
                                <Rock:RockLiteral ID="lDefaultPaymentAmount" runat="server" Label="Default Payment Amount" />
                                <Rock:RockLiteral ID="lAccount" runat="server" Label="Account" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockLiteral ID="lStartDate" runat="server" Label="Registration Starts" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockLiteral ID="lEndDate" runat="server" Label="Registration Ends" />
                            </div>
                        </div>

                        <Rock:RockLiteral ID="lDetails" runat="server" Label="Details"></Rock:RockLiteral>

                        <div class="actions">
                            <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" ToolTip="Alt+m" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                            <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                            <Rock:HiddenFieldWithClass ID="hfHasPayments" runat="server" CssClass="js-instance-has-payments" />
                            <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link js-delete-instance" OnClick="btnDelete_Click" CausesValidation="false" />
                            <span class="pull-right">
                                <asp:LinkButton ID="btnPreview" runat="server" Text="Preview" CssClass="btn btn-link" OnClick="btnPreview_Click" Visible="false" />
                                <asp:LinkButton ID="btnSendPaymentReminder" runat="server" Text="Send Payment Reminders" CssClass="btn btn-link" OnClick="btnSendPaymentReminder_Click" Visible="false" />
                            </span>
                        </div>
                    </fieldset>
                </div>
            </div>

            <asp:Panel ID="pnlTabs" runat="server" Visible="false">

                <ul class="nav nav-pills margin-b-md">
                    <li id="liRegistrations" runat="server" class="active">
                        <asp:LinkButton ID="lbRegistrations" runat="server" Text="Registrations" OnClick="lbTab_Click" />
                    </li>
                    <li id="liRegistrants" runat="server">
                        <asp:LinkButton ID="lbRegistrants" runat="server" Text="Registrants" OnClick="lbTab_Click" />
                    </li>
                    <li id="liPayments" runat="server">
                        <asp:LinkButton ID="lbPayments" runat="server" Text="Payments" OnClick="lbTab_Click" />
                    </li>
                    <li id="liFees" runat="server">
                        <asp:LinkButton ID="lbFees" runat="server" Text="Fees" OnClick="lbTab_Click" />
                    </li>
                    <li id="liDiscounts" runat="server">
                        <asp:LinkButton ID="lbDiscounts" runat="server" Text="Discounts" OnClick="lbTab_Click" />
                    </li>
                    <li id="liLinkage" runat="server">
                        <asp:LinkButton ID="lbLinkage" runat="server" Text="Linkages" OnClick="lbTab_Click" />
                    </li>
                     <li id="liWaitList" runat="server">
                        <asp:LinkButton ID="lbWaitList" runat="server" Text="Wait List" OnClick="lbTab_Click" />
                    </li>
                    <li id="liGroupPlacement" runat="server">
                        <asp:LinkButton ID="lbGroupPlacement" runat="server" Text="Group Placement" OnClick="lbTab_Click" />
                    </li>
                </ul>

                <asp:Panel ID="pnlRegistrations" runat="server" Visible="false" CssClass="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title">
                            <i class="fa fa-user"></i>
                            Registrations
                        </h1>
                    </div>
                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdRegistrationsGridWarning" runat="server" />
                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="fRegistrations" runat="server" OnDisplayFilterValue="fRegistrations_DisplayFilterValue" OnClearFilterClick="fRegistrations_ClearFilterClick">
                                <Rock:SlidingDateRangePicker ID="sdrpRegistrationDateRange" runat="server" Label="Registration Date Range" />
                                <Rock:RockDropDownList ID="ddlRegistrationPaymentStatus" runat="server" Label="Payment Status">
                                    <asp:ListItem Text="" Value="" />
                                    <asp:ListItem Text="Paid in Full" Value="Paid in Full" />
                                    <asp:ListItem Text="Balance Owed" Value="Balance Owed" />
                                </Rock:RockDropDownList>
                                <Rock:RockTextBox ID="tbRegistrationRegisteredByFirstName" runat="server" Label="Registered By First Name" />
                                <Rock:RockTextBox ID="tbRegistrationRegisteredByLastName" runat="server" Label="Registered By Last Name" />
                                <Rock:RockTextBox ID="tbRegistrationRegistrantFirstName" runat="server" Label="Registrant First Name" />
                                <Rock:RockTextBox ID="tbRegistrationRegistrantLastName" runat="server" Label="Registrant Last Name" />
                            </Rock:GridFilter>
                            <Rock:Grid ID="gRegistrations" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gRegistrations_RowSelected" RowItemText="Registration"
                                PersonIdField="PersonAlias.PersonId" CssClass="js-grid-registration" ExportSource="ColumnOutput">
                                <Columns>
                                    <Rock:SelectField ItemStyle-Width="48px" />
                                    <Rock:RockLiteralField ID="lRegisteredBy" HeaderText="Registered By" SortExpression="RegisteredBy" />
                                    <Rock:RockBoundField DataField="ConfirmationEmail" HeaderText="Confirmation Email" ExcelExportBehavior="AlwaysInclude" Visible="false" />
                                    <Rock:RockLiteralField ID="lRegistrants" HeaderText="Registrants" />
                                    <Rock:DateTimeField DataField="CreatedDateTime" HeaderText="When" SortExpression="CreatedDateTime" />
                                    <Rock:RockLiteralField ID="lDiscount" HeaderText="Discount Code" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left" SortExpression="DiscountCode" Visible="false" />
                                    <Rock:RockLiteralField ID="lRegistrationCost" HeaderText="Total Cost"  HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" SortExpression="TotalCost" />
                                    <Rock:RockLiteralField ID="lBalance" HeaderText="Balance Due"  HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" SortExpression="BalanceDue" />
                                    <Rock:DeleteField OnClick="gRegistrations_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlRegistrants" runat="server" Visible="false" CssClass="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title">
                            <i class="fa fa-users"></i>
                            Registrants
                        </h1>
                    </div>
                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdRegistrantsGridWarning" runat="server" />
                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="fRegistrants" runat="server" OnDisplayFilterValue="fRegistrants_DisplayFilterValue" OnClearFilterClick="fRegistrants_ClearFilterClick">
                                <Rock:SlidingDateRangePicker ID="sdrpRegistrantsRegistrantDateRange" runat="server" Label="Registration Date Range" />
                                <Rock:RockTextBox ID="tbRegistrantsRegistrantFirstName" runat="server" Label="First Name" />
                                <Rock:RockTextBox ID="tbRegistrantsRegistrantLastName" runat="server" Label="Last Name" />
                                <Rock:RockDropDownList ID="ddlRegistrantsInGroup" runat="server" Label="In Group" />
                                <Rock:RockDropDownList ID="ddlRegistrantsSignedDocument" runat="server" Label="Signed Document" />
                                <asp:PlaceHolder ID="phRegistrantsRegistrantFormFieldFilters" runat="server" />
                            </Rock:GridFilter>
                            <Rock:Grid ID="gRegistrants" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gRegistrants_RowSelected" RowItemText="Registrant" PersonIdField="PersonId" ExportSource="ColumnOutput">
                                <Columns>
                                    <Rock:SelectField ItemStyle-Width="48px" />
                                    <Rock:RockTemplateField HeaderText="Registrant" SortExpression="PersonAlias.Person.LastName, PersonAlias.Person.NickName" ExcelExportBehavior="NeverInclude">
                                        <ItemTemplate>
                                            <asp:Literal ID="lRegistrant" runat="server"></asp:Literal>
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockBoundField HeaderText="First Name" DataField="Person.NickName" ExcelExportBehavior="AlwaysInclude" Visible="false" />
                                    <Rock:RockBoundField HeaderText="Last Name" DataField="Person.LastName" ExcelExportBehavior="AlwaysInclude" Visible="false" />
                                    <Rock:RockTemplateFieldUnselected HeaderText="Group">
                                        <ItemTemplate>
                                            <asp:Literal ID="lGroup" runat="server"></asp:Literal>
                                        </ItemTemplate>
                                    </Rock:RockTemplateFieldUnselected>
                                    <Rock:RockTemplateField Visible="false" HeaderText="Street 1" ExcelExportBehavior="AlwaysInclude">
                                        <ItemTemplate>
                                            <asp:Literal ID="lStreet1" runat="server"></asp:Literal>
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockTemplateField Visible="false" HeaderText="Street 2" ExcelExportBehavior="AlwaysInclude">
                                        <ItemTemplate>
                                            <asp:Literal ID="lStreet2" runat="server"></asp:Literal>
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockTemplateField Visible="false" HeaderText="City" ExcelExportBehavior="AlwaysInclude">
                                        <ItemTemplate>
                                            <asp:Literal ID="lCity" runat="server"></asp:Literal>
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockTemplateField Visible="false" HeaderText="State" ExcelExportBehavior="AlwaysInclude">
                                        <ItemTemplate>
                                            <asp:Literal ID="lState" runat="server"></asp:Literal>
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockTemplateField Visible="false" HeaderText="Postal Code" ExcelExportBehavior="AlwaysInclude">
                                        <ItemTemplate>
                                            <asp:Literal ID="lPostalCode" runat="server"></asp:Literal>
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockTemplateField Visible="false" HeaderText="Country" ExcelExportBehavior="AlwaysInclude">
                                        <ItemTemplate>
                                            <asp:Literal ID="lCountry" runat="server"></asp:Literal>
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockBoundField HeaderText="Created Datetime" DataField="CreatedDateTime" ExcelExportBehavior="AlwaysInclude" Visible="false" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlPayments" runat="server" Visible="false" CssClass="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title">
                            <i class="fa fa-credit-card"></i>
                            Payments
                        </h1>
                    </div>
                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdPaymentsGridWarning" runat="server" />
                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="fPayments" runat="server" OnDisplayFilterValue="fPayments_DisplayFilterValue" OnClearFilterClick="fPayments_ClearFilterClick">
                                <Rock:SlidingDateRangePicker ID="sdrpPaymentDateRange" runat="server" Label="Transaction Date Range" />
                            </Rock:GridFilter>
                            <Rock:Grid ID="gPayments" runat="server" DisplayType="Full" AllowSorting="true" RowItemText="Payment" OnRowSelected="gPayments_RowSelected" ExportSource="ColumnOutput">
                                <Columns>
                                    <Rock:RockBoundField DataField="AuthorizedPersonAlias.Person.FullNameReversed" HeaderText="Person"
                                        SortExpression="AuthorizedPersonAlias.Person.LastName,AuthorizedPersonAlias.Person.NickName" />
                                    <Rock:RockBoundField DataField="TransactionDateTime" HeaderText="Date / Time" SortExpression="TransactionDateTime" />
                                    <Rock:CurrencyField DataField="TotalAmount" HeaderText="Amount" SortExpression="TotalAmount" />
                                    <Rock:RockBoundField DataField="FinancialPaymentDetail.CurrencyAndCreditCardType" HeaderText="Payment Method" />
                                    <Rock:RockBoundField DataField="FinancialPaymentDetail.AccountNumberMasked" HeaderText="Account" />
                                    <Rock:RockBoundField DataField="TransactionCode" HeaderText="Transaction Code" SortExpression="TransactionCode" ColumnPriority="DesktopSmall" />
                                    <Rock:RockTemplateFieldUnselected HeaderText="Registrar">
                                        <ItemTemplate>
                                            <asp:Literal ID="lRegistrar" runat="server" />
                                        </ItemTemplate>
                                    </Rock:RockTemplateFieldUnselected>
                                    <Rock:RockTemplateField HeaderText="Registrant(s)">
                                        <ItemTemplate>
                                            <asp:Literal ID="lRegistrants" runat="server" />
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlFees" runat="server" Visible="false" CssClass="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title">
                            <i class="fa fa-money"></i>
                            Fees
                        </h1>
                    </div>
                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdFeesGridWarning" runat="server" />
                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="fFees" runat="server" OnDisplayFilterValue="fFees_DisplayFilterValue" OnClearFilterClick="fFees_ClearFilterCick">
                                <Rock:SlidingDateRangePicker ID="sdrpFeeDateRange" runat="server" Label="Fee Date Range" />
                                <Rock:RockDropDownList ID="ddlFeeName" runat="server" Label="Fee Name" AutoPostBack="true" OnSelectedIndexChanged="ddlFeeName_SelectedIndexChanged" ></Rock:RockDropDownList>
                                <Rock:RockCheckBoxList ID="cblFeeOptions" runat="server" Label="Fee Options"></Rock:RockCheckBoxList>
                            </Rock:GridFilter>
                            <Rock:Grid ID="gFees" runat="server" DisplayType="Full" AllowSorting="true" RowItemText="Fee" ExportSource="DataSource" >
                                <Columns>
                                        <Rock:RockBoundField HeaderText ="Registration ID" DataField="RegistrationId" ExcelExportBehavior="AlwaysInclude" Visible="false" />
                                        <Rock:DateField HeaderText ="Registration Date" DataField="RegistrationDate" SortExpression="RegistrationDate" />
                                        <Rock:RockBoundField HeaderText ="Registered By" DataField="RegisteredByName" SortExpression="RegisteredByName" />
                                        <Rock:RockBoundField HeaderText ="Registrant" DataField="RegistrantName" SortExpression="RegistrantName" />
                                        <Rock:RockBoundField HeaderText ="Registrant ID" DataField="RegistrantId" ExcelExportBehavior="AlwaysInclude" Visible="false" />
		                                <Rock:RockBoundField HeaderText ="Fee Name" DataField="FeeName" SortExpression="FeeName" />
                                        <Rock:RockBoundField HeaderText ="Option" DataField="FeeItemName" SortExpression="FeeItemName" />
		                                <Rock:RockBoundField HeaderText ="Quantity" DataField="Quantity" SortExpression="Quantity" />
                                        <Rock:CurrencyField HeaderText ="Cost" DataField="Cost" SortExpression="Cost" />
                                        <Rock:CurrencyField HeaderText ="Fee Total" DataField="FeeTotal" SortExpression="FeeTotal"  />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlDiscounts" runat="server" Visible="false" CssClass="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title">
                            <i class="fa fa-gift"></i>
                            Discounts
                        </h1>
                    </div>
                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdDiscountsGridWarning" runat="server" />
                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="fDiscounts" runat="server" OnDisplayFilterValue="fDiscounts_DisplayFilterValue" OnClearFilterClick="fDiscounts_ClearFilterClick">
                                <Rock:SlidingDateRangePicker ID="sdrpDiscountDateRange" runat="server" Label="Discount Date Range" Help="To filter based on when the discount code was used." />
                                <Rock:RockDropDownList ID="ddlDiscountCode" runat="server" Label="Discount Code" AutoPostBack="true" OnSelectedIndexChanged="ddlDiscountCode_SelectedIndexChanged" EnhanceForLongLists="true"></Rock:RockDropDownList>
                                <Rock:RockTextBox ID="tbDiscountCodeSearch" runat="server" Label="Code Search" Help="Enter a search parameter. Cannot be used with the 'Discount Code' list." />
                            </Rock:GridFilter>
                            <Rock:Grid ID="gDiscounts" runat="server" DisplayType="Full" AllowSorting="true" RowItemText="Discount" ExportSource="DataSource">
                                <Columns>
                                    <Rock:RockBoundField HeaderText="Registration ID" DataField="RegistrationId" ExcelExportBehavior="AlwaysInclude" Visible="false" />
                                    <Rock:RockBoundField HeaderText="Registered By" DataField="RegisteredByName" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left" SortExpression="RegisteredByName" />
                                    <Rock:DateField HeaderText="Registration Date" DataField="RegistrationDate" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left" SortExpression="RegistrationDate" />
                                    <Rock:RockBoundField HeaderText="Registrant Count" DataField="RegistrantCount" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" SortExpression="RegistrantCount" />
                                    <Rock:RockBoundField HeaderText="Discount Code" DataField="DiscountCode" HeaderStyle-HorizontalAlign="Left" ItemStyle-HorizontalAlign="Left" SortExpression="DiscountCode" />
                                    <Rock:RockBoundField HeaderText="Discount" DataField="Discount" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" SortExpression="Discount" />
                                    <Rock:CurrencyField HeaderText="Total Cost" DataField="TotalCost" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" SortExpression="TotalCost" />
                                    <Rock:CurrencyField HeaderText="Discount Qualified Cost" DataField="DiscountQualifiedCost" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" SortExpression="DiscountQualifiedCost" />
                                    <Rock:CurrencyField HeaderText="Total Discount" DataField="TotalDiscount" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" SortExpression="TotalDiscount" />
                                    <Rock:CurrencyField HeaderText="Registration Cost" DataField="RegistrationCost" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" SortExpression="RegistrationCost" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                        <br />
                        <div class="row">
                            <div class="col-md-4 col-md-offset-8 margin-t-md">
                                <asp:Panel ID="pnlDiscountSummary" runat="server" CssClass="panel panel-block">
                                    <div class="panel-heading">
                                        <h1 class="panel-title">Total Results</h1>
                                    </div>
                                    <div class="panel-body">
                                        <div class="row">
                                            <div class="col-xs-8">Total Cost</div>
                                            <div class='col-xs-4 text-right'><asp:Literal ID="lTotalTotalCost" runat="server" /></div>
                                        </div>
                                        <div class="row">
                                            <div class="col-xs-8">Discount Qualified Cost</div>
                                            <div class='col-xs-4 text-right'><asp:Literal ID="lTotalDiscountQualifiedCost" runat="server" /></div>
                                        </div>
                                        <div class="row">
                                            <div class="col-xs-8">Total Discount</div>
                                            <div class='col-xs-4 text-right'><asp:Literal ID="lTotalDiscounts" runat="server" /></div>
                                        </div>
                                        <div class="row">
                                            <div class="col-xs-8">Registration Cost</div>
                                            <div class='col-xs-4 text-right'><asp:Literal ID="lTotalRegistrationCost" runat="server" /></div>
                                        </div>
                                        <br />
                                        <div class="row">
                                            <div class="col-xs-8">Total Registrations</div>
                                            <div class='col-xs-4 text-right'><asp:Literal ID="lTotalRegistrations" runat="server" /></div>
                                        </div>
                                        <div class="row">
                                            <div class="col-xs-8">Total Registrants</div>
                                            <div class='col-xs-4 text-right'><asp:Literal ID="lTotalRegistrants" runat="server" /></div>
                                        </div>
                                    </div>
                                </asp:Panel>
                            </div>
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlLinkages" runat="server" Visible="false" CssClass="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title">
                            <i class="fa fa-link"></i>
                            Linkages
                        </h1>
                    </div>
                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdLinkagesGridWarning" runat="server" />
                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="fLinkages" runat="server" OnDisplayFilterValue="fLinkages_DisplayFilterValue" OnClearFilterClick="fLinkages_ClearFilterClick">
                                <Rock:RockCheckBoxList ID="cblCampus" runat="server" Label="Campuses" DataTextField="Name" DataValueField="Id" />
                            </Rock:GridFilter>
                            <Rock:Grid ID="gLinkages" runat="server" DisplayType="Full" AllowSorting="true" RowItemText="Linkage" ExportSource="ColumnOutput">
                                <Columns>
                                    <Rock:RockTemplateFieldUnselected HeaderText="Calendar Item">
                                        <ItemTemplate>
                                            <asp:Literal ID="lCalendarItem" runat="server" />
                                        </ItemTemplate>
                                    </Rock:RockTemplateFieldUnselected>
                                    <asp:BoundField HeaderText="Campus" DataField="EventItemOccurrence.Campus.Name" SortExpression="EventItemOccurrence.Campus.Name" NullDisplayText="All Campuses" />
                                    <asp:HyperLinkField HeaderText="Group" DataTextField="Group" DataNavigateUrlFields="GroupID" SortExpression="Group" />
                                    <Rock:RockTemplateFieldUnselected HeaderText="Content Item">
                                        <ItemTemplate>
                                            <asp:Literal ID="lContentItem" runat="server" />
                                        </ItemTemplate>
                                    </Rock:RockTemplateFieldUnselected>
                                    <asp:BoundField HeaderText="Public Name" DataField="PublicName" SortExpression="PublicName" />
                                    <asp:BoundField HeaderText="URL Slug" DataField="UrlSlug" SortExpression="UrlSlug" />
                                    <Rock:EditField OnClick="gLinkages_Edit" />
                                    <Rock:DeleteField OnClick="gLinkages_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlWaitList" runat="server" Visible="false" CssClass="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title"><i class="fa fa-clock-o"></i> Wait List</h1>
                    </div>
                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdWaitListWarning" runat="server" />
                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="fWaitList" runat="server" OnDisplayFilterValue="fWaitList_DisplayFilterValue" OnClearFilterClick="fWaitList_ClearFilterClick">
                                <Rock:DateRangePicker ID="drpWaitListDateRange" runat="server" Label="Date Range" />
                                <Rock:RockTextBox ID="tbWaitListFirstName" runat="server" Label="First Name" />
                                <Rock:RockTextBox ID="tbWaitListLastName" runat="server" Label="Last Name" />
                                <asp:PlaceHolder ID="phWaitListFormFieldFilters" runat="server" />
                            </Rock:GridFilter>
                            <Rock:Grid ID="gWaitList" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gWaitList_RowSelected" RowItemText="Wait List Individual" PersonIdField="PersonId" ExportSource="ColumnOutput">
                                <Columns>
                                    <Rock:SelectField ItemStyle-Width="48px" />
                                    <Rock:RockTemplateField HeaderText="Wait List Order" >
                                        <ItemTemplate>
                                            <asp:Literal ID="lWaitListOrder" runat="server"></asp:Literal>
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockTemplateField HeaderText="Wait List Individual" SortExpression="PersonAlias.Person.LastName, PersonAlias.Person.NickName" ExcelExportBehavior="NeverInclude">
                                        <ItemTemplate>
                                            <asp:Literal ID="lWaitListIndividual" runat="server"></asp:Literal>
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockBoundField HeaderText="First Name" DataField="Person.NickName" ExcelExportBehavior="AlwaysInclude" Visible="false" />
                                    <Rock:RockBoundField HeaderText="Last Name" DataField="Person.LastName" ExcelExportBehavior="AlwaysInclude" Visible="false" />
                                    <Rock:RockBoundField HeaderText="Added Datetime" DataField="CreatedDateTime" SortExpression="CreatedDateTime" ExcelExportBehavior="AlwaysInclude" Visible="true" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                </asp:Panel>

                <asp:Panel ID="pnlGroupPlacement" runat="server" Visible="false" CssClass="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title">
                            <i class="fa fa-link"></i>
                            Group Placement
                        </h1>
                    </div>
                    <Rock:GridFilter ID="fGroupPlacements" runat="server" OnDisplayFilterValue="fGroupPlacements_DisplayFilterValue" OnClearFilterClick="fGroupPlacements_ClearFilterClick">
                        <Rock:SlidingDateRangePicker ID="sdrpGroupPlacementsDateRange" runat="server" Label="Registration Date Range" />
                        <Rock:RockTextBox ID="tbGroupPlacementsFirstName" runat="server" Label="First Name" />
                        <Rock:RockTextBox ID="tbGroupPlacementsLastName" runat="server" Label="Last Name" />
                        <Rock:RockDropDownList ID="ddlGroupPlacementsInGroup" runat="server" Label="In Group"  />
                        <Rock:RockDropDownList ID="ddlGroupPlacementsSignedDocument" runat="server" Label="Signed Document" />
                        <asp:PlaceHolder ID="phGroupPlacementsFormFieldFilters" runat="server" />
                    </Rock:GridFilter>
                    <div class="panel-body">
                        <Rock:NotificationBox ID="nbPlacementNotifiction" runat="server" Visible="false" />
                        <div class="row margin-t-md">
                            <div class="col-sm-6">
                                <Rock:GroupPicker ID="gpGroupPlacementParentGroup" runat="server" Label="Parent Group"
                                    OnSelectItem="gpGroupPlacementParentGroup_SelectItem" />
                            </div>
                            <div class="col-sm-6">
                                <Rock:RockCheckBox ID="cbSetGroupAttributes" runat="server" Label="Set Group Member Attributes" Text="Yes"
                                    Help="If there are group member attributes on the target group that have the same key as a registrant attribute, should the registrant attribute value be copied to the group member attribute value?" />
                            </div>
                        </div>
                        <Rock:ModalAlert ID="mdGroupPlacementGridWarning" runat="server" />
                        <Rock:Grid ID="gGroupPlacements" runat="server" DisplayType="Light" EnableResponsiveTable="false" AllowSorting="false" RowItemText="Registrant" ExportSource="ColumnOutput">
                            <Columns>
                                <Rock:RockTemplateField HeaderText="Registrant" SortExpression="PersonAlias.Person.LastName, PersonAlias.Person.NickName">
                                    <ItemTemplate>
                                        <asp:Literal ID="lRegistrant" runat="server"></asp:Literal>
                                    </ItemTemplate>
                                </Rock:RockTemplateField>
                            </Columns>
                        </Rock:Grid>
                        <div class="actions">
                            <asp:LinkButton ID="lbPlaceInGroup" runat="server" OnClick="lbPlaceInGroup_Click" Text="Place" CssClass="btn btn-primary" />
                        </div>
                    </div>
                </asp:Panel>
            </asp:Panel>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
