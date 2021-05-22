using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Global
{
    public static Color[] colors = {
        Color.red, Color.green,
        Color.blue, Color.cyan,
        Color.magenta, Color.yellow,
        Color.gray, Color.black,
        new Color(0.75f, 0.5f, 0.75f), new Color(0.5f, 0.75f, 0.5f)
    };

    public enum GameStatus
    {
        NONE,
        CONNECT,
        LOBBY,
        WAITTING,
        INGAME,
        DAY,
        NIGHT,
        VOTE,
        VOTE_RESULT,
        DEFENSE,
        FINAL_VOTE,
        END
    }
}