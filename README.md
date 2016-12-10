# ffxiv.act.applbot
applbot is an ACT plugin that covers robot, callouts, event broadcast, and timeline for FFXIV boss encounters.

## Installation
1. Download and unblock (if neccesary) .zip file from releases section (https://github.com/applepudding/ffxiv.act.applbot/releases).
2. Extract .zip to ACT Folder (applbot folder inside ACT main folder)

## Usage
er ... add+enable it in ACT - plugin listing and try out :) 
Make sure ACT reset is set to around 35s or 40s. Options - Main Table/Encounters - General - Number of seconds....

## Guides and Screenshots
- visual guide: http://imgur.com/a/g1OyM
- a12s settings: http://imgur.com/a/5s4Us
- a12s dps spot assignment: https://imgur.com/a/Tna90

## Videos
- a12s, 2nd personal kill, full pugs PF @ levi: https://www.twitch.tv/melonpudding/v/106460362

## Credits & Contacts
- Apple Pudding, Ix Xyl, Raccle Lancale, Akela Freya @ Leviathan
- A'oshane Taru @ Odin

## Stuffs on progress
Required fixes:
- a9s + a10s xml (stop working when player dies, and timings)

Missing features:
- sophia ex xml (lazy)
- event warning notification (warning-timing, warning-speak, warning-countdown)
- simulation on plugin (read from chosen log directly)
- full log reader fight recognizer/separator
- multiple encounters in 1 xml
- broadcastable BEEP

## Contributing 
1. Fork it!
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin my-new-feature`
5. Submit a pull request

(template: https://gist.github.com/zenorocha/4526327)