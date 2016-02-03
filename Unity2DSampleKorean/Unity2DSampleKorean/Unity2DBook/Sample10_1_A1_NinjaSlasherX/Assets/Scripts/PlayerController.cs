using UnityEngine;
using System.Collections;

public class PlayerController : BaseCharacterController {
	// === 외부 파라미터（Inspector 표시） =====================
						 public float 	initHpMax = 20.0f;
	[Range(0.1f,100.0f)] public float 	initSpeed = 12.0f;

	// === 외부 파라미터 ======================================
	// 저장 데이터 파라미터
	public static	float 		nowHpMax 				= 0;
	public static	float 		nowHp 	  				= 0;
	public static 	int			score 					= 0;

	// 기본 파라미터
	[System.NonSerialized] public Vector3 	enemyActiveZonePointA;
	[System.NonSerialized] public Vector3 	enemyActiveZonePointB;
	[System.NonSerialized] public float		groundY 	= 0.0f;

	[System.NonSerialized] public int 		comboCount	= 0;

	// 애니메이션 해시 이름
	public readonly static int ANISTS_Idle 	 		= Animator.StringToHash("Base Layer.Player_Idle");
	public readonly static int ANISTS_Walk 	 		= Animator.StringToHash("Base Layer.Player_Walk");
	public readonly static int ANISTS_Run 	 	 	= Animator.StringToHash("Base Layer.Player_Run");
	public readonly static int ANISTS_Jump 	 		= Animator.StringToHash("Base Layer.Player_Jump");
	public readonly static int ANISTS_ATTACK_A 		= Animator.StringToHash("Base Layer.Player_ATK_A");
	public readonly static int ANISTS_ATTACK_B 		= Animator.StringToHash("Base Layer.Player_ATK_B");
	public readonly static int ANISTS_ATTACK_C	 	= Animator.StringToHash("Base Layer.Player_ATK_C");
	public readonly static int ANISTS_ATTACKJUMP_A  = Animator.StringToHash("Base Layer.Player_ATKJUMP_A");
	public readonly static int ANISTS_ATTACKJUMP_B  = Animator.StringToHash("Base Layer.Player_ATKJUMP_B");
	public readonly static int ANISTS_DEAD  		= Animator.StringToHash("Base Layer.Player_Dead");

	// === 캐시 ==========================================
	LineRenderer	hudHpBar;
	TextMesh		hudScore;
	TextMesh 		hudCombo;
	
	// === 내부 파라미터 ======================================
	int 			jumpCount			= 0;

	volatile bool 	atkInputEnabled		= false;
	volatile bool	atkInputNow			= false;

	bool			breakEnabled		= true;
	float 			groundFriction		= 0.0f;

	float 			comboTimer 			= 0.0f;

	// === 코드(지원 함수) ===============================
	public static GameObject GetGameObject() {
		return GameObject.FindGameObjectWithTag ("Player");
	}
	public static Transform GetTranform() {
		return GameObject.FindGameObjectWithTag ("Player").transform;
	}
	public static PlayerController GetController() {
		return GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerController>();
	}
	public static Animator GetAnimator() {
		return GameObject.FindGameObjectWithTag ("Player").GetComponent<Animator>();
	}

	// === 코드（Monobehaviour기본 기능 구현） ================
	protected override void Awake () {
		base.Awake ();
		
		// 캐쉬
		hudHpBar 		= GameObject.Find ("HUD_HPBar").GetComponent<LineRenderer> ();
		hudScore 		= GameObject.Find ("HUD_Score").GetComponent<TextMesh> ();
		hudCombo 		= GameObject.Find ("HUD_Combo").GetComponent<TextMesh> ();
		
		// 파라미터 초기화
		speed 	= initSpeed;
		groundY = groundCheck_C.transform.position.y + 2.0f;
		SetHP(initHpMax,initHpMax);
		
		// 활성 영역을 BoxCollider2D로부터 가져온다
		BoxCollider2D boxCol2D = transform.Find("Collider_EnemyActiveZone").GetComponent<BoxCollider2D>();
		enemyActiveZonePointA = new Vector3 (boxCol2D.center.x - boxCol2D.size.x / 2.0f, boxCol2D.center.y - boxCol2D.size.y / 2.0f);
		enemyActiveZonePointB = new Vector3 (boxCol2D.center.x + boxCol2D.size.x / 2.0f, boxCol2D.center.y + boxCol2D.size.y / 2.0f);
		boxCol2D.transform.gameObject.SetActive(false);
	}

	protected override void Update () {
		base.Update ();
		
		// 상태 표시
		hudHpBar.SetPosition (1, new Vector3 (5.0f * (hp / hpMax), 0.0f, 0.0f));
		hudScore.text = string.Format("Score {0}",score);
		
		if (comboTimer <= 0.0f) {
			hudCombo.gameObject.SetActive(false);
			comboCount = 0;
			comboTimer = 0.0f;
		} else {
			comboTimer -= Time.deltaTime;
			if (comboTimer > 5.0f) {
				comboTimer = 5.0f;
			}
			float s = 0.3f + 0.5f * comboTimer;
			hudCombo.gameObject.SetActive(true);
			hudCombo.transform.localScale = new Vector3(s,s,1.0f);
		}
	}

	protected override void FixedUpdateCharacter () {
		// 현재 스테이트 가져오기
		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

		// 착지했는지 검사
		if (jumped) {
			if ((grounded && !groundedPrev) || 
				(grounded && Time.fixedTime > jumpStartTime + 1.0f)) {
				animator.SetTrigger ("Idle");
				jumped 	  = false;
				jumpCount = 0;
				rigidbody2D.gravityScale = gravityScale;
			}
			if (Time.fixedTime > jumpStartTime + 1.0f) {
				if (stateInfo.nameHash == ANISTS_Idle || stateInfo.nameHash == ANISTS_Walk || 
				    stateInfo.nameHash == ANISTS_Run  || stateInfo.nameHash == ANISTS_Jump) {
					rigidbody2D.gravityScale = gravityScale;
				}
			}
		} else {
			jumpCount = 0;
			rigidbody2D.gravityScale = gravityScale;
		}

		// 공격 중인지 확인
		if (stateInfo.nameHash == ANISTS_ATTACK_A || 
		    stateInfo.nameHash == ANISTS_ATTACK_B || 
		    stateInfo.nameHash == ANISTS_ATTACK_C || 
		    stateInfo.nameHash == ANISTS_ATTACKJUMP_A || 
		    stateInfo.nameHash == ANISTS_ATTACKJUMP_B) {
			// 이동 정지
			speedVx = 0;
		}

		// 캐릭터 방향
		transform.localScale = new Vector3 (basScaleX * dir, transform.localScale.y, transform.localScale.z);

		// 점프 도중에 가로 이동 감속
		if (jumped && !grounded && groundCheck_OnMoveObject == null) {
			if (breakEnabled) {
				breakEnabled = false;
				speedVx *= 0.9f;
			}
		}

		// 이동 정지(감속) 처리
		if (breakEnabled) {
			speedVx *= groundFriction;
		}

		// 카메라
		//Camera.main.transform.position = transform.position + Vector3.back;
	}

	// === 코드(애니메이션 이벤트용 코드) ===============
	public void EnebleAttackInput() {
		atkInputEnabled = true;
	}
	
	public void SetNextAttack(string name) {
		if (atkInputNow == true) {
			atkInputNow = false;
			animator.Play(name);
		}
	}

	// === 코드（기본 액션） =============================
	public override void ActionMove(float n) {
		if (!activeSts) {
			return;
		}

		// 초기화
		float dirOld = dir;
		breakEnabled = false;
		
		// 애니메이션 지정
		float moveSpeed = Mathf.Clamp(Mathf.Abs (n),-1.0f,+1.0f);
		animator.SetFloat("MovSpeed",moveSpeed);
		//animator.speed = 1.0f + moveSpeed;

		// 이동 검사
		if (n != 0.0f) {
			// 이동
			dir 	  = Mathf.Sign(n);
			moveSpeed = (moveSpeed < 0.5f) ? (moveSpeed * (1.0f / 0.5f)) : 1.0f;
			speedVx   = initSpeed * moveSpeed * dir;
		} else {
			// 이동 정지
			breakEnabled = true;
		}

		// 그 시점에서 돌아보기 검사
		if (dirOld != dir) {
			breakEnabled = true;
		}
	}

	public void ActionJump() {
		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
		if (stateInfo.nameHash == ANISTS_Idle || stateInfo.nameHash == ANISTS_Walk || stateInfo.nameHash == ANISTS_Run || 
		    (stateInfo.nameHash == ANISTS_Jump && rigidbody2D.gravityScale >= gravityScale)) {
			switch(jumpCount) {
			case 0 :
				if (grounded) {
					animator.SetTrigger ("Jump");
					//rigidbody2D.AddForce (new Vector2 (0.0f, 1500.0f));	// Bug
					rigidbody2D.velocity = Vector2.up * 30.0f;
					jumpStartTime = Time.fixedTime;
					jumped = true;
					jumpCount ++;
				}
				break;
			case 1 :
				if (!grounded) {
					animator.Play("Player_Jump",0,0.0f);
					rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x,20.0f);
					jumped = true;
					jumpCount ++;
				}
				break;
			}
		}
	}

	public void ActionAttack() {
		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
		if (stateInfo.nameHash == ANISTS_Idle || stateInfo.nameHash == ANISTS_Walk || stateInfo.nameHash == ANISTS_Run || 
		    stateInfo.nameHash == ANISTS_Jump || stateInfo.nameHash == ANISTS_ATTACK_C) {

			animator.SetTrigger ("Attack_A");
			if (stateInfo.nameHash == ANISTS_Jump || stateInfo.nameHash == ANISTS_ATTACK_C) {
				rigidbody2D.velocity = new Vector2(0.0f,0.0f);
				rigidbody2D.gravityScale = 0.1f;
			}
		} else {
			if (atkInputEnabled) {
				atkInputEnabled = false;
				atkInputNow 	= true;
			}
		}
	}

	public void ActionAttackJump() {
		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
		if (grounded && 
		    (stateInfo.nameHash == ANISTS_Idle || stateInfo.nameHash == ANISTS_Walk || stateInfo.nameHash == ANISTS_Run ||
		     stateInfo.nameHash == ANISTS_ATTACK_A || stateInfo.nameHash == ANISTS_ATTACK_B)) {
			animator.SetTrigger ("Attack_C");
			jumpCount = 2;
		} else {
			if (atkInputEnabled) {
				atkInputEnabled = false;
				atkInputNow 	= true;
			}
		}
	}

	public void ActionDamage(float damage) {
		if (!activeSts) {
			return;
		}
		
		animator.SetTrigger ("DMG_A");
		speedVx = 0;
		rigidbody2D.gravityScale = gravityScale;
		
		if (jumped) {
			damage *= 1.5f;
		}
		
		if (SetHP(hp - damage,hpMax)) {
			Dead(true); // 사망
		}
	}
	
	// === 코드（그 외） ====================================
	public override void Dead(bool gameOver) {
		// 사망 처리해도 되는지 확인
		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
		if (!activeSts || stateInfo.nameHash == ANISTS_DEAD) {
			return;
		}
		
		base.Dead (gameOver);
		
		SetHP(0,hpMax);
		Invoke ("GameOver", 3.0f);

		GameObject.Find ("HUD_Dead").GetComponent<MeshRenderer> ().enabled = true;
		GameObject.Find ("HUD_DeadShadow").GetComponent<MeshRenderer> ().enabled = true;
	}
	
	public void GameOver() {
		PlayerController.score = 0;
		Application.LoadLevel(Application.loadedLevelName);
	}
	
	public override bool SetHP(float _hp,float _hpMax) {
		if (_hp > _hpMax) {
			_hp = _hpMax;
		}
		
		nowHp 		= _hp;
		nowHpMax 	= _hpMax;
		return base.SetHP (_hp, _hpMax);
	}

	public void AddCombo() {
		comboCount ++;
		comboTimer += 1.0f;
		hudCombo.text = string.Format("Combo {0}",comboCount);
	}
}


