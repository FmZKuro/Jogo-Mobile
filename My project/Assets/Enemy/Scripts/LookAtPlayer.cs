using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    public Transform cam;               // Variável pública do tipo Transform que irá armazenar a referência da câmera ou do objeto que o personagem deve olhar

    // Update is called once per frame
    void LateUpdate()
    {
        transform.LookAt(cam);         // Faz com que o objeto ao qual esse script está anexado (transform) olhe diretamente para o objeto referenciado por 'cam'
    }
}
