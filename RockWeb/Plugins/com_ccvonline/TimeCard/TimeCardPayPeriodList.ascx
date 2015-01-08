<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TimeCardPayPeriodList.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.TimeCard.TimeCardPayPeriodList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-star"></i>&nbsp;Pay Periods</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" AllowSorting="true" RowItemText="Pay Period" OnRowSelected="gList_RowSelected">
                        <Columns>
                            <Rock:RockTemplateField HeaderText="Pay Period" SortExpression="StartDate">
                                <ItemTemplate>
                                    <asp:Literal runat="server" ID="lPayPeriodName" Text="<%# Container.DataItem.ToString()  %>" />
                                </ItemTemplate>
                                
                            </Rock:RockTemplateField>
                            <Rock:DeleteField OnClick="gList_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
