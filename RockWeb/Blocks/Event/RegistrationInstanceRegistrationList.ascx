<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationInstanceRegistrationList.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationInstanceRegistrationList" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('.js-follow-status').tooltip();
    });
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server">
                <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                <Rock:HiddenFieldWithClass ID="hfHasPayments" runat="server" CssClass="js-instance-has-payments" />

                <asp:Panel ID="pnlRegistrations" runat="server" CssClass="panel panel-block">
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
                                <Rock:RockCheckBoxList ID="cblCampus" runat="server" Label="Campuses" DataTextField="Name" DataValueField="Id" />
                            </Rock:GridFilter>
                            <Rock:Grid ID="gRegistrations" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gRegistrations_RowSelected" RowItemText="Registration"
                                PersonIdField="PersonAlias.PersonId" CssClass="js-grid-registration" ExportSource="ColumnOutput">
                                <Columns>
                                    <Rock:SelectField ItemStyle-Width="48px" />
                                    <Rock:RockLiteralField ID="lRegisteredBy" HeaderText="Registered By" SortExpression="RegisteredBy" />
                                    <Rock:RockBoundField DataField="Campus.Name" HeaderText="Campus" SortExpression="Campus.Name" />
                                    <Rock:RockBoundField DataField="ConfirmationEmail" HeaderText="Confirmation Email" ExcelExportBehavior="AlwaysInclude" Visible="false" />
                                    <Rock:RockLiteralField ID="lRegistrants" HeaderText="Registrants" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </asp:Panel>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
