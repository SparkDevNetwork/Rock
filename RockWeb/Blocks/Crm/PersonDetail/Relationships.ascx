<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Relationships.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.Relationships" %>
<asp:UpdatePanel ID="upRelationships" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfRoleId" runat="server" />
        <div class="card card-profile">
            <div class="card-header group-hover">
                <span class="card-title">
                    <asp:Literal ID="lGroupName" runat="server"></asp:Literal>
                </span>
                <asp:PlaceHolder ID="phEditActions" runat="server">
                    <div class="panel-labels group-hover-item group-hover-show">
                        <asp:LinkButton ID="lbAdd" runat="server" CssClass="btn btn-link btn-xs" Text="Add Relationship" OnClick="lbAdd_Click" CausesValidation="false"><i class="fa fa-plus"></i></asp:LinkButton>
                    </div>
                </asp:PlaceHolder>
            </div>

            <asp:Literal ID="lAccessWarning" runat="server" />
            <asp:Repeater ID="rGroupMembers" runat="server">
                <HeaderTemplate>
                    <div class="horizontal-dl horizontal-dl-striped">
                </HeaderTemplate>
                <ItemTemplate>
                    <dl class="group-hover">
                        <dt><Rock:PersonLink runat="server"
                                PersonId='<%# Eval("PersonId") %>'
                                PersonName='<%# Eval("Person.FullName") %>' /> <span class="text-danger"><asp:Literal ID="lDeceased" runat="server" /></span></dt>
                        <dd class="group-hover-item group-hover-hide"><%# ShowRole ? Eval("GroupRole.Name") : "" %></dd>
                        <div class="group-hover-item group-hover-show group-hover-0-show">
                                <asp:LinkButton ID="lbEdit" runat="server" CssClass="btn btn-default btn-xs btn-square" Text="Edit Relationship" Visible='<%# IsInverseRelationshipsOwner %>'
                                    CommandName="EditRole" CommandArgument='<%# Eval("Id") %>'><i class="fa fa-pencil"></i></asp:LinkButton>
                                <asp:LinkButton ID="lbRemove" runat="server" CssClass="btn btn-danger btn-xs btn-square" Text="Remove Relationship" Visible='<%# IsInverseRelationshipsOwner %>'
                                    CommandName="RemoveRole" CommandArgument='<%# Eval("Id") %>'><i class="fa fa-times"></i></asp:LinkButton>
                        </div>
                    </dl>
                </ItemTemplate>
                <FooterTemplate>
                    </div>
                    <div ID="divNoRelationshipsFound" runat="server" Visible='<%# rGroupMembers.Items.Count == 0 %>' class="card-body"></div>
                </FooterTemplate>
            </asp:Repeater>
        </div>

        <Rock:ModalDialog ID="modalAddPerson" runat="server" Title="Add Relationship" ValidationGroup="NewRelationship">
            <Content>
                <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="NewRelationship" />

                <div id="divExistingPerson" runat="server">
                    <fieldset>
                        <Rock:GroupRolePicker ID="grpRole" runat="server" Label="Relationship Type" ValidationGroup="NewRelationship" />
                        <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" Required="true" ValidationGroup="NewRelationship" />
                        <asp:Panel ID="pnlSelectedPerson" runat="server" />
                    </fieldset>
                </div>
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
