<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TimeCardList.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.TimeCard.TimeCardList" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <Rock:Grid ID="gList" runat="server" AllowSorting="true" OnRowSelected="gList_RowSelected" >
            <Columns>
                <Rock:RockBoundField DataField="TimeCardPayPeriod" HeaderText="Pay Period" SortExpression="TimeCardPayPeriod.StartDate" />
                <Rock:EnumField DataField="TimeCardStatus" HeaderText="Status" />
                <Rock:RockTemplateField HeaderText="Hours">
                    <ItemTemplate>
                        <span><%#this.GetHoursHtml( Container.DataItem as com.ccvonline.TimeCard.Model.TimeCardDay) %></span>
                    </ItemTemplate>
                </Rock:RockTemplateField>
            </Columns>
        </Rock:Grid>

    </ContentTemplate>
</asp:UpdatePanel>
