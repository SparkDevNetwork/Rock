<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AddFamily.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.AddFamily" %>

<asp:UpdatePanel ID="upAddFamily" runat="server">
    <ContentTemplate>


        <asp:ValidationSummary ID="valSummaryTop" runat="server"  
            HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error" />

        <asp:Panel ID="pnlFamilyData" runat="server">
            <div class="row-fluid">

                <div class="span4 form-horizontal">

                    <fieldset>
                        <Rock:LabeledTextBox ID="tbFamilyName" runat="server" LabelText="Family Name" Required="true" CssClass="input-meduim" />
                    </fieldset>

                </div>

                <div class="span8 form-horizontal">

                    <fieldset>
                        <Rock:CampusPicker ID="cpCampus" runat="server" Required="true" />
                        <Rock:LabeledTextBox ID="tbStreet1" runat="server" LabelText="Address Line 1" />
                        <Rock:LabeledTextBox ID="tbStreet2" runat="server" LabelText="Address Line 2" />
                        <div class="control-group">
                            <div class="control-label">City</div>
                            <div class="controls">
                                <asp:TextBox ID="tbCity" runat="server" />
                                &nbsp;&nbsp;&nbsp;&nbsp;State&nbsp;&nbsp;
                                <Rock:StateDropDownList ID="ddlState" runat="server" UseAbbreviation="true" CssClass="input-mini" />
                                &nbsp;&nbsp;&nbsp;&nbsp;Zip&nbsp;&nbsp;
                                <asp:TextBox ID="tbZip" runat="server" CssClass="input-small" />
                            </div>
                        </div>
                    </fieldset>
                </div>

            </div>

            <Rock:NewFamilyMembers id="nfmMembers" runat="server" OnAddFamilyMemberClick="nfmMembers_AddFamilyMemberClick" />

        </asp:Panel>

        <asp:Panel ID="pnlAttributes" runat="server" Visible="true">
        </asp:Panel>

        <div class="actions">
            <asp:LinkButton ID="btnPrevious" runat="server" Text="Previous" CssClass="btn btn-mini" OnClick="btnPrevious_Click" Visible="false" CausesValidation="false" />
            <asp:LinkButton ID="btnNext" runat="server" Text="Next" CssClass="btn btn-primary btn-mini" OnClick="btnNext_Click" />
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
