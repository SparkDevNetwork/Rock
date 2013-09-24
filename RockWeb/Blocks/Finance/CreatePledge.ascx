<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CreatePledge.ascx.cs" Inherits="RockWeb.Blocks.Finance.CreatePledge" %>

<asp:UpdatePanel ID="upCreatePledge" runat="server">
    <ContentTemplate>
        <asp:Panel runat="server" ID="pnlForm" CssClass="row-fluid">
            <div class="span12">
                <fieldset>
                    <legend><asp:Literal ID="lLegendText" runat="server"/></legend>
                    <Rock:DataTextBox ID="tbFirstName" runat="server" Label="First Name" SourceTypeName="Rock.Model.Person, Rock" PropertyName="NickName"/>
                    <Rock:DataTextBox ID="tbLastName" runat="server" Label="Last Name" SourceTypeName="Rock.Model.Person, Rock" PropertyName="LastName"/>
                    <asp:Repeater ID="rptAccounts" runat="server" OnItemDataBound="rptAccounts_ItemDataBound">
                        <ItemTemplate>
                            <asp:HiddenField ID="hfId" runat="server"/>
                            <Rock:RockTextBox ID="tbAmount" runat="server" PrependText="$"/>
                        </ItemTemplate>
                    </asp:Repeater>
                    <Rock:DataTextBox ID="tbEmail" runat="server" Label="Email" TextMode="Email" SourceTypeName="Rock.Model.Person, Rock" PropertyName="Email"/>
                    <Rock:DatePicker ID="dtpStartDate" runat="server" Label="Start Date" SourceTypeName="Rock.Model.FinancialPledge, Rock" PropertyName="StartDate" Visible="False"/>
                    <Rock:DatePicker ID="dtpEndDate" runat="server" Label="End Date" SourceTypeName="Rock.Model.FinancialPledge, Rock" PropertyName="EndDate" Visible="False"/>
                    <Rock:DataDropDownList ID="ddlFrequencyType" runat="server" SourceTypeName="Rock.Model.FinancialPledge, Rock" PropertyName="PledgeFrequencyValueId"/>
                    <asp:Panel ID="pnlConfirm" runat="server" CssClass="alert alert-info" Visible="False">
                        <p><strong>Information</strong> A pledge already exists for you.  Would your like to create an additional pledge?</p>
                        <div>
                            <asp:LinkButton ID="btnConfirmYes" runat="server" CssClass="btn btn-success" OnClick="btnConfirmYes_Click" CausesValidation="True"><i class="icon-ok"></i> Yes</asp:LinkButton>
                            <asp:LinkButton ID="btnConfirmNo" runat="server" CssClass="btn" OnClick="btnConfirmNo_Click"><i class="icon-remove"></i> No</asp:LinkButton>
                        </div>
                    </asp:Panel>
                </fieldset>
                <div class="form-actions">
                    <asp:Button ID="btnSave" runat="server" Text="Save Pledge" OnClick="btnSave_Click" CssClass="btn" CausesValidation="True"/>
                </div>
            </div>
        </asp:Panel>
        <asp:Panel runat="server" ID="pnlReceipt" Visible="False" CssClass="row-fluid">
            <div class="span12">
                <h3>Pledge complete!</h3>
                <p><strong><asp:Literal ID="lPersonFullName" runat="server"/></strong>, you pledged to give:</p>
                <asp:Repeater ID="rptCompletedPledges" runat="server" OnItemDataBound="rptCompletedPledges_ItemDataBound">
                    <ItemTemplate>
                        <div class="well">
                            <p>
                                <strong>
                                    <asp:Literal ID="lAmount" runat="server"/> 
                                    <asp:Literal ID="lFrequency" runat="server"/>
                                </strong> to the
                                <strong><asp:Literal ID="lAccountName" runat="server"/></strong>
                            </p>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            <div class="form-actions">
                <asp:LinkButton runat="server" CssClass="btn" ID="btnGivingProfile" OnClick="btnGivingProfile_Click">
                    <i class="icon-user"></i> Go to your giving profile
                </asp:LinkButton>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>