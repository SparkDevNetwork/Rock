using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace church.ccv.Web.Model
{
    public enum LoginResponse
    {
        Invalid,
        LockedOut,
        Confirm,
        Success
    }

    [Serializable]
    public class LoginData
    {
        public string Username;
        public string Password;
        public string Persist;
    }
    
    [Serializable]
    public class PersonWithLoginModel
    {
        public string FirstName;
        public string LastName;
        public string Email;

        public string Username;
        public string Password;

        public string ConfirmAccountUrl;
        public string AccountCreatedEmailTemplateGuid;
        public string AppUrl;
        public string ThemeUrl;
    }

    [Serializable]
    public class CreateLoginModel
    {
        public enum Response
        {
            Created, //Used if we did create a login for the user
            Emailed, //Used if we emailed the user existing credentials
            Failed //Used if something horrible happened
        }

        public int PersonId;
        
        public string Username;
        public string Password;

        public string ConfirmAccountUrl;
        public string ConfirmAccountEmailTemplateGuid;
        public string ForgotPasswordEmailTemplateGuid;

        //TODO: Get all endpoints working consistently with either NO ROOT, or ALL ROOT
        public string AppUrlWithRoot;
        public string ThemeUrlWithRoot;
    }

    [Serializable]
    public class DuplicatePersonInfo
    {
        public int Id;
        public string FullName;
        public string Gender;
        public string Birthday;
        public bool HasUsernames;
    }
}
