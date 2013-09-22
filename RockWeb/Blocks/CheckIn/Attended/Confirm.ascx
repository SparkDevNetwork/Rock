<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Confirm.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Attended.Confirm" %>

<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>

    <Rock:ModalAlert ID="maWarning" runat="server" />
    <asp:Panel ID="pnlConfirm" runat="server" CssClass="attended">
        <div class="row checkin-header">
            <div class="col-md-3 checkin-actions">
                <asp:LinkButton ID="lbBack" CssClass="btn btn-large btn-primary" runat="server" OnClick="lbBack_Click" Text="Back"/>
            </div>

            <div class="col-md-6">
                <h1>Confirm</h1>
            </div>

            <div class="col-md-3 checkin-actions">
                <asp:LinkButton ID="lbDone" CssClass="btn btn-large btn-primary" runat="server" OnClick="lbDone_Click" Text="Done"/>
            </div>
        </div>

        <div class="checkin-body">    
            <div class="row">
                <Rock:Grid ID="gPersonList" runat="server" DataKeyNames="PersonId,LocationId,ScheduleId" DisplayType="Light" OnRowCommand="gPersonList_Print" EmptyDataText="No People Selected">
                    <Columns>
                        <asp:BoundField DataField="PersonId" Visible="false" />                    
                        <asp:BoundField DataField="Name" HeaderText="Name" />
                        <asp:BoundField DataField="Location" HeaderText="Assigned To" />
                        <asp:BoundField DataField="LocationId" Visible="false" />
                        <asp:BoundField DataField="Schedule" HeaderText="Time" />
                        <asp:BoundField DataField="ScheduleId" Visible="false" />
                        <Rock:EditValueField HeaderText="Edit" ControlStyle-CssClass="btn btn-large btn-primary btn-checkin-select" OnClick="gPersonList_Edit" />
                        <Rock:DeleteField HeaderText="Delete" ControlStyle-CssClass="btn btn-large btn-primary btn-checkin-select" OnClick="gPersonList_Delete" />
                        <asp:TemplateField HeaderText="Print">
                            <ItemTemplate>
                                <asp:LinkButton ID="btnPrint" runat="server" CssClass="btn btn-large btn-primary btn-checkin-select" CommandName="Print" CommandArgument="<%# Container.DataItemIndex %>">
                                    <i class="icon-print"></i>
                                </asp:LinkButton>
                            </ItemTemplate> 
                        </asp:TemplateField>
                    </Columns>
                </Rock:Grid>
            </div>
            <div class="row">
                <div class="col-md-9"></div>
                <div class="col-md-3">
                    <asp:LinkButton ID="lbPrintAll" CssClass="btn btn-primary btn-block btn-checkin-select" runat="server" OnClick="lbPrintAll_Click" Text="Print All" />
                </div>
            </div>
        </div>
        
    </asp:Panel>
</ContentTemplate>
</asp:UpdatePanel>