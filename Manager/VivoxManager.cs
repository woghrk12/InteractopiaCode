using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Android;
using Photon.Pun;
using VivoxUnity;
using DG.Tweening;

public class Vivox
{
    #region Variables

    public Client Client = null;

    public ILoginSession LoginSession = null;
    public IChannelSession ChannelSession = null;

    #endregion Variables

    #region Properties

    public Uri Server { private set; get; } = new("https://unity.vivox.com/appconfig/14568-inter-97851-udash");
    public string Issuer { private set; get; } = "14568-inter-97851-udash";
    public string Domain { private set; get; } = "mtu1xp.vivox.com";
    public string TokenKey { private set; get; } = "M0taBjRwJ0ckGTTBXxAJvKVSehAjeEV0";

    public TimeSpan TimeSpan { private set; get; } = TimeSpan.FromSeconds(90);

    #endregion Properties
}

public class VivoxManager
{
    private readonly int MAX_DISTANCE = 7;

    #region Variables

    private Vivox vivox = new();

    private bool isMute = true;

    public Action<string, string> ReceiveMessageEvent = null;

    #endregion Variables

    #region Properties

    public string LoginStatus => "Login : " + (vivox.LoginSession != null ? vivox.LoginSession.State.ToString() : "Null");
    public string ChannelStatus => "Voice : " + (vivox.ChannelSession != null ? vivox.ChannelSession.ChannelState.ToString() + " " + vivox.ChannelSession.Channel.Name : "Null");
    
    public bool IsConnected => vivox.ChannelSession != null && vivox.ChannelSession.ChannelState == ConnectionState.Connected;

    public bool IsMute => isMute;
    public bool IsVivoxMute => vivox.Client != null ? vivox.Client.AudioInputDevices.Muted : false;

    #endregion Properties

    #region Methods

    public void Init()
    {
        vivox.Client = new();
        vivox.Client.Uninitialize();
        vivox.Client.Initialize();

        SetLocalMute();

        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
    }

    public void Clear()
    {
        vivox.Client.Uninitialize();
    }

    public void Login(string userId)
    {
        if (vivox.LoginSession != null && vivox.LoginSession.State == LoginState.LoggedIn) return;

        AccountId accountId = new(vivox.Issuer, userId, vivox.Domain);

        vivox.LoginSession = vivox.Client.GetLoginSession(accountId);
        
        vivox.LoginSession.BeginLogin(vivox.Server, vivox.LoginSession.GetLoginToken(vivox.TokenKey, vivox.TimeSpan),
            callback =>
            {
                try
                {
                    vivox.LoginSession.EndLogin(callback);
                    GameManager.SetLoadingPanel(false);
                    GameManager.UI.FadeIn(GlobalDefine.fadeEffectDuration)
                        .Play();
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            });
    }

    public void JoinChannel()
    {
        Channel3DProperties properties = new(MAX_DISTANCE, 1, 1f, AudioFadeModel.LinearByDistance);
        ChannelId channelId = new(vivox.Issuer, PhotonNetwork.CurrentRoom.Name, vivox.Domain, ChannelType.Positional, properties);

        vivox.ChannelSession = vivox.LoginSession.GetChannelSession(channelId);
        vivox.ChannelSession.MessageLog.AfterItemAdded += OnReceivedMessage;

        vivox.ChannelSession.PropertyChanged += OnChannelStatusChanged;

        vivox.ChannelSession.BeginConnect(true, true, true, vivox.ChannelSession.GetConnectToken(vivox.TokenKey, vivox.TimeSpan),
            callback => 
            {
                try
                {
                    vivox.ChannelSession.EndConnect(callback);
                }
                catch (Exception e)
                {
                    vivox.ChannelSession.PropertyChanged -= OnChannelStatusChanged;
                    Debug.Log(e);
                }
            });
    }

    public void LeaveChannel()
    {
        vivox.ChannelSession.Disconnect(callback =>
            {
                vivox.ChannelSession.PropertyChanged -= OnChannelStatusChanged;
                vivox.ChannelSession = null;
            });
    }

    public void SetMuteRemote(string userId)
    {
        if (vivox.ChannelSession == null) return;
        if (vivox.ChannelSession.ChannelState != ConnectionState.Connected) return;

        vivox.ChannelSession.Participants[GetUserIp(userId)].LocalMute = true;
    }

    public void SetUnmuteRemote(string userId)
    {
        if (vivox.ChannelSession == null) return;
        if (vivox.ChannelSession.ChannelState != ConnectionState.Connected) return;

        vivox.ChannelSession.Participants[GetUserIp(userId)].LocalMute = false;
    }

    public void BlockVoice()
    {
        vivox.Client.AudioInputDevices.Muted = true;
    }

    public void ReleaseVoice()
    {
        vivox.Client.AudioInputDevices.Muted = isMute;
    }

    public void SetLocalMute()
    {
        vivox.Client.AudioInputDevices.Muted = true;
        isMute = true;
    }

    public void SetLocalUnmute()
    {
        vivox.Client.AudioInputDevices.Muted = false;
        isMute = false;
    }

    public void SendMessage(string message)
    {
        vivox.ChannelSession.BeginSendText(message,
            callback =>
            {
                try
                {
                    vivox.ChannelSession.EndSendText(callback);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            });
    }

    public void SetPosition(Transform characterTransform = null)
    {
        if (vivox.ChannelSession == null) return;
        if (vivox.ChannelSession.ChannelState != ConnectionState.Connected) return;

        if (characterTransform != null)
        {
            vivox.ChannelSession.Set3DPosition(characterTransform.position, characterTransform.position, characterTransform.forward, characterTransform.up);
        }
        else
        {
            vivox.ChannelSession.Set3DPosition(Vector3.zero, Vector3.zero, Vector3.forward, Vector3.up);
        }
    }

    #endregion Methods

    #region Helper Methods

    private string GetUserIp(string userId)
    {
        return $"sip:.{vivox.Issuer}.{userId}.@{vivox.Domain}";
    }

    #endregion Helper Methods

    #region Event Methods

    private void OnChannelStatusChanged(object sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName.Equals("ChannelState"))
        {
            if (vivox.ChannelSession.ChannelState == ConnectionState.Connected)
            {
                SetPosition();
            }
        }
    }

    private void OnReceivedMessage(object sender, QueueItemAddedEventArgs<IChannelTextMessage> messageItem)
    {
        string name = messageItem.Value.Sender.Name;
        string message = messageItem.Value.Message;

        ReceiveMessageEvent?.Invoke(name, message);
    }

    #endregion Event Methods
}
