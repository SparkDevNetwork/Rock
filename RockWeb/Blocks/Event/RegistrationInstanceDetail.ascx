<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationInstanceDetail.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationInstanceDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="wizard">

            <div class="wizard-item complete">
                <asp:LinkButton ID="lbWizardTemplate" runat="server" OnClick="lbTemplate_Click">
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

                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-file-o"></i>
                        <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
                    <div class="panel-labels">
                        <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                        <Rock:HighlightLabel ID="hlType" runat="server" LabelType="Type" />
                    </div>
                </div>
                <div class="panel-body">

                    <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <div id="pnlEditDetails" runat="server">

                        <Rock:RegistrationInstanceEditor ID="rieDetails" runat="server" />

                        <div class="actions">
                            <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                            <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                        </div>
                    </div>

                    <fieldset id="fieldsetViewDetails" runat="server">
                        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockLiteral ID="lName" runat="server" Label="Name" />
                                <Rock:RockLiteral ID="lAccount" runat="server" Label="Account" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockLiteral ID="lMaxAttendees" runat="server" Label="Maximum Attendees" />
                            </div>
                        </div>

                        <Rock:RockLiteral ID="lDetails" runat="server" Label="Details"></Rock:RockLiteral>

                        <div class="actions">
                            <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                            <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                            <Rock:HiddenFieldWithClass ID="hfHasPayments" runat="server" CssClass="js-instance-has-payments" />
                            <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link js-delete-instance" OnClick="btnDelete_Click" CausesValidation="false" />
                            <span class="pull-right">
                                <asp:LinkButton ID="btnPreview" runat="server" Text="Preview" CssClass="btn btn-link" OnClick="btnPreview_Click" Visible="false" />
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
                    <li id="liLinkage" runat="server">
                        <asp:LinkButton ID="lbLinkage" runat="server" Text="Linkages" OnClick="lbTab_Click" />
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
                            <Rock:Grid ID="gRegistrations" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gRegistrations_RowSelected" RowItemText="Registration" CssClass="js-grid-registration" PersonIdField="PersonId">
                                <Columns>
                                    <Rock:SelectField ItemStyle-Width="48px"/>
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
                                <asp:PlaceHolder ID="phRegistrantFormFieldFilters" runat="server" />
                            </Rock:GridFilter>
                            <Rock:Grid ID="gRegistrants" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gRegistrants_RowSelected" RowItemText="Registrant" PersonIdField="PersonId">
                                <Columns>
                                    <Rock:SelectField ItemStyle-Width="48px"/>
                                    <Rock:RockTemplateField HeaderText="Registrant">
                                        <ItemTemplate>
                                            <asp:Literal ID="lRegistrant" runat="server"></asp:Literal>
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockTemplateFieldUnselected HeaderText="Group">
                                        <ItemTemplate>
                                            <asp:Literal ID="lGroup" runat="server"></asp:Literal>
                                        </ItemTemplate>
                                    </Rock:RockTemplateFieldUnselected>
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
                            <Rock:Grid ID="gLinkages" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gLinkages_RowSelected" RowItemText="Linkage">
                                <Columns>
                                    <Rock:RockTemplateFieldUnselected HeaderText="Calendar Item">
                                        <ItemTemplate>
                                            <asp:Literal ID="lCalendarItem" runat="server" /></ItemTemplate>
                                    </Rock:RockTemplateFieldUnselected>
                                    <asp:BoundField HeaderText="Campus" DataField="EventItemOccurrence.Campus.Name" SortExpression="EventItemOccurrence.Campus.Name" NullDisplayText="All Campuses" />
                                    <asp:HyperLinkField HeaderText="Group" DataTextField="Group" DataNavigateUrlFields="GroupID" SortExpression="Group" />
                                    <asp:BoundField HeaderText="Public Name" DataField="PublicName" SortExpression="PublicName" />
                                    <asp:BoundField HeaderText="URL Slug" DataField="UrlSlug" SortExpression="UrlSlug" />
                                    <Rock:DeleteField OnClick="gLinkages_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>

                </asp:Panel>

            </asp:Panel>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
