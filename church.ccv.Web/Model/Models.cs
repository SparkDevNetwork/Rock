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
    public class RegAccountData
    {
        public string FirstName;
        public string LastName;
        public string Email;

        public string CellPhoneNumber;

        public string Username;
        public string Password;
    }
}
