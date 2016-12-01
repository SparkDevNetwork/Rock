<%@ Control Language="C#" AutoEventWireup="true" CodeFile="NSGroupDetailLava.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Groups.NSGroupDetailLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlGroupView" runat="server">
            <asp:Literal ID="lContent" runat="server"></asp:Literal>

            <asp:Literal ID="lDebug" runat="server"></asp:Literal>
        </asp:Panel>
        
        <asp:Panel ID="pnlGroupEdit" runat="server" Visible="false">
            
            <asp:ValidationSummary ID="vsGroupEdit" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
            
            <div class="row">
                <asp:Literal ID="tbName" runat="server"/>
            </div>

            <div class="row">
                <asp:Literal Text="There's nothing to manage just yet." runat="server"/>

                <asp:Panel ID="pnlSchedule" runat="server" Visible="false" CssClass="row">
                    <div class="col-sm-6">
                        <Rock:DayOfWeekPicker ID="dowWeekly" runat="server" CssClass="input-width-md" Label="Day of the Week" />
                    </div>
                    <div class="col-sm-6">
                        <Rock:TimePicker ID="timeWeekly" runat="server" Label="Time of Day" />
                    </div>
                </asp:Panel>

                <div class="row">
                    <div class="col-md-12">
                        <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="true" />
                    </div>
                </div>

                <div class="actions">
                    <asp:Button ID="btnSaveGroup" visible="false" runat="server" AccessKey="s" CssClass="btn btn-primary" Text="Save" OnClick="btnSaveGroup_Click" />
                    <asp:LinkButton id="lbCancelGroup" runat="server" AccessKey="c" CssClass="btn btn-primary" OnClick="lbCancelGroup_Click" CausesValidation="false">Cancel</asp:LinkButton>
                </div>

            </div>

        </asp:Panel>

        

    </ContentTemplate>
</asp:UpdatePanel>
