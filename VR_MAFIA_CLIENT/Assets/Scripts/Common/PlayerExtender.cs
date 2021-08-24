using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class PlayerExtender
{
    public static bool Alive(this Player p)
    {
        return p.CustomProperties.ContainsKey("Alive") && (bool)p.CustomProperties["Alive"];
    }
}
