
public static class Constants
{
    public const string ServerUrl = "http://101.79.11.181:3000";
    public const string SocketServerURL = "http://101.79.11.181:3000";

    public enum ResponseType
    {
        INVALID_EMAIL = 0,
        INVALID_PASSWORD = 1,
        SUCCESS = 2
    }

    
    // PlayerA = Black(선공), PlayerB = White(후공)
    public enum PlayerType { None, PlayerA, PlayerB }   
    public enum GameType { None, SinglePlay, DualPlay, MultiPlay }

    public const int BlockColumnCount = 15;
}
