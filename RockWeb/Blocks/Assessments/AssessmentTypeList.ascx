<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AssessmentTypeList.ascx.cs" Inherits="RockWeb.Blocks.Assessments.AssessmentTypeList" %>

<asp:UpdatePanel runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdAlert" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <%-- Block-specific Header --%>
                <h1 class="panel-title"><i class="fa fa-directions"></i>Assessment Types</h1>
            </div>
            <div class="panel-body">
                <Rock:NotificationBox ID="nbBlockStatus" runat="server" />
                <asp:Panel ID="pnlList" runat="server">
                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="rFilter" runat="server">
                            <%-- Block-specific Filter Fields --%>
                            <Rock:RockTextBox ID="txbTitleFilter" runat="server" Label="Title" />
                            <Rock:RockDropDownList ID="ddlRequiresRequestFilter" runat="server" Label="Requires Request">
                                <asp:ListItem></asp:ListItem>
                                <asp:ListItem Text="Yes" Value="Yes"></asp:ListItem>
                                <asp:ListItem Text="No" Value="No"></asp:ListItem>
                            </Rock:RockDropDownList>
                            <Rock:RockDropDownList ID="ddlIsActiveFilter" runat="server" Label="Is Active">
                                <asp:ListItem></asp:ListItem>
                                <asp:ListItem Text="Yes" Value="Yes"></asp:ListItem>
                                <asp:ListItem Text="No" Value="No"></asp:ListItem>
                            </Rock:RockDropDownList>
                        </Rock:GridFilter>
                        <Rock:Grid ID="gList" runat="server">
                            <Columns>
                                <%-- Block-specific List Columns --%>
                                <Rock:RockBoundField DataField="Title" HeaderText="Title" SortExpression="Title" Visible="true" ExcelExportBehavior="AlwaysInclude" />
                                <Rock:BoolField DataField="RequiresRequest" HeaderText="Requires Request" SortExpression="RequiresRequest" />
                                <Rock:BoolField DataField="IsActive" HeaderText="Is Active" SortExpression="IsActive" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </asp:Panel>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
