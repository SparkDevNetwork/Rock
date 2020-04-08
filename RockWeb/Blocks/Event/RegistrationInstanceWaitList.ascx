<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationInstanceWaitList.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationInstanceWaitList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server">

            <div class="panel panel-block">

                <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />

                <asp:Panel ID="pnlWaitList" runat="server" CssClass="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title"><i class="fa fa-clock-o"></i>Wait List</h1>
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
                            <Rock:Grid ID="gWaitList" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gWaitList_RowSelected" RowItemText="Wait List Individual" PersonIdField="PersonId" ExportSource="ColumnOutput" >
                                <Columns>
                                    <Rock:SelectField ItemStyle-Width="48px" />
                                    <Rock:RockTemplateField HeaderText="Wait List Order">
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

            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
