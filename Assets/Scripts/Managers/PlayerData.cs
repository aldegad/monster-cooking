using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Netcode;

public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    public ulong clientId;
    public int characterId;
    public Vector3 position;

    public PlayerData(ulong clientId, int characterId = -1, Vector3 position = new Vector3())
    {
        this.clientId = clientId;
        this.characterId = characterId;
        this.position = position;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref this.clientId);
        serializer.SerializeValue(ref this.characterId);
        serializer.SerializeValue(ref this.position);
    }

    public bool Equals(PlayerData other)
    {
        return this.clientId == other.clientId &&
            this.characterId == other.characterId &&
            this.position == other.position;
    }
}