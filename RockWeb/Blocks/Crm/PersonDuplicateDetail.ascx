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


                <div class="row">
                    <div class="col-md-6">
                        <asp:Literal runat="server" ID="lPersonInfoCol1" />
                    </div>
                    <div class="col-md-6">
                        <asp:Literal runat="server" ID="lPersonInfoCol2" />
                    </div>
                </div>

                <div class="row">
                    <div class="col-md-6">
                        
                    </div>
                </div>

                <h5>Possible Matches</h5>
                <Rock:Grid ID="gList" runat="server" AllowSorting="true" OnRowDataBound="gList_RowDataBound">
                    <Columns>
                        <asp:BoundField DataField="DuplicatePerson.FirstName" HeaderText="First Name" SortExpression="DuplicatePerson.FirstName, DuplicatePerson.LastName" />
                        <asp:BoundField DataField="DuplicatePerson.LastName" HeaderText="Last Name" SortExpression="DuplicatePerson.LastName, DuplicatePerson.FirstName" />
                        <asp:BoundField DataField="DuplicatePerson.Email" HeaderText="Email" SortExpression="DuplicatePerson.Email" />
                        <Rock:EnumField DataField="DuplicatePerson.Gender" HeaderText="Gender" SortExpression="DuplicatePerson.Gender" />

                        <asp:BoundField DataField="DuplicatePerson.Age" HeaderText="Age" SortExpression="DuplicatePerson.Age" />
                        <Rock:DateTimeField DataField="DuplicatePerson.ModifiedDateTime" HeaderText="ModifiedDateTime" SortExpression="DuplicatePerson.ModifiedDateTime" />
                        <asp:BoundField DataField="Score" HeaderText="Score" />
                        <asp:BoundField DataField="Capacity" HeaderText="Capacity" />
                        <%--
                            <asp:BoundField DataField="Campus" HeaderText="Campus" SortExpression="Campus"  />
                            <asp:BoundField DataField="Addresses" HeaderText="Addresses" />
                            <asp:BoundField DataField="PhoneNumbers" HeaderText="Phone Numbers" />
                        --%>
                        <asp:TemplateField>
                            <ItemTemplate>
                                <asp:LinkButton runat="server" ID="btnMerge" CssClass="btn btn-primary" Text="Merge" OnClick="btnMerge_Click" />
                                <asp:LinkButton runat="server" ID="btnNotDuplicate" CssClass="btn btn-action" Text="Confirm Not Duplicate" OnClick="btnNotDuplicate_Click" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </Rock:Grid>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
