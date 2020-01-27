using UnityEngine;

namespace GrabIt
{

[System.Serializable]
public class GrabObjectProperties{
	
	public bool m_useGravity = false;
	public float m_drag = 10;
	public float m_angularDrag = 10;
	public RigidbodyConstraints m_constraints = RigidbodyConstraints.FreezeRotation;		

}

public class GrabIt : MonoBehaviour {

	[Header("Input")]
	[SerializeField] KeyCode m_rotatePitchPosKey = KeyCode.I;
	[SerializeField] KeyCode m_rotatePitchNegKey = KeyCode.K;
	[SerializeField] KeyCode m_rotateYawPosKey = KeyCode.L;
	[SerializeField] KeyCode m_rotateYawNegKey = KeyCode.J;

	[Header("Grab properties")]

	[SerializeField]
	[Range(4,50)]
	float m_grabSpeed = 7;

	[SerializeField]
	[Range(0.1f ,5)]
	float m_grabMinDistance = 1;

	[SerializeField]
	[Range(4 ,25)]
	float m_grabMaxDistance = 10;

	[SerializeField]
	[Range(1,10)]
	float m_scrollWheelSpeed = 5;

	[SerializeField]
	[Range(50,500)]
	float m_angularSpeed = 300;

	[SerializeField]
	[Range(10,50)]
	float m_impulseMagnitude = 25;


	

	[Header("Affected Rigidbody Properties")]
	[SerializeField] GrabObjectProperties m_grabProperties = new GrabObjectProperties();	

	GrabObjectProperties m_defaultProperties = new GrabObjectProperties();

	[Header("Layers")]
	[SerializeField]
	LayerMask m_collisionMask;

	

	Rigidbody m_targetRB = null;
	Transform m_transform;	

	Vector3 m_targetPos;
	GameObject m_hitPointObject;
	float m_targetDistance;

	bool m_grabbing = false;
	bool m_applyImpulse = false;
	bool m_isHingeJoint = false;

	//Debug
	LineRenderer m_lineRenderer;

	

	void Awake()
	{
		m_transform = transform;
		m_hitPointObject = new GameObject("Point");

		m_lineRenderer = GetComponent<LineRenderer>();
	}

	
	void Update()
	{
		if( m_grabbing )
		{

			m_targetDistance += Input.GetAxisRaw("Mouse ScrollWheel") * m_scrollWheelSpeed;			
			m_targetDistance = Mathf.Clamp(m_targetDistance , m_grabMinDistance , m_grabMaxDistance);

			m_targetPos = m_transform.position + m_transform.forward * m_targetDistance;
						
			if(!m_isHingeJoint){
				if(Input.GetKey(m_rotatePitchPosKey) || Input.GetKey(m_rotatePitchNegKey) || Input.GetKey(m_rotateYawPosKey) || Input.GetKey(m_rotateYawNegKey)){
					m_targetRB.constraints = RigidbodyConstraints.None;
				}else{
					m_targetRB.constraints = m_grabProperties.m_constraints;
				}
			}
			

			if( Input.GetMouseButtonUp(0) ){				
				Reset();
				m_grabbing = false;
			}else if ( Input.GetMouseButtonDown(1) ){
				m_applyImpulse = true;
			}

			
		}
		else
		{

			if(Input.GetMouseButtonDown(0))
			{
				RaycastHit hitInfo;
				if(Physics.Raycast(m_transform.position , m_transform.forward , out hitInfo , m_grabMaxDistance , m_collisionMask ))
				{
					Rigidbody rb = hitInfo.collider.GetComponent<Rigidbody>();
					if(rb != null){							
						Set( rb , hitInfo.distance);						
						m_grabbing = true;
					}
				}
			}
		}
		
	}
	
	void Set(Rigidbody target , float distance)
	{	
		m_targetRB = target;
		m_isHingeJoint = target.GetComponent<HingeJoint>() != null;		

		//Rigidbody default properties	
		m_defaultProperties.m_useGravity = m_targetRB.useGravity;	
		m_defaultProperties.m_drag = m_targetRB.drag;
		m_defaultProperties.m_angularDrag = m_targetRB.angularDrag;
		m_defaultProperties.m_constraints = m_targetRB.constraints;

		//Grab Properties	
		m_targetRB.useGravity = m_grabProperties.m_useGravity;
		m_targetRB.drag = m_grabProperties.m_drag;
		m_targetRB.angularDrag = m_grabProperties.m_angularDrag;
		m_targetRB.constraints = m_isHingeJoint? RigidbodyConstraints.None : m_grabProperties.m_constraints;
		
		
		m_hitPointObject.transform.SetParent(target.transform);							

		m_targetDistance = distance;
		m_targetPos = m_transform.position + m_transform.forward * m_targetDistance;

		m_hitPointObject.transform.position = m_targetPos;
		m_hitPointObject.transform.LookAt(m_transform);
				
	}

	void Reset()
	{		
		//Grab Properties	
		m_targetRB.useGravity = m_defaultProperties.m_useGravity;
		m_targetRB.drag = m_defaultProperties.m_drag;
		m_targetRB.angularDrag = m_defaultProperties.m_angularDrag;
		m_targetRB.constraints = m_defaultProperties.m_constraints;
		
		m_targetRB = null;

		m_hitPointObject.transform.SetParent(null);
		
		if(m_lineRenderer != null)
			m_lineRenderer.enabled = false;
	}

	void Grab()
	{
		Vector3 hitPointPos = m_hitPointObject.transform.position;
		Vector3 dif = m_targetPos - hitPointPos;

		if(m_isHingeJoint)
			m_targetRB.AddForceAtPosition( m_grabSpeed  * dif * 100 , hitPointPos , ForceMode.Force);
		else
			m_targetRB.velocity = m_grabSpeed * dif;		

		
		if(m_lineRenderer != null){
			m_lineRenderer.enabled = true;
			m_lineRenderer.SetPositions( new Vector3[]{ m_targetPos , hitPointPos });
		}
	}

	void Rotate()
	{
		if(Input.GetKey(m_rotatePitchPosKey)){
			m_targetRB.AddTorque(  m_transform.right * m_angularSpeed );			
		}else if(Input.GetKey(m_rotatePitchNegKey)){
			m_targetRB.AddTorque(  - m_transform.right * m_angularSpeed );
		}

		if(Input.GetKey(m_rotateYawPosKey)){
			m_targetRB.AddTorque( - m_transform.up * m_angularSpeed );
		}else if(Input.GetKey(m_rotateYawNegKey)){
			m_targetRB.AddTorque( m_transform.up * m_angularSpeed );
		}
	}
	
	void FixedUpdate()
	{
		if(!m_grabbing)
			return;
		
		if(!m_isHingeJoint)
			Rotate();
		
		Grab();		

		if(m_applyImpulse){
			m_targetRB.velocity = m_transform.forward * m_impulseMagnitude;
			Reset();
			m_grabbing = false;
			m_applyImpulse = false;
		}
		
	}

}

}
