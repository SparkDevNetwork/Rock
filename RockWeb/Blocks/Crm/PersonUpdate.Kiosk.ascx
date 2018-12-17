﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonUpdate.Kiosk.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonUpdateKiosk" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <script>

            var isTouchDevice = 'ontouchstart' in document.documentElement;

            Sys.Application.add_load(function () {
                $(document).ready(function() {

                    //
                    // search
                    //
                    if ($(".js-pnlsearch").is(":visible")) {

                        // setup digits buttons
                        $('.js-pnlsearch .tenkey a.digit').click(function () {
                            $phoneNumber = $("input[id$='tbPhone']");
                            $phoneNumber.val($phoneNumber.val() + $(this).html());
                            return false;
                        });
                        $('.js-pnlsearch .tenkey a.back').click(function () {
                            $phoneNumber = $("input[id$='tbPhone']");
                            $phoneNumber.val($phoneNumber.val().slice(0, -1));
                            return false;
                        });
                        $('.js-pnlsearch .tenkey a.clear').click(function () {
                            $phoneNumber = $("input[id$='tbPhone']");
                            $phoneNumber.val('');
                            return false;
                        });

                        // set focus to the input unless on a touch device
                        if (!isTouchDevice) {
                            $('.kiosk-phoneentry').focus();
                        }
                    }
                });
            });

        </script>

        <Rock:NotificationBox ID="nbBlockConfigErrors" runat="server" NotificationBoxType="Danger" />


        <asp:Panel ID="pnlSearch" runat="server" CssClass="js-pnlsearch" DefaultButton="lbSearchNext">
            <header>
                <h1>Please Enter Your Phone Number</h1>
            </header>

            <main>
                <div class="row">
                    <div class="col-md-8 margin-b-lg">
                        <Rock:NotificationBox ID="nbSearch" runat="server" NotificationBoxType="Warning"></Rock:NotificationBox>

                        <Rock:RockTextBox ID="tbPhone" type="number" CssClass="kiosk-phoneentry" runat="server" Label="Phone Number" />

                        <div class="tenkey kiosk-phone-keypad">
                            <div>
                                <a href="#" class="btn btn-default btn-lg digit">1</a>
                                <a href="#" class="btn btn-default btn-lg digit">2</a>
                                <a href="#" class="btn btn-default btn-lg digit">3</a>
                            </div>
                            <div>
                                <a href="#" class="btn btn-default btn-lg digit">4</a>
                                <a href="#" class="btn btn-default btn-lg digit">5</a>
                                <a href="#" class="btn btn-default btn-lg digit">6</a>
                            </div>
                            <div>
                                <a href="#" class="btn btn-default btn-lg digit">7</a>
                                <a href="#" class="btn btn-default btn-lg digit">8</a>
                                <a href="#" class="btn btn-default btn-lg digit">9</a>
                            </div>
                            <div>
                                <a href="#" class="btn btn-default btn-lg command back">Back</a>
                                <a href="#" class="btn btn-default btn-lg digit">0</a>
                                <a href="#" class="btn btn-default btn-lg command clear">Clear</a>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-4">
                        <asp:LinkButton ID="lbSearchNext" runat="server" OnClick="lbSearchNext_Click" CssClass="btn btn-primary btn-kiosk btn-kiosk-lg">Next</asp:LinkButton>
                    </div>
                </div>
            </main>

            <footer>
                <div class="container">
                    <div class="row">
                        <div class="col-md-8">
                            <asp:LinkButton ID="lbSearchCancel" runat="server" OnClick="lbSearchCancel_Click" CssClass="btn btn-default btn-kiosk">Cancel</asp:LinkButton>
                        </div>

                    </div>
                </div>
            </footer>

        </asp:Panel>

        <asp:Panel ID="pnlPersonSelect" runat="server" Visible="false" CssClass="js-pnlpersonselect js-kioskscrollpanel">
            <header>
                <h1>Select Your Profile</h1>
            </header>

            <main class="clearfix js-scrollcontainer">
                <div class="scrollpanel">
                    <div class="scroller">
                        <asp:PlaceHolder ID="phPeople" runat="server"></asp:PlaceHolder>
                    </div>
                </div>
            </main>

            <footer>
                <div class="container">
                    <div class="row">
                        <div class="col-md-8">
                            <asp:LinkButton ID="lbPersonSelectBack" runat="server" OnClick="lbPersonSelectBack_Click" CssClass="btn btn-default btn-kiosk">Back</asp:LinkButton>
                            <asp:LinkButton ID="lbPersonSelectCancel" runat="server" OnClick="lbPersonSelectCancel_Click" CssClass="btn btn-default btn-kiosk">Cancel</asp:LinkButton>
                        </div>
                        <div class="col-md-4">
                            <asp:LinkButton ID="lbAddPerson" runat="server" OnClick="lbAddPerson_Click" CssClass="btn btn-default btn-kiosk btn-kiosk-lg pull-right">Add New Person</asp:LinkButton>
                        </div>
                    </div>
                </div>
            </footer>
        </asp:Panel>

        <asp:Panel ID="pnlProfilePanel" runat="server" Visible="false">
            <header>
                <h1>My Profile</h1>
            </header>

            <main>
                <asp:HiddenField id="hfPersonId" runat="server" />
                <div class="row">
                    <div class="col-md-8">

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbFirstName" Label="First Name" runat="server"  Required="true" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbLastName" Label="Last Name" runat="server" Required="true" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-8">
                                <Rock:EmailBox ID="tbEmail" runat="server" Label="Email" />
                            </div>
                            <div class="col-md-4">
                                <Rock:DatePicker ID="dpBirthdate" runat="server" AllowFutureDateSelection="false"  Label="Birthdate" />
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:PhoneNumberBox ID="pnbHomePhone" Label="Home Phone" runat="server" />
                            </div>
                            <div class="col-md-6">
                                <Rock:PhoneNumberBox ID="pnbMobilePhone" Label="Mobile Phone" runat="server" />
                            </div>
                        </div>

                        <Rock:AddressControl ID="acAddress" ShowAddressLine2="false" runat="server" />

                        <Rock:RockTextBox ID="tbOtherUpdates" runat="server" Label="Other Updates" TextMode="MultiLine" Rows="3" />
                    </div>
                    <div class="col-md-4">
                        <asp:LinkButton ID="lbProfileNext" runat="server" OnClick="lbProfileNext_Click" CssClass="btn btn-primary btn-kiosk btn-kiosk-lg">Next</asp:LinkButton>
                        <div class="alert alert-info margin-t-md">
                            <asp:Literal ID="lUpdateMessage" runat="server" />
                        </div>
                    </div>
                </div>

            </main>

            <footer>
                <div class="container">
                    <div class="row">
                        <div class="col-md-8">
                            <asp:LinkButton ID="lbProfileBack" CausesValidation="false" runat="server" OnClick="lbProfileBack_Click" CssClass="btn btn-default btn-kiosk">Back</asp:LinkButton>
                            <asp:LinkButton ID="lbProfileCancel" CausesValidation="false" runat="server" OnClick="lbProfileCancel_Click" CssClass="btn btn-default btn-kiosk">Cancel</asp:LinkButton>
                        </div>
                    </div>
                </div>
            </footer>
        </asp:Panel>

         <asp:Panel ID="pnlComplete" runat="server" Visible="false">
            <header>
                <h1>Update Sent</h1>
            </header>

            <main>

            </main>
                <div class="row">
                    <div class="col-md-8">
                        <asp:Literal ID="lCompleteMessage" runat="server" />

                        <asp:Literal ID="lDebug" runat="server" />
                    </div>
                    <div class="col-md-4">
                        <asp:LinkButton ID="lbCompleteDone" runat="server" OnClick="lbCompleteDone_Click" CssClass="btn btn-primary btn-kiosk btn-kiosk-lg">Done</asp:LinkButton>
                    </div>
                </div>
             <footer>

             </footer>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
