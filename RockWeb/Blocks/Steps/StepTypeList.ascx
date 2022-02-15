<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StepTypeList.ascx.cs" Inherits="RockWeb.Blocks.Steps.StepTypeList" %>

<asp:UpdatePanel ID="upMain" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <asp:Panel ID="pnlContent" runat="server">

            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-map-marker"></i>Step Types</h1>
                </div>
                <div class="panel-body">
                    <Rock:NotificationBox ID="nbBlockStatus" runat="server" NotificationBoxType="Info" />
                    <asp:Panel ID="pnlList" runat="server">
                        <div class="grid grid-panel">
                            <Rock:GridFilter ID="rFilter" runat="server" OnClearFilterClick="rFilter_ClearFilterClick">
                                <Rock:RockTextBox ID="txbNameFilter" runat="server" Label="Name" />
                                <Rock:RockDropDownList ID="ddlHasDurationFilter" runat="server" Label="Spans Time">
                                    <asp:ListItem></asp:ListItem>
                                    <asp:ListItem Text="Yes" Value="Yes"></asp:ListItem>
                                    <asp:ListItem Text="No" Value="No"></asp:ListItem>
                                </Rock:RockDropDownList>
                                <Rock:RockDropDownList ID="ddlAllowMultipleFilter" runat="server" Label="Allow Multiple">
                                    <asp:ListItem></asp:ListItem>
                                    <asp:ListItem Text="Yes" Value="Yes"></asp:ListItem>
                                    <asp:ListItem Text="No" Value="No"></asp:ListItem>
                                </Rock:RockDropDownList>
                                <Rock:RockDropDownList ID="ddlActiveFilter" runat="server" Label="Active Status">
                                    <asp:ListItem></asp:ListItem>
                                    <asp:ListItem Text="Active" Value="Active"></asp:ListItem>
                                    <asp:ListItem Text="Inactive" Value="Inactive"></asp:ListItem>
                                </Rock:RockDropDownList>
                            </Rock:GridFilter>
                            <Rock:Grid ID="gStepType" runat="server" AllowSorting="false" ShowConfirmDeleteDialog="true">
                                <Columns>
                                    <Rock:ReorderField />
                                    <Rock:RockTemplateField ExcelExportBehavior="NeverInclude" HeaderStyle-Width="48px">
                                        <ItemTemplate>
                                            <i class="fa-fw <%# Eval( "IconCssClass" ) %>"></i>
                                        </ItemTemplate>
                                    </Rock:RockTemplateField>
                                    <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" Visible="true" ExcelExportBehavior="AlwaysInclude" />
                                    <Rock:BoolField DataField="HasDuration" HeaderText="Spans Time" />
                                    <Rock:BoolField DataField="AllowMultipleInstances" HeaderText="Allow Multiple" />
                                    <Rock:RockBoundField DataField="StartedCount" HeaderText="Started" DataFormatString="{0:N0}" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" />
                                    <Rock:RockBoundField DataField="CompletedCount" HeaderText="Completed" DataFormatString="{0:N0}" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" />
                                    <Rock:LinkButtonField ID="lbBulkEntry" Text="<i class='fa fa-truck'></i>" CssClass="btn btn-default btn-sm btn-square" OnClick="gStepType_BulkEntry" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                                    <Rock:SecurityField TitleField="Name" />
                                    <Rock:DeleteField OnClick="gStepType_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </asp:Panel>
                </div>
            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
