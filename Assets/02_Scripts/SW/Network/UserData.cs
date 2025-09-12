public class UserData : Singleton<UserData>
{
    public string Nickname { get; private set; }
    public int Rank { get; private set; }

    public void SetUserInfo(string nickname, int rank)
    {
        Nickname = nickname;
        Rank = rank;
    }
}
