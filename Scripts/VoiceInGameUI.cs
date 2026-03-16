using Unity.Netcode;
using UnityEngine;
using Unity.Services.Multiplayer;
using UnityEngine.SceneManagement;
using Unity.Services.Vivox;
using UnityEngine.UI;
public class VoiceInGameUI : MonoBehaviour
{
    public Button Mic;
    public Button MuteMic;
    public Button Speaker;
    public Button MuteSpeaker;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        Mic.gameObject.SetActive(false);
        Speaker.gameObject.SetActive(false);
        MuteMic.gameObject.SetActive(true);
        MuteSpeaker.gameObject.SetActive(true);
        //OnMuteMic();
        //OnMuteSpeaker();
        OnUnmuteMic();
        OnUnMuteSpeaker();
    }

    public void OnMuteMic()
    {
        VivoxService.Instance.MuteInputDevice();
        Mic.gameObject.SetActive(false);
        MuteMic.gameObject.SetActive(true);
        Debug.Log("Mic Muted");
    }

    public void OnUnmuteMic()
    {
        VivoxService.Instance.UnmuteInputDevice();
        Mic.gameObject.SetActive(true);
        MuteMic.gameObject.SetActive(false);
        Debug.Log("Unmute Mic");
    }

    public void OnMuteSpeaker()
    {
        VivoxService.Instance.MuteOutputDevice();
        MuteSpeaker.gameObject.SetActive(true);
        Speaker.gameObject.SetActive(false);
        Debug.Log("Speaker Muted");
    }

    public void OnUnMuteSpeaker()
    {
        VivoxService.Instance.UnmuteOutputDevice();
        MuteSpeaker.gameObject.SetActive(false);
        Speaker.gameObject.SetActive(true);
        Debug.Log("Unmute Speaker");
    }
}