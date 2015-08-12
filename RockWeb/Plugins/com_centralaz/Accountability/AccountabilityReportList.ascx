<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountabilityReportList.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Accountability.AccountabilityReportList" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">Report List</h1>
            </div>

            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" DisplayType="Full" DataKeyNames="Id" OnRowSelected="gList_ExpandRow" OnRowDataBound="gList_RowDataBound">
                        <Columns>
                            <asp:TemplateField SortExpression="SubmitForDate" HeaderText="Report">
                                <ItemTemplate>
                                    <asp:Literal ID="lSubmitForDate" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField SortExpression="Score" HeaderText="Score">
                                <ItemTemplate>
                                    <asp:Literal ID="lScore" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField SortExpression="Percent" HeaderText="Percent">
                                <ItemTemplate>
                                    <asp:Literal ID="lPercent" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </Rock:Grid>
                </div>

                <asp:HiddenField ID="hfActiveDialog" runat="server" />

                <Rock:ModalDialog ID="mdReport" runat="server" Title="Report Summary" SaveButtonText="Back">
                    <Content>
                        <fieldset>
                            <asp:Literal ID="lReportContent" runat="server" />
                        </fieldset>
                    </Content>
                </Rock:ModalDialog>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
