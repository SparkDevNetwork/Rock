<%@ Control Language="C#" AutoEventWireup="true" CodeFile="HospitalList.ascx.cs" Inherits="RockWeb.Plugins.org_secc.PastoralCare.HospitalList" %>

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
                    <h1 class="panel-title"><i class="fa fa-hospital-o"></i> Hospitalization List</h1>
                </div>
                
                <Rock:Grid ID="gReport" runat="server" AllowSorting="true" EmptyDataText="No Results" DataKeyNames="Id" OnRowSelected="gReport_RowSelected">
                    <Columns>
                        <Rock:RockBoundField DataField="Hospital" HeaderText="Hospital" SortExpression="Hospital"></Rock:RockBoundField>
                        <Rock:PersonField DataField="PersonToVisit" HeaderText="Person To Visit" SortExpression="Person.LastName" />
                        <Rock:RockBoundField DataField="Age" HeaderText="Age" SortExpression="Age"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="Room" HeaderText="Room" SortExpression="Room"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="AdmitDate" DataFormatString="{0:MM/dd/yyyy}" HeaderText="Admit Date" SortExpression="AdmitDate"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="Description" HeaderText="Description" SortExpression="Description"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="Visits" HeaderText="Visits" SortExpression="Visits"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="LastVisitor" HeaderText="Last Visitor" SortExpression="LastVisitor"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="LastVisitDate" HeaderText="Last Visit Date" SortExpression="LastVisitDate"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="LastVisitNotes" HeaderText="Last Visit Notes" SortExpression="LastVisitNotes"></Rock:RockBoundField>
                        <Rock:RockBoundField DataField="DischargeDate" HeaderText="Discharge Date" SortExpression="DischargeDate" Visible="false"></Rock:RockBoundField>
                        <Rock:RockTemplateField HeaderText="Status" SortExpression="Status">
                            <ItemTemplate>
                                <span class="label <%# Convert.ToString(Eval("Status"))=="Active"?"label-success":"label-default" %>"><%# Eval("Status") %></span>
                            </ItemTemplate>
                        </Rock:RockTemplateField>
                        <Rock:BoolField DataField="Communion" HeaderText="Com." SortExpression="Communion" />
                        <Rock:RockTemplateField HeaderText="Actions" ItemStyle-Width="160px">
                            <ItemTemplate>
                                <a href="<%# "https://maps.google.com/?q="+Eval("HospitalAddress").ToString() %>" target="_blank" class="btn btn-default"><i class="fa fa-map-o" title="View Map"></i></a>
                                <a href="<%# "/Pastoral/Hospitalization/"+Eval("Workflow.Id") %>" class="btn btn-default"><i class="fa fa-pencil"></i></a>
                                <Rock:BootstrapButton id="btnReopen" runat="server" CommandArgument='<%# Eval("Workflow.Id") %>' CssClass="btn btn-warning" ToolTip="Reopen Workflow" OnCommand="btnReopen_Command" Visible='<%# Convert.ToString(Eval("Status"))!="Active" %>'><i class="fa fa-undo"></i></Rock:BootstrapButton>
                            </ItemTemplate>
                        </Rock:RockTemplateField>
                    </Columns>
                </Rock:Grid>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
