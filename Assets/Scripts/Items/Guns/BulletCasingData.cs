
using UnityEngine;
    
[CreateAssetMenu(fileName = "Bullet Casing", menuName = "Guns...")]
public class BulletCasingData : ScriptableObject
{
    public BulletCasing Prefab;
    [Range(0f, 1f)]
    public float BounceCoefficient = 0.2f;
}
