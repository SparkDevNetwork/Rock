<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonDuplicateDetail.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDuplicateDetail" %>
<%@ Import Namespace="Rock.Utility.Enums" %>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-user"></i> Person Duplicates</h1>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbNoDuplicatesMessage" CssClass="alert-grid" runat="server" NotificationBoxType="Success" Text="No duplicates found for this person." />

                <div class="grid grid-panel">
                    <Rock:Grid ID="gList" runat="server" AllowSorting="True" OnRowDataBound="gList_RowDataBound" PersonIdField="PersonId">
                        <Columns>
                            <Rock:SelectField ShowHeader="false" />
                            <Rock:RockTemplateField HeaderText="Confidence" ItemStyle-HorizontalAlign="Right" SortExpression="ConfidenceScore" HeaderStyle-HorizontalAlign="Right">
                                <ItemTemplate>
                                    <%# GetConfidenceScoreColumnHtml((double?)Eval("ConfidenceScore")) %>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="Campus" HeaderText="Campus" SortExpression="Campus" />
                            <Rock:RockTemplateField HeaderText="Account Protection Profile" SortExpression="AccountProtectionProfile">
                                <ItemTemplate>
                                    <%# GetAccountProtectionProfileColumnHtml((AccountProtectionProfile)Eval("DuplicatePerson.AccountProtectionProfile")) %>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="FirstName" HeaderText="First Name" SortExpression="FirstName, LastName" />
                            <Rock:RockBoundField DataField="LastName" HeaderText="Last Name" SortExpression="LastName, FirstName" />
                            <Rock:RockBoundField DataField="Email" HeaderText="Email" SortExpression="Email" />
                            <Rock:EnumField DataField="Gender" HeaderText="Gender" SortExpression="Gender" />

                            <Rock:RockBoundField DataField="Age" HeaderText="Age" SortExpression="Age" />

                            
                            <Rock:RockTemplateField HeaderText="Addresses">
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
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateField HeaderText="Phone Numbers">
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
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateField ItemStyle-HorizontalAlign="Right"  HeaderStyle-HorizontalAlign="Right">
                                <ItemTemplate>
                                    <p>
                                        <a class="btn btn-square btn-default js-view-person" onclick="<%# GetPersonViewOnClick((int)Eval("PersonId")) %>" data-toggle="tooltip" title="View Person"><i class="fa fa-user fa-fw"></i></a>
                                    </p>
                                    <p>
                                        <asp:LinkButton runat="server" ID="btnNotDuplicate" CssClass="btn btn-square btn-default js-not-duplicate" data-toggle="tooltip" title="Not Duplicate" OnClick="btnNotDuplicate_Click" CommandName="NotDuplicate" CommandArgument='<%# Eval("PersonDuplicateId") %>'><i class="fa fa-ban fa-fw"></i></asp:LinkButton>
                                    </p>
                                    <p>
                                        <asp:LinkButton runat="server" ID="btnIgnoreDuplicate" CssClass="btn btn-square btn-default js-ignore-duplicate" data-toggle="tooltip" title="Ignore" OnClick="btnIgnoreDuplicate_Click" CommandName="IgnoreDuplicate" CommandArgument='<%# Eval("PersonDuplicateId") %>'><i class="fa fa-bell-slash fa-fw"></i></asp:LinkButton>
                                    </p>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

            <script>
                Sys.Application.add_load(function () {

                    $('.js-view-person').tooltip();
                    $('.js-not-duplicate').tooltip();
                    $('.js-ignore-duplicate').tooltip();

                    $('.js-not-duplicate').on('click', function (e) {
                        // make sure the element that triggered this event isn't disabled
                        if (e.currentTarget && e.currentTarget.disabled) {
                            return false;
                        }

                        e.preventDefault();

                        Rock.dialogs.confirm("Are you sure this is not a duplicate?", function (result) {
                            if (result) {
                                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                            }
                        })
                    });


                    $('.js-ignore-duplicate').on('click', function (e) {
                        // make sure the element that triggered this event isn't disabled
                        if (e.currentTarget && e.currentTarget.disabled) {
                            return false;
                        }

                        e.preventDefault();

                        Rock.dialogs.confirm("This will ignore this potential duplicate until the score changes.  Are you sure you want to ignore this as a duplicate?", function (result) {
                            if (result) {
                                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                            }
                        })
                    });
                });
            </script>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
