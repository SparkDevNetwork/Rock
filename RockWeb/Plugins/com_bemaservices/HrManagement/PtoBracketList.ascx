<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PtoBracketList.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.HrManagement.PtoBracketList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server">

            <div id="pnlPtoBrackets" runat="server">

                <div class="panel panel-block">

                    <div class="panel-heading">
                        <h1 class="panel-title pull-left">
                            <i class="fa fa-list"></i>
                            Brackets
                        </h1>

                        <div class="panel-labels">
                            <Rock:HighlightLabel ID="hlSyncStatus" runat="server" LabelType="Info" Visible="false" Text="<i class='fa fa-exchange'></i>" />
                        </div>
                    </div>

                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                                <Rock:RockCheckBox ID="cbActive" runat="server" Label="Active" RepeatDirection="Horizontal" />
                                <%--<asp:PlaceHolder ID="phAttributeFilters" runat="server" />--%>
                            </Rock:GridFilter>
                            <Rock:Grid ID="gPtoBrackets" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gPtoBracket_Edit" OnRowDataBound="gPtoBrackets_RowDataBound">
                                <Columns>
                                    <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                                    <Rock:RockLiteralField ID="lSummary" HeaderText="Summary" />
                                    <Rock:RockLiteralField ID="lStatus" HeaderText="Status" SortExpression="IsActive" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
