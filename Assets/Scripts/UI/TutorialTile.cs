using UnityEngine;
using UnityEngine.Events;


public class TutorialTile : MonoBehaviour
    {
        [Header("Tutorial Text")]
        [Multiline,Tooltip("Enter what tutorial should say")]public string tutorialText;

        public UnityEvent<string> tutorialEvent;
        public UnityEvent tutorialEventClose;

        [Header("Destroy Settings")]
        public bool shouldBeDestroyed = false;
        [Tooltip("Only works when the bool is true")] public float destroyTime = 0.4f;

        // Start is called before the first frame update
        void Start()
        {
            tutorialEvent.AddListener(GameObject.FindGameObjectWithTag("TutorialPopUp").GetComponent<TutorialPopUp>().showPopUp);
            tutorialEventClose.AddListener(GameObject.FindGameObjectWithTag("TutorialPopUp").GetComponent<TutorialPopUp>().hidePopUp);
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                Debug.Log("Tutorial Tile Triggered");
                tutorialEvent.Invoke(tutorialText);  
                // Destroy(gameObject, 0.1f);
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                tutorialEventClose.Invoke();  
                if (shouldBeDestroyed)
                {
                    Destroy(gameObject, destroyTime);
                }
            }
        }
    }
