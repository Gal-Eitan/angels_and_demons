using UnityEngine;
using UnityEngine.Events;

public class PlayerDetector : MonoBehaviour
{
    [SerializeField] private UnityEvent OnPlayerDetect;
    private float rayDistance = 3f;
    [SerializeField]
    private Vector3 subVectorToDetector;
    private Vector3 detectorPosition;

    void Start()
    {
        detectorPosition=transform.position+subVectorToDetector;
    }

    void Update()
    {

        // שליחת הקרן ממיקום השחקן כלפי מטה
        RaycastHit2D hit = Physics2D.Raycast(detectorPosition, Vector2.down, rayDistance);

        // ציור הקרן בחלון ה-Scene לצורכי דיבאג (לא נראה במשחק עצמו)
        Debug.DrawRay(detectorPosition, Vector2.down * rayDistance, Color.red);
        Debug.DrawRay(transform.position, Vector2.down * rayDistance, Color.green);

        // בדיקה האם הקרן פגעה במשהו
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            //Debug.Log("פגעתי ב: " + hit.collider.name);
            //Debug.Log("נקודת הפגיעה המדויקת: " + hit.point);

            OnPlayerDetect.Invoke();
        }
    }
}