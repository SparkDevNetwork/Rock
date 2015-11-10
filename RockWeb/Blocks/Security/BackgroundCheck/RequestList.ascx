<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RequestList.ascx.cs" Inherits="RockWeb.Blocks.Security.BackgroundCheck.RequestList" %>

<asp:UpdatePanel ID="upList" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server">

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-file-text-o"></i> Requests</h1>
                </div>
                <div class="panel-body">

                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="fRequest" runat="server">
                            <Rock:DateRangePicker ID="drpRequestDates" runat="server" Label="Request Date Range" />
                            <Rock:DateRangePicker ID="drpResponseDates" runat="server" Label="Response Date Range" />
                        </Rock:GridFilter>
        
                        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                
                        <Rock:Grid ID="gRequest" runat="server" AllowSorting="true">
                            <Columns>

                                <Rock:RockBoundField DataField="Person" HeaderText="Name" SortExpression="Name" />
                                <Rock:DateField DataField="RequestDate" HeaderText="Request Date" SortExpression="RequestDate" />
                                <Rock:DateField DataField="ResponseDate" HeaderText="Response Date" SortExpression="ResponseDate" />

                                <Rock:RockTemplateField HeaderText="Record Found">
                                    <ItemTemplate>
                                        <%# ((bool)Eval("RecordFound")) ? "<span class='label label-danger'>Yes</span>" : "No" %>
                                    </ItemTemplate>
                                </Rock:RockTemplateField>
                            
                                <Rock:LinkButtonField HeaderText="Response XML" Text="Link" />
                                <Rock:LinkButtonField HeaderText="Response Document" Text="Link" />

                                <Rock:RockTemplateField HeaderText="Record Found">
                                    <ItemTemplate>
                                        <%# ((bool)Eval("Complete")) ? "<span class='label label-success'>Complete</span>" : "<span class='label label-default'>Pending</span>" %>
                                    </ItemTemplate>
                                </Rock:RockTemplateField>

                            </Columns>
                        </Rock:Grid>
                    </div>

                </div>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
