public static class PlayerState
{
    static private PlayerStateEnum playerState = PlayerStateEnum.WALK;

    static public void setPlayerState(PlayerStateEnum state)
    {
        playerState = state;
    }

    static public PlayerStateEnum getPlayerState()
    {
        return playerState;
    }
}

public enum PlayerStateEnum
{
    SIT,
    WALK
}