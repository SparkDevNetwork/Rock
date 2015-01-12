<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TimeCardEmployeeCardList.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Hr.TimeCardEmployeeCardList" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        
        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-star"></i>&nbsp;Employee Time Cards</h1>
            </div>
            <div class="panel-body">

                <Rock:RockLiteral ID="lblPayPeriod" runat="server" />

                <Rock:Grid ID="gList" runat="server" AllowSorting="true" OnRowSelected="gList_RowSelected" OnRowDataBound="gList_RowDataBound">
                    <Columns>
                        <Rock:RockBoundField DataField="PersonAlias.Person" HeaderText="Person" SortExpression="PersonAlias.Person.LastName, PersonAlias.Person.FirstName" />
                        <Rock:EnumField DataField="TimeCardStatus" HeaderText="Status" SortExpression="TimeCardStatus" />
                        <Rock:RockTemplateField HeaderText="Hours">
                            <ItemTemplate>
                                <asp:Label runat="server" ID="lRegularHours" CssClass="js-hour-type badge badge-default" data-toggle="tooltip" data-placement="top" title="Regular" />
                                <asp:Label runat="server" ID="lOvertimeHours" CssClass="js-hour-type badge badge-danger" data-toggle="tooltip" data-placement="top" title="Overtime" />
                                <asp:Label runat="server" ID="lPaidVacationHours" CssClass="js-hour-type badge badge-success" data-toggle="tooltip" data-placement="top" title="Vacation" />
                                <asp:Label runat="server" ID="lPaidHolidayHours" CssClass="js-hour-type badge badge-info" data-toggle="tooltip" data-placement="top" title="Holiday" />
                                <asp:Label runat="server" ID="lPaidSickHours" CssClass="js-hour-type badge badge-warning" data-toggle="tooltip" data-placement="top" title="Sick" />
                            </ItemTemplate>
                        </Rock:RockTemplateField>
                    </Columns>
                </Rock:Grid>
            </div>
        </asp:Panel>

        <script>
            Sys.Application.add_load(function () {
                $('.js-hour-type').tooltip();
            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
