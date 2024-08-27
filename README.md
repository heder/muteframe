# mute-frame: high visibility Teams mute status!

After being notified on basically every other standup for the last year that "you are on mute", I decided to do something about it. Try to solve the problem with... code...

mute-frame is a small net8 application that integrates with Teams (specifically "New Teams"). It subscribes to the mute/unmute status of your current Teams call or meeting and when muted displays a very prominent red border around the screen where Teams is currently located.

## Usage

Clone, compile, put in your favorite directory and run.

Unfortunately, the process of integrating any app with New Teams via the API is a little involved. This is by design. Even Stream Deck needs the exact same procedure.
* First, enable API Access in Teams. This is done via Settings -> Privacy -> Manage API -> Enable API
* On startup of the application, a small settings dialog will appear. 
* To initialize integration, start a call or meeting in Teams and click on the mute or unmute buttons in the app's settings box. (The dialog can be opened by right-clicking the icon in windows systray/notification area)
* A confirmation popup should appear inside Teams. Click approve and you should be all set.

On the list of todo:s are:
* Make the app install itself as autostartup.
* Settings for border type, thickness and color
* Settings for if border should follow the Teams app or stay on primary monitor.

## Thanks

[@emmaelisen](https://www.github.com/emmaelisen) for being the prime source of "You are on mute, Thomas".

Some of the teams integration code is grabbed and inspired by code from the following accounts.
[@MrRoundRobin](https://github.com/MrRoundRobin)  
[@EionRobb](https://github.com/EionRobb)
