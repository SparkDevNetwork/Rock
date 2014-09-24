<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonDuplicateDetail.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDuplicateDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-user"></i> Person Duplicates</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" AllowSorting="True" OnRowDataBound="gList_RowDataBound" PersonIdField="PersonId">
                        <Columns>
                            <Rock:SelectField ShowHeader="false" />
                            <asp:TemplateField HeaderText="Confidence" ItemStyle-HorizontalAlign="Right" SortExpression="ConfidenceScore">
                                <ItemTemplate>
                                    <%# GetConfidenceScoreColumnHtml((double?)Eval("ConfidenceScore")) %>
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

                                                    <strong><%# Eval("GroupLocationTypeValue.Value") %></strong>
                                                    <p>
                                                        <%# Eval("Location.FormattedHtmlAddress") %>
                                                    </p>
                                                </li>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </ul>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Phone Numbers">
                                <ItemTemplate>
                                    <ul class="list-unstyled">
                                        <asp:Repeater ID="rptrPhoneNumbers" runat="server" DataSource='<%# GetPhoneNumbers(Eval("DuplicatePerson") as Rock.Model.Person) %>'>
                                            <ItemTemplate>
                                                <li class="phonenumber clearfix">

                                                    <strong><%# Eval("NumberTypeValue.Value") %></strong>
                                                    <p>
                                                        <%# Eval("NumberFormatted") %>
                                                    </p>
                                                </li>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </ul>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <a class="btn btn-default js-view-person" onclick="<%# GetPersonViewOnClick((int)Eval("PersonId")) %>" data-toggle="tooltip" title="View Person"><i class="fa fa-user"></i></a>
                                    <asp:LinkButton runat="server" ID="btnNotDuplicate" CssClass="btn btn-default js-not-duplicate" data-toggle="tooltip" title="Not Duplicate" OnClick="btnNotDuplicate_Click" CommandName="NotDuplicate" CommandArgument='<%# Eval("PersonDuplicateId") %>'><i class="fa fa-ban"></i></asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

            <script>
                Sys.Application.add_load(function () {

                    $('.js-view-person').tooltip();
                    $('.js-not-duplicate').tooltip();

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
