using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPacket
{
    public partial class GameServer
    {
        private void EnrollLobbyHandler(User user)
        {
            user.On(PacketType.SET_NAME_REQ, OnSetNameReq);
            user.On(PacketType.ROOM_LIST_REQ, OnRoomListReq);
            user.On(PacketType.CREATE_ROOM_REQ, OnCreateRoomReq);
            user.On(PacketType.JOIN_ROOM_REQ, OnJoinRoomReq);
        }
        //이름 변경을 요청하면 이름을 변경하고 결과를 전송
        private void OnSetNameReq(MySocket socket, Packet packet)
        {
            var user = socket as User;
            var data = new SetNameReqData(packet.Bytes);
            if (user.SetName(data.UserName))
                Console.WriteLine($"setname : {user.Id} {user.Name}");
        }
        //방 목록을 요청하면 방 목록들을 전송
        private void OnRoomListReq(MySocket socket, Packet packet)
        {
            var user = socket as User;
            if (user.SendRoomList(GetRoomInfos()))
                Console.WriteLine($"room list req : {user.Id}");
        }
        //방 생성을 요청하면 방을 생성하고 결과를 전송
        private void OnCreateRoomReq(MySocket socket, Packet packet)
        {
            var user = socket as User;
            var data = new CreateRoomReqData(packet.Bytes);
            if (user.CreateRoom(data.RoomName))
                Console.WriteLine($"create room req : {user.Id}");
        }
        // 방 입장을 요청하면 입장을 처리하고 결과를 전송
        private void OnJoinRoomReq(MySocket socket, Packet packet)
        {
            var user = socket as User;
            var data = new JoinRoomReqData(packet.Bytes);
            var room = roomMap.ContainsKey(data.RoomId) ? roomMap[data.RoomId] : null;
            if (user.JoinRoom(room))
                Console.WriteLine($"join room : {user.Id}");
        }
    }
}
