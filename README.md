# Trevizani Roleplay

Text-based roleplay game mode for RAGE MP.

The source code has some things in Portuguese and I intend to globalize it soon.

To use the gamemode you also need to use the [UCP](https://github.com/GuilhermeTrevizani/trevizani-roleplay-ucp) and the [client-side](https://github.com/GuilhermeTrevizani/trevizani-roleplay-client).

I'll soon update the documentation with the configuration steps. But you can find out and configure it by reading and understanding the source code.

# Setup
1. Create a Discord application [here](https://discord.com/developers/applications).
2. Configure your Discord App Id on client-side [here](https://github.com/GuilhermeTrevizani/trevizani-roleplay-client/blob/b069369fb3a1c9dd61b65a6cf5b80ce7eb5d0ebf/src/base/constants.ts#L3).
3. Configure your Discord App Id and Discord Client Secret on server-side in your conf.json putting these section below in the file.
```
"settings": {
    "language": "en-US",
    "discordClientId": "DISCORD_CLIENT_ID",
    "discordClientSecret": "DISCORD_CLIENT_SECRET",
    "discordBotToken": "", 
    "announcementDiscordChannel": "",
    "companyAnnouncementDiscordChannel": "",
    "governmentAnnouncementDiscordChannel": "",
    "staffDiscordChannel": "",
    "premiumGoldDiscordRole": "",
    "premiumSilverDiscordRole": "",
    "premiumBronzeDiscordRole": "",
    "mainDiscordGuild": "",
    "roleplayAnnouncementDiscordChannel": "",
    "firefighterEmergencyCallDiscordChannel": "",
    "policeEmergencyCallDiscordChannel": "",
    "databaseConnection": ""
  }
```
4. Configure your database connection [here](https://github.com/GuilhermeTrevizani/trevizani-roleplay-server/blob/e2a1a790e7517d2c43d748ff42c3595efd766dcf/src/TrevizaniRoleplay.Core/Models/Server/Constants.cs#L39).
5. Build client-side with `yarn build` and put the files in your client_resources folder. By default the files are created in `C:\RAGEMP\server-files\client_packages`.
6. You need to use .NET 9.0 runtime files, you can use [these](https://drive.google.com/file/d/1tLWE1ByteqL5SQ3iz3WZkDaXdI8KRXv7/view). Put them inside `server-files\dotnet\runtime`.
7. Run your server to create the database.
8. Configure your Discord Url on UCP [here](https://github.com/GuilhermeTrevizani/trevizani-roleplay-ucp/blob/63df99b4d2bcbf166c3fa7a31898637884d33604/.env#L2).
9. Configure Discord Client Id, Secret and Redirect Uri in TrevizaniRoleplay.Api's appsettings.json.

The user creation can be on both sides (game or UCP), but to create a Character you need to create it on UCP. The first user created has the highest staff rank.
