# Twitch Chat Integration

Uses a Twitch chat bot to read messages from Twitch chat and displays them inside of the Stardew Valley chat.
Every message sender is assigned some color that their messages will be displayed in for better readability,
though these colors are not unique to each person.
In multiplayer, the Twitch messages will only be sent to the chat of the mod user, the other players won't see them.

## Usage
You will need to provide a Twitch account that the chat bot can use, you can use the one you use to stream or a separate one.

### To Login:
* Create a "Bot Chat Token" from here: [https://twitchtokengenerator.com/](https://twitchtokengenerator.com/) choose the "Bot Chat Token" option.
* Go through the login process and fill out the Captcha
* In the `Generated Tokens` section, copy the `ACCESS TOKEN` value.
* If not using GMCM, run the game once to generate the `config.json` file. Once the file is made, open it in a text editor and fill out the `Username` and `Password` (Use the `ACCESS TOKEN` you generated in the last step) sections.
* Make sure to also fill out the `TargetChannel` section with the channel you want to inspect (usually your own channel).

## Acknowledgements

The idea for this mod comes from [RyanGoods](https://github.com/StardewModders/mod-ideas/issues/1047).

The code for the Twitch bot comes from [this article](https://medium.com/swlh/writing-a-twitch-bot-from-scratch-in-c-f59d9fed10f3),
I seriously couldn't have made this mod without it.

Also, at this point pretty much all of the updates have been done by [SocksTheWolf](https://github.com/SocksTheWolf), so all the credit for cool new features go to him!
