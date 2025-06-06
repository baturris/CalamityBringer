using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitSpark : MonoBehaviour
{
    public float rotationSpeed = 180f; // grados por segundo

    private float animationLength;

    void Start()
    {
        // Empieza sin rotaci�n (por si viene del prefab con rotaci�n aplicada)
        transform.rotation = Quaternion.identity;

        // Obtener la duraci�n de la animaci�n
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            animationLength = anim.GetCurrentAnimatorStateInfo(0).length;
            Destroy(gameObject, animationLength);
        }
        else
        {
            Destroy(gameObject, 1f); // fallback
        }
    }

    void Update()
    {
        // Rota sobre el eje Z constantemente
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}
