<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Bio.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.Bio" %>

<div class="actions" style="display: none;">
    <asp:LinkButton ID="lbEditPerson" runat="server" CssClass="edit btn btn-xs" OnClick="lbEditPerson_Click"><i class="icon-pencil"></i> Edit Individual</asp:LinkButton>
</div>

<div class="row">
	<div class="col-md-6">
		<h1 class="title"><asp:Literal ID="lName" runat="server" /></h1>
	</div>
    <div class="col-md-6 labels">

        <Rock:PersonProfileBadgeList id="blStatus" runat="server" />

        <ul class="nav pull-right">
            <li class="dropdown">
                <a class="persondetails-actions dropdown-toggle" data-toggle="dropdown" href="#" tabindex="0">
                    <i class="icon-cog"></i>
                    <span>Actions</span>
                    <b class="caret"></b>
                </a>
                <ul class="dropdown-menu">
                    <li>
                        <a href="/MyAccount" tabindex="0">Add to Starting Point</a>
                        <a href="/MyAccount" tabindex="0">Add to Foundations</a>
                        <a href="/MyAccount" tabindex="0">Email Individual</a>
                    </li>
                    <li class="divider"></li>
                    <li><a href="">Report Data Error</a></li>
                </ul>
            </li>
        </ul>

    </div>
</div> <!-- end row -->
<div class="row">
	<div class="col-md-2">
        <div class="photo">
            <asp:PlaceHolder ID="phImage" runat="server"></asp:PlaceHolder>
        </div>
    </div>

    <div class="col-md-4">
        <div class="summary">
            <div class="tags clearfix">
                <Rock:TagList ID="tlPersonTags" runat="server" />
            </div>
            <div class="demographics">
                <asp:Literal ID="lAge" runat="server" />
                <asp:Literal ID="lGender" runat="server" /><br />
                <asp:Literal ID="lMaritalStatus" runat="server" /> 
                <asp:Literal ID="lAnniversary" runat="server" />
            </div>
        </div>
    </div>

    <div class="col-md-6">
        <div class="personcontact">

            <ul class="unstyled phonenumbers">
            <asp:Repeater ID="rptPhones" runat="server">
                <ItemTemplate>
                    <li data-value="<%# Eval("Number") %>"><%# (bool)Eval("IsUnlisted") ? "Unlisted" : Rock.Model.PhoneNumber.FormattedNumber(Eval("Number").ToString()) %> <small><%# Eval("NumberTypeValue.Name") %></small></li>
                </ItemTemplate>
            </asp:Repeater>
            </ul>

            <div class="email">
                <asp:Literal ID="lEmail" runat="server" />
            </div>
        </div>
    </div>
</div>

