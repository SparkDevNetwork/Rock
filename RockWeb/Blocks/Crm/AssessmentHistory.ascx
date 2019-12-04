<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AssessmentHistory.ascx.cs" Inherits="RockWeb.Blocks.Crm.AssessmentHistory" %>

<asp:UpdatePanel ID="upSettings" runat="server">
    <ContentTemplate>
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-check-square"></i>
                    Assessment History
                </h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server">
                        <Rock:AssessmentTypePicker ID="atpAssessmentType" runat="server" Label="Assessment" Required="false" EntityTypeName="Rock.Model.AssessmentType" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gAssessments" runat="server" AllowSorting="true" OnRowDataBound="gAssessments_RowDataBound">
                        <Columns>
                            <Rock:RockBoundField DataField="AssessmentTypeTitle" HeaderText="Assessment" SortExpression="AssessmentTypeTitle" />
                            <Rock:RockBoundField DataField="StatusText" HeaderText="Status" SortExpression="Status" />
                            <Rock:RockBoundField DataField="RequestedDateTime" HeaderText="Requested" SortExpression="RequestedDateTime" />
                            <Rock:RockBoundField DataField="RequesterPersonFullName" HeaderText="Requested By" SortExpression="RequesterPersonFullName" />
                            <Rock:RockBoundField DataField="CompletedDateTime" HeaderText="Completed" SortExpression="CompletedDateTime" />
                            <asp:TemplateField ItemStyle-CssClass="grid-columncommand">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnDelete" runat="server" OnClick="btnDelete_Click" CssClass="btn btn-danger btn-sm" CommandArgument='<%# (Container.DataItem as AssessmentViewModel).AssessmentId %>'>
                                        <i class="fa fa-times"></i>
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
