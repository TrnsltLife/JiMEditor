![](https://c10.patreonusercontent.com/4/patreon-media/p/campaign/10351808/14902c8d307c42f9892d1a8a0e57000a/eyJ3IjoxMjAwLCJ3ZSI6MX0%3D/2.png?token-time=1683158400&token-hash=6WVShSh7WB-b9BMXvRvQIsZw282ZHPXQ7ZtR3KylWHU%3D)

## [Your Journey Patreon](http://www.patreon.com/YourJourneyRPG)

# JiMEditor - v0.50 - Now for *Mac* as well as Windows!
(See info on Mac installation below!)

A Scenario editor for the Journeys in Middle Earth board game. The scenarios are playable in the companion playback app, [Your Journey](https://github.com/TrnsltLife/your-journey), forked from [GlowPuff's original project](https://github.com/GlowPuff/JiMEditor) with new tiles from [Celstra](https://github.com/Celstra/JiMEditor).


## Download the latest release
If you just want to download and run the app, grab it here: 

[Download the Newest Release!](https://github.com/TrnsltLife/JiMEditor/releases/tag/v0.50)! 
New: Mac Intel and Mac Apple Silicon downloads now available!

New: You can now translate the persistent text of a Text Event. Previous newest feature: You can now specify the Starting Position for Heroes precisely. And you can indicate how much a Hero can Scout at the start of a Scenario and at the start of the Action Phase! 

Use Item Events to give Trinkets or Mounts to the heroes as rewards, and use Title Events to give the heroes Titles! Featuring tiles from Spreading War, Battle Maps with Terrain Tokens, enemies from all the expansions, enemy attack descriptions, text boxes that allow you to edit with special icons, bold and italic, and more! Now with a Random Scenario Generator, Graph View of the scenario, and ability to package translations of the scenario in a single file.

## Mac Installation of the unsigned app
### First Steps
1. Download the file for your Mac architecture: Apple Silicon (arm64) or Intel (x64).
2. Unzip the file you downloaded, if it wasn't unzipped automatically.
3. Double-click the Your Journey Editor app to try to launch it.
4. It probably won't run and you may see a message like: "Your Journey Editor" is damaged and can't be opened. You should move it to the Trash. Move to Trash / Cancel.
5. Click Cancel, so it doesn't throw the app in the Trash.
6. Because this app is not signed with an Apple developer id account, it won't run without you telling Gatekeeper to allow it to run. Here's how to do that.

### Installation Option 1: System Settings
1. Open System Settings > Privacy & Security
2. Scroll toward the bottom of that window, past the "Security" heading, and look for an item like: "Your Journey Editor" was blocked to protect your Mac.
3. If you see that item, click the Open Anyway button.
4. It should allow you to double-click the app and launch it now and in the future.
5. You may have to allow downloads of future version in the same way.

### Installation Option 2: Run script instructing Gatekeeper to allow this app
1. Open a Terminal in the folder that contains the contents you just unzipped, including the Your Journey Editor.app file (name may vary by version) and the YourJourneyEditor-RunMeFirst.command file.
2. You can open a terminal by right clicking on the folder in the Finder and choosing "New Terminal at Folder".
3. Type: ./YourJourneyEditor-RunMeFirst.command
4. This will run the script to disable Gatekeeper on the app. You should then be able to double click the app to launch it.
5. You could also skip the script and directly type the command: xattr -cr "Your Journey Editor"*.app
6. Once you have completed these steps, you should be able to double-click the "Your Journey Editor" app and it should launch.

Note that save files will be stored inside your Documents folder in a folder called /Your Journey.

## Documentation
You can find documentation for the app [in GlowPuff's Wiki](https://github.com/GlowPuff/JiMEditor/wiki). Although it is somewhat out of date at this point, it should still get you started.

### GlowPuff's Demonstration video
[On YouTube](https://www.youtube.com/watch?v=J5u6YwjxIgU) - Again, this is somewhat out of date.

### Enabling new Collections (expansions)
1. Click the Edit Scenario gear icon at the top of the main scenario editor window.
2. Click the checkboxes next to the collections you want to enable.
3. Note: deselecting a collection will prevent you from using further enemies or tiles from that collection, but won't currently remove any tiles or enemies from that collection that you have already added to the scenario.

### Enemy Attack Descriptions
1. Access the Enemy Attack Descriptions using the Enemy Attack Group list in the sidebar.
2. You can add a new Enemy Attack Group or duplicate an existing one.
3. You cannot delete the core Enemy Attack Groups. "Uluk" is the first Enemy Attack Group in the list that you can delete.
4. Click the gear icon to edit an Enemy Attack Group. You can add, delete, or edit the Enemy Activation Descriptions.

### Enemy Events
1. Enemy Pool monsters will now use the default Enemy Attack Group Descriptions.
2. You can choose any Enemy Attack Group for enemies you create in the Scripted Enemies tab, including custom Enemy Attack Groups that you create. Look for the drop-down box on the Scripted Enemies tab.
3. By the way, you can now set two movement values. The first one is the enemy's normal movement; the second is the movement if they can't reach their target.

## Your Journey - Companion Playback App
The companion app that plays back your scenarios is in [this repository](https://github.com/TrnsltLife/your-journey).

[Direct link to latest releases](https://github.com/TrnsltLife/your-journey/releases)

[Direct link to GlowPuff's releases](https://github.com/GlowPuff/your-journey/releases).

## Discuss the project at Board Game Geek
The official forum post for this project: [BGG Forum Post](https://boardgamegeek.com/thread/2488415/custom-scenario-editor-and-companion-app-create-yo)

## YourJourneyScenarios repository
I created a repository to store any scenarios people might wish to share, as well as some test scenarios: [YourJourneyScenarios](https://github.com/TrnsltLife/YourJourneyScenarios). Make sure to read the README for instructions on downloading and installing the scenarios.
