using Dissonance;
using Dissonance.Audio.Playback;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceChatManager : MonoBehaviour
{
    private DissonanceComms comms;
    public string LocalVoiceName => comms.LocalPlayerName;
    private readonly Dictionary<string, PlayerCharacter> playerMap = new Dictionary<string, PlayerCharacter>();
    private void Awake()
    {
        comms = GetComponent<DissonanceComms>();
        comms.OnPlayerJoinedSession += OnPlayerJoinedSession;
        comms.IsMuted = true;
    }

    private void OnPlayerJoinedSession(VoicePlayerState obj)
    {
        if (playerMap.ContainsKey(obj.Name))
            InitPlayback(obj.Name);
    }
    private IEnumerator SetSpatialSound(VoicePlayback playback)
    {
        while (playback.AudioSource.spatialBlend != 0)
            yield return null;
        playback.AudioSource.spatialBlend = 1f;
    }
    private void InitPlayback(string voiceName)
    {
        Debug.Log("Init Player " + voiceName);
        var player = comms.FindPlayer(voiceName);
        var playback = player.Playback as VoicePlayback;
        playback.transform.parent = playerMap[voiceName].transform;
        playback.transform.localPosition = Vector3.zero;
        StartCoroutine(SetSpatialSound(playback));
    }
    public void MappingPlayer(string voiceName, PlayerCharacter character)
    {
        playerMap[voiceName] = character;
        if (comms.FindPlayer(voiceName) != null)
            InitPlayback(voiceName);
    }
    public void SetMute(bool muted)
    {
        comms.IsMuted = muted;
    }
}
