<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TextToGiveSetup.ascx.cs" Inherits="RockWeb.Blocks.Finance.TextToGiveSetup" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbAlert" runat="server" />
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-mobile"></i> Text To Give</h1>
            </div>
            <div class="panel-body">
                <form>
                    <div class="form-row col-sm-12">
                        <div class="form-group col-sm-12">
                            <p class="help-block">Setup your first gift on-line, future gifts can be completed via text.</p>
                        </div>
                    </div>
                    <div class="form-row col-md-6">
                        <div class="form-group col-sm-12">
                            <Rock:CampusAccountAmountPicker ID="caapDetailPicker" runat="server" />
                        </div>
                        <div class="form-group col-sm-12 visible-sm-block visible-xs-block">
                            <hr />
                        </div>
                    </div>                    
                    <div class="form-row col-md-6">
                        <div class="form-group col-sm-6">
                            <Rock:RockTextBox ID="tbFirstName" runat="server" CssClass="control-wrapper" Placeholder="First" />
                        </div>
                        <div class="form-group col-sm-6">
                            <Rock:RockTextBox ID="tbLastName" runat="server" CssClass="control-wrapper" Placeholder="Last" />
                        </div>
                        <div class="form-group col-sm-12">
                            <Rock:AddressControl ID="acAddress" runat="server" UseStateAbbreviation="true" UseCountryAbbreviation="false" ShowAddressLine2="false" />
                        </div>
                        <div class="form-group col-sm-12">
                            <Rock:RockTextBox ID="tbEmail" runat="server" CssClass="control-wrapper" Placeholder="Email" />
                        </div>
                        <div class="form-group col-sm-12 visible-sm-block visible-xs-block">
                            <hr />
                        </div>
                    </div>                    
                    <div class="form-row col-md-6">
                        <div class="form-group col-sm-12">
                            <Rock:RockDropDownList ID="ddlSavedAccountPicker" runat="server" DataTextField="Name" DataValueField="Id" />
                        </div>
                    </div>
                    <div class="form-row col-sm-12">
                        <asp:Button ID="btnSubmit" runat="server" OnClick="btnSubmit_Click" CausesValidation="true" formnovalidate="formnovalidate" Text="Give" />
                    </div>
                </form>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>