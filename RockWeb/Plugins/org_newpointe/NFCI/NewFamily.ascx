<%@ Control Language="C#" AutoEventWireup="true" CodeFile="NewFamily.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.NFCI.NewFamily" %>

<asp:UpdatePanel ID="upAddGroup" runat="server">
    <ContentTemplate>
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-plus-square-o"></i><asp:Literal ID="lTitle" runat="server"></asp:Literal>
                </h1>
            </div>
            <div class="panel-body">

                <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                <asp:Panel runat="server" ID="pnlNewFamily">
                    <h2>Parent Info</h2>
                    <div class="row">
                        <div class="col-md-6"><Rock:RockTextBox runat="server" ID="rtbParentFirstName" Label="First Name" Required="true" RequiredErrorMessage="Parent's First Name is Required"></Rock:RockTextBox></div>
                        <div class="col-md-6"><Rock:RockTextBox runat="server" ID="rtbParentLastName" Label="Last Name" Required="true" RequiredErrorMessage="Parent's Last Name is Required"></Rock:RockTextBox></div>
                    </div>
                    <div class="row">
                        <div class="col-md-12"><Rock:PhoneNumberBox runat="server" ID="pnbParentPhoneNumber" Label="Mobile Phone Number" Required="true" RequiredErrorMessage="Mobile Phone Number is Required"></Rock:PhoneNumberBox></div>
                    </div>
    
                    <h2>Children</h2>
                    <table class="table table-groupmembers">
                        <thead>
                            <tr>
                                <th>Name</th>
                                <th>Gender</th>
                                <th>Birthdate/Grade</th>
                                <th>Allergies</th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater runat="server" ID="rKids" OnItemDataBound="rKids_ItemDataBound" OnItemCommand="rKids_ItemCommand">
                                <ItemTemplate>
                                    <tr>
                                        <td>
                                            <Rock:RockTextBox runat="server" ID="rtpKidFirstName" Label="" Required="true" RequiredErrorMessage="First Name is Required for all Children" Placeholder="First Name"></Rock:RockTextBox>
                                            <div style="margin-top:5px;">
                                                <Rock:RockTextBox runat="server" ID="rtpKidLastName" Label="" Required="true" RequiredErrorMessage="Last Name is Required for all Children" Placeholder="Last Name"></Rock:RockTextBox>
                                            </div>
                                        </td>
                                        <td>
                                            <Rock:RockRadioButtonList runat="server" ID="rblGender" Label="" Required="true" RequiredErrorMessage="Gender is Required for all Children">
                                                <asp:ListItem Text="Male" Value="1" />
                                                <asp:ListItem Text="Female" Value="2" />
                                            </Rock:RockRadioButtonList>
                                        </td>
                                        <td>
                                            <Rock:DatePicker runat="server" ID="dpBirthdate" Label="" Required="true" RequiredErrorMessage="Birthdate is Required for all Children"></Rock:DatePicker>
                                            <div style="margin-top:5px;">
                                                <Rock:GradePicker runat="server" ID="gpGrade" Label=""></Rock:GradePicker>
                                            </div>
                                        </td>
                                        <td><Rock:RockTextBox runat="server" ID="rtbAllergy"></Rock:RockTextBox></td>
                                        <td><asp:LinkButton ID="lbDelete" runat="server" Text='<i class="fa fa-times"></i>' CssClass="btn btn-danger pull-right btn-lg" CommandName="Delete" CausesValidation="false"></asp:LinkButton></td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                        <tfoot>
                            <tr>
                                <td colspan="6" class="text-right"><asp:LinkButton runat="server" ID="lbAddKid" Text='<i class="fa fa-plus"></i>' CssClass="btn btn-success btn-lg" OnClick="lbAddKid_Click" CausesValidation="false" /></td>
                            </tr>
                        </tfoot>
                    </table>
                </asp:Panel>
                <hr />
                <div class="actions">
                    <asp:LinkButton runat="server" ID="lbCancel" CssClass="btn btn-lg btn-default pull-left" Text="Cancel" OnClientClick="history.back();" CausesValidation="false"></asp:LinkButton>
                    <asp:LinkButton runat="server" ID="lbSubmit" CssClass="btn btn-primary pull-right btn-lg" Text="Submit" OnClick="lbSubmit_Click"></asp:LinkButton>
                </div>
            
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
