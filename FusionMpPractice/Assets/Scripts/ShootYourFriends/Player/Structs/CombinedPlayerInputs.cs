using Fusion;

public class CombinedPlayerInputs : INetworkInput
{
    public PlayerNetworkData PlayerA;
    public PlayerNetworkData PlayerB;
    public PlayerNetworkData PlayerC;
    public PlayerNetworkData PlayerD;

    public PlayerNetworkData this[int i]
    {
        get
        {
            switch (i)
            {
                case 0:
                    return PlayerA;
                case 1:
                    return PlayerB;
                case 2:
                    return PlayerC;
                case 3:
                    return PlayerD;
                default:
                    return default;
            }
        }
        set
        {
            switch (i)
            {
                case 0:
                    PlayerA = value;
                    return;
                case 1:
                    PlayerA = value;
                    return;
                case 2:
                    PlayerA = value;
                    return;
                case 3:
                    PlayerA = value;
                    return;
                default:
                    return;
            }
        }
    }
}