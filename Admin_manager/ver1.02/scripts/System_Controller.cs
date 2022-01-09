
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class System_Controller : UdonSharpBehaviour
{
    [UdonSynced] private int totalVisits = 0;

    [SerializeField] private string Admin_name;

    [SerializeField] private Text Count;
    [SerializeField] private Text Total;
    [SerializeField] private Text NowTime;
    [SerializeField] private Text Admin;
    [SerializeField] private Text SelectName;
    [SerializeField] private Text IDs;
    [SerializeField] private Text SelectID;

    [SerializeField] private Data_array[] Names;

    [SerializeField] private int MaxLimit = 20;

    private int localPlayerCount = 0;

    /*===========================================*/
    private VRCPlayerApi Selected_player;

    private bool set = false;
    [UdonSynced] private int num;

    [SerializeField] Button Conclude1;
    [SerializeField] Button Conclude2;
    [SerializeField] Button Conclude3;
    [SerializeField] Button Conclude4;
    [SerializeField] Button Conclude5;
    [SerializeField] Button Conclude6;

    [SerializeField] private AudioClip Select;
    [SerializeField] private AudioClip Cansel;
    [SerializeField] private AudioClip Banish;
    [SerializeField] private AudioClip Mute;

    [SerializeField] private AudioSource System_Audio;

    [SerializeField] private GameObject Position;

    void Start()
    {

    }

    void Update()
    {
        NowTime.text = System.DateTime.Now.ToString("HH:mm:ss");
        Total.text = totalVisits.ToString("0000");

        if (set)
        {
            if (Networking.IsOwner(this.gameObject))
            {
                for (int i = 0; i < Names.Length; i++)
                {
                    if (set)
                    {
                        if (string.IsNullOrEmpty(Names[i].SyncData))
                        {
                            VRCPlayerApi Seted_Player;
                            string Mobile = "";
                            string playerID = "";

                            Seted_Player = VRCPlayerApi.GetPlayerById(totalVisits);

                            if (Seted_Player.IsUserInVR())
                            {
                                Mobile = "VR";
                            }
                            else
                            {
                                Mobile = "PC";
                            }
                            playerID = totalVisits.ToString("000");

                            this.Names[i].SyncData = $"{DateTime.Now:HH:mm:ss} <color=#8BC34A>[{Mobile}]</color> [{playerID}]：{Seted_Player.displayName}";

                            set = false;
                        }
                    }
                }
            }
        }
        else
        {
            set = false;
        }
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (player == Networking.LocalPlayer)
        {
            //admin承認(adminならオーナをadminに設定)
            if (player.displayName == Admin_name)
            {
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);

                Admin.text = Admin_name;

                //本来はadmin承認内に書く
                Conclude1.interactable = true;
                Conclude2.interactable = true;
                Conclude3.interactable = true;
                Conclude4.interactable = true;
                Conclude5.interactable = true;
                Conclude6.interactable = true;
            }
        }

        if (Networking.IsOwner(this.gameObject))
        {
            totalVisits++;
        }

        localPlayerCount++;

        DispPlayerInfo();
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        localPlayerCount--;

        DeletePlayerInfo(player);
    }

    public override void OnPlayerRespawn(VRCPlayerApi player)
    {
        if (player == Selected_player)
        {
            Selected_player.TeleportTo(Position.transform.position, Position.transform.rotation);
        }
    }

    public void DispPlayerInfo()
    {
        Count.text = localPlayerCount.ToString("00") + "/" + MaxLimit.ToString();

        set = true;
    }
    public void DeletePlayerInfo(VRCPlayerApi player)
    {
        Count.text = localPlayerCount.ToString("00") + "/" + MaxLimit.ToString();

        VRCPlayerApi person = player;

        if (Networking.IsOwner(this.gameObject))
        {
            for (int i = 0; i < this.Names.Length; i++)
            {
                if (!string.IsNullOrEmpty(this.Names[i].SyncData))
                {
                    string[] arr = this.Names[i].SyncData.Split('：');

                    string player_Na = "";
                    player_Na = person.displayName;

                    if (arr[1] == player_Na)
                    {
                        this.Names[i].SyncData = null;
                        return;
                    }
                }
            }
        }
    }
    /*===================================================================================*/
    //selectの処理
    public void Select_player()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetPersonal");
    }
    public void SetPersonal()
    {
        System_Audio.PlayOneShot(Select);

        Selected_player = VRCPlayerApi.GetPlayerById(num);

        if (Selected_player != null)
        {
            SelectName.text = Selected_player.displayName;
            IDs.text = num.ToString("000");
        }
        else
        {
            SelectName.text = "Error";
            IDs.text = "000";
        }
    }

    public void Choose_left()
    {
        if (num > 0)
        {
            if (Networking.IsOwner(this.gameObject))
            {
                System_Audio.PlayOneShot(Select);
                num--;
            }

            SelectID.text = num.ToString("000");
        }
    }

    public void Choose_right()
    {
        if (num < 1000)
        {
            if (Networking.IsOwner(this.gameObject))
            {
                System_Audio.PlayOneShot(Select);
                num++;
            }

            SelectID.text = num.ToString("000");
        }
    }

    //canselの処理
    public void Cansel_player()
    {
        if (Selected_player == null)
        {
            System_Audio.PlayOneShot(Cansel);

            return;
        }
        else
        {
            System_Audio.PlayOneShot(Cansel);

            //強制ミュート解除
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ForceVoiceOn");

            //nullセット
            Selected_player = null;
            SelectName.text = "-";
            IDs.text = "000";
        }
    }

    public void ForceVoiceOn()
    {
        Selected_player.SetVoiceGain(25.0f);
        Selected_player.SetVoiceDistanceFar(15.0f);

        Selected_player.SetAvatarAudioGain(10.0f);
        Selected_player.SetAvatarAudioFarRadius(40.0f);
        Selected_player.SetAvatarAudioNearRadius(40.0f);
    }

    //Muteの処理
    public void Mute_player()
    {
        if (Selected_player == null)
        {
            System_Audio.PlayOneShot(Cansel);

            return;
        }
        else
        {
            System_Audio.PlayOneShot(Mute);
            //強制ミュート
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ForceMute");
        }
    }
    public void ForceMute()
    {
        Selected_player.SetVoiceGain(0.0f);
        Selected_player.SetVoiceDistanceFar(0.0f);

        Selected_player.SetAvatarAudioGain(0.0f);
        Selected_player.SetAvatarAudioFarRadius(0.0f);
        Selected_player.SetAvatarAudioNearRadius(0.0f);
    }
    //Banish(追放)の処理
    public void Bunish_player()
    {
        if (Selected_player == null)
        {
            System_Audio.PlayOneShot(Cansel);

            return;
        }
        else
        {
            System_Audio.PlayOneShot(Banish);

            //強制テレポート
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ForceMove");
        }
    }

    public void ForceMove()
    {
        if (Networking.LocalPlayer == Selected_player)
        {
            Selected_player.TeleportTo(Position.transform.position, Position.transform.rotation);
        }
    }
}

