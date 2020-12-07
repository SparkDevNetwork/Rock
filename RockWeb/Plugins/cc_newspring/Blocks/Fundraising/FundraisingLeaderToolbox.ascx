<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FundraisingLeaderToolbox.ascx.cs" Inherits="RockWeb.Plugins.cc_newspring.Blocks.Fundraising.FundraisingLeaderToolbox" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlView" runat="server">
            <asp:HiddenField ID="hfGroupId" runat="server" />
            <section class="bg-white soft xs-soft-half clearfix push-bottom xs-push-half-bottom rounded shadowed">
                <div class="row">
                    <div class="col-md-8 margin-b-md">
                        <asp:Literal ID="lMainTopContentHtml" runat="server" />
                    </div><div class="col-md-4 margin-t-lg">
                        <div class="hidden">
                        <asp:Image ID="imgOpportunityPhoto" runat="server" CssClass="title-image img-responsive" />
                        </div>
                        <asp:LinkButton ID="btnMainPage" runat="server" CssClass="btn btn-primary btn-block margin-t-md" Text="Trip Page" OnClick="btnMainPage_Click" />
                    </div>
                </div>
            </section>

            <section class="bg-white soft xs-soft-half clearfix push-bottom xs-push-half-bottom rounded shadowed">
                <h2>Trip Participants</h2>
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
            </section>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
