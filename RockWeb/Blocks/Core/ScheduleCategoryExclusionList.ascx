<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ScheduleCategoryExclusionList.ascx.cs" Inherits="RockWeb.Blocks.Core.ScheduleCategoryExclusionList" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlList" runat="server">

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-calendar-minus-o"></i> Schedule Exclusions</h1>
                </div>
                <div class="panel-body">

                    <div class="grid grid-panel">
                        <Rock:Grid ID="rGrid" runat="server" RowItemText="Exclusion" OnRowSelected="rGrid_Edit" TooltipField="Description">
                            <Columns>
                                <Rock:RockBoundField DataField="Title" HeaderText="Title" />
                                <Rock:DateField DataField="StartDate" HeaderText="Start" />
                                <Rock:DateField DataField="EndDate" HeaderText="End" />
                                <Rock:DeleteField OnClick="rGrid_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>

                </div>
            </div>

        </asp:Panel>

        <Rock:ModalDialog ID="modalDetails" runat="server" Title="Schedule Exclusion" ValidationGroup="Exclusion">
            <Content>
                <asp:HiddenField ID="hfIdValue" runat="server" />
                <div class="row">
                    <div class="col-sm-6">
                        <Rock:RockTextBox ID="tbTitle" runat="server" Label="Title" Required="true" ValidationGroup="Exclusion" />
                    </div>
                    <div class="col-sm-6">
                        <Rock:DateRangePicker ID="drpExclusion" runat="server" Label="Exclude Dates" Required="true" ValidationGroup="Exclusion" />
                    </div>
                </div>
            </Content>
        </Rock:ModalDialog>

        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
