<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AddFamily.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.AddFamily" %>

<asp:UpdatePanel ID="upAddFamily" runat="server">
    <ContentTemplate>


        <asp:ValidationSummary ID="valSummaryTop" runat="server"  
            HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error" />

        <asp:Panel ID="pnlFamilyData" runat="server">
            <div class="row">

                <div class="col-md-4">

                    <fieldset>
                        <Rock:RockTextBox ID="tbFamilyName" runat="server" Label="Family Name" Required="true" CssClass="input-meduim" />
                    </fieldset>

                </div>

                <div class="col-md-8">

                    <fieldset>
                        <Rock:CampusPicker ID="cpCampus" runat="server" Required="true" />
                        <Rock:RockTextBox ID="tbStreet1" runat="server" Label="Address Line 1" />
                        <Rock:RockTextBox ID="tbStreet2" runat="server" Label="Address Line 2" />

                        <div class="row">
                            <div class="col-lg-7">
                                <Rock:RockTextBox ID="tbCity" Label="City"  runat="server" />
                            </div>
                            <div class="col-lg-2">
                                <Rock:StateDropDownList ID="ddlState" Label="State" runat="server" UseAbbreviation="true" CssClass="input-mini" />
                            </div>
                            <div class="col-lg-3">
                                <Rock:RockTextBox ID="tbZip" Label="Zip" runat="server" CssClass="input-small" />
                            </div>
                        </div>

                    </fieldset>
                </div>

            </div>

            <div class="row">
                <div class="col-md-12">
                    <Rock:NewFamilyMembers id="nfmMembers" runat="server" OnAddFamilyMemberClick="nfmMembers_AddFamilyMemberClick" />
                </div>
            </div>
            

        </asp:Panel>

        <asp:Panel ID="pnlAttributes" runat="server" Visible="true">
        </asp:Panel>

        <div class="actions">
            <asp:LinkButton ID="btnPrevious" runat="server" Text="Previous" CssClass="btn" OnClick="btnPrevious_Click" Visible="false" CausesValidation="false" />
            <asp:LinkButton ID="btnNext" runat="server" Text="Next" CssClass="btn btn-primary" OnClick="btnNext_Click" />
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
