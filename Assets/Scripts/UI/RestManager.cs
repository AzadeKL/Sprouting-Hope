using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class RestManager : MonoBehaviour
{
    [SerializeField]
    private GameObject floatingTextPrefab;
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


        //if (player.GetComponent<PlayerInventory>().money < 500)
        //{
        //    var randomNum = Random.Range(0, maxExclusive: 10000);
        //    var money = player.GetComponent<PlayerInventory>().money;
        //    if (500 - money > randomNum)
        //    {
        //        player.GetComponent<PlayerInventory>().money += 100;
        //        FlyText();
        //    }

        //}

    }

    [ContextMenu("Fly Some Text")]
    public void FlyText()
    {
        return;
        var result = Instantiate(floatingTextPrefab, player.transform.position + Vector3.up, Quaternion.identity);
        FloatingText floatingText = result.GetComponent<FloatingText>();
        floatingText.Initialize("You found 100 dollars under your hard pillow");

    }
}
