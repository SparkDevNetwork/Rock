<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonDuplicateDetail.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonDuplicateDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-user"></i>&nbsp;Person Duplicates</h1>



                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlblTest" runat="server" LabelType="Info" Text="Label" />
                </div>
            </div>
            <div class="panel-body">
                <Rock:Grid ID="gList" runat="server" AllowSorting="true" OnRowDataBound="gList_RowDataBound" PersonIdField="PersonId" >
                    <Columns>
                        <Rock:SelectField />
                        <asp:BoundField DataField="DuplicatePerson.FirstName" HeaderText="First Name" SortExpression="DuplicatePerson.FirstName, DuplicatePerson.LastName" />
                        <asp:BoundField DataField="DuplicatePerson.LastName" HeaderText="Last Name" SortExpression="DuplicatePerson.LastName, DuplicatePerson.FirstName" />
                        <asp:BoundField DataField="DuplicatePerson.Email" HeaderText="Email" SortExpression="DuplicatePerson.Email" />
                        <Rock:EnumField DataField="DuplicatePerson.Gender" HeaderText="Gender" SortExpression="DuplicatePerson.Gender" />

                        <asp:BoundField DataField="DuplicatePerson.Age" HeaderText="Age" SortExpression="DuplicatePerson.Age" />
                        <Rock:DateTimeField DataField="DuplicatePerson.ModifiedDateTime" HeaderText="ModifiedDateTime" SortExpression="DuplicatePerson.ModifiedDateTime" />
                        <asp:BoundField DataField="Score" HeaderText="Score" DataFormatString="{0:P}" NullDisplayText="-" />
                        <asp:TemplateField HeaderText="Campus">
                            <ItemTemplate>
                                <asp:Literal ID="lCampus" runat="server" Text='<%# GetCampus(Eval("DuplicatePerson") as Rock.Model.Person) %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Addresses">
                            <ItemTemplate>
                                <asp:Literal ID="lAddresses" runat="server" Text='<%# GetCampus(Eval("DuplicatePerson") as Rock.Model.Person) %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="PhoneNumbers">
                            <ItemTemplate>
                                <asp:Literal ID="lPhoneNumbers" runat="server" Text='<%# GetCampus(Eval("DuplicatePerson") as Rock.Model.Person) %>' />
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
