using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum EntityType
{
    SceneryObj,
    Unit,
    Structure,
    Other,
    Undeclared
}

public enum Tileset
{
    Desert,
    Undeclared
}
public class EntityBase : MonoBehaviour
{
    [Header("Entity Base Components")]
    public Flags flags = new Flags();

    public Vector3Int intPos = Vector3Int.zero;

    public EntityType entityType = EntityType.Undeclared;
    public Tileset tileset = Tileset.Undeclared;

    public float currentHitpoints;
    public float maxHitpoints;

    public GameObject gfxParent;

    public virtual void TakeDamage(float damgeInput)
    {
        currentHitpoints -= damgeInput;

        if (currentHitpoints <= 0 )
        {
            TryDeath();
        }
    }

    public virtual void TryDeath()
    {
        Destroy(gameObject);
    }
}

public class Flags
{
    public bool destructable;
    public bool targetable;
}
