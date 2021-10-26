using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.UI;
using XRTC;

public class UIRTCClient : MonoBehaviour
{
    public Transform root;

    public GameObject itemTemp ;

    public List<GameObject> players;

    public RawImage bgRawImage;

    private GameObject CreateView(string pName , Texture texture,string account)
    {
        var go = GameObject.Instantiate(itemTemp, Vector3.zero, quaternion.identity ,  root);
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
                SetBgRawImage(_clientConnection.WebCam);
                return;
            }

            if (_clientConnection.Clients.TryGetValue(account, out var client))
            {
                SetBgRawImage( client.PeerTexture);
            }
        });
        return go;
    }

    private RTCClientConnection _clientConnection;


    private void Awake()
    {
        WebRTC.Initialize(Launch.S.useSoft ? EncoderType.Software : EncoderType.Hardware);
        Screen.sleepTimeout = SleepTimeout.NeverSleep; // Int32.MaxValue;
    }
    private void OnDestroy()
    {
        WebRTC.Dispose();
        _clientConnection?.Dispose();
        
    }

    public async void RefreshView()
    {
        foreach (var go in players)
        {
            Destroy(go);
        }
        players.Clear();

        CreateView("自己", _clientConnection.WebCam, "");
        
        foreach (var kv in _clientConnection.Clients)
        {
           var go =  CreateView(kv.Value.Name, kv.Value.PeerTexture, kv.Key);
           if (!kv.Value.Clip) continue;
           var clipSource = go.AddComponent<AudioSource>();
           clipSource.clip = kv.Value.Clip;
           clipSource.loop = true;
           clipSource.Play();
        }
    }

    public async void MuteMicrophone()
    {
         
    }

    public async void ChangeCamera()
    {
        _clientConnection.ChangeCamera();
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
    private  async  void Start()
    {
        itemTemp.SetActive(false);
        StartCoroutine(WebRTC.Update());
        players = new List<GameObject>();
        _clientConnection = new RTCClientConnection(Launch.S.tokenSession,
            Launch.S.config.signal,
            Launch.S.config.stun, new Vector2Int(Launch.S.config.pWidth, Launch.S.config.pHeight));
        
        _clientConnection.OnPeerChanged.SubscribeForever(des =>
        {
            this.RefreshView();
            
        });

        async void ProcessOffline(string d)
        {
            await Launch.S.GoToLoginAsync();
        }

        _clientConnection.OnOffline.SubscribeOnce(ProcessOffline);
        
        await _clientConnection.ConnectAsync( Launch.S.useFrontCamera,this.GetCancellationTokenOnDestroy());
        SetBgRawImage(_clientConnection.WebCam);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
}
