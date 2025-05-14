using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezableEntity : MonoBehaviour
{
    public bool Frozen { get; private set; } = false;
    private Vector3 frozenPosition = Vector3.zero;

    private void FixedUpdate()
    {
        OnFixedUpdate();
    }

    protected virtual void OnFixedUpdate()
    {
        if (Frozen)
            MaintainFreeze();
    }

    public void Freeze()
    {
        Frozen = true;
        frozenPosition = transform.position;

        TryGetComponent<Rigidbody>(out Rigidbody rb);
        if (rb != null)
            rb.velocity = Vector3.zero;
    }

    public void Unfreeze()
    {
        Frozen = false;
    }

    private void MaintainFreeze()
    {
        // simply override position (this could optimised later to prevent unnecessary movement logic)
        transform.position = frozenPosition;
    }
}
