using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        await DoLogin(account.text, pwd.text);
    }

    private async Task DoLogin(string userName, string password)
    {
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
            await Launch.S.GoToRTCAsync();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            if (btLogin.gameObject == null) return;
            btLogin.gameObject.SetActive(true);
            loginText.gameObject.SetActive(false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        loginText.gameObject.SetActive(false);
    }

    public async void ClickAccount1()
    {
        await DoLogin(Launch.S.config.accounts[0] , Launch.S.config.accounts[1]);
    }
    
    public async void ClickAccount2()
    {
        await DoLogin(Launch.S.config.accounts[2] , Launch.S.config.accounts[3]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
