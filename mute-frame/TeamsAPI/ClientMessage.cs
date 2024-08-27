using System.Text.Json.Serialization;

namespace MuteFrame.TeamsApi;

internal class ClientMessage
{
    [JsonPropertyName("action")]
    public string Action { get; set; } = "";

    [JsonPropertyName("parameters")]
    public ClientMessageParameter? Parameters { get; set; }

    [JsonPropertyName("requestId")]
    public int RequestId { get; set; }
}

internal class ClientMessageParameter
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
}

internal static class MeetingAction
{
    public const string None = "none";
    public const string QueryMeetingState = "query-state";

    public const string Mute = "mute";
    public const string Unmute = "unmute";
    public const string ToggleMute = "toggle-mute";

    public const string HideVideo = "hide-video";
    public const string ShowVideo = "show-video";
    public const string ToggleVideo = "toggle-video";

    public const string UnblurBackground = "unblur-background";
    public const string BlurBackground = "blur-background";
    public const string ToggleBlurBackground = "toggle-background-blur";

    public const string LowerHand = "lower-hand";
    public const string RaiseHand = "raise-hand";
    public const string ToggleHand = "toggle-hand";

    public const string StopRecording = "stop-recording";
    public const string StartRecording = "start-recording";
    public const string ToggleRecording = "toggle-recording";

    public const string LeaveCall = "leave-call";
    public const string React = "send-react";
    public const string ToggleUI = "toggle-ui";
    public const string StopSharing = "stop-sharing";
}

public static class ClientMessageParameterType
{
    public const string ReactApplause = "applause";
    public const string ReactLaugh = "laugh";
    public const string ReactLike = "like";
    public const string ReactLove = "love";
    public const string ReactWow = "wow";
    public const string ToggleUiChat = "chat";
    public const string ToggleUiSharing = "sharing-tray";
}