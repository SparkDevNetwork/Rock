<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupDetailLava.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupDetailLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        
        <asp:HiddenField id="hfPostbackBind" ClientIDMode="Static" runat="server" />

        <asp:Panel ID="pnlGroupView" runat="server">
            <asp:Literal ID="lContent" runat="server"></asp:Literal>

            <asp:Literal ID="lDebug" runat="server"></asp:Literal>
        </asp:Panel>
        
        <asp:Panel ID="pnlGroupEdit" runat="server" Visible="false">
            <asp:ValidationSummary ID="vsGroupEdit" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
            
            <div class="row">
                <div class="col-md-6">
                    <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.Group, Rock" PropertyName="Name" />
                </div>
                <div class="col-md-6">
                    <Rock:RockCheckBox ID="cbIsActive" runat="server" Text="Active" />
                </div>
            </div>

            <div class="row">
                <div class="col-md-12">
                    <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.Group, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                </div>
            </div>

            <div class="row">
                <div class="col-md-12">
                    <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="true" />
                </div>
            </div>

            <div class="actions">
                <asp:Button ID="btnSave" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="btnSave_Click" />
                <asp:LinkButton id="lbCancel" runat="server" CssClass="btn btn-link" OnClick="lbCancel_Click" CausesValidation="false">Cancel</asp:LinkButton>
            </div>

        </asp:Panel>

        <asp:Panel ID="pnlEditGroupMember" runat="server" Visible="false">
            <asp:ValidationSummary ID="vsEditGroupMember" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
            <Rock:NotificationBox ID="nbGroupMemberErrorMessage" runat="server" NotificationBoxType="Danger" />

            <div class="row">
                <div class="col-md-6">
                    <Rock:PersonPicker runat="server" ID="ppGroupMemberPerson" Label="Person" Required="true"/>
                </div>
                <div class="col-md-6">
                    <Rock:RockRadioButtonList ID="rblStatus" runat="server" Label="Status" RepeatDirection="Horizontal" />
                </div>
            </div>

            <div class="row">
                <div class="col-md-6">
                    <Rock:RockDropDownList runat="server" ID="ddlGroupRole" DataTextField="Name" DataValueField="Id" Label="Role" Required="true" />
                </div>
                <div class="col-md-6">
                    <asp:PlaceHolder ID="phGroupMemberAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                </div>
            </div>

            <div class="actions">
                <asp:Button ID="btnSaveGroupMember" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="btnSaveGroupMember_Click" />
                <asp:LinkButton id="btnCancelGroupMember" runat="server" CssClass="btn btn-link" OnClick="btnCancelGroupMember_Click" CausesValidation="false">Cancel</asp:LinkButton>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
