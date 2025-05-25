Technical Briefing: RadioNation – iOS Radio Streaming App (Unity + Native Integration)

Purpose:

This prompt is intended to onboard an AI assistant to the architecture, purpose, and implementation of the RadioNation app so it can offer accurate guidance and support across Unity and native iOS layers. The agent should read all referenced files carefully and ask no questions until fully understood.

🔧 Project Overview

RadioNation is a Unity-based mobile radio streaming application targeting iOS and Android, currently focused on iOS native playback using AVPlayer via Objective-C++ integration.

💡 Key Features & Technologies

Feature	Implementation
Audio streaming	AVPlayer inside RadioStreamManager.mm
Metadata (Now Playing)	Lock screen/Control Center via MPNowPlayingInfoCenter
Remote control support	MPRemoteCommandCenter handles Apple Watch/Lock Screen media buttons
Background audio	Enabled via AVAudioSessionCategoryPlayback
Playback state sync	NowPlaying state sync via syncPlaybackStateToNowPlaying()
Bluetooth disconnect handling	AVAudioSessionRouteChangeNotification to auto-stop
Metadata polling	1-second interval to fetch now_playing from a custom API
Error logging	GetLastPlaybackError() stored in lastErrorReason, visible in Unity debug UI

🧩 Architecture Diagram (Simplified)

[Unity C# UI Layer]
        |
        v
[iOSRadioLauncher.cs] <--> [RadioStreamManager.mm]
        |                           |
[UnityPlayer Game UI]        [AVPlayer / MPNowPlayingInfoCenter]

📂 Essential Files to Attach

RadioStreamManager.mm – Core native iOS audio engine and logic.
iOSRadioLauncher.cs – Unity bridge class calling native functions.
HandleOptionsMenu.cs – UI script that shows error/debug info.
RadioPlayer.cs (if separate) – Coordinates playback state, button handling.
NowPlayingMetadataHandler.cs (optional) – If exists, handles Unity-side metadata.
BuildSettings / ExternalTools screenshot – To show code signing setup.

🔍 Questions the AI Agent Might Ask — Already Answered:

Q1: How does Unity call into native iOS?
✅ Through DllImport("__Internal") functions in iOSRadioLauncher.cs.

Q2: How is Now Playing metadata retrieved?
✅ Via custom endpoint: https://www.wamballa.com/metadata/?station=.... The native layer polls this every second when StatePlaying.

Q3: How does the app handle remote controls and Bluetooth changes?
✅ Using MPRemoteCommandCenter and AVAudioSessionRouteChangeNotification. These update playback state and error logs.

Q4: What happens when a stream fails or stops?
✅ lastErrorReason is set internally in native code and can be polled by Unity using GetLastPlaybackErrorMessage().

Q5: What’s missing or not yet implemented?
❌ Real buffering percentage
❌ Album artwork from metadata (favicon only for now)
❌ App lifecycle pause/resume handling
❌ AirPlay support

🧠 Agent Instruction
Take your time reviewing each file. Understand the flow of playback from Unity button press through to AVPlayer stream start. Study how Unity gets metadata and how state sync works for lock screen and Apple Watch controls. Cross-reference function calls. No assumptions. Ensure full understanding before offering suggestions or answering follow-ups.

