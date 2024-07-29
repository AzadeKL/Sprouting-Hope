using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class RestManager : MonoBehaviour
{
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private Volume volumeEffect;


    [SerializeField]
    private float restTimer = 5;
    [SerializeField]
    private float speed = 5;
    public void RestLogic()
    {
        Time.timeScale = 5;
        StartCoroutine(Resting());
    }



    IEnumerator Resting()
    {
        HidePlayer();
        DOVirtual.Float(0, 1, 1f, x => volumeEffect.weight = x).SetUpdate(true);
        yield return new WaitForSeconds(restTimer);
        Time.timeScale = 1;
        DOVirtual.Float(1, 0, 0.4f, x => volumeEffect.weight = x);
        ShowPlayer();
    }

    private void HidePlayer()
    {
        player.gameObject.SetActive(false);
    }

    private void ShowPlayer()
    {
        player.gameObject.SetActive(true);
    }
}
