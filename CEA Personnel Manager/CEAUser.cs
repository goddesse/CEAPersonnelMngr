using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.DirectoryServices.AccountManagement;
using System.Web;
using System.Windows;

namespace CEA_Personnel_Manager
{
    public class CEARole
    {
        public CEARole(List<string> groupList)
        {
            using(PrincipalContext pctx = new PrincipalContext(ContextType.Domain, DomainName, DomainContainer))
            {
                try
                {
                    foreach(string g in groupList)
                    {
                        using(var gp = GroupPrincipal.FindByIdentity(pctx, IdentityType.SamAccountName, g))
                        {
                            groups.Add(g);
                        }
                    }

                    /* To accomodate my hacky logical security group consisting of 2 separate equivalent groups,
                       only use the first group's membership so we don't get duplicates. Will fix later by normalizing
                       the student worker group to only one. */
                    string firstSecGroup = (string)groups.ToArray().GetValue(0);
                    using (var gp = GroupPrincipal.FindByIdentity(pctx, IdentityType.SamAccountName, firstSecGroup))
                    {
                        foreach(UserPrincipal up in gp.GetMembers())
                         {
                            Users.Add(new CEAUser(up));
                         }
                    }
                }
                catch(InvalidCastException) { return; } //Purposefully swallow this because we honest-to-God don't care if we can't cast from any non-UserPrincipal
                catch(Exception e)
                {
                    System.Diagnostics.Process.Start("mailto:ada@uark.edu?subject=CEA Personnel Manager Exception&body=" + HttpUtility.HtmlEncode(e.ToString()));
                }
            }
        }

        public bool AddUser(string username)
        {
            using(PrincipalContext pctx = new PrincipalContext(ContextType.Domain, DomainName, DomainContainer))
            {
                try
                {
                    foreach(string g in groups)
                    {
                        using(var gp = GroupPrincipal.FindByIdentity(pctx, IdentityType.SamAccountName, g))
                        {
                            var user = UserPrincipal.FindByIdentity(pctx, IdentityType.SamAccountName, username);
                            gp.Members.Add(user);
                            gp.Save();
                            Users.Add(new CEAUser(user));
                        }
                    }
                }
                catch(ArgumentNullException) { MessageBox.Show("That user doesn't seem to exist.", null, MessageBoxButton.OK, MessageBoxImage.Error); return false; }
                catch(PrincipalExistsException) { MessageBox.Show("User already belongs to the group!", "Informational", MessageBoxButton.OK, MessageBoxImage.Information); return true; }
                catch(InvalidCastException) { return false; } //Purposefully swallow this because we honest-to-God don't care if we can't cast from any non-UserPrincipal
                catch(Exception e)
                {
                    System.Diagnostics.Process.Start("mailto:ada@uark.edu?subject=CEA Personnel Manager Exception&body=" + HttpUtility.HtmlEncode(e.ToString()));
                    return false;
                }
            }

            return true;
        }

        public bool RemoveUser(string username)
        {
            using(PrincipalContext pctx = new PrincipalContext(ContextType.Domain, DomainName, DomainContainer))
            {
                try
                {
                    foreach(string g in groups)
                    {
                        using(var gp = GroupPrincipal.FindByIdentity(pctx, IdentityType.SamAccountName, g))
                        {
                            var user = UserPrincipal.FindByIdentity(pctx, IdentityType.SamAccountName, username);
                            gp.Members.Remove(user);
                            gp.Save();

                            for(int i = 0; i < Users.Count; ++i)
                            {
                                if (Users[i].Username == user.SamAccountName)
                                    Users.RemoveAt(i);
                            }
                        }
                    }
                }
                catch(InvalidCastException) { return false; } //Purposefully swallow this because we honest-to-God don't care if we can't cast from any non-UserPrincipal
                catch(Exception e)
                {
                    System.Diagnostics.Process.Start("mailto:ada@uark.edu?subject=CEA Personnel Manager Exception&body=" + HttpUtility.HtmlEncode(e.ToString()));
                    return false;
                }   
            }

            return true;
        }

        private static UserPrincipal NewMethod(string username, PrincipalContext pctx)
        {
            return UserPrincipal.FindByIdentity(pctx, IdentityType.SamAccountName, username);
        }

        public ObservableCollection<CEAUser> Users = new ObservableCollection<CEAUser>();
        protected List<string> groups = new List<string>();
        protected static readonly string DomainName = "uark.edu";
        protected static readonly string DomainContainer = "DC=uark,DC=edu";


        public class CEAUser : INotifyPropertyChanged
        {
            public CEAUser(UserPrincipal u) { user = u; }

            public event PropertyChangedEventHandler PropertyChanged;

            public void NotifyPropertyChanged(string propName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }

            public string Username => user.SamAccountName;
            public string DisplayName => user.DisplayName;
            public string GivenName => user.GivenName;
            public string Surname => user.Surname;
            private UserPrincipal user;
        }
    }

    public static class CEARoles
    {
        public static CEARole CEAAdministrators = new CEARole(new List<string> { "CEA Admin Staff" }); //Full-time administrative staff (nothing to do with IT)
        public static CEARole CEAGraduateAssistants = new CEARole(new List<string> { "CEA GA Staff" }); //Graduate assistants, almost the same access as the admins
        public static CEARole CEATechnicalAssistants = new CEARole(new List<string> { "CEA Lab Staff", "CEA TCA Staff" }); //Part-time, student workers, low privileges
    }
}
