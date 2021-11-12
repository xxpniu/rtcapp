using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using RTC.ProtoGrpc.LoginServer;
using RTC.XNet;
using UnityEngine;
using UnityEngine.TestTools;

public class CreateAccountTest
{
    [Serializable]
    public class Config
    {
        public string stun;
        public string login;
        public string signal;
    }

    [Test]
    [TestCase("andrew", "123456", Explicit = false)]
    [TestCase("jy", "123456", Explicit = false)]
    public async Task CreateAccountTestWithEnumeratorPasses(string account, string pwd)
    {
        /*
        var  config = JsonUtility.FromJson<Config>(Resources.Load<TextAsset>("config").text);
        var cfg = config.login.Split(':');
        var loginAddress = "localhost";//cfg[0];
        var loginPort = int.Parse(cfg[1]);

        var login = new LogChannel(loginAddress, loginPort);
        var client =  login.CreateClient<LoginServerService.LoginServerServiceClient>();
        var result = await client.RegisterAsync(new C2L_RegisterAccount
        {
            Email = "xsoft@xsoft.com",
            Nickname = "xsoft",
            Password = pwd,
            UserName = account
        });
        //yield return UniTask.ToCoroutine(result);
        Debug.Log(result);*/
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        //yield return null;
        await Task.CompletedTask;
    }
}
