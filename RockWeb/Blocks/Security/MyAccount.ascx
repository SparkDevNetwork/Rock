<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MyAccount.ascx.cs" Inherits="RockWeb.Blocks.Security.MyAccount" %>

<script>
    $(function () {
        $(".photo a").fluidbox();
    });
</script>

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

            <div class="col-md-6">
                <asp:Literal ID="lEmail" runat="server" /><br />
                <asp:Literal ID="lGender" runat="server" /><br />
                <asp:Literal ID="lAge" runat="server" />
            </div>

            <div class="col-md-6">

                <asp:Repeater ID="rptPhones" runat="server">
                    <ItemTemplate>
                        <%# (bool)Eval("IsUnlisted") ? "Unlisted" : Rock.Model.PhoneNumber.FormattedNumber(Eval("Number").ToString()) %> <small><%# Eval("NumberTypeValue.Name") %></small><br />
                    </ItemTemplate>
                </asp:Repeater>
            
                <asp:LinkButton ID="lbEditPerson" runat="server" CssClass="btn btn-primary btn-xs" OnClick="lbEditPerson_Click"><i class="fa fa-pencil"></i> Edit</asp:LinkButton>
        
            </div>

        </div>

    </div>

</div>



