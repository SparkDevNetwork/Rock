<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CareItemList.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.PastoralCare.CareItemList" %>
<%@ Import Namespace="Rock" %>
<script>
    Sys.Application.add_load(function () {
        $('.js-legend-badge').tooltip({ html: true, container: 'body', delay: { show: 200, hide: 100 } });
    });

    //Sys.WebForms.PageRequestManager.getInstance().add_endRequest(scrollToGrid);
    function scrollToGrid() {
        if (!$('.js-grid-header').visible(true)) {
            $('html, body').animate({
                scrollTop: $('.js-grid-header').offset().top + 'px'
            }, 'fast');
        }
    }
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlNoTypes" class="alert alert-info" runat="server">
            <asp:Literal ID="lNoTypes" runat="server" Text="There are no current care types." />
            <asp:LinkButton ID="lbNoTpyesConfig" runat="server" CssClass="btn btn-xs btn-square btn-default pull-right pull-right" OnClick="lbCareTypes_Click" CausesValidation="false"><i title="Options" class="fa fa-gear"></i></asp:LinkButton>
        </asp:Panel>

        <ul class="nav nav-pills margin-b-md">
            <asp:Repeater ID="rptCareTypes" runat="server" OnItemCommand="rptCareTypes_ItemCommand">
                <ItemTemplate>
                    <li class='<%# SelectedTypeId.HasValue && (int)Eval("Id") == SelectedTypeId.Value ? "active" : "" %> block-status <%# (bool)Eval("IsActive") ? "" : "inactive-item"  %>' title='<%# (bool)Eval("IsActive") ? "" : "This care type is inactive."  %>'>
                        <asp:LinkButton ID="lbCareType" runat="server" CommandArgument='<%# Eval("Id") %>' CommandName="Display">
                                                <%# ( Container.DataItem as TypeSummary ).Name %>
                        </asp:LinkButton>
                    </li>
                </ItemTemplate>
            </asp:Repeater>
        </ul>
        <asp:Panel ID="pnlGrid" runat="server" CssClass="panel panel-block js-grid-header" Visible="false">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lCareItem" runat="server"></asp:Literal></h1>
                <div class="pull-right">
                    <asp:LinkButton ID="lbCareTypes" runat="server" CssClass="btn btn-xs btn-square btn-default pull-right pull-right" OnClick="lbCareTypes_Click" CausesValidation="false"> <i title="Options" class="fa fa-gear"></i></asp:LinkButton>
                </div>
            </div>
            <div class="panel-body">
                <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                        <Rock:SlidingDateRangePicker ID="sdrpDateRange" runat="server" Label="Date Range" />
                        <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" />
                        <Rock:PersonPicker ID="ppContactor" runat="server" Label="Requester" />
                        <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status">
                            <asp:ListItem Text="All" Value="" />
                            <asp:ListItem Text="Active" Value="Active" />
                            <asp:ListItem Text="Inactive" Value="Inactive" />
                        </Rock:RockDropDownList>
                        <asp:PlaceHolder ID="phAttributeFilters" runat="server" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gItems" runat="server" OnRowSelected="gItems_Edit" CssClass="js-grid-requests" AllowSorting="true" OnRowDataBound="gItems_RowDataBound" ExportSource="ColumnOutput">
                        <Columns>
                            <Rock:SelectField />
                            <Rock:RockBoundField DataField="ContactDateTime" HeaderText="Request Date" SortExpression="CareItem.ContactDateTime" />
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="CareItem.PersonAlias.Person.LastName,PersonAlias.Person.NickName" />
                            <Rock:RockBoundField DataField="ContactorName" HeaderText="Requester" SortExpression="CareItem.ContactorPersonAlias.Person.LastName,ContactorPersonAlias.Person.NickName" />
                            <Rock:RockBoundField DataField="LastContactDate" HeaderText="Last Contact Date" SortExpression="LastContactDate" />
                            <Rock:RockBoundField DataField="LastContactor" HeaderText="Last Contactor" />
                            <Rock:RockBoundField DataField="LastContactNote" HeaderText="Last Contact" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>

        <script>
            $(".my-workflows .list-as-blocks li").on("click", function () {
                $(".my-workflows .list-as-blocks li").removeClass('active');
                $(this).addClass('active');
            });
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
