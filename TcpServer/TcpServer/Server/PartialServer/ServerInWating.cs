using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public partial class GameServer
    {
        private void EnrollWatingHandler(User user)
        {
            user.On(PacketType.LEAVE_ROOM_REQ, OnLeaveRoomReq);
            user.On(PacketType.GAME_START_REQ, OnGameStartReq);
        }
        #region waiting handler
        private void OnLeaveRoomReq(MySocket socket, Packet packet)
        {
            var user = socket as User;
            var room = user.Room;
            if (room.Leave(user))
                Console.WriteLine($"leave : {user.Id}");
        }
        private void OnGameStartReq(MySocket socket, Packet packet)
        {
            var user = socket as User;
            var room = user.Room;
            room.GameStart(user);
            Console.WriteLine($"start : {user.Id}");
        }
        #endregion
    }
}
