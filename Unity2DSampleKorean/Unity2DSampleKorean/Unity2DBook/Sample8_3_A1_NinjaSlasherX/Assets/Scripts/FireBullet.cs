using UnityEngine;
using System.Collections;

public enum FIREBULLET
{
	ANGLE,
	HOMING,
	HOMING_Z,
}

public class FireBullet : MonoBehaviour {

	// === 외부 파라미터（Inspector 표시） =====================
	public FIREBULLET 	fireType 		= FIREBULLET.HOMING;

	public float 		attackDamage 	= 1;
	public Vector2		attackNockBackVector;

	public bool			penetration		= false;

	public float 		lifeTime 		= 3.0f;
	public float 		speedV 			= 10.0f;
	public float 		speedA 			= 0.0f;
	public float 		angle			= 0.0f;

	public float		homingTime		= 0.0f;
	public float 		homingAngleV	= 180.0f;
	public float 		homingAngleA	= 20.0f;

	public Vector3		bulletScaleV	= Vector3.zero;
	public Vector3		bulletScaleA	= Vector3.zero;

	public Sprite 		hiteSprite;
	public Vector3		hitEffectScale  = Vector3.one;
	public float		rotateVt		= 360.0f;

	// === 외부 파라미터 ======================================
	[System.NonSerialized] public Transform 	ownwer;
	[System.NonSerialized] public GameObject 	targetObject;
	[System.NonSerialized] public bool 			attackEnabled;	

	// === 내부 파라미터 ======================================
	float				fireTime;
	Vector3 			posTarget;
	float 				homingAngle;
	Quaternion			homingRotate;
	float 				speed;

	// === 코드（Monobehaviour기본 기능 구현） ================
	void Start() {
		// 주인 검사
		if (!ownwer) {
			return;
		}

		// 초기화
		targetObject 	= PlayerController.GetGameObject();
		posTarget 		= targetObject.transform.position + new Vector3 (0.0f, 1.0f, 0.0f);

		switch (fireType) {
		case FIREBULLET.ANGLE		:
			speed = (ownwer.localScale.x < 0.0f) ? -speedV : +speedV;
			break;
		case FIREBULLET.HOMING:
			speed = speedV;
			homingRotate = Quaternion.LookRotation (posTarget - transform.position);
			break;
		case FIREBULLET.HOMING_Z	:
			speed = speedV;
			break;
		}
		
		fireTime 	 	= Time.fixedTime;
		homingAngle  	= angle;
		attackEnabled	= true;
		Destroy (this.gameObject, lifeTime);
	}

	void OnTriggerEnter2D(Collider2D other) {
		// 주인 검사
		if (!ownwer) {
			return;
		}
		// 자기 자신에게 닿았는지 검사
		if ((other.isTrigger ||
		     (ownwer.tag == "Player" && other.tag == "PlayerBody") 		 ||
		     (ownwer.tag == "Player" && other.tag == "PlayerArm")  		 ||
		     (ownwer.tag == "Player" && other.tag == "PlayerArmBullet")  ||
		     (ownwer.tag == "Enemy"  && other.tag == "EnemyBody")  		 ||
		     (ownwer.tag == "Enemy"  && other.tag == "EnemyArm")   		 ||
		     (ownwer.tag == "Enemy"  && other.tag == "EnemyArmBullet" ) )) {
			return;
		}

		// 벽에 닿았는지 검사
		if (!penetration) {
			GetComponent<SpriteRenderer>().sprite = hiteSprite;
			GetComponent<SpriteRenderer>().color  = new Color(1.0f,1.0f,1.0f,0.5f);
			transform.localScale = hitEffectScale;
			Destroy (this.gameObject,0.1f);
		}
	}

	void Update() {
		// 스프라이트 이미지 회전 처리
		transform.Rotate (0.0f, 0.0f, Time.deltaTime * rotateVt);
	}

	void FixedUpdate() {
		// 타깃 설정
		bool homing = ((Time.fixedTime - fireTime) < homingTime);
		if (homing) {
			posTarget = targetObject.transform.position + new Vector3 (0.0f, 1.0f, 0.0f);
		}

		// 호밍 처리
		switch(fireType) {
		case FIREBULLET.ANGLE 	 : // 지정한 각도로 발사
			rigidbody2D.velocity = Quaternion.Euler (0.0f,0.0f,angle) * new Vector3 (speed, 0.0f, 0.0f);
			break;
			
		case FIREBULLET.HOMING   : // 완전 호밍
		{
			if (homing) {
				homingRotate = Quaternion.LookRotation (posTarget - transform.position);
			}
			Vector3 vecMove			= (homingRotate * Vector3.forward) * speed;
			rigidbody2D.velocity 	= Quaternion.Euler (0.0f,0.0f,angle) * vecMove;
		}
			break;
			
		case FIREBULLET.HOMING_Z : // 지정한 각도 범위 안에서 호밍
			if (homing) {
				float 	targetAngle = Mathf.Atan2 (	posTarget.y - transform.position.y, 
				                                    posTarget.x - transform.position.x) * Mathf.Rad2Deg;
				float	deltaAngle  = Mathf.DeltaAngle(targetAngle,homingAngle);
				float 	deltaHomingAngle = homingAngleV * Time.fixedDeltaTime;
				if (Mathf.Abs(deltaAngle) >= deltaHomingAngle) {
					homingAngle += (deltaAngle < 0.0f) ? +deltaHomingAngle : -deltaHomingAngle;
				}
				homingAngleV += (homingAngleA * Time.fixedDeltaTime);
				homingRotate = Quaternion.Euler (0.0f, 0.0f, homingAngle);
			}
			rigidbody2D.velocity = (homingRotate * Vector3.right) * speed;
			break;
		}

		// 가속도 계산
		speed += speedA * Time.fixedDeltaTime;

		// 스케일 계산
		transform.localScale += bulletScaleV;
		bulletScaleV += bulletScaleA * Time.fixedDeltaTime;
		if (transform.localScale.x < 0.0f || transform.localScale.y < 0.0f || transform.localScale.z < 0.0f) {
			Destroy (this.gameObject);
		}
	}

}
