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
    private Dictionary<int, VoicePlayback> pbDict = new Dictionary<int, VoicePlayback>();

    public string LocalPid => net.PlayerName;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        comms = GetComponent<DissonanceComms>();
        net = GetComponent<PhotonCommsNetwork>();
    }
    private void Update()
    {
        foreach (var pb in pbDict.Values)
            pb.AudioSource.spatialBlend = 1f;
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
    public void InitPlayBack(int id, string pid, GameObject remotePlayer)
    {
        var pb = comms.FindPlayer(pid).Playback as VoicePlayback;
        pb.transform.parent = remotePlayer.transform;
        pb.transform.localPosition = Vector3.zero;
        pb.AudioSource.minDistance = 10f;
        pb.AudioSource.maxDistance = 30f;
        pbDict[id] = pb;
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
