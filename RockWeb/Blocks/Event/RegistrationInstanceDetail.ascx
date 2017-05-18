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

            <div class="panel panel-block">

                <div class="panel-heading panel-follow clearfix">
                    <h1 class="panel-title"><i class="fa fa-file-o"></i>
                        <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                        <Rock:HighlightLabel ID="hlType" runat="server" LabelType="Type" />
                    </div>
                    <asp:Panel runat="server" ID="pnlFollowing" CssClass="panel-follow-status js-follow-status" data-toggle="tooltip" data-placement="top" title="Click to Follow"></asp:Panel>
                </div>
                <div class="panel-body">

                    <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

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
                                <Rock:RockLiteral ID="lAccount" runat="server" Label="Account" />
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
                        <h1 class="panel-title"><i class="fa fa-user"></i> Registrations</h1>
                    </div>
                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdRegistrationsGridWarning" runat="server" />
                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="fRegistrations" runat="server" OnDisplayFilterValue="fRegistrations_DisplayFilterValue">
                                <Rock:DateRangePicker ID="drpRegistrationDateRange" runat="server" Label="Date Range" />
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
                                    <Rock:RockTemplateField HeaderText="Registered By">
                                        <ItemTemplate>
                                            <asp:Literal ID="lRegisteredBy" runat="server"></asp:Literal>
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockTemplateField HeaderText="Registrants">
                                        <ItemTemplate>
                                            <asp:Literal ID="lRegistrants" runat="server"></asp:Literal>
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:DateTimeField DataField="CreatedDateTime" HeaderText="When" SortExpression="CreatedDateTime" />
                                    <Rock:RockTemplateField HeaderText="Discount Code" ItemStyle-HorizontalAlign="Center" SortExpression="DiscountCode" Visible="false">
                                        <ItemTemplate>
                                            <asp:Label ID="lDiscount" runat="server" CssClass="label label-default" />
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockTemplateField HeaderText="Total Cost" ItemStyle-HorizontalAlign="Right" SortExpression="TotalCost">
                                        <ItemTemplate>
                                            <asp:Label ID="lCost" runat="server" CssClass="label label-info"></asp:Label>
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockTemplateField HeaderText="Balance Due" ItemStyle-HorizontalAlign="Right" SortExpression="BalanceDue">
                                        <ItemTemplate>
                                            <Rock:HiddenFieldWithClass ID="hfHasPayments" runat="server" CssClass="js-has-payments" />
                                            <asp:Label ID="lBalance" runat="server" CssClass="label"></asp:Label>
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:DeleteField OnClick="gRegistrations_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlRegistrants" runat="server" Visible="false" CssClass="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title"><i class="fa fa-users"></i> Registrants</h1>
                    </div>
                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdRegistrantsGridWarning" runat="server" />
                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="fRegistrants" runat="server" OnDisplayFilterValue="fRegistrants_DisplayFilterValue">
                                <Rock:DateRangePicker ID="drpRegistrantDateRange" runat="server" Label="Date Range" />
                                <Rock:RockTextBox ID="tbRegistrantFirstName" runat="server" Label="First Name" />
                                <Rock:RockTextBox ID="tbRegistrantLastName" runat="server" Label="Last Name" />
                                <Rock:RockDropDownList ID="ddlInGroup" runat="server" Label="In Group"  />    
                                <Rock:RockDropDownList ID="ddlSignedDocument" runat="server" Label="Signed Document" />
                                <asp:PlaceHolder ID="phRegistrantFormFieldFilters" runat="server" />
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
                        <h1 class="panel-title"><i class="fa fa-credit-card"></i> Payments</h1>
                    </div>
                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdPaymentsGridWarning" runat="server" />
                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="fPayments" runat="server" OnDisplayFilterValue="fPayments_DisplayFilterValue">
                                <Rock:DateRangePicker ID="drpPaymentDateRange" runat="server" Label="Date Range" />
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

                <asp:Panel ID="pnlLinkages" runat="server" Visible="false" CssClass="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title"><i class="fa fa-link"></i> Linkages</h1>
                    </div>
                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdLinkagesGridWarning" runat="server" />
                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="fLinkages" runat="server" OnDisplayFilterValue="fLinkages_DisplayFilterValue">
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
                            <Rock:GridFilter ID="fWaitList" runat="server" OnDisplayFilterValue="fWaitList_DisplayFilterValue">
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
                        <h1 class="panel-title"><i class="fa fa-link"></i> Group Placement</h1>
                    </div>
                    <div class="panel-body">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:GroupPicker ID="gpGroupPlacementParentGroup" runat="server" Label="Parent Group"
                                    OnSelectItem="gpGroupPlacementParentGroup_SelectItem" />
                            </div>
                            <div>
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