using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PlayerStartPosition : MonoBehaviour
{
    private Transform _playerTransform;

    [Inject]
    public void Construct(PlayerCharacterController controller)
    {
        controller.SetPositionAndRotation(transform.position, transform.rotation.eulerAngles);
    }
}
