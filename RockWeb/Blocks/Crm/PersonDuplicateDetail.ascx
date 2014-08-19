<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonDuplicateDetail.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDuplicateDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-user"></i>&nbsp;Person Duplicates</h1>
            </div>
            <div class="panel-body">
                <Rock:Grid ID="gList" runat="server" AllowSorting="True" OnRowDataBound="gList_RowDataBound" PersonIdField="PersonId">
                    <Columns>
                        <Rock:SelectField />
                        <asp:TemplateField HeaderText="Formatted Score" ItemStyle-HorizontalAlign="Right" SortExpression="Score">
                            <ItemTemplate>
                                <%# GetMatchHtml((double?)Eval("Score")) %>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:BoundField DataField="DuplicatePerson.FirstName" HeaderText="First Name" SortExpression="DuplicatePerson.FirstName, DuplicatePerson.LastName" />
                        <asp:BoundField DataField="DuplicatePerson.LastName" HeaderText="Last Name" SortExpression="DuplicatePerson.LastName, DuplicatePerson.FirstName" />
                        <asp:BoundField DataField="DuplicatePerson.Email" HeaderText="Email" SortExpression="DuplicatePerson.Email" />
                        <Rock:EnumField DataField="DuplicatePerson.Gender" HeaderText="Gender" SortExpression="DuplicatePerson.Gender" />

                        <asp:BoundField DataField="DuplicatePerson.Age" HeaderText="Age" SortExpression="DuplicatePerson.Age" />

                        <asp:TemplateField HeaderText="Campus">
                            <ItemTemplate>
                                <ul class="list-unstyled">
                                    <asp:Repeater ID="rptrCampuses" runat="server" DataSource='<%# GetCampuses(Eval("DuplicatePerson") as Rock.Model.Person) %>'>
                                        <ItemTemplate>
                                            <li class="campus clearfix">
                                                <p>
                                                    <%# Eval("Name") %>
                                                </p>
                                            </li>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </ul>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Addresses">
                            <ItemTemplate>
                                <ul class="list-unstyled">
                                    <asp:Repeater ID="rptrAddresses" runat="server" DataSource='<%# GetGroupLocations(Eval("DuplicatePerson") as Rock.Model.Person) %>'>
                                        <ItemTemplate>
                                            <li class="address clearfix">

                                                <strong><%# Eval("GroupLocationTypeValue.Name") %></strong>
                                                <p>
                                                    <%# Eval("Location.FormattedHtmlAddress") %>
                                                </p>
                                            </li>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </ul>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="PhoneNumbers">
                            <ItemTemplate>
                                <ul class="list-unstyled">
                                    <asp:Repeater ID="rptrPhoneNumbers" runat="server" DataSource='<%# GetPhoneNumbers(Eval("DuplicatePerson") as Rock.Model.Person) %>'>
                                        <ItemTemplate>
                                            <li class="phonenumber clearfix">

                                                <strong><%# Eval("NumberTypeValue.Name") %></strong>
                                                <p>
                                                    <%# Eval("NumberFormatted") %>
                                                </p>
                                            </li>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </ul>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField>
                            <ItemTemplate>
                                <asp:LinkButton runat="server" ID="btnNotDuplicate" CssClass="btn btn-action js-not-duplicate" Text="Not Duplicate" OnClick="btnNotDuplicate_Click" CommandName="NotDuplicate" CommandArgument='<%# Eval("PersonDuplicateId") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </Rock:Grid>
            </div>

            <script>
                Sys.Application.add_load(function () {
                    $('.js-not-duplicate').on('click', function (e) {
                        // make sure the element that triggered this event isn't disabled
                        if (e.currentTarget && e.currentTarget.disabled) {
                            return false;
                        }

                        e.preventDefault();

                        Rock.dialogs.confirm("Are you sure this is not a duplicate?", function (result) {
                            if (result) {
                                var postbackJs = e.target.href ? e.target.href : e.target.parentElement.href;

                                // need to do unescape because firefox might put %20 instead of spaces
                                postbackJs = unescape(postbackJs);

                                // Careful!
                                eval(postbackJs);
                            }
                        })
                    });
                });
            </script>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
