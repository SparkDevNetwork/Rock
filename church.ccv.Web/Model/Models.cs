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
    public class RegAccountData
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
    public class RegisterResponseData
    {
        public enum Status
        {
            Created, //Used when a new person and account is created
            Duplicates, //Used when there's already a person with the given email address
            Help //Used when there's an error in registering
        }

        public string RegisterStatus;
        public List<DuplicatePersonInfo> Duplicates;
    }

    [Serializable]
    public class DuplicatePersonInfo
    {
        public int Id;
        public string FullName;
        public string Gender;
        public string Birthday;
    }
}
