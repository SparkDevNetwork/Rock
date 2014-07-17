<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AddFamily.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.AddFamily" %>

<asp:UpdatePanel ID="upAddFamily" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlFamilyData" runat="server">
        
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-plus-square-o"></i> <asp:Literal ID="lTitle" runat="server"></asp:Literal></h1>
                </div>
                <div class="panel-body">

                    <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <div class="row">
                        <div class="col-md-4">
                            <h4>Family Members</h4>
                        </div>
                        <div class="col-md-8">
                            <div class="form-horizontal">
                                <div class="form-group form-no-margin">
                                    <label class="col-sm-8 control-label label-right">
                                        <asp:Literal ID="lAdultCaption" runat="server" />
                                    </label>
                                    <div class="col-sm-4">
                                        <Rock:RockDropDownList ID="ddlMaritalStatus" CssClass="input-sm" runat="server" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:NewFamilyMembers id="nfmMembers" runat="server" OnAddFamilyMemberClick="nfmMembers_AddFamilyMemberClick" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-4">
                            <Rock:CampusPicker ID="cpCampus" runat="server" Required="true" />
                        </div>

                        <div class="col-md-8">
                            <Rock:AddressControl ID="acAddress" runat="server" UseStateAbbreviation="false" UseCountryAbbreviation="false" />
                        </div>

                    </div>

                    <asp:Panel ID="pnlAttributes" runat="server" Visible="true">
                    </asp:Panel>

                    <div class="actions">
                        <asp:LinkButton ID="btnPrevious" runat="server" Text="Previous" CssClass="btn btn-link" OnClick="btnPrevious_Click" Visible="false" CausesValidation="false" />
                        <asp:LinkButton ID="btnNext" runat="server" Text="Next" CssClass="btn btn-primary" OnClick="btnNext_Click" />
                    </div>
                </div>
            </div>    
        </asp:Panel>

        

    </ContentTemplate>
</asp:UpdatePanel>
