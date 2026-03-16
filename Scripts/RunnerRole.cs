using UnityEngine;
using Unity.Netcode;

public class RunnerRole : NetworkBehaviour
{
    public NetworkVariable<bool> isFrozen = new NetworkVariable<bool>(false);
    private FreezeVisualController freezeVisual;
    private BaseMovement movement;

    private void Awake()
    {
        freezeVisual = GetComponentInChildren<FreezeVisualController>(true);
        movement = GetComponent<BaseMovement>();
    }

    public override void OnNetworkSpawn()
    {
        isFrozen.OnValueChanged += OnFrozenStateChanged;

        OnFrozenStateChanged(false, isFrozen.Value);
    }

    public override void OnNetworkDespawn()
    {
        isFrozen.OnValueChanged -= OnFrozenStateChanged;
    }

    private void OnFrozenStateChanged(bool oldValue, bool newValue)
    {
        if (freezeVisual != null)
        {
            if (newValue)
                freezeVisual.TriggerFreeze();
            else
                freezeVisual.TriggerUnfreeze();
        }

        // Only disable movement on local owner
        if (IsOwner && movement != null)
        {
            movement.SetMovementEnabled(!newValue);
        }
    }

    public void TryUnfreeze()
    {
        if (!IsOwner) return;

        foreach (var player in RoleHandler.AllPlayers)
        {
            if (player.CurrentRole == PlayerRole.Runner && player != this.GetComponent<RoleHandler>())
            {
                float dist = Vector3.Distance(transform.position, player.transform.position);
                if (dist <= 5f)
                {
                    UnfreezeRequestServerRpc(player.OwnerClientId);
                    return;
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UnfreezeRequestServerRpc(ulong targetClientId, ServerRpcParams rpcParams = default)
    {
        var senderId = rpcParams.Receive.SenderClientId;
        var sender = NetworkManager.Singleton.ConnectedClients[senderId].PlayerObject.GetComponent<RoleHandler>();

        if (sender.CurrentRole != PlayerRole.Runner)
        {
            Debug.LogWarning($"[UnfreezeRequest] Client {senderId} tried to unfreeze but isn’t a Runner.");
            return;
        }

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(targetClientId, out var targetConn))
        {
            var targetHandler = targetConn.PlayerObject.GetComponent<RoleHandler>();
            var runner = targetConn.PlayerObject.GetComponent<RunnerRole>();

            if (targetHandler.CurrentRole == PlayerRole.Runner && runner != null)
            {
                runner.isFrozen.Value = false;
                Debug.Log($"[UnfreezeRequest] {sender.name} unfroze {targetHandler.name}");
            }
        }
    }
}
