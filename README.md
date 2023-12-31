Local settings for Steam accounts that have previously logged in on your PC are stored in a file called localconfig.vdf.

Each Steam account on your PC has a separate localconfig.vdf file located in its corresponding path: Steam\userdata\STEAMID\config.

Within this file, the visibility state of the user for the next login is defined by the digit following the ePersonaState key value in the WebStorage section.

```
"WebStorage"
{
    ...
    ...
    "FriendStoreLocalPrefs_STEAMID "		"{\"ePersonaState\":1,\"strNonFriendsAllowedToMsg\":\"\"}"
    ...
    ...
}
```

The digit after the ePersonaState key value defines the visibility state for the next login as follows:

```
0: Offline
1: Online
3: Away
7: Invisible
```

By modifying this value using a simple program, it's possible to make the user appear invisible for the next login, regardless of their online status when Steam was last closed.

# SteamInvisible.exe

![Screenshot_SteamInvisible](https://github.com/mouchesvolantes/SteamInvisible/assets/94894376/05a82e4f-1e1b-4752-8f79-c9fa0ea64b19)

## What does this program do?

When you run this program, it alters the upcoming login status of accounts that were  previously logged in on this PC, effectively changing their status to invisible.

## How does it work?

The program reads the localconfig.vdf files in Steam\userdata\STEAMID\config\ and changes the key value ePersonaState which defines the visibility state for the next login to invisible.

By using the following command-line arguments, you can exclude specific accounts from undergoing modification.

Command-Line Arguments

```
-nogui       Automatically closes the program upon completion of the operation.

-path        Specifies the installation path of Steam (if different from the default path).

-ignore      The online status of the provided Steam IDs remains unchanged.

-startsteam  Starts Steam once the program has finished its operation.
```
```
Usage example:

SteamInvisible.exe -nogui -startsteam -path "C:\Program Files (x86)\Steam\" -ignore "12345678;11223344"
```

# SteamInvisibleOnFirstLogin.exe

![Screenshot_SteamInvisibleOnFirstLogin](https://github.com/mouchesvolantes/SteamInvisible/assets/94894376/0a0c00d3-fab7-4f72-a5d7-5e856ae22bf2)

## What does this program do?

When logging in to Steam with an account or the first time on a PC, the visibility state is always set to online. If you run this program during your login, it will render your first login as invisible.

## How does it work?

While you're logging in, the program will wait for Steam to generate the localconfig.vdf file, and it will then immediately close Steam as soon as this file is created - before going online in Steam Friends. The program will then proceed to change the ePersonaState key value, configuring your account's next visibility status to be invisible.

Command-Line Arguments

```
-path        Specifies the installation path of Steam (if different from the default path).
```
