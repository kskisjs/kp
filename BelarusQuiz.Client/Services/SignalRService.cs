using Microsoft.AspNetCore.SignalR.Client;
using BelarusQuiz.Shared.Models;

namespace BelarusQuiz.Client.Services;

public class SignalRService
{
    private HubConnection? _hub;
    public bool IsConnected => _hub?.State == HubConnectionState.Connected;

    // ── События (подписываются ViewModels) ───────────────────────────────────
    public event Action<RoomInfo>?       OnRoomCreated;
    public event Action<RoomInfo>?       OnPlayerJoined;
    public event Action<string>?         OnPlayerLeft;
    public event Action<RoomInfo>?       OnPlayerReadyChanged;
    public event Action<RoomInfo>?       OnGameStarted;
    public event Action<QuestionDto>?    OnQuestionReceived;
    public event Action<int>?            OnTimerTick;
    public event Action<RoundResultDto>? OnRoundResult;
    public event Action<GameResultDto>?  OnGameFinished;
    public event Action<string>?         OnError;

    public async Task ConnectAsync(string serverUrl = "http://localhost:5000")
    {
        if (_hub != null)
        {
            await _hub.DisposeAsync();
        }

        _hub = new HubConnectionBuilder()
            .WithUrl($"{serverUrl}/gamehub")
            .WithAutomaticReconnect()
            .Build();

        _hub.On<RoomInfo>       ("RoomCreated",          r  => OnRoomCreated?.Invoke(r));
        _hub.On<RoomInfo>       (HubEvents.PlayerJoined,  r  => OnPlayerJoined?.Invoke(r));
        _hub.On<string>         (HubEvents.PlayerLeft,    n  => OnPlayerLeft?.Invoke(n));
        _hub.On<RoomInfo>       (HubEvents.PlayerReadyChanged, r => OnPlayerReadyChanged?.Invoke(r));
        _hub.On<RoomInfo>       (HubEvents.GameStarted,   r  => OnGameStarted?.Invoke(r));
        _hub.On<QuestionDto>    (HubEvents.QuestionReceived, q => OnQuestionReceived?.Invoke(q));
        _hub.On<int>            (HubEvents.TimerTick,     t  => OnTimerTick?.Invoke(t));
        _hub.On<RoundResultDto> (HubEvents.RoundResult,   r  => OnRoundResult?.Invoke(r));
        _hub.On<GameResultDto>  (HubEvents.GameFinished,  r  => OnGameFinished?.Invoke(r));
        _hub.On<string>         (HubEvents.Error,         e  => OnError?.Invoke(e));

        await _hub.StartAsync();
    }

    public Task CreateRoom(string nickname, int rounds = 10, int timer = 15)
        => Invoke("CreateRoom", nickname, rounds, timer);

    public Task JoinRoom(string code, string nickname)
        => Invoke("JoinRoom", code, nickname);

    public Task SetReady(bool ready)
        => Invoke("SetReady", ready);

    public Task SubmitAnswer(AnswerDto answer)
        => Invoke("SubmitAnswer", answer);

    public Task LeaveRoom()
        => Invoke("LeaveRoom");

    private async Task Invoke(string method, params object[] args)
    {
        if (_hub == null || _hub.State != HubConnectionState.Connected) return;
        try { await _hub.InvokeCoreAsync(method, args); }
        catch (Exception ex) { OnError?.Invoke(ex.Message); }
    }

    public async Task DisconnectAsync()
    {
        if (_hub != null) await _hub.DisposeAsync();
    }
}
