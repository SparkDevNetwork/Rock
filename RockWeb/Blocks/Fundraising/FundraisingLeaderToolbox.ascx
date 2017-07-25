<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FundraisingLeaderToolbox.ascx.cs" Inherits="RockWeb.Blocks.Fundraising.FundraisingLeaderToolbox" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlView" runat="server">
            <asp:HiddenField ID="hfGroupId" runat="server" />

            <div class="row">

                <%-- Left Sidebar --%>
                <div class="col-md-4 margin-t-lg">
                    <asp:Image ID="imgOpportunityPhoto" runat="server" CssClass="title-image img-responsive" />
                    <asp:LinkButton ID="btnMainPage" runat="server" CssClass="btn btn-primary btn-block margin-t-md" Text="Main Page" OnClick="btnMainPage_Click" />
                </div>

                <%-- Main --%>
                <div class="col-md-8 margin-b-md">
                    <asp:Literal ID="lMainTopContentHtml" runat="server" />
                </div>
            </div>

            <div class="margin-t-md">
                <Rock:Grid ID="gGroupMembers" runat="server" OnRowSelected="gGroupMembers_RowSelected" DataKeyNames="Id" PersonIdField="PersonId" AllowSorting="true">
                    <Columns>
                        <Rock:SelectField />
                        <Rock:DateTimeField DataField="DateTimeAdded" HeaderText="Date Added" SortExpression="DateTimeAdded" />
                        <asp:BoundField DataField="FullName" HeaderText="Name" SortExpression="Person.LastName,Person.NickName" />
                        <Rock:EnumField DataField="Gender" HeaderText="Gender" SortExpression="Person.Gender" />
                        <Rock:CurrencyField DataField="FundingRemaining" HeaderText="Funding Remaining" NullDisplayText=""/>
                        <asp:BoundField DataField="GroupRoleName" HeaderText="Role" SortExpression="GroupRole.Name" />
                    </Columns>
                </Rock:Grid>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
