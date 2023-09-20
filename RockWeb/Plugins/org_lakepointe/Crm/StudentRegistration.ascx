<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StudentRegistration.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Crm.StudentRegistration" %>
<asp:UpdatePanel ID="upStudentRegistration" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Danger" />

        <asp:Panel ID="pnlSignupForm" runat="server" Visible="false">
            <asp:Literal ID="lIntro" runat="server" />
            <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="vgSignup" />
            <asp:Panel ID="pnlCampusPicker" runat="server" Visible="false">
                <div class="row">
                    <div class="col-md-6">
                        <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" Visible="true" Required="true" ValidationGroup="vgSignup" RequiredErrorMessage="Campus is required." />
                    </div>
                </div>
            </asp:Panel>
            <h3>Student Information</h3>
            <div class="row">
                <div class="col-md-6">
                    <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" Visible="true" Required="true" ValidationGroup="vgSignup" RequiredErrorMessage="First Name is required." />
                </div>
                <div class="col-md-6">
                    <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" Visible="true" Required="true" ValidationGroup="vgSignup" RequiredErrorMessage="Last Name is required." />
                </div>
            </div>
            <div class="row">
                <div class="col-md-6">
                    <Rock:PhoneNumberBox ID="tbPhone" runat="server" Label="Phone" Visible="true" Required="false" ValidationGroup="vgSignup" RequiredErrorMessage="Phone Number is required." />
                    <Rock:RockCheckBox ID="cbPermissionToText" runat="server" Text="Okay to Text?" Checked="true" Help="Is it okay if Lakepointe texts you at this number?" />
                    <Rock:RockDropDownList ID="rddlPhoneType" runat="server" Label="Whose Phone is This?" Visible="true" Required="false">
                        <asp:ListItem Text="My Mobile" Value="Mine" />
                        <asp:ListItem Text="My Home" Value="Home" />
<%--                        <asp:ListItem Text="My Mother's Mobile" Value="Mom's" />
                        <asp:ListItem Text="My Father's Mobile" Value="Dad's" />--%>
                        <asp:ListItem Text="Other" Value="Other" />
                    </Rock:RockDropDownList>
                </div>
                <div class="col-md-6">
                    <Rock:DatePartsPicker ID="dpBirthDate" runat="server" Label="Birth Date" Visible="true" Required="true" ValidationGroup="vgSignup" AllowFutureDates="false" RequiredErrorMessage="Birth Date is required." FutureDatesErrorMessage="" />
                    <Rock:RockRadioButtonList ID="rrblGender" runat="server" Label="Gender" Required="true" ValidationGroup="vgSignup" >
                        <asp:ListItem Text="Male" Value="Male" Selected="False" />
                        <asp:ListItem Text="Female" Value="Female" Selected="False" />
                    </Rock:RockRadioButtonList>
                </div>
            </div>
            <div class="row">
                <div class="col-md-5">
                    <Rock:EmailBox ID="tbEmail" runat="server" Label="Email" Visible="true" Required="false" ValidationGroup="vgSignup" RequiredErrorMessage="Email Address is required." />
                </div>
                <div class="col-md-5">
                    <Rock:RockTextBox ID="tbSchool" runat="server" Label="School" Visible="true" Required="false" />
                </div>
                <div class="col-md-2">
                    <%--<Rock:RockDropDownList ID="rddlGrade" runat="server" Label="Grade" Visible="true" Required="true" ValidationGroup="vgSignup" RequiredErrorMessage="Grade is required." >
                        <asp:ListItem Text="" Value="" />
                        <asp:ListItem Text="Senior" Value="0" />
                        <asp:ListItem Text="Junior" Value="1" />
                        <asp:ListItem Text="Sophomore" Value="2" />
                        <asp:ListItem Text="Freshman" Value="3" />
                        <asp:ListItem Text="8th Grade" Value="4" />
                        <asp:ListItem Text="7th Grade" Value="5" />
                        <asp:ListItem Text="6th Grade" Value="6" />
                    </Rock:RockDropDownList>--%>
                    <Rock:GradePicker ID="gpGrade" runat="server" Label="Grade" Visible="true" Required="true" ValidationGroup="vgSignup" RequiredErrorMessage="Grade is required." />
                </div>
            </div>
            <h3>Home Address</h3>
            <div class="row">
                <div class="col-md-12">
                    <Rock:RockTextBox ID="tbAddress" runat="server" Label="Address" Visible="true" Required="false" />
                </div>
            </div>
            <div class="row">
                <div class="col-md-6">
                    <Rock:RockTextBox ID="tbCity" runat="server" Label="City" Visible="true" Required="false" />
                </div>
                <div class="col-md-3">
                    <Rock:RockTextBox ID="tbState" runat="server" Label="State" Visible="true" Required="false" />
                </div>
                <div class="col-md-3">
                    <Rock:RockTextBox ID="tbZip" runat="server" Label="Zip" Visible="true" Required="false" />
                </div>
            </div>
            <h3>Parent Information</h3>
            <div class="row">
                <div class="col-md-6">
                    <Rock:RockTextBox ID="tbParentFirstName" runat="server" Label="First Name" Visible="true" Required="true" ValidationGroup="vgSignup" RequiredErrorMessage="First Name is required." />
                </div>
                <div class="col-md-6">
                    <Rock:RockTextBox ID="tbParentLastName" runat="server" Label="Last Name" Visible="true" Required="true" ValidationGroup="vgSignup" RequiredErrorMessage="Last Name is required." />
                </div>
            </div>
            <div class="row">
                <div class="col-md-6">
                    <Rock:PhoneNumberBox ID="pnbParentPhone" runat="server" Label="Phone" Visible="true" Required="false" ValidationGroup="vgSignup" RequiredErrorMessage="Phone Number is required." />
                </div>
                <div class="col-md-6">
                    <Rock:EmailBox ID="embParentEmail" runat="server" Label="Email" Visible="true" Required="false" ValidationGroup="vgSignup" RequiredErrorMessage="Email Address is required." />
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <Rock:RockRadioButtonList ID="rrblService" runat="server" Label="If your parents were to attend church, which service would they prefer?">
                        <asp:ListItem Text="English Speaking" Value="English" Selected="True" />
                        <asp:ListItem Text="Spanish Speaking" Value="Spanish" Selected="False" />
                    </Rock:RockRadioButtonList>
                </div>
            </div>
            <div class="row">
                <div class="col-md-12">
                    <asp:LinkButton ID="lbReset" runat="server" Visible="true" CausesValidation="false" CssClass="btn btn-default">Reset</asp:LinkButton>
                    <asp:LinkButton ID="lbNext" runat="server" Visible="true" CausesValidation="true" ValidationGroup="vgSignup" CssClass="btn btn-default"
                        OnClientClick="scrollToTop()">Next</asp:LinkButton>
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="pnlSnapshot" runat="server" Visible="false">
            <div class="panel-heading">
                <h3 class="panel-title">Student Photo</h3>
            </div>
            <div class="panel-body">
                <div class="row">
                    <asp:HiddenField ClientIDMode="Static" ID="hfImage" runat="server" />
                    <asp:Panel CssClass="col-md-12" runat="server" ID="pnlAPhoto">
                        <div class="row">
                            <div class="col-md-3">
                                <asp:LinkButton ID="lbCamera" runat="server" CssClass="btn btn-primary" CausesValidation="false" OnClick="lbCamera_Click" >
                                    <i class="fal fa-camera"></i> Take Photo&nbsp&nbsp
                                </asp:LinkButton>
                            </div>
                            <div class="col-md-9">
                                <canvas id="canvas" width=320 height=320></canvas>
                            </div>
                        </div>
                    </asp:Panel>
                </div>
                <div class="row">
                    <div class="col-md-12">
                        <asp:LinkButton ID="lbCancel" runat="server" Visible="true" CausesValidation="false" CssClass="btn btn-default">Cancel</asp:LinkButton>
                        <asp:LinkButton ID="lbSubmit" runat="server" Visible="true" CausesValidation="true" ValidationGroup="vgSignup" CssClass="btn btn-default"
                            OnClientClick="if (this.value === 'Saving...') { return false; } else { this.value = 'Saving...'; return true; }" >Submit</asp:LinkButton>
                    </div>
                </div>
            </div>
        </asp:Panel>

       <Rock:ModalDialog ID="dlgCamera" runat="server" Title="Take Student Photo" OnCancelScript="$('#capture').click();"
            ValidateRequestMode="Disabled" ValidationGroup="TakePicture" FormNoValidate="formnovalidate">
            <Content>
                <video style="transform: rotateY(180deg); -webkit-transform:rotateY(180deg); -moz-transform:rotateY(180deg);" ID="player" width=320 height=320 autoplay></video>
                <button ID="capture" hidden="hidden" onclick="return snap();" onclose="return loadImages();" formnovalidate="formnovalidate" autofocus="autofocus">Capture</button>
            </Content>
        </Rock:ModalDialog>

        <asp:Panel ID="pnlConfirmation" runat="server" Visible="false">
            <div class="panel-body">
                <div class="row">
                    <div class="col-md-12">
                        <asp:Literal ID="lConfirmation" runat="server" />
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-12">
                        <asp:LinkButton ID="lbFinished" runat="server" Visible="true" CausesValidation="false" CssClass="btn btn-default">Finished</asp:LinkButton>
                    </div>
                </div>
        </asp:Panel>

        <script type="text/javascript">

            function snap()
            {
                const context = canvas.getContext('2d');
                const player = document.getElementById('player');

                // Draw the video frame to the canvas.
                context.drawImage(player, 0, 0, canvas.width, canvas.height);
                hfImage.value = canvas.toDataURL('image/jpeg');
                
                // Stop all video streams.
                if (player.srcObject != null) {
                    player.srcObject.getVideoTracks().forEach(track => track.stop());
                }

                return false;
            }

            function snapImage()
            {
                const player = document.getElementById('player');
                const constraints = { video: { width: 320, height: 320 } };
                navigator.mediaDevices.getUserMedia(constraints)
                    .then((stream) => {
                        player.srcObject = stream;
                    });
            }

            function scrollToTop() {
                window.scrollTo(0, 0);
            }

        </script>
    </ContentTemplate>
</asp:UpdatePanel>