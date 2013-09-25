<%@ Control Language="C#" AutoEventWireup="true" CodeFile="NewAccount.ascx.cs" Inherits="RockWeb.Blocks.Security.NewAccount" %>
<script type="text/javascript">

    Sys.Application.add_load(function () {

        var availabilityMessageRow = $('#availabilityMessageRow');
        var usernameUnavailable = $('#availabilityMessage');
        var usernameTextbox = $('#<%= tbUserName.ClientID %>');

        availabilityMessageRow.hide();

        usernameTextbox.blur(function () {
            if ($(this).val()) {
                $.ajax({
                    type: 'GET',
                    contentType: 'application/json',
                    dataType: 'json',
                    url: Rock.settings.get('baseUrl') + 'api/userlogins/available/' + escape($(this).val()),
                    success: function (getData, status, xhr) {

                        if (getData) {
                            if (availabilityMessageRow.is(':visible')) {
                                usernameUnavailable.html('This username is available.');
                                usernameUnavailable.addClass('success');
                                usernameUnavailable.removeClass('warning');
                            }
                        } else {
                            availabilityMessageRow.show();
                            usernameUnavailable.html('This username is already taken!');
                            usernameUnavailable.addClass('warning');
                            usernameUnavailable.removeClass('success');
                        }
                    },
                    error: function (xhr, status, error) {
                        alert(status + ' [' + error + ']: ' + xhr.responseText);
                    }
                });
            }
        });

    });

</script>

<asp:UpdatePanel ID="upNewAccount" runat="server">
<ContentTemplate>

    <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="alert alert-error block-message error"/>
    <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error"/>

    <asp:PlaceHolder ID="phUserInfo" runat="server" Visible="true">

        <div class="row-fluid">

            <div class="span6">

               <fieldset>
                    <legend>New Account</legend>
                    <Rock:RockTextBox ID="tbUserName" runat="server" Label="Username" Required="true" ></Rock:RockTextBox>
                    <dl id="availabilityMessageRow">
                        <dt></dt>
                        <dd><div id="availabilityMessage" class="alert"/></dd>
                    </dl>
                    <Rock:RockTextBox ID="tbPassword" runat="server" Label="Password" Required="true" TextMode="Password" ></Rock:RockTextBox>
                    <Rock:RockTextBox ID="tbPasswordConfirm" runat="server" Label="Confirmation" Required="true" TextMode="Password" ></Rock:RockTextBox>
                    <asp:CompareValidator ID="cvPassword" runat="server" ControlToCompare="tbPassword" ControlToValidate="tbPasswordConfirm" ErrorMessage="Password and Confirmation do not match" Display="Dynamic" CssClass="validation-error"></asp:CompareValidator>

                </fieldset>

            </div>

            <div class="span6">

                <fieldset>
                    <legend>Your Information</legend> 
                    <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" Required="true" ></Rock:RockTextBox>
                    <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" Required="true" ></Rock:RockTextBox>
                    <Rock:RockTextBox ID="tbEmail" runat="server" Label="Email" Required="true" ></Rock:RockTextBox>
                    <Rock:RockDropDownList ID="ddlGender" runat="server" Label="Gender" CssClass="input-small">
                        <asp:ListItem Text="" Value="U"></asp:ListItem>
                        <asp:ListItem Text="Male" Value="M"></asp:ListItem>
                        <asp:ListItem Text="Female" Value="F"></asp:ListItem>
                    </Rock:RockDropDownList>
                    <div class="control-group">
                        <asp:Label runat="server" AssociatedControlID="ddlBirthMonth" Text="Birthdate"></asp:Label>
                        <div class="controls">
                            <asp:DropDownList ID="ddlBirthMonth" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlBirthMonth_IndexChanged" class="input-small">
                                <asp:ListItem Text="Month" Value="0"></asp:ListItem>
                                <asp:ListItem Text="January" Value="1"></asp:ListItem>
                                <asp:ListItem Text="February" Value="2"></asp:ListItem>
                                <asp:ListItem Text="March" Value="3"></asp:ListItem>
                                <asp:ListItem Text="April" Value="4"></asp:ListItem>
                                <asp:ListItem Text="May" Value="5"></asp:ListItem>
                                <asp:ListItem Text="June" Value="6"></asp:ListItem>
                                <asp:ListItem Text="July" Value="7"></asp:ListItem>
                                <asp:ListItem Text="August" Value="8"></asp:ListItem>
                                <asp:ListItem Text="September" Value="9"></asp:ListItem>
                                <asp:ListItem Text="October" Value="10"></asp:ListItem>
                                <asp:ListItem Text="November" Value="11"></asp:ListItem>
                                <asp:ListItem Text="December" Value="12"></asp:ListItem>
                            </asp:DropDownList> &nbsp;
                            <asp:DropDownList ID="ddlBirthDay" runat="server" class="input-small">
                                <asp:ListItem Text="Day" Value="0"></asp:ListItem>
                            </asp:DropDownList> &nbsp;
                            <asp:DropDownList ID="ddlBirthYear" runat="server" class="input-small">
                                <asp:ListItem Text="Year" Value="0"></asp:ListItem>
                            </asp:DropDownList>
                        </div>
                    </div>
               </fieldset>

            </div>
        
        </div>

        <div class="actions">
            <asp:Button ID="btnUserInfoNext" runat="server" Text="Next" CssClass="btn btn-primary" OnClick="btnUserInfoNext_Click" />
        </div>

    </asp:PlaceHolder>

    <asp:PlaceHolder ID="phDuplicates" runat="server" Visible="false">

        <div class="alert warning">
            <asp:Literal ID="lFoundDuplicateCaption" runat="server" />
        </div>

        <Rock:Grid ID="gDuplicates" runat="server">
        <Columns>
            <asp:TemplateField>
                <HeaderTemplate>You?</HeaderTemplate>
                <ItemTemplate>
                    <input type="radio" value='<%# Eval("Id") %>' name="DuplicatePerson" />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="FullName" HeaderText="Name" />
            <asp:BoundField DataField="Gender" HeaderText="Gender" />
            <asp:BoundField DataField="BirthDate" HeaderText="Birth Day" DataFormatString="{0:MMMM d}" />
        </Columns>
        </Rock:Grid>

        <div class="inputs-list not-me">
            <label>
                <input type="radio" value="0" name="DuplicatePerson" />        
                <span>None of these are me</span>    
            </label>
        </div>

        <div class="actions">
            <asp:Button ID="btnDuplicatesPrev" runat="server" Text="Previous" CssClass="btn" OnClick="btnDuplicatesPrev_Click" />
            <asp:Button ID="btnDuplicatesNext" runat="server" Text="Next" CssClass="btn btn-primary" OnClick="btnDuplicatesNext_Click" />
        </div>

    </asp:PlaceHolder>

    <asp:PlaceHolder ID="phSendLoginInfo" runat="server" Visible="false">

        <asp:HiddenField ID="hfSendPersonId" runat="server" />

        <div class="alert warning">
            <asp:Literal ID="lExistingAccountCaption" runat="server"/>
        </div>

        <div class="actions">
            <asp:Button ID="btnSendPrev" runat="server" Text="Previous" CssClass="btn" OnClick="btnSendPrev_Click" />
            <asp:Button ID="btnSendYes" runat="server" Text="Yes, send it" CssClass="btn btn-primary" OnClick="btnSendYes_Click" />
            <asp:Button ID="btnSendLogin" runat="server" Text="No, just let me login" CssClass="btn btn-primary" OnClick="btnSendLogin_Click" />
        </div>

    </asp:PlaceHolder>

    <asp:PlaceHolder ID="phSentLoginInfo" runat="server" Visible="false">

        <div>
            <asp:Literal ID="lSentLoginCaption" runat="server" />
        </div>

    </asp:PlaceHolder>

    <asp:PlaceHolder ID="phConfirmation" runat="server" Visible="false">

        <div class="alert warning">
            <asp:Literal ID="lConfirmCaption" runat="server" />
        </div>

    </asp:PlaceHolder>

    <asp:PlaceHolder ID="phSuccess" runat="server" Visible="false">

        <div class="alert alert-success success">
            <asp:Literal ID="lSuccessCaption" runat="server" />
        </div>

    </asp:PlaceHolder>

</ContentTemplate>
</asp:UpdatePanel>
