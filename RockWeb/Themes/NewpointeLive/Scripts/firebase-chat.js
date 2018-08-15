/**
 * Copyright 2015 Google Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
'use strict';

//$(document).ready(function () {
//    myTimer();
//})

//function myTimer() {
//    console.log(' every 30 seconds...');
//}

//var myVar = setInterval(function () { myTimer() }, 30000);

// Initializes FriendlyChat.
function FriendlyChat() {
    this.checkSetup();

    // Shortcuts to DOM Elements.
    this.messageList = document.getElementById('messages');
    this.messageForm = document.getElementById('form1');
    this.messageInput = document.getElementById('message');
    this.submitButton = document.getElementById('submit');
    //this.submitImageButton = document.getElementById('submitImage');
    //this.imageForm = document.getElementById('image-form');
    this.mediaCapture = document.getElementById('mediaCapture');
    this.userPic = document.getElementById('user-pic');
    this.userName = document.getElementById('user-name');
    this.signInButton = document.getElementById('sign-in');
    this.signOutButton = document.getElementById('sign-out');
    this.signInSnackbar = $("#must-signin-snackbar");
    this.alertDanger = $("#danger-alert");
    this.token = $('input[name *= "hdnToken"]').val();

    this.date = $('input[name *= "hdnDate"]').val();

    // Saves message on form submit.
    this.messageForm.addEventListener('submit', this.saveMessage.bind(this));
    this.signOutButton.addEventListener('click', this.signOut.bind(this));
    this.signInButton.addEventListener('click', this.signIn.bind(this));

    // Toggle for the button.
    var buttonTogglingHandler = this.toggleButton.bind(this);
    this.messageInput.addEventListener('keyup', buttonTogglingHandler);
    this.messageInput.addEventListener('change', buttonTogglingHandler);

    this.initFirebase();
    // Events for image upload.
    //this.submitImageButton.addEventListener('click', function () {
    //this.mediaCapture.click();
    //}.bind(this));
    //this.mediaCapture.addEventListener('change', this.saveImageMessage.bind(this));
}

$(document).ready(function(){
    var mess = document.getElementById('message')
    $(mess).keypress(function (e) {
        if (e.keyCode == 13) {
            e.preventDefault();

            var button = document.getElementById('submit');

            button.click();
        }
    });
});

// Sets up shortcuts to Firebase features and initiate firebase auth.
FriendlyChat.prototype.initFirebase = function () {
    var token = this.token;

    this.auth = firebase.auth();

    this.auth.signInWithCustomToken(token).then(function (data)
    {
        var userDisplayName = $('input[name *= "hdnDisplayName"]').val();
        var userEmail = $('input[name *= "hdnEmail"]').val();
        var userPhoto = $('input[name *= "hdnPhoto"]').val();

        data.updateEmail(userEmail);

        data.updateProfile({
            displayName: userDisplayName,
            photoURL: userPhoto
        }).then(function () {
            //alert('updated');
            // successfully updated
        }, function (error) {
            //error occurred
            console.log('error occurred')
            console.log(error);
        });
    }).catch(function (error) {
        // Handle Errors here.
        var errorCode = error.code;
        var errorMessage = error.message;
        // ...
    });




    this.database = firebase.database();
    this.storage = firebase.storage();

    //this.auth.onAuthStateChanged(this.onAuthStateChanged.bind(this));
    this.loadMessages();
};

// Loads chat messages history and listens for upcoming ones.
FriendlyChat.prototype.loadMessages = function () {

    
    this.messagesRef = this.database.ref('public' + this.date + '/messages');

    this.messagesRef.off();

    var setMessage = function (data) {
        var val = data.val();
        this.displayMessage(data.key, val.name, val.text, val.photoUrl, val.imageUrl);

    }.bind(this);
    this.messagesRef.limitToLast(12).on('child_added', setMessage);
    this.messagesRef.limitToLast(12).on('child_changed', setMessage);
};

// Saves a new message on the Firebase DB.
FriendlyChat.prototype.saveMessage = function (e) {
    e.preventDefault();

    // Check that the user entered a message and is signed in.
    if (this.messageInput.value && this.checkSignedInWithMessage()) {

        var currentUser = this.auth.currentUser;

        this.messagesRef.push({
            name: currentUser.displayName,
            text: this.messageInput.value,
            photoUrl: currentUser.photoURL || '/images/profile_placeholder.png'
        }).then(function () {
            FriendlyChat.resetMaterialTextfield(this.messageInput);
            this.toggleButton();
        }.bind(this)).catch(function (error) {
            console.error('Error writing new message to Firebase Database', error);
        });
    }
};

// Sets the URL of the given img element with the URL of the image stored in Firebase Storage.
//FriendlyChat.prototype.setImageUrl = function (imageUri, imgElement) {
//    imgElement.src = imageUri;

//    // TODO(DEVELOPER): If image is on Firebase Storage, fetch image URL and set img element's src.
//};

// Saves a new message containing an image URI in Firebase.
// This first saves the image in Firebase storage.
//FriendlyChat.prototype.saveImageMessage = function (event) {
//    var file = event.target.files[0];

//    // Clear the selection in the file picker input.
//    this.imageForm.reset();

//    // Check if the file is an image.
//    if (!file.type.match('image.*')) {
//        var data = {
//            message: 'You can only share images',
//            timeout: 2000
//        };
//        this.signInSnackbar.MaterialSnackbar.showSnackbar(data);
//        return;
//    }
//    // Check if the user is signed-in
//    if (this.checkSignedInWithMessage()) {

//        // TODO(DEVELOPER): Upload image to Firebase storage and add message.

//    }
//};

// Signs-in Friendly Chat.
FriendlyChat.prototype.signIn = function (googleUser) {
    var provider = new firebase.auth.GoogleAuthProvider();
    this.auth.signInWithPopup(provider);
};

// Signs-out of Friendly Chat.
FriendlyChat.prototype.signOut = function () {
    this.auth.signOut();
};

// Triggers when the auth state change for instance when the user signs-in or signs-out.
//FriendlyChat.prototype.onAuthStateChanged = function (user) {
//    if (user) { // User is signed in!
//        // Get profile pic and user's name from the Firebase user object.
//        var profilePicUrl = user.photoUrl;   // TODO(DEVELOPER): Get profile pic.
//        var userName = user.displayName;        // TODO(DEVELOPER): Get user's name.

//        // Set the user's profile pic and name.
//        this.userPic.style.backgroundImage = 'url(' + profilePicUrl + ')';
//        this.userName.textContent = userName;

//        // Show user's profile and sign-out button.
//        this.userName.removeAttribute('hidden');
//        this.userPic.removeAttribute('hidden');
//        this.signOutButton.removeAttribute('hidden');

//        // Hide sign-in button.
//        this.signInButton.setAttribute('hidden', 'true');

//        // We load currently existing chant messages.
//        this.loadMessages();
//    } else { // User is signed out!
//        // Hide user's profile and sign-out button.
//        this.userName.setAttribute('hidden', 'true');
//        this.userPic.setAttribute('hidden', 'true');
//        this.signOutButton.setAttribute('hidden', 'true');

//        // Show sign-in button.
//        this.signInButton.removeAttribute('hidden');
//    }
//};

// Returns true if user is signed-in. Otherwise false and displays a message.
FriendlyChat.prototype.checkSignedInWithMessage = function () {
    if (this.auth.currentUser) {
        return true;
    }

    // Display a message to the user using a Toast.
    var message = 'You must sign-in first';
    var timeout = 5000;

    this.alertDanger.html(message);

    this.alertDanger.fadeTo(2000, 500).slideUp(500, function () {
        $('#danger-alert').alert('close');
    });

    //var sb = this.signInSnackbar.MaterialSnackbar;
    //this.signInSnackbar.MaterialSnackbar.showSnackbar(data);
    return false;
};

// Resets the given MaterialTextField.
FriendlyChat.resetMaterialTextfield = function (element) {
    element.value = '';
    element.parentNode.MaterialTextfield.boundUpdateClassesHandler();
};

// Template for messages.
FriendlyChat.MESSAGE_TEMPLATE =
    '<div class="message-container">' +
      '<div class="spacing"><div class="pic"></div></div>' +
      '<div class="message"></div>' +
      '<div class="name"></div>' +
    '</div>';

// A loading image URL.
FriendlyChat.LOADING_IMAGE_URL = 'https://www.google.com/images/spin-32.gif';

// Displays a Message in the UI.
FriendlyChat.prototype.displayMessage = function (key, name, text, picUrl, imageUri) {
    var div = document.getElementById(key);
    // If an element for that message does not exists yet we create it.
    if (!div) {
        var container = document.createElement('div');
        container.innerHTML = FriendlyChat.MESSAGE_TEMPLATE;
        div = container.firstChild;
        div.setAttribute('id', key);
        this.messageList.appendChild(div);
    }
    if (picUrl) {
        div.querySelector('.pic').style.backgroundImage = 'url(' + picUrl + ')';
    }
    div.querySelector('.name').textContent = name;
    var messageElement = div.querySelector('.message');
    if (text) { // If the message is text.
        messageElement.textContent = text;
        // Replace all line breaks by <br>.
        messageElement.innerHTML = messageElement.innerHTML.replace(/\n/g, '<br>');
    } else if (imageUri) { // If the message is an image.
        var image = document.createElement('img');
        image.addEventListener('load', function () {
            this.messageList.scrollTop = this.messageList.scrollHeight;
        }.bind(this));
        this.setImageUrl(imageUri, image);
        messageElement.innerHTML = '';
        messageElement.appendChild(image);
    }
    // Show the card fading-in.
    setTimeout(function () { div.classList.add('visible') }, 1);
    this.messageList.scrollTop = this.messageList.scrollHeight;
    this.messageInput.focus();
};

// Enables or disables the submit button depending on the values of the input
// fields.
FriendlyChat.prototype.toggleButton = function () {
    if (this.messageInput.value) {
        this.submitButton.removeAttribute('disabled');
    } else {
        this.submitButton.setAttribute('disabled', 'true');
    }
};

// Checks that the Firebase SDK has been correctly setup and configured.
FriendlyChat.prototype.checkSetup = function () {
    if (!window.firebase || !(firebase.app instanceof Function) || !window.config) {
        window.alert('You have not configured and imported the Firebase SDK. ' +
            'Make sure you go through the codelab setup instructions.');
    } else if (config.storageBucket === '') {
        window.alert('Your Firebase Storage bucket has not been enabled. Sorry about that. This is ' +
            'actually a Firebase bug that occurs rarely.' +
            'Please go and re-generate the Firebase initialisation snippet (step 4 of the codelab) ' +
            'and make sure the storageBucket attribute is not empty.');
    }
};

window.onload = function () {
    window.friendlyChat = new FriendlyChat();
};

$(document).ready(function () {
    $('#btnPrivatePrayer').on('click', function () {
        var person = $('input[name *= "hdnPerson"]').val();
        var room = $('input[name *= "hdnRoom"]').val();
        var name = $('input[name *= "hdnDisplayName"]').val();

        $.ajax({
            type: "POST",
            url: 'https://' + window.location.host + "/API/PrivatePrayerRequests/",
            data: { Person_Id: person, RoomId: room, Answered: false, Name: name },
            dataType: 'json',
            success: function (data) {
                //TODO redirect to private chat popup
                window.location.replace("https://" + window.location.host + "/page/1123?RoomId=" + room);
            }, error: function () {
                console.log('error occurred');
            }
        })
    });
});

