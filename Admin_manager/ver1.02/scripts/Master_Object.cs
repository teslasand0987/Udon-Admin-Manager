using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class Maser_object : UdonSharpBehaviour
{
    [SerializeField] private Text Master_Name;

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        Master_Name.text = Networking.GetOwner(this.gameObject).displayName;
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        Master_Name.text = Networking.GetOwner(this.gameObject).displayName;
    }
}
