using UnityEngine;

public class Water : MonoBehaviour
{

    public void AnimEvent()
    {
        Destroy(gameObject.transform.parent.transform.gameObject);
    }
}
