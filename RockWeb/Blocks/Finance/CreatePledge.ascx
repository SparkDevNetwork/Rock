<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CreatePledge.ascx.cs" Inherits="RockWeb.Blocks.Finance.CreatePledge" %>

<asp:UpdatePanel ID="upCreatePledge" runat="server">
    <ContentTemplate>
        <asp:Panel runat="server" ID="pnlForm">
            <fieldset>
                <legend><asp:Literal ID="lLegendText" runat="server"/></legend>
                <Rock:DataTextBox ID="tbFirstName" runat="server" LabelText="First Name" SourceTypeName="Rock.Model.Person, Rock" PropertyName="NickName"/>
                <Rock:DataTextBox ID="tbLastName" runat="server" LabelText="Last Name" SourceTypeName="Rock.Model.Person, Rock" PropertyName="LastName"/>
                <%--<Rock:DataTextBox ID="tbAmount" runat="server" PrependText="$" LabelText="Total Amount" SourceTypeName="Rock.Model.Pledge, Rock" PropertyName="Amount"/>--%>
                
                <asp:Repeater ID="rptAccounts" runat="server" OnItemDataBound="rptAccounts_ItemDataBound">
                    <ItemTemplate>
                        <asp:HiddenField ID="hfId" runat="server"/>
                        <Rock:LabeledTextBox ID="tbAmount" runat="server" PrependText="$"/>
                    </ItemTemplate>
                </asp:Repeater>

                <Rock:DataTextBox ID="tbEmail" runat="server" LabelText="Email" TextMode="Email" SourceTypeName="Rock.Model.Person, Rock" PropertyName="Email"/>
                <Rock:DateTimePicker ID="dtpStartDate" runat="server" LabelText="Start Date" SourceTypeName="Rock.Model.FinancialPledge, Rock" PropertyName="StartDate" Visible="False"/>
                <Rock:DateTimePicker ID="dtpEndDate" runat="server" LabelText="End Date" SourceTypeName="Rock.Model.FinancialPledge, Rock" PropertyName="EndDate" Visible="False"/>
                <Rock:DataDropDownList ID="ddlFrequencyType" runat="server" SourceTypeName="Rock.Model.FinancialPledge, Rock" PropertyName="PledgeFrequencyValueId"/>
                <asp:Panel ID="pnlConfirm" runat="server" CssClass="alert alert-info" Visible="False">
                    <p><strong>Hey!</strong> You currently have a pledge in the system. Do you want to replace it with this one?</p>
                    <div class="actions">
                        <asp:LinkButton ID="btnConfirmYes" runat="server" CssClass="btn btn-success" OnClick="btnConfirmYes_Click" CausesValidation="True"><i class="icon-ok"></i> Yes</asp:LinkButton>
                        <asp:LinkButton ID="btnConfirmNo" runat="server" CssClass="btn" OnClick="btnConfirmNo_Click"><i class="icon-remove"></i> No</asp:LinkButton>
                    </div>
                </asp:Panel>
            </fieldset>
            <div class="actions">
                <asp:Button ID="btnSave" runat="server" Text="Save Pledge" OnClick="btnSave_Click" CssClass="btn" CausesValidation="True"/>
            </div>
        </asp:Panel>
        <asp:Panel runat="server" ID="pnlReceipt" Visible="False">
            <h1></h1>
            <asp:Hyperlink runat="server" CssClass="btn" ID="btnGivingProfile">
                <i class="icon-user"></i> Go to your giving profile
            </asp:Hyperlink>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>