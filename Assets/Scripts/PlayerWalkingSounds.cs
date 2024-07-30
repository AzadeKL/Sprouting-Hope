using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerWalkingSounds : MonoBehaviour
{
    [SerializeField]
    private Transform playerPosition;

    [SerializeField] private Tilemap farmLand;

    [SerializeField]
    private List<AudioClip> audioClips;

    [SerializeField]
    private AudioSource audioSourceField;
    [SerializeField]
    private AudioSource audioSourceFarmland;
    [SerializeField]
    private Rigidbody2D rb;



    private void Update()
    {



        if (rb.velocity.sqrMagnitude > Mathf.Epsilon)
        {
            var result = farmLand.HasTile(farmLand.WorldToCell(playerPosition.position));

            ChangeFootStepSound(result);
        }
        else
        {

            StopAll();
        }
    }


    private void StopAll()
    {
        audioSourceField.volume = 0f;
        audioSourceFarmland.volume = 0f;
    }

    private void ChangeFootStepSound(bool flag)
    {
        StopAll();

        if (flag)
        {
            audioSourceField.volume = 1f;
        }
        else
        {
            audioSourceFarmland.volume = 1f;
        }

    }

}
