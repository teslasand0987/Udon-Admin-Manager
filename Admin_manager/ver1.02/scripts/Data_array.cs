using UdonSharp;
using UnityEngine;
using UnityEngine.UI;


public class Data_array : UdonSharpBehaviour
{
    [SerializeField]
    private Text TargetText = null;

    [UdonSynced]
    public string SyncData = string.Empty;


    private void Update()
    {
        this.TargetText.text = this.SyncData;
    }
    }


