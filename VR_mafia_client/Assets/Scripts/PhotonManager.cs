using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Dissonance;
using Dissonance.Audio.Playback;
using Dissonance.Integrations.PhotonUnityNetworking2;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    #region singleton
    private static PhotonManager instance;
    public static PhotonManager Instance
    {
        get
        {
            if (instance == null)
                instance = GameObject.FindObjectOfType<PhotonManager>();
            return instance;
        }
    }
    #endregion
    [HideInInspector] public Action OnConnect;
    [HideInInspector] public Action OnJoined;
    [HideInInspector] public Action OnLeft;
    [SerializeField] private GameObject TriggerPrefab;
    private DissonanceComms comms;
    private PhotonCommsNetwork net;

    public string LocalPid => net.PlayerName;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        comms = GetComponent<DissonanceComms>();
        net = GetComponent<PhotonCommsNetwork>();
    }
    public void Connect(int userId)
    {
        try
        {
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.GameVersion = "1.0";
            PhotonNetwork.NickName = userId.ToString();
            PhotonNetwork.ConnectUsingSettings();
        }
        catch { }
    }
    public void JoinRoom(int roomId)
    {
        try
        {
            PhotonNetwork.JoinOrCreateRoom(roomId.ToString(), null, null);
        }
        catch { }
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    public VoicePlayerState FindPlayer(string pid)
    {
        return comms.FindPlayer(pid);
    }
    public void InitTrigger(GameObject localPlayer)
    {
        Instantiate(TriggerPrefab, localPlayer.transform);
    }
    async public void InitPlayBack(string pid, GameObject remotePlayer)
    {
        var pb = comms.FindPlayer(pid).Playback as VoicePlayback;
        var audio = pb.GetComponent<AudioSource>();
        pb.transform.parent = remotePlayer.transform;
        pb.transform.localPosition = Vector3.zero;
        audio.spatialize = true;
        audio.spatialBlend = 1f;
        await Task.Delay(1000);
        //spatialBlend가 계속 초기화됨
        //audio.minDistance = 1;
    }

    public void Mute(bool enable)
    {
        comms.IsMuted = enable;
    }
    public override void OnConnected()
    {
        Debug.Log("[Photon Manager] : connected");
        OnConnect?.Invoke();
    }
    public override void OnJoinedRoom()
    {
        Debug.Log("[Photon Manager] : Join room success");
        OnJoined?.Invoke();
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("[Photon Manager] : Join Room fail");
        Application.Quit();
    }
    public override void OnLeftRoom()
    {
        Debug.Log("[Photon Manager] : Leave Room");
        OnLeft?.Invoke();
    }
}
