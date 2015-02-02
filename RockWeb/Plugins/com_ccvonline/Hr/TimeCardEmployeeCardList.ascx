<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TimeCardEmployeeCardList.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Hr.TimeCardEmployeeCardList" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-clock-o"></i>&nbsp;Employee Time Cards
                </h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="lblPayPeriod" runat="server" LabelType="Info" />
                </div>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:NotificationBox runat="server" ID="nbApproveSuccess" NotificationBoxType="Success" Dismissable="true" Text="The selected time cards were successfully approved." Visible="false" />
                    <Rock:Grid ID="gList" runat="server" AllowSorting="true" OnRowSelected="gList_RowSelected" OnRowDataBound="gList_RowDataBound">
                        <Columns>
                            <Rock:SelectField />
                            <Rock:RockBoundField DataField="PersonAlias.Person" HeaderText="Person" SortExpression="PersonAlias.Person.LastName, PersonAlias.Person.FirstName" />

                            <Rock:RockTemplateField HeaderText="Employee #">
                                <ItemTemplate>
                                    <asp:Literal ID="lEmployeeNumber" runat="server" />
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateField HeaderText="Department">
                                <ItemTemplate>
                                    <asp:Literal ID="lDepartment" runat="server" />
                                </ItemTemplate>
                            </Rock:RockTemplateField>

                            <Rock:EnumField DataField="TimeCardStatus" HeaderText="Status" SortExpression="TimeCardStatus" />
                            <Rock:RockTemplateField HeaderText="Hours">
                                <ItemTemplate>
                                    <asp:Label runat="server" ID="lRegularHours" CssClass="js-hour-type badge badge-default" data-toggle="tooltip" data-placement="top" title="Regular" />
                                    <asp:Label runat="server" ID="lOvertimeHours" CssClass="js-hour-type badge badge-danger" data-toggle="tooltip" data-placement="top" title="Overtime" />
                                    <asp:Label runat="server" ID="lPaidVacationHours" CssClass="js-hour-type badge badge-success" data-toggle="tooltip" data-placement="top" title="Vacation" />
                                    <asp:Label runat="server" ID="lPaidHolidayHours" CssClass="js-hour-type badge badge-info" data-toggle="tooltip" data-placement="top" title="Paid Holiday" />
                                    <asp:Label runat="server" ID="lPaidSickHours" CssClass="js-hour-type badge badge-warning" data-toggle="tooltip" data-placement="top" title="Sick" />
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>

        <script>
            Sys.Application.add_load(function () {
                $('.js-hour-type').tooltip();
            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
