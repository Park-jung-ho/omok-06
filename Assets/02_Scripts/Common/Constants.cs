
public static class Constants
{
    public const string ServerUrl = "http://localhost:3000";
    public const string SocketServerURL = "http://localhost:3000";

    public enum MultiplayControllerState { CreateRoom, JoinRoom, StartGame, EndGame, ExitRoom }

    public enum ResponseType
    {
        INVALID_EMAIL = 0,
        INVALID_PASSWORD = 1,
        SUCCESS = 2
    }

    
    public enum PlayerType { None, PlayerA, PlayerB }
    public enum GameType { SinglePlay, DualPlay, MultiPlay }

    
    
   

    public const int BlockColumnCount = 14;
}