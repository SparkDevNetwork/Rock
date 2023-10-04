<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SurveyResultList.ascx.cs" Inherits="RockWeb.Plugins.com_shepherdchurch.SurveySystem.SurveyResultList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbUnauthorizedMessage" runat="server" NotificationBoxType="Warning" />
        <Rock:NotificationBox ID="nbDangerMessage" runat="server" NotificationBoxType="Danger" Visible="true" />

        <asp:Panel ID="pnlResultList" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-list-ul"></i> <asp:Literal ID="ltTitle" runat="server" /></h1>
                <div class="pull-right">
                    <a id="aChartLink" runat="server" class="btn btn-xs btn-default"><i class="fa fa-chart-pie"></i> View Charts</a>
                </div>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />

            	    <Rock:GridFilter ID="gfList" runat="server" OnApplyFilterClick="gfList_ApplyFilterClick" OnDisplayFilterValue="gfList_DisplayFilterValue">
                	    <Rock:PersonPicker ID="ppCompletedBy" runat="server" Label="Completed By" />
                	    <Rock:DateRangePicker ID="drpDateCompleted" runat="server" Label="Date Completed" />
                        <asp:PlaceHolder ID="phAttributeFilters" runat="server" />
	                </Rock:GridFilter>

                    <Rock:Grid ID="gList" runat="server" AllowSorting="true">
                        <Columns>
                            <Rock:PersonField DataField="CreatedByPersonAlias.Person" HeaderText="Completed By" SortExpression="CreatedByPersonAlias.Person.FullName" />
                            <Rock:DateField DataField="CreatedDateTime" HeaderText="Completed Date" SortExpression="CreatedDateTime" />
                            <Rock:BoolField DataField="DidPass" HeaderText="Did Pass" SortExpression="DidPass" />
                            <Rock:DateField DataField="TestResult" HeaderText="Score" SortExpression="TestResult" DataFormatString="{0}%" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>