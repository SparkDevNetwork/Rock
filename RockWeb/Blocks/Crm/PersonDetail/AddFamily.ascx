<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AddFamily.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.AddFamily" %>

<asp:UpdatePanel ID="upAddFamily" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-plus-square-o"></i>
                    <asp:Literal ID="lTitle" runat="server"></asp:Literal></h1>
            </div>
            <div class="panel-body">

                <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <asp:Panel ID="pnlFamilyData" runat="server">

                    <div class="row">
                        <div class="col-md-12">
                            <h4>Family Members</h4>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:NewFamilyMembers ID="nfmMembers" runat="server" OnAddFamilyMemberClick="nfmMembers_AddFamilyMemberClick" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-4">
                            <Rock:CampusPicker ID="cpCampus" runat="server" Required="true" />
                            <Rock:RockDropDownList ID="ddlMaritalStatus" runat="server" Required="true" 
                                Help="The marital status to use for the adults in this family."
                                Label="Marital Status of Adults" RequiredErrorMessage="Marital Status of Adults is Required" />
                        </div>

                        <div class="col-md-8">
                            <Rock:AddressControl ID="acAddress" runat="server" UseStateAbbreviation="false" UseCountryAbbreviation="false" />
                        </div>
                    </div>

                </asp:Panel>

                <asp:Panel ID="pnlContactInfo" runat="server" Visible="false">
                    <Rock:NewFamilyContactInfo ID="nfciContactInfo" runat="server" />
                </asp:Panel>

                <asp:Panel ID="pnlAttributes" runat="server" Visible="true">
                </asp:Panel>

                <div class="actions">
                    <asp:LinkButton ID="btnPrevious" runat="server" Text="Previous" CssClass="btn btn-link" OnClick="btnPrevious_Click" Visible="false" CausesValidation="false" />
                    <asp:LinkButton ID="btnNext" runat="server" Text="Next" CssClass="btn btn-primary" OnClick="btnNext_Click" />
                </div>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
