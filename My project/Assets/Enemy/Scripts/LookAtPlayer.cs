using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    public Transform cam;               // Vari�vel p�blica do tipo Transform que ir� armazenar a refer�ncia da c�mera ou do objeto que o personagem deve olhar

    // Update is called once per frame
    void LateUpdate()
    {
        transform.LookAt(cam);         // Faz com que o objeto ao qual esse script est� anexado (transform) olhe diretamente para o objeto referenciado por 'cam'
    }
}
