<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationInstanceRegistrantList.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationInstanceRegistrantList" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('.js-follow-status').tooltip();
    });
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" runat="server">

            <asp:HiddenField ID="hfRegistrationInstanceId" runat="server" />
            <asp:HiddenField ID="hfRegistrationTemplateId" runat="server" />

            <asp:Panel ID="pnlRegistrants" runat="server" CssClass="panel panel-block">
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
                                <Rock:RockLiteralField ID="rlSignedDocument" HeaderText="Signed Documents" Text="" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" Visible="false" />
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

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
