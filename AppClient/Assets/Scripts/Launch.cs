using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using RTC.ProtoGrpc.Data;
using RTC.XNet;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = RTC.XNet.Logger;

public class Launch : XSingleton<Launch>
{
    private class  UnityLogger  : Logger
    {
        public override void WriteLog(DebuggerLog log)
        {
            Debug.Log($"{log.Type}:{log}");
        }
    }
    
    [Serializable]
    public class Config
    {
        public string stun;
        public string login;
        public string signal;
        public string[] accounts;
        public int pWidth ;
        public int pHeight;
    }
    // Start is called before the first frame update
    private async void Start()
    {
        config = JsonUtility.FromJson<Config>(Resources.Load<TextAsset>("config").text);
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
        Debugger.Logger = new UnityLogger();
    }

    public TokenSession tokenSession;

    public Config config;

    public bool useFrontCamera = true;
    public bool useSoft = true;

    public async Task GoToLoginAsync()
    {
        await SceneManager.LoadSceneAsync("Login"); 
        
    }

    public async Task GoToRTCAsync()
    {
        await SceneManager.LoadSceneAsync("RtcClient");
    }
}
