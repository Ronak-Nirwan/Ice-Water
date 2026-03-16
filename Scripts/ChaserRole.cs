using UnityEngine;
using Unity.Netcode;

public class ChaserRole : NetworkBehaviour
{
    [SerializeField] private float freezeRange = 2f;


    private void OnEnable()
    {
        SetLayerRecursivelyActiveOnly(gameObject, "outline");
    }

    private void OnDisable()
    {
        SetLayerRecursivelyActiveOnly(gameObject,"Default");
    }

    public void TryFreeze()
    {
        if (!IsOwner) return;

        foreach (var player in RoleHandler.AllPlayers)
        {
            if (player.CurrentRole == PlayerRole.Runner)
            {
                float dist = Vector3.Distance(transform.position, player.transform.position);
                if (dist <= freezeRange)
                {
                    FreezeRequestServerRpc(player.OwnerClientId);
                    return;
                }
            }
        }
    }

    void SetLayerRecursivelyActiveOnly(GameObject obj, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);

        if (obj.activeInHierarchy)
            obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursivelyActiveOnly(child.gameObject, layerName);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void FreezeRequestServerRpc(ulong targetClientId, ServerRpcParams rpcParams = default)
    {
        var senderId = rpcParams.Receive.SenderClientId;
        var sender = NetworkManager.Singleton.ConnectedClients[senderId].PlayerObject.GetComponent<RoleHandler>();

        if (sender.CurrentRole != PlayerRole.Chaser)
        {
            Debug.LogWarning($"[FreezeRequest] Client {senderId} tried to freeze but isn’t a Chaser.");
            return;
        }

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(targetClientId, out var targetConn))
        {
            var targetHandler = targetConn.PlayerObject.GetComponent<RoleHandler>();
            var runner = targetConn.PlayerObject.GetComponent<RunnerRole>();

            if (targetHandler.CurrentRole == PlayerRole.Runner && runner != null)
            {
                runner.isFrozen.Value = true;
                Debug.Log($"[FreezeRequest] {sender.name} froze {targetHandler.name}");
            }
        }
    }
}
