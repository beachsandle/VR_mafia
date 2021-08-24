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
    public static bool Alive(this Player p)
    {
        return p.CustomProperties.ContainsKey("Alive") && (bool)p.CustomProperties["Alive"];
    }
    public static void Die(this Player p)
    {
        p.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "Alive", false } });
    }
}

public static class RoomExtenter
{
    public static string HostName(this PhotonRoom r)
    {
        return r.CustomProperties.ContainsKey("HostName") ? (string)r.CustomProperties["HostName"] : "";
    }
    public static string HostName(this RoomInfo r)
    {
        return r.CustomProperties.ContainsKey("HostName") ? (string)r.CustomProperties["HostName"] : "";
    }
    public static void SetHostName(this PhotonRoom r, string hostName)
    {
        r.SetCustomProperties(new Hashtable() { { "HostName", hostName } });
    }
}
