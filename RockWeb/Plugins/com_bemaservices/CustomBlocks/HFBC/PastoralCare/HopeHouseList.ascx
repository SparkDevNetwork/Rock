<%@ Control Language="C#" AutoEventWireup="true" CodeFile="HopeHouseList.ascx.cs" Inherits="RockWeb.Plugins.org_bemaservices.PastoralCare.HopeHouseList" %>

<asp:UpdatePanel ID="upReport" runat="server">
    <ContentTemplate>
                
        <asp:Panel runat="server" ID="pnlInfo" Visible="false">
            <div class="panel-heading">
                <asp:Literal Text="Information" runat="server" ID="ltHeading" />
            </div>
            <div class="panel-body">
                <asp:Literal Text="" runat="server" ID="ltBody" />
            </div>
        </asp:Panel>
        <asp:Panel runat="server" ID="pnlMain" Visible="true">

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-hospital-o"></i> Hope House Guest List</h1>
                </div>
                
                <Rock:Grid ID="gReport" runat="server" AllowSorting="true" EmptyDataText="No Results" DataKeyNames="Id" OnRowSelected="gReport_RowSelected">
                    <Columns>
                        <Rock:RockBoundField DataField="Patient" HeaderText="Patient" SortExpression="Person.LastName"></Rock:RockBoundField>
                        <Rock:PersonField DataField="Patient.PrimaryFamily.Campus" HeaderText="Campus" SortExpression="Patient.PrimaryFamily.Campus" />
                        <Rock:PersonField DataField="Patient.ConnectionStatusValue" HeaderText="Connection Status" SortExpression="Patient.ConnectionStatusValue" />
                        <Rock:RockBoundField DataField="Contact" HeaderText="Contact" SortExpression="Person.LastName"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="CareGiver" HeaderText="CareGiver" SortExpression="Person.LastName"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="DateActive" DataFormatString="{0:MM/dd/yyyy}" HeaderText="Date Active" SortExpression="DateActive"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="Illness" HeaderText="Illness" SortExpression="Illness"></Rock:RockBoundField>

                        <Rock:RockTemplateField HeaderText="Actions" ItemStyle-Width="160px">
                            <ItemTemplate>
                               <a href="<%# "/Pastoral/HopeHouse/"+Eval("Workflow.Id") %>" class="btn btn-default"><i class="fa fa-pencil"></i></a>
                                <Rock:BootstrapButton id="btnReopen" runat="server" CommandArgument='<%# Eval("Workflow.Id") %>' CssClass="btn btn-warning" ToolTip="Reopen Workflow" OnCommand="btnReopen_Command" Visible='<%# Convert.ToString(Eval("Status"))!="Active" %>'><i class="fa fa-undo"></i></Rock:BootstrapButton>
                            </ItemTemplate>
                        </Rock:RockTemplateField>
                    </Columns>
                </Rock:Grid>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
