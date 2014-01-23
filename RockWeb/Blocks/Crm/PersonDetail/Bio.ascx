<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Bio.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.Bio" %>

<script>
    $(function () {
        $(".photo a").fluidbox();
    });
</script>

<div class="rollover-container">
    <div class="actions rollover-item">
        <asp:LinkButton ID="lbEditPerson" runat="server" CssClass="edit btn btn-link btn-xs" OnClick="lbEditPerson_Click"><i class="fa fa-pencil"></i> Edit Individual</asp:LinkButton>
    </div>

    <div class="row">
	    <div class="col-sm-6">
		    <h1 class="title name"><asp:Literal ID="lName" runat="server" /></h1>
	    </div>
        <div class="col-sm-6 labels">

            <Rock:PersonProfileBadgeList id="blStatus" runat="server" />

        </div>
    </div> 

    <div class="row">
	    <div class="col-sm-4">
            <div class="photo">
                <asp:PlaceHolder ID="phImage" runat="server" />
            </div>
        </div>

        <div class="col-sm-8">
            <div class="row">
                <div class="col-md-4">
                    <div class="summary">
                        <div class="taglist clearfix">
                            <Rock:TagList ID="taglPersonTags" runat="server"/>
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

                        <ul class="list-unstyled phonenumbers">
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
        </div>

        
    </div>
</div>


