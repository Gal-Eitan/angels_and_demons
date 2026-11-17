using UnityEngine;

public class AngelInteractor : MonoBehaviour
{
    [SerializeField]
    private GameObject reflection;
    [SerializeField]
    private GameObject boulder;
    
    [SerializeField]
    private float rotationInZ;
    
    [SerializeField]
    private GameObject player;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            //Debug.Log("player is near angel");
            player.GetComponent<PlayerController>().onInteract.AddListener(correctMirror);
        }
        
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            
            //Debug.Log("player isnt near angel");
            player.GetComponent<PlayerController>().onInteract.RemoveListener(correctMirror);
        }
    }

    
    
    public async void correctMirror()
    {
        reflection.transform.Rotate(new Vector3(0,0,rotationInZ));
        //Debug.Log("corrected Mirror");

        await Awaitable.WaitForSecondsAsync(3f);


        boulder.SetActive(true);
        //Debug.Log("released Boulder");

    }
}
