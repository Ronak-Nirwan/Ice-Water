using Unity.Netcode;
using UnityEngine;

public class TickRate : MonoBehaviour
{
    void Awake()
    {
        NetworkManager.Singleton.NetworkConfig.TickRate = 15;
    }
}
