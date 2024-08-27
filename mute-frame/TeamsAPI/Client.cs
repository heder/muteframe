﻿using System.ComponentModel;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MuteFrame.TeamsApi;

public class TeamsClient : INotifyPropertyChanged
{
    public bool IsBackgroundBlurred { get => isBackgroundBlurred; set { if (value != IsBackgroundBlurred) _ = SendCommand(value ? MeetingAction.BlurBackground : MeetingAction.UnblurBackground); } }
    public bool IsHandRaised { get => isHandRaised; set { if (value != IsHandRaised) _ = SendCommand(value ? MeetingAction.RaiseHand : MeetingAction.LowerHand); } }
    public bool IsMuted { get => isMuted; set { if (value != IsMuted) _ = SendCommand(value ? MeetingAction.Mute : MeetingAction.Unmute); } }
    public bool IsVideoOn { get => isVideoOn; set { if (value != IsVideoOn) _ = SendCommand(value ? MeetingAction.ShowVideo : MeetingAction.HideVideo); } }
    public bool IsSharing { get => isSharing; set { if (value != IsSharing) _ = value ? SendCommand(MeetingAction.ToggleUI, ClientMessageParameterType.ToggleUiSharing) : _ = SendCommand(MeetingAction.StopSharing); } }

    public bool IsRecordingOn { get => isRecordingOn; }
    public bool IsInMeeting { get => isInMeeting; }

    public bool HasUnreadMessages { get => hasUnreadMessages; }

    public bool IsConnected => ws is not null && ws.State == WebSocketState.Open;

    public bool CanToggleBlur { get; private set; }
    public bool CanToggleHand { get; private set; }
    public bool CanToggleMute { get; private set; }
    public bool CanToggleShareTray { get; private set; }
    public bool CanToggleVideo { get; private set; }

    public bool CanToggleChat { get; private set; }
    public bool CanStopSharing { get; private set; }

    public bool CanLeave { get; private set; }
    public bool CanPair { get; private set; }
    public bool CanReact { get; private set; }

    private bool isMuted;
    private bool isVideoOn;
    private bool isHandRaised;
    private bool isInMeeting;
    private bool isRecordingOn;
    private bool isBackgroundBlurred;
    private bool hasUnreadMessages;
    private bool isSharing;

    private readonly SemaphoreSlim sendSemaphore = new(1);
    private readonly SemaphoreSlim receiveSemaphore = new(1);
    private readonly SemaphoreSlim connectingSemaphore = new(1);


    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };


    private ClientInfo clientInfo;

    private ClientWebSocket? ws;

    public event EventHandler? Disconnected;
    public event EventHandler? Connected;

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler<TokenReceivedEventArgs>? TokenReceived;
    public event EventHandler<SuccessReceivedEventArgs>? SuccessReceived;
    public event EventHandler<EventArgs>? ErrorReceived;

    public TeamsClient(string manufacturer, string device, string app, string appVersion, string? token = null)
    {
        clientInfo = new()
        {
            App = app,
            AppVersion = appVersion,
            Device = device,
            Manufacturer = manufacturer,
            Token = token
        };
    }

    public async Task Connect(bool waitForTeams = true, CancellationToken cancellationToken = default)
    {
        try
        {
            await connectingSemaphore.WaitAsync(cancellationToken);

            if (Process.GetProcessesByName("ms-teams").Length == 0 && !waitForTeams)
                return;

            while (Process.GetProcessesByName("ms-teams").Length == 0)
                await Task.Delay(1000, cancellationToken);

            if (ws is not null)
            {
                switch (ws.State)
                {
                    case WebSocketState.Open:
                        return;
                    case WebSocketState.Connecting:
                        await WaitOpen(cancellationToken);
                        return;
                    case WebSocketState.Aborted:
                    case WebSocketState.Closed:
                    case WebSocketState.None:
                        await Disconnect(cancellationToken);
                        break;
                    case WebSocketState.CloseReceived:
                    case WebSocketState.CloseSent:
                        await WaitClose(cancellationToken);
                        await Disconnect(cancellationToken);
                        break;
                }
            }

            ws = new();

            var url = clientInfo.GetServerUrl();

            await ws.ConnectAsync(url, cancellationToken);

            await WaitOpen(cancellationToken);

            if (ws.State != WebSocketState.Open)
                throw new WebSocketException($"Invalid state after connect: ${ws.State}");

            Connected?.Invoke(this, System.EventArgs.Empty);
            Receive(cancellationToken);
            await SendCommand(MeetingAction.QueryMeetingState, null, cancellationToken);
        }
        finally
        {
            connectingSemaphore.Release();
        }
    }

    public async Task Disconnect(CancellationToken cancellationToken = default)
    {
        if (ws is null) return;

        var initialState = ws.State;

        switch (ws.State)
        {
            case WebSocketState.Open:
            case WebSocketState.Connecting:
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, cancellationToken);
                break;
            case WebSocketState.CloseSent:
                await WaitClose(cancellationToken);
                break;
            case WebSocketState.CloseReceived:
            case WebSocketState.Aborted:
            case WebSocketState.Closed:
            case WebSocketState.None:
                break;
        }

        ws.Dispose();
        ws = null;

        if (initialState != WebSocketState.Closed)
            Disconnected?.Invoke(this, System.EventArgs.Empty);
    }

    public async Task Reconnect(bool waitForTeams, CancellationToken cancellationToken = default)
    {
        await Disconnect(cancellationToken);
        await Connect(waitForTeams, cancellationToken);
    }

    private async Task WaitClose(CancellationToken cancellationToken = default)
    {
        if (ws is null) return;

        while (ws is not null && ws.State != WebSocketState.Closed && ws.State != WebSocketState.Aborted)
            await Task.Delay(25, cancellationToken);
    }

    private async Task WaitOpen(CancellationToken cancellationToken = default)
    {
        if (ws is null) throw new InvalidOperationException();

        while (ws.State == WebSocketState.Connecting)
            await Task.Delay(25, cancellationToken);
    }

    private async void Receive(CancellationToken cancellationToken = default)
    {
        if (ws is null || ws.State != WebSocketState.Open)
            throw new InvalidOperationException("Not connected");

        try
        {
            await receiveSemaphore.WaitAsync(cancellationToken);

            var buffer = new byte[1024];

            while (true)
            {
                var result = await ws.ReceiveAsync(buffer, cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    if (ws.CloseStatus != WebSocketCloseStatus.NormalClosure)
                    {
                        Disconnected?.Invoke(this, System.EventArgs.Empty);
                        _ = Reconnect(true, cancellationToken);
                    }

                    return;
                }

                if (!result.EndOfMessage || result.Count == 0)
                    throw new NotImplementedException("Invalid Message received");

                var data = Encoding.UTF8.GetString(buffer, 0, result.Count);

                Debug.WriteLine(data);

                var message = JsonSerializer.Deserialize<ServerMessage>(data, jsonSerializerOptions) ?? throw new InvalidDataException();

                if (message.Response == "Success")
                    SuccessReceived?.Invoke(this, new SuccessReceivedEventArgs(message.RequestId ?? 0));

                if (!string.IsNullOrEmpty(message.ErrorMsg))
                {
                    if (message.ErrorMsg.EndsWith("no active call"))
                        continue;

                    ErrorReceived?.Invoke(this, new EventArgs(message.ErrorMsg));
                }

                if (message.TokenRefresh is not null)
                {
                    clientInfo.Token = message.TokenRefresh;
                    TokenReceived?.Invoke(this, new TokenReceivedEventArgs(message.TokenRefresh));
                }

                if (message.MeetingUpdate is null || message.MeetingUpdate.MeetingState is null)
                    continue;

                if (isSharing != message.MeetingUpdate.MeetingState.IsSharing)
                {
                    isSharing = message.MeetingUpdate.MeetingState.IsSharing;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSharing)));
                }

                if (isMuted != message.MeetingUpdate.MeetingState.IsMuted)
                {
                    isMuted = message.MeetingUpdate.MeetingState.IsMuted;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsMuted)));
                }

                if (hasUnreadMessages != message.MeetingUpdate.MeetingState.HasUnreadMessages)
                {
                    hasUnreadMessages = message.MeetingUpdate.MeetingState.HasUnreadMessages;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasUnreadMessages)));
                }

                if (isVideoOn != message.MeetingUpdate.MeetingState.IsVideoOn)
                {
                    isVideoOn = message.MeetingUpdate.MeetingState.IsVideoOn;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVideoOn)));
                }

                if (isHandRaised != message.MeetingUpdate.MeetingState.IsHandRaised)
                {
                    isHandRaised = message.MeetingUpdate.MeetingState.IsHandRaised;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsHandRaised)));
                }

                if (isInMeeting != message.MeetingUpdate.MeetingState.IsInMeeting)
                {
                    isInMeeting = message.MeetingUpdate.MeetingState.IsInMeeting;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsInMeeting)));
                }

                if (isRecordingOn != message.MeetingUpdate.MeetingState.IsRecordingOn)
                {
                    isRecordingOn = message.MeetingUpdate.MeetingState.IsRecordingOn;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRecordingOn)));
                }

                if (isBackgroundBlurred != message.MeetingUpdate.MeetingState.IsBackgroundBlurred)
                {
                    isBackgroundBlurred = message.MeetingUpdate.MeetingState.IsBackgroundBlurred;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBackgroundBlurred)));
                }

                if (CanToggleMute != message.MeetingUpdate.MeetingPermissions.CanToggleMute)
                {
                    CanToggleMute = message.MeetingUpdate.MeetingPermissions.CanToggleMute;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanToggleMute)));
                }

                if (CanToggleShareTray != message.MeetingUpdate.MeetingPermissions.CanToggleShareTray)
                {
                    CanToggleShareTray = message.MeetingUpdate.MeetingPermissions.CanToggleShareTray;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanToggleShareTray)));
                }

                if (CanToggleVideo != message.MeetingUpdate.MeetingPermissions.CanToggleVideo)
                {
                    CanToggleVideo = message.MeetingUpdate.MeetingPermissions.CanToggleVideo;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanToggleVideo)));
                }

                if (CanToggleHand != message.MeetingUpdate.MeetingPermissions.CanToggleHand)
                {
                    CanToggleHand = message.MeetingUpdate.MeetingPermissions.CanToggleHand;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanToggleHand)));
                }

                if (CanToggleBlur != message.MeetingUpdate.MeetingPermissions.CanToggleBlur)
                {
                    CanToggleBlur = message.MeetingUpdate.MeetingPermissions.CanToggleBlur;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanToggleBlur)));
                }

                if (CanToggleChat != message.MeetingUpdate.MeetingPermissions.CanToggleChat)
                {
                    CanToggleChat = message.MeetingUpdate.MeetingPermissions.CanToggleChat;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanToggleChat)));
                }

                if (CanStopSharing != message.MeetingUpdate.MeetingPermissions.CanStopSharing)
                {
                    CanStopSharing = message.MeetingUpdate.MeetingPermissions.CanStopSharing;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanStopSharing)));
                }

                if (CanLeave != message.MeetingUpdate.MeetingPermissions.CanLeave)
                {
                    CanLeave = message.MeetingUpdate.MeetingPermissions.CanLeave;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanLeave)));
                }

                if (CanPair != message.MeetingUpdate.MeetingPermissions.CanPair)
                {
                    CanPair = message.MeetingUpdate.MeetingPermissions.CanPair;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanPair)));
                }

                if (CanReact != message.MeetingUpdate.MeetingPermissions.CanReact)
                {
                    CanReact = message.MeetingUpdate.MeetingPermissions.CanReact;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanReact)));
                }
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            receiveSemaphore.Release();

            if (ws is not null && ws.State != WebSocketState.Open && ws.State != WebSocketState.Connecting)
                _ = Reconnect(true, cancellationToken);
        }
    }



    private async Task SendCommand(string action, string? type = null, CancellationToken cancellationToken = default)
    {
        if (ws is null)
        {
            await Connect(true, cancellationToken);
        }

        switch (ws!.State)
        {
            case WebSocketState.Aborted:
            case WebSocketState.CloseReceived:
                await Connect(true, cancellationToken);
                break;
            case WebSocketState.Connecting:
                await WaitOpen(cancellationToken);
                break;
            case WebSocketState.CloseSent:
            case WebSocketState.Closed:
                throw new InvalidOperationException("Not Connected");
            case WebSocketState.Open:
            case WebSocketState.None:
                break;
        }

        var message = JsonSerializer.Serialize<ClientMessage>(new ClientMessage()
        {
            Action = action,
            Parameters = type is null ? null : new ClientMessageParameter
            {
                Type = type
            }
        }, jsonSerializerOptions);

        try
        {
            await sendSemaphore.WaitAsync(cancellationToken);

            await ws.SendAsync(Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text, true, cancellationToken);
        }
        finally
        {
            sendSemaphore.Release();
        }
    }

    public async Task LeaveCall(CancellationToken cancellationToken = default)
    {
        await SendCommand(MeetingAction.LeaveCall, null, cancellationToken);
    }


    public async Task ReactApplause(CancellationToken cancellationToken = default)
        => await SendCommand(MeetingAction.React, ClientMessageParameterType.ReactApplause, cancellationToken);

    public async Task ReactLaugh(CancellationToken cancellationToken = default)
        => await SendCommand(MeetingAction.React, ClientMessageParameterType.ReactLaugh, cancellationToken);

    public async Task ReactLike(CancellationToken cancellationToken = default)
        => await SendCommand(MeetingAction.React, ClientMessageParameterType.ReactLike, cancellationToken);

    public async Task ReactLove(CancellationToken cancellationToken = default)
        => await SendCommand(MeetingAction.React, ClientMessageParameterType.ReactLove, cancellationToken);

    public async Task ReactWow(CancellationToken cancellationToken = default)
        => await SendCommand(MeetingAction.React, ClientMessageParameterType.ReactWow, cancellationToken);

    public async Task UpdateState(CancellationToken cancellationToken = default)
        => await SendCommand(MeetingAction.QueryMeetingState, null, cancellationToken);

    public async Task ToggleChat(CancellationToken cancellationToken = default)
        => await SendCommand(MeetingAction.ToggleUI, ClientMessageParameterType.ToggleUiChat, cancellationToken);
}

