<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContributionElectionList.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.HumanResources.ContributionElectionList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlList" CssClass="panel panel-block" runat="server">


            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-folder-open"></i>Contribution Elections</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gContributionElections" runat="server" RowItemText="Contribution Election" TooltipField="Description">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                            <Rock:RockBoundField DataField="Amount" HeaderText="Amount" ItemStyle-HorizontalAlign="Right" />
                            <Rock:RockBoundField DataField="ActiveDate" HeaderText="Active Date" />
                            <Rock:RockBoundField DataField="InactiveDate" HeaderText="Inactive Date" />
                            <Rock:EditField OnClick="gContributionElections_Edit" />
                            <Rock:DeleteField OnClick="gContributionElections_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </asp:Panel>

        <Rock:ModalDialog ID="mdDetails" runat="server" Title="Contribution Election" ValidationGroup="ContributionElection">
            <Content>

                <asp:HiddenField ID="hfIdValue" runat="server" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:DatePicker ID="dpActiveDate" Label="Active Date" runat="server" Required="true" />
                        <Rock:DatePicker ID="dpInactiveDate" Label="Inactive Date" runat="server" />
                    </div>
                    <div class="col-md-6">
                        <Rock:RockDropDownList ID="ddlAccounts" runat="server" Label="Account" DataValueField="Id" DataTextField="Name" />
                        <Rock:NumberBox ID="nbAmount" runat="server" MinimumValue="0" SourceTypeName="com.centralaz.HumanResources.ContributionElection, com.centralaz.HumanResources" PropertyName="EAmount" Required="true" Label="Amount" />
                        <Rock:RockCheckBox ID="cbFixedAmount" Label="Fixed Amount" runat="server" Required="true" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
