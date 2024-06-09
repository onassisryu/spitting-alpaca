using Photon.Pun;
using UnityEngine;

public class CameraMovementTest : MonoBehaviourPun
{
    public GameObject objectTofollowObject;
    public Transform objectTofollow;
    public float followSpeed = 10f;
    public float sensitivity = 100f;
    public float clampAngle = 70f;

    private float rotX;
    private float rotY;

    public Transform realCamera;
    public Vector3 dirNormalized;
    public Vector3 finalDir;
    public float minDistance = 3f;
    public float maxDistance = 3f;
    public float finalDistance;
    public float smoothness = 10f;

    // Start is called before the first frame update
    void Start()
    {
        if (!photonView.IsMine)
        {
            gameObject.SetActive(false);
            return;
        }

        rotX = transform.localRotation.eulerAngles.x;
        rotY = transform.localRotation.eulerAngles.y;

        dirNormalized = realCamera.localPosition.normalized;
        finalDistance = realCamera.localPosition.magnitude;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        {
            gameObject.SetActive(false);
            return;
        }

        if (UIManager.Instance.menuActiveSelf)
        {
            return;
        }

        // objectTofollowObject�� null�̰ų� ��Ȱ��ȭ�� ��� ���ο� object�� ã���ϴ�.
        if (objectTofollowObject == null || !objectTofollowObject.activeSelf)
        {
            GameObject[] tagObjects = GameObject.FindGameObjectsWithTag("FollowCam");
            foreach (GameObject tagObject in tagObjects)
            {
                if (tagObject != null) // null üũ �߰�
                {
                    PhotonView tagPhotonView = tagObject.GetComponent<PhotonView>();

                    if (tagPhotonView != null && tagPhotonView.IsMine && tagObject.activeSelf) // tagPhotonView null check
                    {
                        objectTofollowObject = tagObject;
                        objectTofollow = objectTofollowObject.transform;
                        break;
                    }
                }
            }
        }

        rotX += -(Input.GetAxis("Mouse Y")) * sensitivity * Time.deltaTime;
        rotY += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;

        rotX = Mathf.Clamp(rotX, -15, clampAngle);
        Quaternion rot = Quaternion.Euler(rotX, rotY, 0);
        transform.rotation = rot;
    }

    void LateUpdate()
    {
        if (!photonView.IsMine)
        {
            gameObject.SetActive(false);
            return;
        }

        if (UIManager.Instance.menuActiveSelf)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, objectTofollow.position, followSpeed * Time.deltaTime);

        finalDir = transform.TransformPoint(dirNormalized * maxDistance);

        RaycastHit hit; // check obstacle

        if (Physics.Linecast(transform.position, finalDir, out hit))
        {
            finalDistance = Mathf.Clamp(hit.distance, minDistance, maxDistance);
        }
        else
        {
            finalDistance = maxDistance;
        }
        realCamera.localPosition = Vector3.Lerp(realCamera.localPosition, dirNormalized * finalDistance, Time.deltaTime * smoothness);
    }
}
