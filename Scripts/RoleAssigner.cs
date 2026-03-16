using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class RoleAssigner : NetworkBehaviour
{
    [SerializeField] private int chaserCount = 1;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            AssignRoles();
        }
    }

    private void AssignRoles()
    {
        int chasersAssigned = 0;

        foreach (var player in RoleHandler.AllPlayers)
        {
            if (chasersAssigned < chaserCount)
            {
                player.SetRole(PlayerRole.Chaser);
                chasersAssigned++;
            }
            else
            {
                player.SetRole(PlayerRole.Runner);
            }
        }
    }
}
