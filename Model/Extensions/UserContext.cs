using Model.Extensions;
using System;
using System.DirectoryServices.AccountManagement;
using System.Security;

namespace Model
{
    public class UserContext
    {
        public delegate void ExceptionHandler(object sender, ErrorMessageEventArgs e);
        public event ExceptionHandler OnException;
        string userAccount;
        string userSID;
        string userDisplayName;
        string domainName;
        SecureString userPassword;

        public string UserAccount { get => userAccount; }
        public string UserSID
        {
            get
            {
                if(String.IsNullOrEmpty(userSID))
                {
                    InitContext();
                }
                return userSID;
            }
        }
        public string UserDisplayName { get => userDisplayName; }
        public string DomainName { get => domainName; }
        public SecureString UserPassword { get => userPassword; }

        public UserContext(string domainName = "", string userAccount = "", SecureString userPassword = null)
        {
            this.domainName = domainName;
            this.userAccount = userAccount;
            this.userPassword = userPassword;

            InitContext();
        }
        public UserContext(string domainName = "", string userAccount = "", string userPassword = "")
        {
            this.domainName = domainName;
            this.userAccount = userAccount;
            this.userPassword = userPassword.ToSecureString();
          
            InitContext();
        }
        public UserContext(string domainName = "", string userAccount = "", string userPassword = "", bool passwordEncripted = false)
        {
            this.domainName = domainName;
            this.userAccount = userAccount;
            this.userPassword = userPassword.DecryptString();

            InitContext();
        }
        public string GetPassword()
        {
            return userPassword.ToInsecureString();
        }
        protected void InitContext()
        {
            userDisplayName = UserPrincipal.Current.DisplayName;
            
            if (String.IsNullOrWhiteSpace(DomainName) || String.IsNullOrWhiteSpace(UserAccount))
            {
                try
                {
                    userSID = UserPrincipal.Current.Sid.ToString();
                }
                catch (Exception exception)
                {
                    OnException?.Invoke(this, new ErrorMessageEventArgs(exception, "Произошел сбой при чтении локальных данных"));
                }
            }
            else
            {
                try
                {
                    using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, DomainName, UserAccount, GetPassword()))
                    {
                        UserPrincipal user = UserPrincipal.FindByIdentity(pc, UserAccount);

                        string isValid = pc.UserName;

                        if (user != null)
                        {
                            userSID = user.Sid.ToString();
                            userDisplayName = user.DisplayName;
                        }
                    }
                }
                catch (Exception exception)
                {
                    OnException?.Invoke(this, new ErrorMessageEventArgs(exception, "Произошел сбой при подключении к домену"));
                }
            }
        }
    }
}
 
