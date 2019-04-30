<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationInstanceList.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationInstanceList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server">

            <div id="pnlInstances" runat="server">

                <div class="panel panel-block">

                    <div class="panel-heading">
                        <h1 class="panel-title pull-left">
                            <i class="fa fa-file-o"></i>
                            <asp:Literal ID="lHeading" runat="server" Text="Instances" />
                        </h1>
                    </div>

                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                                <Rock:DateRangePicker ID="drpDates" runat="server" Label="Date Range" />
                                <Rock:RockDropDownList ID="ddlActiveFilter" runat="server" Label="Active Status">
                                    <asp:ListItem Text="[All]" Value="all"></asp:ListItem>
                                    <asp:ListItem Text="Active" Value="active"></asp:ListItem>
                                    <asp:ListItem Text="Inactive" Value="inactive"></asp:ListItem>
                                </Rock:RockDropDownList>
                            </Rock:GridFilter>
                            <Rock:Grid ID="gInstances" runat="server" DisplayType="Full" AllowSorting="true" OnRowSelected="gInstances_Edit" RowItemText="Instance" CssClass="js-grid-instances" >
                                <Columns>
                                    <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                                    <Rock:DateField DataField="StartDateTime" HeaderText="Start Date" SortExpression="StartDateTime" />
                                    <Rock:DateField DataField="EndDateTime" HeaderText="End Date" SortExpression="EndDateTime" />
                                    <Rock:RockBoundField DataField="Registrants" HeaderText="Registrants" />
                                    <Rock:RockBoundField DataField="WaitList" HeaderText="Wait List" />
                                    <Rock:BoolField DataField="IsActive" HeaderText="Active" SortExpression="IsActive" />
                                    <Rock:DeleteField OnClick="DeleteInstance_Click" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>

                </div>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
