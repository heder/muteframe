namespace MuteFrame.TeamsApi;
public class EventArgs(string errorMessage) : System.EventArgs
{
    public string ErrorMessage { get; } = errorMessage;
}

public class SuccessReceivedEventArgs(int requestId) : System.EventArgs
{
    public int RequestId { get; } = requestId;
}

public class TokenReceivedEventArgs(string token) : System.EventArgs
{
    public string Token { get; } = token;
}
