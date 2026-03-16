using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public enum PlayerRole { Unassigned, Chaser, Runner }

public class RoleHandler : NetworkBehaviour
{
    public static List<RoleHandler> AllPlayers = new List<RoleHandler>();

    private BaseMovement movement;
    private RunnerRole runnerRole;
    private ChaserRole chaserRole;

    public NetworkVariable<PlayerRole> currentRole = new NetworkVariable<PlayerRole>(
        PlayerRole.Unassigned,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public PlayerRole CurrentRole => currentRole.Value;

    void Awake()
    {
        movement = GetComponent<BaseMovement>();
        runnerRole = GetComponent<RunnerRole>();
        chaserRole = GetComponent<ChaserRole>();
    }

    public override void OnNetworkSpawn()
    {
        AllPlayers.Add(this);

        currentRole.OnValueChanged += (oldValue, newValue) =>
        {
            ApplyRoleInternal(newValue);
        };

        ApplyRoleInternal(currentRole.Value);
    }

    public override void OnNetworkDespawn()
    {
        AllPlayers.Remove(this);
    }

    public void SetRole(PlayerRole role)
    {
        if (IsServer)
            currentRole.Value = role;
    }

    private void ApplyRoleInternal(PlayerRole role)
    {
        if (runnerRole != null) runnerRole.enabled = (role == PlayerRole.Runner);
        if (chaserRole != null) chaserRole.enabled = (role == PlayerRole.Chaser);
    }
}
