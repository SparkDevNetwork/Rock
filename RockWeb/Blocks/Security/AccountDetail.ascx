<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountDetail.ascx.cs" Inherits="RockWeb.Blocks.Security.AccountDetail" %>

<script>
    $(function () {
        $(".photo a").fluidbox();
    });
</script>

<div class="panel panel-block">
    <div class="panel-heading">
        <h1 class="panel-title"><i class="fa fa-user"></i> My Account</h1>
    </div>
    <div class="panel-body">

        <div class="row">

            <div class="col-sm-3">
                <div class="photo">
                    <asp:PlaceHolder ID="phImage" runat="server" />
                </div>
            </div>

            <div class="col-sm-9">

                <h1 class="title name">
                    <asp:Literal ID="lName" runat="server" /></h1>

                <div class="row">

                    <div class="col-sm-6">
                        <ul class="person-demographics list-unstyled">
                            <li><asp:Literal ID="lEmail" runat="server" /></li>
                            <li><asp:Literal ID="lGender" runat="server" /></li>
                            <li><asp:Literal ID="lAge" runat="server" /></li>
                        </ul>
                    </div>

                    <div class="col-sm-6">

                        <ul class="phone-list list-unstyled">
                        <asp:Repeater ID="rptPhones" runat="server">
                            <ItemTemplate>
                                <li><%# (bool)Eval("IsUnlisted") ? "Unlisted" : FormatPhoneNumber( Eval("CountryCode"), Eval("Number") ) %> <small><%# Eval("NumberTypeValue.Value") %></small></li>
                            </ItemTemplate>
                        </asp:Repeater>
                        </ul>

                        <asp:Literal ID="lAddress" runat="server" />

                        <asp:LinkButton ID="lbEditPerson" runat="server" CssClass="btn btn-primary btn-xs" OnClick="lbEditPerson_Click"><i class="fa fa-pencil"></i> Edit</asp:LinkButton>
        
                    </div>

                </div>

            </div>

        </div>

    </div>
</div>






