<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Relationships.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDetail.Relationships" %>
<asp:UpdatePanel ID="upEditFamily" runat="server">
    <ContentTemplate>

        <section class="panel panel-default">

            <div class="panel-heading clearfix">
                <h3 class="panel-title pull-left">
                    <asp:PlaceHolder ID="phGroupTypeIcon" runat="server"></asp:PlaceHolder>
                    <asp:Literal ID="lGroupName" runat="server"></asp:Literal></h3>
                <asp:PlaceHolder ID="phEditActions" runat="server">
                    <div class="actions pull-right">
                        <asp:LinkButton ID="lbAdd" runat="server" CssClass="edit" Text="Add Relationship" OnClick="lbAdd_Click"><i class="icon-plus"></i></asp:LinkButton>
                    </div>
                </asp:PlaceHolder>
            </div>

            <div class="panel-body">
                <ul class="personlist">
                    <asp:Repeater ID="rGroupMembers" runat="server">
                        <ItemTemplate>
                            <li>
                                <Rock:PersonLink runat="server"
                                    PersonId='<%# Eval("PersonId") %>'
                                    PersonName='<%# Eval("Person.FullName") %>'
                                    Role='<%# ShowRole ? Eval("GroupRole.Name") : "" %>'
                                    PhotoId='<%# Eval("Person.PhotoId") %>' />
                                <div class="actions pull-right">
                                    <asp:LinkButton ID="lbEdit" runat="server" CssClass="edit" Text="Edit Relationship"
                                         CommandName="EditRole" CommandArgument='<%# Eval("Id") %>'><i class="icon-pencil"></i></asp:LinkButton>
                                    <asp:LinkButton ID="lbRemove" runat="server" CssClass="edit remove-relationship" Text="Remove Relationship" 
                                        CommandName="RemoveRole"  CommandArgument='<%# Eval("Id") %>'><i class="icon-remove"></i></asp:LinkButton>
                                </div>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>
            </div>

            <Rock:ModalDialog ID="modalAddPerson" runat="server" Title="Add Relationship" Content-Height="380">
                <Content>

                    <div id="divExistingPerson" runat="server">
                        <fieldset>
                            <Rock:GroupRolePicker ID="grpRole" runat="server" LabelText="Relationship Type" Required="true" />
                            <Rock:PersonPicker2 ID="ppPerson" runat="server" />
                        </fieldset>
                    </div>

                </Content>
            </Rock:ModalDialog>

        </section>

    </ContentTemplate>
</asp:UpdatePanel>
