using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Grpc.Core;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.UI;
using XRTC;
using Debug = UnityEngine.Debug;
using Debugger=  RTC.XNet.Debugger;

public class UIRTCClient : MonoBehaviour
{
    public Transform root;

    public GameObject itemTemp ;

    public List<GameObject> players;

    public RawImage bgRawImage;

    private GameObject CreateView(string pName , Texture texture,string account)
    {
        var go = Instantiate(itemTemp, Vector3.zero, Quaternion.identity ,  root);
        go.SetActive(true);
        var text = go.transform.Find("Text").GetComponent<Text>();
        
        text.text = pName;
        var rawImage = go.transform.Find("Button/RawImage").GetComponent<RawImage>();
        rawImage.texture = texture;
        players.Add(go);
        go.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
        {
            Debug.Log(account);
            if (string.IsNullOrEmpty(account))
            {
                SetBgRawImage(clientConnection.WebCam);
                return;
            }

            if (clientConnection.Clients.TryGetValue(account, out var client))
            {
                SetBgRawImage( client.PeerTexture);
            }
        });
        return go;
    }

    private RTCClientConnection clientConnection;


    private void Awake()
    {
        WebRTC.Initialize(Launch.S.useSoft ? EncoderType.Software : EncoderType.Hardware);
        Screen.sleepTimeout = SleepTimeout.NeverSleep; // Int32.MaxValue;
    }
    private void OnDestroy()
    {
        WebRTC.Dispose();
        clientConnection?.Dispose();
        
    }

    public async void RefreshView()
    {
        foreach (var go in players)
        {
            Destroy(go);
        }
        players.Clear();

        CreateView("自己", clientConnection.WebCam, "");
        
        foreach (var kv in clientConnection.Clients)
        {
           var go =  CreateView(kv.Value.Name, kv.Value.PeerTexture, kv.Key);
           if (!kv.Value.Clip) continue;
           var clipSource = go.AddComponent<AudioSource>();
           clipSource.volume = 1;
           clipSource.maxDistance = 999;
           clipSource.minDistance = 0.01f;
           clipSource.clip = kv.Value.Clip;
           clipSource.loop = true;
           clipSource.Play();
        }

        await Task.CompletedTask;
    }

    public async void MuteMicrophone()
    {
        await Task.CompletedTask; 
    }

    public async void ChangeCamera()
    {
        clientConnection.ChangeCamera();
        await Task.CompletedTask;
    }

    private void SetBgRawImage(Texture texture)
    {
        bgRawImage.texture = texture;
        bgRawImage.SetNativeSize();
        var parentRect = bgRawImage.transform.parent.GetComponent<RectTransform>();
        var parentSize = parentRect.rect;
        var rect = bgRawImage.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.anchorMax = rect.anchorMin= new Vector2(0.5f, 0.5f);
        var p =  parentSize.width/rect.rect.width ;
        rect.sizeDelta = new Vector2(parentRect.rect.width , p* rect.sizeDelta.y);
    }

    // Start is called before the first frame update
    private async void Start()
    {
        itemTemp.SetActive(false);
        StartCoroutine(WebRTC.Update());
        players = new List<GameObject>();
        clientConnection = new RTCClientConnection(Launch.S.tokenSession,
            Launch.S.roomId,
            Launch.S.config.signal,
            Launch.S.config.iceServers,
            new Vector2Int(Launch.S.config.pWidth, Launch.S.config.pHeight));

        clientConnection.OnPeerChanged.SubscribeForever(des => { this.RefreshView(); });

        async void ProcessOffline(string d)
        {
            Debugger.Log("Offline retry");
            await Launch.S.GoToLoginAsync(true);
        }

        clientConnection.OnOffline.SubscribeOnce(ProcessOffline);
        
        try
        {
            await clientConnection.ConnectAsync(Launch.S.useFrontCamera, this.GetCancellationTokenOnDestroy());

            SetBgRawImage(clientConnection.WebCam);

            RefreshView();
        }
        catch(Exception ex)
        {
            Debug.LogException(ex);
            await Launch.S.GoToLoginAsync();
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
}
