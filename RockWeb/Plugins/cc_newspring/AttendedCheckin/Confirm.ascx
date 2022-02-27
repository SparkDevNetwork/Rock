<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Confirm.ascx.cs" Inherits="RockWeb.Plugins.cc_newspring.AttendedCheckin.Confirm" %>

<asp:UpdatePanel ID="pnlContent" runat="server" UpdateMode="Conditional">
    <ContentTemplate>

        <asp:Panel ID="pnlConfirm" runat="server" CssClass="attended">

            <Rock:ModalAlert ID="maAlert" runat="server" />
            <asp:HiddenField ID="hfLabelReprint" runat="server" Value="false" />

            <div class="row checkin-header push-quarter-bottom">
                <div class="col-xs-2 checkin-actions">
                    <Rock:BootstrapButton ID="lbBack" CssClass="btn btn-lg btn-primary" runat="server" OnClick="lbBack_Click">
                    <span class="fa fa-arrow-left"></span>
                    </Rock:BootstrapButton>
                </div>

                <div class="col-xs-8 text-center">
                    <h1>Confirm</h1>
                </div>

                <div class="col-xs-2 checkin-actions text-right">
                    <Rock:BootstrapButton ID="lbNext" CssClass="btn btn-lg btn-primary" runat="server" OnClick="lbNext_Click">
                    <span class="fa fa-arrow-right"></span>
                    </Rock:BootstrapButton>
                </div>
            </div>

            <div class="checkin-body selected-grid">
                <div class="row">
                    <asp:UpdatePanel ID="pnlSelectedGrid" runat="server">
                        <ContentTemplate>
                            <div class="grid hard-top">
                                <Rock:Grid ID="gPersonList" runat="server" DataKeyNames="PersonId,GroupId,LocationId,ScheduleId,CheckedIn"
                                    DisplayType="Light" EnableResponsiveTable="true" ShowFooter="false" EmptyDataText="No People Selected" CssClass="three-col-with-controls"
                                    OnRowCommand="gPersonList_Print" OnRowDataBound="gPersonList_RowDataBound" OnGridRebind="gPersonList_GridRebind">
                                    <Columns>
                                        <Rock:RockTemplateField HeaderStyle-CssClass="col-xs-3" HeaderText="Name" ItemStyle-CssClass="col-xs-3" >
                                            <ItemTemplate>
                                                <%# Eval("Name") %><asp:Literal ID="ltContent" runat="server" />
                                            </ItemTemplate>
                                        </Rock:RockTemplateField>
                                        <Rock:RockBoundField HeaderStyle-CssClass="col-xs-1 centered" HeaderText="Age" ItemStyle-CssClass="col-xs-1 centered" DataField="Age" Visible="false" />
                                        <Rock:RockBoundField HeaderStyle-CssClass="col-xs-1 centered" HeaderText="Grade" ItemStyle-CssClass="col-xs-1 centered" DataField="Grade" Visible="false" />
                                        <Rock:RockBoundField HeaderStyle-CssClass="col-xs-2" HeaderText="Location" ItemStyle-CssClass="col-xs-2 overflow-inherit" DataField="Location" />
                                        <Rock:RockBoundField HeaderStyle-CssClass="col-xs-2" HeaderText="Schedule" ItemStyle-CssClass="col-xs-2" DataField="Schedule" />
                                        <Rock:RockBoundField HeaderStyle-CssClass="col-xs-1 centered" HeaderText="Checked In" ItemStyle-CssClass="col-xs-1 centered" HeaderStyle-Wrap="false" />
                                        <Rock:EditField HeaderStyle-CssClass="col-xs-1 centered" HeaderText="Edit" ControlStyle-CssClass="col-xs-1 btn btn-lg btn-primary" OnClick="gPersonList_Edit" />
                                        <Rock:RockTemplateField HeaderStyle-CssClass="col-xs-1 centered" HeaderText="Print">
                                            <ItemTemplate>
                                                <asp:LinkButton ID="btnPrint" runat="server" CssClass="col-xs-1 btn btn-lg btn-primary" CommandName="Print" CommandArgument="<%# Container.DataItemIndex %>">
                                                    <i class="fa fa-print"></i>
                                                </asp:LinkButton>
                                            </ItemTemplate>
                                        </Rock:RockTemplateField>
                                        <Rock:DeleteField HeaderStyle-CssClass="col-xs-1 centered" HeaderText="Delete" ControlStyle-CssClass="col-xs-1 btn btn-lg btn-primary accent-bold-color accent-bold-color-bordered" OnClick="gPersonList_Delete" />
                                    </Columns>
                                </Rock:Grid>
                                <div class="col-xs-offset-6 col-xs-3 hard-right push-half-top">
                                    <asp:PlaceHolder ID="phPrinterStatus" runat="server"></asp:PlaceHolder>
                                </div>
                                <div class="col-xs-3 hard-right push-quarter-top">
                                    <Rock:BootstrapButton ID="lbPrintAll" CssClass="btn btn-primary btn-lg btn-block btn-checkin-select" runat="server" OnClick="lbPrintAll_Click" Text="Print All" />
                                </div>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
