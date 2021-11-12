using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using RTC.ProtoGrpc.Data;
using RTC.ProtoGrpc.LoginServer;
using RTC.XNet;
using UnityEngine;
using UnityEngine.UI;

public class UILogin : MonoBehaviour
{
    public InputField account;

    public InputField pwd;
    public Button btLogin;
    public Toggle frontCamera;
    public Toggle software;
    public Text loginText;

    public async void Login()
    {
        Launch.S.autoLogin = false;
        var id = SystemInfo.deviceUniqueIdentifier;
        var password = Md5Tool.GetMd5Hash(id);

        await DoLogin(id, password);
    }

    private async Task DoLogin(string userName, string password)
    {
        
        if(string.IsNullOrEmpty(account.text)) return;
        
        
        btLogin.gameObject.SetActive(false);
        loginText.gameObject.SetActive(true);
        Launch.S.useSoft = software.isOn;
        Launch.S.useFrontCamera = frontCamera.isOn;
        try
        {
            var accountVal = userName;// account.text;
            var pwdVal = password;
            if (string.IsNullOrEmpty(accountVal) || string.IsNullOrEmpty(pwdVal))
            {
                Debug.LogError("Empty Log error");
                throw new Exception("Empty");
            }

            var cfg = Launch.S.config.login.Split(':');
            var loginAddress = cfg[0];
            var loginPort = int.Parse(cfg[1]);

            var login = new LogChannel(loginAddress, loginPort);
            var client = await login.CreateClientAsync<LoginServerService.LoginServerServiceClient>();
            var result = await client.LoginAsync(new C2L_Login
            {
                UserName = accountVal,
                Password = pwdVal
            });
            Launch.S.tokenSession = result.Token;
            PlayerPrefs.SetString("__ROOM_ID", account.text);
            await Launch.S.GoToRtcAsync(account.text);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
                    
            btLogin.gameObject.SetActive(true);
            loginText.gameObject.SetActive(false);
        }

    }

    // Start is called before the first frame update
    async void Start()
    {
        loginText.gameObject.SetActive(false);
        account.text = PlayerPrefs.GetString("__ROOM_ID", 
            Md5Tool.GetMd5Hash(DateTime.Now.ToString(CultureInfo.CurrentCulture)));
        
        //auto

        if (Launch.S.autoLogin)
        {

            await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: this.GetCancellationTokenOnDestroy());

            Login();
        }
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
