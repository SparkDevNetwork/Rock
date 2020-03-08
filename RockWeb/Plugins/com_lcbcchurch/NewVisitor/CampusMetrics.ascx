<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CampusMetrics.ascx.cs" Inherits="RockWeb.Plugins.com_lcbcchurch.NewVisitor.CampusMetrics" %>

<style type="text/css">
    .table thead th.total {
        border-right: 1px solid #000 !important;
    }

    .table td.total {
        border-right: 1px solid #000 !important;
    }
</style>
<asp:UpdatePanel ID="upnlContent" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
        <script>
            Sys.Application.add_load(function () {
                $(document).ready(function () {

                    $('.grid-label').hover(function () {
                        $(this).find('span').toggle("slide");
                    });
                });
            });
        </script>
        <div class="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <h1 class="panel-title"><i runat="server" id="iIcon"></i> <asp:Literal ID="lTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">
                <%-- View Panel --%>
                <asp:Panel ID="pnlView" runat="server">
                    <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" Visible="false" />
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <div class="grid grid-panel">
                        <Rock:Grid ID="gCampusMetrics" runat="server" CssClass="js-grid-requests" AllowSorting="true" OnRowSelected="gCampusMetrics_Selected">
                            <Columns>
                                <Rock:CampusField DataField="CampusId" HeaderText="Campus" SortExpression="CampusName" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </asp:Panel>
                <%-- Edit Panel --%>
                <asp:Panel ID="pnlEditModal" runat="server" Visible="false">
                    <Rock:ModalDialog ID="mdEdit" runat="server" OnSaveClick="lbSave_Click" Title="Custom Block Configuration"  OnCancelScript="clearDialog();">
                        <Content>
                            <asp:UpdatePanel ID="upnlEdit" runat="server">
                                <ContentTemplate>
                                    <div class="row">
                                        <div class="col-md-4">
                                            <Rock:PagePicker ID="ppCampusStaffPersonalContactsPage" runat="server" Label="Campus Staff Personal Contacts Page" Required="true" />
                                        </div>
                                        <div class="col-md-4">
                                            <Rock:RockDropDownList ID="ddlSecurityRole" runat="server" CssClass="input-width-xl" Label="View all Campus Security Role" Required="true" />
                                        </div>
                                        <div class="col-md-4">
                                            <Rock:RockDropDownList ID="ddlCampusAttribute" runat="server" CssClass="input-width-xl" Label="Assigned Campus Attribute" Required="true" />
                                        </div>
                                    </div>

                                    <div class="row">
                                        <div class="col-md-4">
                                            <Rock:NumberBox ID="numSuccessMinimum" Label="Success Minimum" runat="server" CssClass="input-md" Required="true" MinimumValue="0" />
                                        </div>
                                        <div class="col-md-4">
                                            <Rock:NumberBox ID="numWarningMinimum" Label="Warning Minimum" runat="server" CssClass="input-md" Required="true" MinimumValue="0" />
                                        </div>
                                    </div>

                                    <div class="row">
                                        <div class="col-md-12">
                                            <label>Metrics</label>

                                            <p>Configures the metrics you would like to display. Each Item must have a numerator metric, a corresponding demoninator metric and a label to be used as the column heading.</p>
                                            <Rock:Grid ID="gMetrics" runat="server" EmptyDataText="No Metrics Found" RowItemText="Metric" DisplayType="Light" EnableResponsiveTable="false">
                                                <Columns>
                                                    <Rock:ReorderField />
                                                    <Rock:RockTemplateField HeaderText="Label">
                                                        <ItemTemplate>
                                                            <%# Eval("Label") %>
                                                        </ItemTemplate>
                                                        <EditItemTemplate>
                                                            <Rock:RockTextBox ID="tbLabel" runat="server" Required="true" RequiredErrorMessage="Label is required" ValidationGroup="Metric" />
                                                        </EditItemTemplate>
                                                    </Rock:RockTemplateField>
                                                    <Rock:RockTemplateField HeaderText="Numerator Metric">
                                                        <ItemTemplate>
                                                            <asp:Literal ID="lNumeratorMetric" runat="server" />
                                                        </ItemTemplate>
                                                        <EditItemTemplate>
                                                            <Rock:MetricCategoryPicker ID="mcpNumeratorMetric" runat="server" Required="true" RequiredErrorMessage="Numerator Metric is required" ValidationGroup="Metric"/>
                                                        </EditItemTemplate>
                                                    </Rock:RockTemplateField>
                                                    <Rock:RockTemplateField HeaderText="Denominator Metric">
                                                        <ItemTemplate>
                                                            <asp:Literal ID="lDenominatorMetric" runat="server" />
                                                        </ItemTemplate>
                                                        <EditItemTemplate>
                                                            <Rock:MetricCategoryPicker ID="mcpDenominatorMetric" runat="server" Required="true" RequiredErrorMessage="Denominator Metric is required" ValidationGroup="Metric"/>
                                                        </EditItemTemplate>
                                                    </Rock:RockTemplateField>
                                                    <Rock:RockTemplateField HeaderText="Use Value Option">
                                                        <ItemTemplate>
                                                            <asp:Literal ID="lUseValueOption" runat="server" />
                                                        </ItemTemplate>
                                                        <EditItemTemplate>
                                                            <Rock:RockDropDownList ID="ddlValueOptionType" runat="server" Required="true" RequiredErrorMessage="Value Option Type is required" ValidationGroup="Metric"/>
                                                        </EditItemTemplate>
                                                    </Rock:RockTemplateField>
                                                    <Rock:RockTemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center" HeaderStyle-CssClass="span1" ItemStyle-CssClass="grid-columncommand" ItemStyle-Wrap="false">
                                                        <ItemTemplate>
                                                            <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CommandName="Edit" CssClass="btn btn-default btn-sm" CausesValidation="false"><i class="fa fa-pencil"></i></asp:LinkButton>
                                                        </ItemTemplate>
                                                        <EditItemTemplate>
                                                            <asp:LinkButton ID="lbSave" runat="server" Text="Save" CommandName="Update" CssClass="btn btn-sm btn-success" ValidationGroup="Metric"><i class="fa fa-check"></i></asp:LinkButton>
                                                            <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CommandName="Cancel" CssClass="btn btn-sm btn-warning" CausesValidation="false"><i class="fa fa-minus"></i></asp:LinkButton>
                                                        </EditItemTemplate>
                                                    </Rock:RockTemplateField>
                                                    <Rock:DeleteField OnClick="gMetrics_RowDelete" />
                                                </Columns>
                                            </Rock:Grid>
                                        </div>
                                    </div>
                                    <Rock:NotificationBox ID="nbError" runat="server" Heading="Error" Title="Error." NotificationBoxType="Danger" Visible="false" />

                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </Content>
                    </Rock:ModalDialog>

                </asp:Panel>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
