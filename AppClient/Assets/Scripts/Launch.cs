using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Grpc.Core;
using RTC.ProtoGrpc.Data;
using RTC.XNet;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
//using Logger = RTC.XNet.;

public class Launch : XSingleton<Launch>
{
    private  void WriteLog(DebuggerLog log)
    {
        switch (log.Type)
        {
            case LoggerType.Debug:
            case LoggerType.Waring:
                Debug.LogWarning($"{log}");
                break;
            case LoggerType.Error:
                Debug.LogError(log);
                break;
            default:
                Debug.Log(log);
                break;
        }
    }

    [Serializable]
    public class Config
    {
        public RTCIceServer[] iceServers;
        public string login;
        public string signal;
        public int pWidth ;
        public int pHeight;
    }

    public bool autoLogin;

    // Start is called before the first frame update
    private async void Start()
    {

        var text = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "config.json"), Encoding.UTF8);
        
        config = JsonUtility.FromJson<Config>(text);
        await GoToLoginAsync();
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            await Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
            await Application.RequestUserAuthorization(UserAuthorization.Microphone);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void Awake()
    {
        base.Awake();
        Debugger.Logger = WriteLog;
    }

    public TokenSession tokenSession;

    public string roomId;

    public Config config;

    public bool useFrontCamera = true;
    public bool useSoft = true;

    public async Task GoToLoginAsync(bool auto = false)
    {
        autoLogin = auto;
        await GrpcEnvironment.ShutdownChannelsAsync();
        
        await SceneManager.LoadSceneAsync("Login");
    }

    public async Task GoToRtcAsync(string room)
    {
        this.roomId = room;
        await SceneManager.LoadSceneAsync("RtcClient");
    }
}
