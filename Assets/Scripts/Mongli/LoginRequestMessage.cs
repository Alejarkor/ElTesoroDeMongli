using Mirror;

public struct LoginRequestMessage : NetworkMessage
{
    public int user_id;
    public string access_token;
}
