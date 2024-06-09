using UnityEngine;

public class DT_Caught : MonoBehaviour
{
    private DT_Tornado tornadoReference;
    private SpringJoint spring;

    [HideInInspector]
    public Rigidbody rigid;

    // Use this for initialization
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (tornadoReference != null && spring != null)
        {
            // Lift spring so objects are pulled upwards by dynamically updating the anchor position
            Vector3 newPosition = spring.connectedAnchor;
            newPosition.y = transform.position.y - tornadoReference.transform.position.y;
            spring.connectedAnchor = newPosition;
        }
    }

    void FixedUpdate()
    {
        if (tornadoReference == null)
        {
            return;
        }

        // Rotate object around tornado center
        Vector3 direction = transform.position - tornadoReference.transform.position;
        // Project
        Vector3 projection = Vector3.ProjectOnPlane(direction, tornadoReference.GetRotationAxis());
        projection.Normalize();
        Vector3 normal = Quaternion.AngleAxis(130, tornadoReference.GetRotationAxis()) * projection;
        normal = Quaternion.AngleAxis(tornadoReference.lift, projection) * normal;
        rigid.AddForce(normal * tornadoReference.GetStrength(), ForceMode.Force);

        Debug.DrawRay(transform.position, normal * 10, Color.red);
    }

    // Call this when tornado Reference already exists
    public void Init(DT_Tornado tornadoRef, Rigidbody tornadoRigidbody, float springForce)
    {
        enabled = true;
        tornadoReference = tornadoRef;

        spring = gameObject.AddComponent<SpringJoint>();
        spring.spring = springForce;
        spring.connectedBody = tornadoRigidbody;
        spring.autoConfigureConnectedAnchor = false;

        // Set initial position of the caught object relative to its position and the tornado
        Vector3 initialPosition = transform.position - tornadoReference.transform.position;
        initialPosition.y = transform.position.y;
        spring.connectedAnchor = initialPosition;

        Debug.Log("Spring initialized with anchor at " + spring.connectedAnchor);
    }

    public void Release()
    {
        enabled = false;
        if (spring != null)
        {
            Destroy(spring);
        }
    }
}
