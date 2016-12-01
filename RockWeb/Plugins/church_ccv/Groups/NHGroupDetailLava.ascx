<%@ Control Language="C#" AutoEventWireup="true" CodeFile="NHGroupDetailLava.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Groups.NHGroupDetailLava" %>

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
                    <asp:Button ID="btnSaveGroup" runat="server" AccessKey="s" CssClass="btn btn-primary" Text="Save" OnClick="btnSaveGroup_Click" />
                    <asp:LinkButton id="lbCancelGroup" runat="server" AccessKey="c" CssClass="btn btn-link" OnClick="lbCancelGroup_Click" CausesValidation="false">Cancel</asp:LinkButton>
                </div>
            </div>
        </asp:Panel>


        <asp:Panel ID="pnlEditGroupMember" runat="server" Visible="false">
            
            <asp:ValidationSummary ID="vsEditGroupMember" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
            <asp:CustomValidator ID="cvEditGroupMember" runat="server" Display="None" />
            <Rock:NotificationBox ID="nbGroupMemberErrorMessage" runat="server" NotificationBoxType="Danger" />

            <div class="row">
                <div class="col-md-6">
                    <Rock:PersonPicker runat="server" ID="ppGroupMemberPerson" Label="Person" Required="true"/>
                </div>
                <div class="col-md-6">
                    <Rock:RockRadioButtonList ID="rblStatus" runat="server" Label="Member Status" RepeatDirection="Horizontal" />
                </div>
            </div>

            <div class="row">
                <div class="col-md-6">
                    <asp:PlaceHolder ID="phGroupMemberAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                </div>
            </div>

            <div class="actions">
                <asp:Button ID="btnSaveGroupMember" runat="server" AccessKey="s" CssClass="btn btn-primary" Text="Save" OnClick="btnSaveGroupMember_Click" />
                <asp:LinkButton id="btnCancelGroupMember" runat="server" AccessKey="c" CssClass="btn btn-link" OnClick="btnCancelGroupMember_Click" CausesValidation="false">Cancel</asp:LinkButton>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
