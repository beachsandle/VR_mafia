using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PhotonRoom = Photon.Realtime.Room;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public static class PlayerExtender
{
    public static bool GetAlive(this Player p)
    {
        return p.CustomProperties.ContainsKey("Alive") && (bool)p.CustomProperties["Alive"];
    }
    public static void Die(this Player p)
    {
        p.SetCustomProperties(new Hashtable() { { "Alive", false } });
    }
    public static string GetVoiceName(this Player p)
    {
        return p.CustomProperties.ContainsKey("VoiceName") ? (string)p.CustomProperties["VoiceName"] : "";
    }
}

public static class RoomExtenter
{
    public static string GetHostName(this PhotonRoom r)
    {
        return r.CustomProperties.ContainsKey("HostName") ? (string)r.CustomProperties["HostName"] : "";
    }
    public static string GetHostName(this RoomInfo r)
    {
        return r.CustomProperties.ContainsKey("HostName") ? (string)r.CustomProperties["HostName"] : "";
    }
    public static void SetHostName(this PhotonRoom r, string hostName)
    {
        r.SetCustomProperties(new Hashtable() { { "HostName", hostName } });
    }
}
