# Warhammer 40K: Rogue Trader - SpeechMod
By [Osmodium](https://github.com/Osmodium)

## This mod is made for Warhammer 40K: Rogue Trader and introduces TTS (TextToSpeech) in most places.
Version: 0.6.0

**Disclaimer: UNDER DEVELOPMENT**

- Windows: Should work with the features implemented.

- OSX: Not tested, let me know if it works.

**Works with all languages as long as you have a voice in that language installed.**

[How to unlock more voices in Windows 10](https://www.ghacks.net/2018/08/11/unlock-all-windows-10-tts-voices-system-wide-to-get-more-of-them/)

### How to install

 1. Download the W40KSpeechMod mod file and unzip.
 2. Please note that the game comes with its own built in Unity Mod Manager so you do not need to install another one.

 3. Navigate to *%userprofile%\AppData\LocalLow\Owlcat Games\Warhammer 40000 Rogue Trader\UnityModManager\\*
 4. Copy the *W40KSpeechMod* folder into the *UnityModManager* folder
 5. Launch Warhammer 40K: Rogue Trader, you may need to hit **ctrl+F10** to see the mod manager window.

### Known issues / limitations

*If you find issues or would like to request features, please use the issues tracker in GitHub [here](https://github.com/Osmodium/WH40KRTSpeechMod/issues)*

#### Limitations:
- None known

#### Todo:
- A bunch, see https://github.com/Osmodium/W40KRogueTraderSpeechMod/blob/main/Todo.txt for more info.

### How to use

#### 1) Dialog
When in dialog mode you can now press the play button next to the left image to listen to the current block of dialog. If autoplay is enabled, you don't have to push the playbutton.

![Playbutton for the current dialog](https://dashvoid.com/speechmod/w40krt/1_DialogPlayButton_0_6_0.png)

#### 2) Inspection Information
When inspecting items and links (through *right-click->Info* or just *right-click* for expanded tooltip) *hover* over the text and *left click*.

![Here the hover behaviour is set to underline the text, see the settings for more custumization](https://dashvoid.com/speechmod/w40krt/2_ExpandedTooltips_0_6_0.png)

#### 4) 🔳 Journal Quest text
**NOT IMPLEMENTED YET** In the journal, each of the bigger text blocks and important stuff can be played through the play button adjacent to the text.

#### 5) 🔳 Lexica text
**NOT IMPLEMENTED YET** In the lexica the text blocks (defined by Owlcat) can be played by pressing the play button adjacent to the text.

#### 6) 🔳 Book Event text
**NOT IMPLEMENTED YET** When encountering a book event, the text can be played by hovering the text part (it will apply the chosen hover effect) and left-clicking.

#### 7) ✅ Messagebox text
The various pop-up boxes that eventually shows up throughout the game, can be played when hovered and left-clicked.

![Some texts might be so important that I decided to add support for them.](https://dashvoid.com/speechmod/w40krt/4_MessageBoxes_0_6_0.png)

#### 8) 🔳 Space battle(?) Results text
**NOT IMPLEMENTED YET** When your amy has defeated an enemy, the resulting message text is also supported for playback when hovered and left-clicked. Might not exist, haven't played that far yet.

#### 9) ✅ Tutorial Windows text
Both big and small tutorial windows text is supported and can be played by hovering and left-clicking.

![Tutorials can be helpful to learn more.](https://dashvoid.com/speechmod/w40krt/3_TutorialSmall_0_6_0.png)

#### 10) ✅? Character story under summary and biography
**MIGHT WORK** When inspecting a character, the story of that character is displayed both under *Summary* and under *Biography*, and are both supported by hovering and left-clicking.

### Settings

The different settings (available through *ctrl+f10* if not overridden in the UMM) for SpeechMod
- **Narrator Voice**: The settings for the voice used for either all or non-gender specific text in dialogs when *Use gender specific voices* is turned on.
	- *Nationality*: Just shows the selected voices nationality.
	- **Speech rate**: The speed of the voice the higher number, the faster the speech.
		- Windows: from -10 to 10 (relative speed from 0).
		- macOS: from 150 to 300 (words per minute).
	- Windows Only:
		- **Speech volume**: The volume of the voice from 0 to 100.
		- **Speech pitch**: The pitch of the voice from -10 to 10.
	-**Preview Voice**: Used to preview the settings of the voice.
- **Use gender specific voices**: Specify voices for female and male dialog parts. Each of the voices can be adjusted with rate, volume and pitch where available.
- Windows Only:
	- **Interrupt speech on play**: 2 settings: *Interrupt and play* or *Add to queue*, hope this speaks for itself.
- **Auto play dialog**: When enabled, dialogs will be played automatically when theres no voice acted dialog.
- **Auto play ignores voiced dialog lines**: Only available when using auto play dialog. This option makes the auto play ignore when there is voiced dialog, remember to turn dialog off in the settings.
- **Playback barks**: When clicking on points of interests in the world, the small description is called a "Bark". Enabling this feature reads the bark aloud in the narrator voice. This also applies to character "banter" which is in the same style.
- **Enable color on hover**: This is used only for the text boxes when inspecting items, and colors the text the selected color when hovering the text box.
- **Enable font style on hover**: As above this is only used for text boxes, but lets you set the style of the font.
- **Phonetic Dictionary Reload**: Reloads the PhoneticDictionary.json into the game, to facilitate modificaton while playing.

![Settings for SpeechMod](https://dashvoid.com/speechmod/w40krt/0_Settings_0_6_0.png)

### Motivation
*Why did I create this mod?*
After having created the "same" mod for [Pathfinder: Wrath of the Righteous](https://www.nexusmods.com/pathfinderwrathoftherighteous/mods/241), I got requests of doing it for this game too, and since I also wanted to play it, I gratefully obliged :).

I have come to realize that I spend a lot of my energy through the day on various activities, so when I get to play a game I rarely have enough energy left over to focus on reading long passages of text. So I thought it nice if I could get a helping hand so I wouldn't miss out on the excellent stories and writing in text heavy games.

After I started creating this mod, I have thought to myself that if I struggle with this issue, imagine what people with genuine disabilities must go through and possibly miss out on, which motivated me even more to get this mod working and release it. I really hope that it will help and encourage more people to get as much out of the game as possible.

### Contribute
If you find a name in the game which is pronounced funny by the voice, you can add it to the PhoneticDictionary.json in the mod folder (don't uninstall the mod as this will be deleted). I don't have a great way of submitting changes to this besides through GitHub pull requests, which is not super user friendly. But let's see if we can build a good pronunciation database for the voice together.
Also feel free to hit me up with ideas, issues and PRs on GitHub or NexusMods :)

### Acknowledgments
- [Chad Weisshaar](https://chadweisshaar.com/blog/author/wp_admin/) for his blog about [Windows TTS for Unity](https://chadweisshaar.com/blog/2015/07/02/microsoft-speech-for-unity/)
- [dope0ne](https://forums.nexusmods.com/index.php?/user/895998-dope0ne/) (zer0bits) for providing code to support macOS in the original mod, and various exploration work.
- Owlcat Modding Discord channel members
- Join the [Discord](https://discord.gg/EFWq7rJFNN)