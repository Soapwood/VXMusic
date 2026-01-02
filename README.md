<div align="center">
<img src=".github/branding/VXMusicGitBannerNoLogoTransparentWhite.png" alt="Alt Text" style="max-width: 100%">

[![Discord](https://img.icons8.com/?size=100&id=M725CLW4L7wE&format=png&color=000000)](https://t.co/Z2eSKfYpfs)
[![BlueSky](https://img.icons8.com/?size=100&id=3ovMFy5JDSWq&format=png&color=000000)](https://bsky.app/profile/soapwood.net)

[![Latest Stable Release](https://img.shields.io/github/v/release/Soapwood/VXMusic?style=for-the-badge&color=dda0dd)](https://github.com/Soapwood/VXMusic/releases/latest)

![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/Soapwood/VXMusic/.github%2Fworkflows%2Fvxmusic-publish-release.yml?branch=main&style=flat-square)
![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/Soapwood/VXMusic/total)

<div align="center">

### Music Recognition Overlay for Social VR
#### Also suitable for Desktop use!

# Get VXMusic
[![Download](https://img.icons8.com/?size=100&id=103169&format=png&color=000000)](https://github.com/Soapwood/VXMusic/releases/latest)

Download Installer

## Quick Start Menu

<div>
    <a href="#how-to-use" class="button">How to Use</a> |
    <a href="#steamvr" class="button">Add to SteamVR</a>
</div>

<div align="left">

# Installation
An Installer is available from the Releases page. Click the download button above!

The install wizard will take care of the rest.

[VXMusic can also be easily installed as a SteamVR Plugin with one click!](#steamvr)

# Updates
You will be automatically notified when updates are available.

<img src=".github/readme/AutoUpdater.png" alt="VXMusic Desktop" style="max-width: 80%">

You can also choose if you want to be prompted when new versions are available, or manually update on the bottom right of the Desktop Client.

<img src=".github/readme/UpdateOptions.png" alt="VXMusic Desktop" style="max-width: 80%">

# How to Use
[//]: # (### 🎵 Recognise music at live events!)

[//]: # ()
[//]: # (<img src=".github/readme/OverlayDemo.gif" alt="VXMusic Desktop" style="max-width: 80%">)

### Using the Overlay, simply pull the trigger, and boop with your index finger!

<img src=".github/readme/OverlayRecognition.gif" alt="VXMusic Desktop" style="max-width: 80%">

### Recognition also works on Desktop! Enjoy recognising music on livestreams or videos.

## Recognition
VXMusic uses **Shazam** for music recognition.

Recognised tracks are stored in your `My Documents/VXMusic` folder.

For easy access, you can click the `Open Track Library` button on the Recognition tab.

<img src=".github/readme/OpenTrackLibrary.png" alt="VXMusic Desktop" style="max-width: 80%">

### If you are playing VRChat, VXMusic will name the trackfile after the World you are currently in for later inspection.

<img src=".github/readme/IfYouAreRunningVRChat.png" alt="VXMusic Desktop" style="max-width: 80%">

### Tip: You can also add recognised tracks to <a href="#spotify" class="button">Spotify Playlists</a> and scrobble on <a href="#connections" class="button">Last.fm</a>!

## Notifications
VXMusic supports two VR notification services that allow you to receive HUD toast notifications while in VR - **SteamVR**, **XSOverlay** and **OVR Toolkit**.

To choose which service you would like to use, simply click on your desired service from the Notifications tab.

<img src=".github/readme/NotificationServices.png" alt="VXMusic Desktop" width="60%">

### SteamVR
SteamVR has a built-in Notifications service that VXMusic can use to send you HUD toast notifications.

This is plug and play and doesn't need any additional configuration.

### XSOverlay
<img src=".github/readme/XSOverlayLogo.jpg" alt="XSOverlayLogo.jpg" width="30%">

VXMusic also interfaces with XSOverlay for HUD toast notifications.

XSOverlay is the recommended Notification service for how responsive and reactive it is. Ultimately it is purely aesthetical, but you will benefit from the bespoke configuration offered by XSOverlay if you are already a user. 

XSOverlay is a paid Overlay available on [Steam](https://store.steampowered.com/app/1173510/XSOverlay/), and is purely optional when using VXMusic.

### OVR ToolKit
<img src=".github/readme/OVRToolKit.jpg" alt="OVRToolKit.jpg" width="30%">

OVR ToolKit is another fantastic option for notifications.

This also has a lovely UI for notifications, and a more bespoke desktop application for settings.

OVRToolkit is a paid Overlay available on [Steam](https://store.steampowered.com/app/1068820/OVR_Toolkit__Desktop_Overlay/), and is purely optional when using VXMusic.

## Optional Notifications - VRChat ChatBox

VXMusic can send ChatBox notifications to VRChat to share Recognition results with your friends!

<img src=".github/readme/ChatBoxNotifications.gif" alt="VXMusic Desktop" width="60%">

You can also enable/disable this from the Notifications tab.

<img src=".github/readme/EnableVRChatNotifications.png" alt="VXMusic Desktop" width="60%">

## Connections
Instead of just tracking recognised songs in a text file, VXMusic also supports external Music tracking services, **Spotify** and **Last.fm**!

### Spotify
#### ⚠️ Note: Spotify Integration
```
The Spotify Integration App is currently undergoing review by Spotify.

Until this is complete, it is currently required to manually add Users to the App configuration for the Playlists feature to work.

Please reach out on Discord to have your Spotify account added!
```

To connect VXMusic to your Spotify account, simply click `Connect` on the Connections tab and follow the login instructions on your browser.

<img src=".github/readme/ConnectSpotify.png" alt="VXMusic Desktop" width="30%">

Tracks will be added to automatically created Playlists. 

_If you are playing in VRChat, the current World will also be used to name the playlists for later inspection._

<img src=".github/readme/SpotifyPlaylists.png" alt="VXMusic Desktop" width="30%">

### Last.fm
To connect VXMusic to your Last.fm account, navigate to the Connections tab and enter your Login details.

<img src=".github/readme/LastFmLogin.png" alt="VXMusic Desktop" width="30%">

Recognised tracks will be automatically scrobbled to your Last.fm account while using VXMusic!

<img src=".github/readme/LastFmPage.png" alt="VXMusic Desktop" width="60%">


## SteamVR
VXMusic can be installed as a SteamVR Plugin.

With this, you can automatically launch VXMusic when you launch SteamVR.

- Navigate to the Settings Tab
- Click "Install" under Install as SteamVR Overlay. You will be prompted when the installation is complete.
- It is recommended that you enable "Launch Overlay on Startup" for quicker startup!

<img src=".github/readme/InstallAsSteamVRPlugin.png" alt="VXMusic Desktop" width="60%">

## Reporting Bugs

Please direct all bug reports to the `bug_reports` channel on the VX [Discord](https://t.co/Z2eSKfYpfs)!

Each Bug report requires logs to be posted as part of the report. 

For quick access to your logs, click `Open Logs Directory` in the About tab.

Your feedback is greatly appreciated.

<img src=".github/readme/OpenLogs.png" alt="VXMusic Desktop" width="60%">


## License
**VXMusic** is protected under the **Mozilla Public License Version 2.0**.

_Okay... so what does this actually mean?_

You are free to fork and change VXMusic as you wish - the source code for VXMusic is open and freely distributed.

However, the MPL 2.0 license does not grant you any rights to use the Virtual Xtensions/VXMusic name, logos, or trademark branding.

If you distribute your changes, you must share your modified source code. This ensures that others can benefit from your improvements.

### Font
BRUSHSTRIKE is a brush typeface designed by Francesco Canovaro. Free for non-commercial use.

## Special Thanks

Special thanks to the testing team and early adopters. Your feedback has been immeasurable. <3

# Made in 🇮🇪
