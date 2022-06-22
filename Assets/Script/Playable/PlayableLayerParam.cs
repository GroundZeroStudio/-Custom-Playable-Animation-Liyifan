using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class PlayableLayerParam : ScriptableObject
{
    public uint LayerIndex = 0;
    [SerializeField, Tooltip("上半身AvaterMask")]
    public AvatarMask LayerAvatarMask;
}
