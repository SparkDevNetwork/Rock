<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PledgeList.ascx.cs" Inherits="RockWeb.Blocks.Finance.Administration.PledgeList" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdGridWarning" runat="server"/>
        <Rock:GridFilter ID="rFilter" runat="server">
            <Rock:PersonPicker ID="ppPerson" runat="server"/>
        </Rock:GridFilter>
        <Rock:Grid ID="gPledges" ItemType="Rock.Model.Pledge" runat="server" AutoGenerateColumns="False" AllowSorting="True" AllowPaging="True" PageSize="2" DataKeyNames="Id" SelectMethod="GetPledges" DeleteMethod="DeletePledge">
            <Columns>
                <asp:BoundField DataField="Person" HeaderText="Person" SortExpression="PersonId"/>
                <asp:BoundField DataField="Fund" HeaderText="Fund" SortExpression="FundId"/>
                <asp:BoundField DataField="Amount" HeaderText="Amount" SortExpression="Amount" DataFormatString="{0:C}"/>
                <asp:TemplateField HeaderText="Payment Schedule">
                    <ItemTemplate>
                         <%# string.Format("{0:C}", Item.FrequencyAmount) %> <%# Item.FrequencyTypeValue.ToString() %>
                    </ItemTemplate>
                </asp:TemplateField>
                <Rock:DateField DataField="StartDate" HeaderText="Starts" SortExpression="StartDate"/>
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:LinkButton ID="lbDeletePledge" runat="server" CssClass="btn btn-danger btn-mini" ToolTip="Delete" CommandName="Delete" CommandArgument="Id">
                            <i class="icon-remove"></i>
                        </asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>