<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MoreThanUsPledgeEntry.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Finance.MoreThanUsPledgeEntry" %>
<asp:UpdatePanel ID="upMain" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbMain" runat="server" Visible="false" />
        <asp:HiddenField ID="hfPledgeId" runat="server" />
        <asp:Panel ID="pnlPledgeEntry" runat="server" CssClass="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="far fa-poll-people"></i>More than Us Pledge</h1>
            </div>
            <asp:Panel ID="pnlEdit" runat="server" CssClass="panel-body" Visible="false">
                
                <asp:ValidationSummary ID="varFinancialPledge" runat="server"  ValidationGroup="vgPledge" CssClass="alert alert-validation"/>
                        <fieldset>
                            <div class="row">
                                <div class="col-sm-6">
                                    <Rock:PersonPicker ID="pPerson" runat="server" EnableSelfSelection="true" ExpandSearchOptions="false"  Label="Person" Required="true"  ValidationGroup="vgPledge" RequiredErrorMessage="Person selection is required." />
                                </div>
                                <div class="col-sm-6">
                                    <Rock:AccountPicker ID="pPledgeAccount" runat="server" Label="Account" Required="true" AllowMultiSelect="false" ValidationGroup="vgPledge" RequiredErrorMessage="Account Selection is required." />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-sm-6">
                                    <Rock:DatePicker ID="dpStartDate" runat="server" Label="Start Date" Required="true" ValidationGroup="vgPledge" RequiredErrorMessage="Start Date is requried." />
                                </div>
                                <div class="col-sm-6">
                                    <Rock:DatePicker ID="dpEndDate" runat="server" Label="End Date" Required="true" ValidationGroup="vgPledge" RequiredErrorMessage="End Date is required" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-sm-6">
                                    <Rock:CurrencyBox ID="tbPledgeAmount" runat="server" Label="Pledge Amount" Required="true" ValidationGroup="vgPledge" RequiredErrorMessage="Pledge Amount is required." />                                  
                                </div>
                                <div class="col-sm-6">
                                    <Rock:CurrencyBox ID="tbOneTimeGiftAmount" runat="server" Label="One Time Gift Amount" Required="false" ValidationGroup="vgPledge" />
                                </div>
                            </div>                           
                            <asp:HiddenField ID="hfSource" runat="server" />
                        </fieldset>
                        <div class="actions" >
                            <asp:LinkButton ID="lbSave" runat="server" CssClass="btn btn-primary" CausesValidation="true" ValidationGroup="vgPledge">Save</asp:LinkButton>
                            <asp:LinkButton ID="lbSaveAndAdd" runat="server" CssClass="btn btn-secondary" CausesValidation="true" ValidationGroup="vgPledge">Save Then Add New </asp:LinkButton>
                            <asp:LinkButton ID="lbCancel" runat="server" CssClass="btn btn-clear" CausesValidation="false">Cancel</asp:LinkButton>

                        </div>
            </asp:Panel>
            <asp:Panel ID="pnlView" runat="server" CssClass="panel-body" Visible="false">
                <div class="row">
                    <div class="col-sm-6">
                        <Rock:RockLiteral ID="lPerson" runat="server" Label="Person"/>
                    </div>
                    <div class="col-sm-6">
                        <Rock:RockLiteral ID="lAccount" runat="server" Label="Account" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-sm-6">
                        <Rock:RockLiteral ID="lStartDate" runat="server" Label="Start Date" />
                    </div>
                    <div class="col-sm-6">
                        <Rock:RockLiteral ID="lEndDate" runat="server" Label="End Date" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-sm-6">
                        <Rock:RockLiteral ID="lPledgeAmount" runat="server" Label="Pledge Amount" />
                    </div>
                    <div class="col-sm-6">
                        <Rock:RockLiteral ID="lOneTimeGiftAmount" runat="server" Label="One Time Gift" />
                    </div>
                </div>
                <div class="actions">
                    <asp:LinkButton ID="lbEdit" runat="server" CssClass="btn btn-primary" CausesValidation="false">Edit</asp:LinkButton>
                </div>

            </asp:Panel>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>