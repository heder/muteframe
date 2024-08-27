# mute-frame: high visibility Teams mute status!

After being notified on basically every other standup for the last year that "you are on mute", I decided to do something about it. Try to solve the problem with... code...

mute-frame is a small net8 application that integrates with Teams (specifically "New Teams"). It subscribes to the mute/unmute status of your current Teams call or meeting and when muted displays a very prominent red border around the screen where Teams is currently located.

## Usage

Clone, compile, put in your favorite directory and run.
This is version proof-of-concept. 

On the list of todo:s are:
* Make the app install itself as autostartup.
* Settings for border type, thickness and color
* Settings for of border should follow the Teams app or stay on primary monitor.

Unfortunately, the process of integrating any app with New Teams via the APi is a little involved. This is by design. Even Stream Deck needs the exact same procedure.
* First, enable API Access in Teams. This is done via Settings -> Privacy -> Manage API -> Enable API
* On startup of the application, a small settings dialog will appear. 
* To initialize integration, start a call or meeting and click on the mute or unmute buttons in the dialog box (not in teams)
* A confirmation popup should appear inside Teams. Click approve and you should be all set.

## Thanks

[@emmaelisen](https://www.github.com/emmaelisen) for being the prime source of "You are on mute, Thomas".

Some of the teams integration code is grabbed and inspired by code on the following accounts.
[@MrRoundRobin](https://github.com/MrRoundRobin)  
[@EionRobb](https://github.com/EionRobb)
