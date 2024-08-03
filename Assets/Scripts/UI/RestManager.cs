using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class RestManager : MonoBehaviour
{
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private GameManager gameManager;
    [SerializeField]
    private GameObject RestUIPanelMain;
    [SerializeField]
    private Volume volumeEffect;


    [SerializeField]
    private float restTimer = 5;
    [SerializeField]
    private float speed = 5;
    public void RestLogic()
    {
        Time.timeScale = speed;
        StartCoroutine(Resting());
    }



    IEnumerator Resting()
    {
        gameManager.escDisabled = true;
        HidePlayer();
        DOVirtual.Float(0, 1, 1f, x => volumeEffect.weight = x).SetUpdate(true);
        yield return new WaitForSeconds(restTimer);
        Time.timeScale = 1;
        DOVirtual.Float(1, 0, 0.4f, x => volumeEffect.weight = x).OnComplete(HideRestPanel);
        ShowPlayer();
    }

    private void HideRestPanel()
    {
        RestUIPanelMain.SetActive(false);
        RestUIPanelMain.transform.GetChild(0).gameObject.SetActive(true);
        gameManager.escDisabled = false;
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
