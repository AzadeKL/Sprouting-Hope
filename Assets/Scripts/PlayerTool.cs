using DG.Tweening;
using UnityEngine;

public class PlayerTool : MonoBehaviour
{
    public Transform visualParent;

    public SpriteRenderer visual;
    public Vector3 direction;
    private bool animating = false;


    void Update()
    {
        if (animating) return;


        if (Input.GetMouseButtonUp(0))
        {
            animating = true;
            visualParent.DOShakeRotation(1f).OnComplete(() => animating = false);
        }




        Vector3 mousePosition = Input.mousePosition;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        mousePosition.z = 0;


        direction = mousePosition - visualParent.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        visualParent.rotation = Quaternion.Euler(new Vector3(0, 0, angle));


        if (direction.x < 0)
        {
            visual.flipY = true;
        }
        else
        {
            visual.flipY = false;
        }



    }
}
