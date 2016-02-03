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
	
	public static 	bool 		checkPointEnabled 		= false;
	public static 	string		checkPointSceneName 	= "";
	public static 	string		checkPointLabelName 	= "";
	public static	float 		checkPointHp 			= 0;
	
	public static 	bool 		itemKeyA				= false;
	public static 	bool 		itemKeyB 				= false;
	public static 	bool 		itemKeyC 				= false;

	// 외부로부터 조작하는 처리를 위한 파라미터
	public static 	bool		initParam 	  			= true;

	// 기본 파라미터
	[System.NonSerialized] public float		groundY 	= 0.0f;
	[System.NonSerialized] public bool		superMode	= false;

	[System.NonSerialized] public int 		comboCount	= 0;

	[System.NonSerialized] public Vector3 	enemyActiveZonePointA;
	[System.NonSerialized] public Vector3 	enemyActiveZonePointB;

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

	// === 코드(Monobehaviour 기본 기능 구현) ================
	protected override void Awake () {
		base.Awake ();

		#if xxx
		Debug.Log (">>> ANISTS_Idle : " + ANISTS_Idle);
		Debug.Log (">>> ANISTS_Walk : " + ANISTS_Walk);
		Debug.Log (">>> ANISTS_Run : " + ANISTS_Run);
		Debug.Log (">>> ANISTS_Jump : " + ANISTS_Jump);
		Debug.Log (">>> ANITAG_ATTACK_A : " + ANISTS_ATTACK_A);
		Debug.Log (">>> ANITAG_ATTACK_B : " + ANISTS_ATTACK_B);
		Debug.Log (">>> ANITAG_ATTACK_C : " + ANISTS_ATTACK_C);
		Debug.Log(string.Format("0 -> {0}",playerAnim.GetLayerName (0)));
		Debug.Log(string.Format("1 -> {0}",playerAnim.GetLayerName (1)));
		#endif

		// !!! 가비지 콜렉터 강제 실행 !!!
		System.GC.Collect ();
		// !!!!!!!!!!!!!!!!!!!!!

		// 캐시
		hudHpBar 		= GameObject.Find ("HUD_HPBar").GetComponent<LineRenderer> ();
		hudScore 		= GameObject.Find ("HUD_Score").GetComponent<TextMesh> ();
		hudCombo 		= GameObject.Find ("HUD_Combo").GetComponent<TextMesh> ();

		// 파라미터 초기화
		speed 			= initSpeed;
		groundY 		= groundCheck_C.transform.position.y + 2.0f;

		// 활성 영역을 BoxCollider2D로부터 가져온다
		BoxCollider2D boxCol2D = transform.Find("Collider_EnemyActiveZone").GetComponent<BoxCollider2D>();
		enemyActiveZonePointA = new Vector3 (boxCol2D.center.x - boxCol2D.size.x / 2.0f, boxCol2D.center.y - boxCol2D.size.y / 2.0f);
		enemyActiveZonePointB = new Vector3 (boxCol2D.center.x + boxCol2D.size.x / 2.0f, boxCol2D.center.y + boxCol2D.size.y / 2.0f);
		boxCol2D.transform.gameObject.SetActive(false);

		if (initParam) {
			SetHP(initHpMax,initHpMax);
			initParam = false;
		}
		if (SetHP(PlayerController.nowHp,PlayerController.nowHpMax)) {
			// HP가 없을 때에는 1부터 시작
			SetHP(1,initHpMax);
		}

		// 체크 포인터에서부터 다시 시작
		if (checkPointEnabled) {
			StageTrigger_CheckPoint[] triggerList = GameObject.Find("Stage").GetComponentsInChildren<StageTrigger_CheckPoint>();
			foreach(StageTrigger_CheckPoint trigger in triggerList) {
				if (trigger.labelName == checkPointLabelName) {
					transform.position = trigger.transform.position;
					groundY = transform.position.y;
					Camera.main.GetComponent<CameraFollow>().SetCamera(trigger.cameraParam);
					break;
				}
			}
		}
		Camera.main.transform.position = new Vector3(transform.position.x,
		                                             groundY,
		                                             Camera.main.transform.position.z);

		// HUD 표시 상태를 설정
		Transform hud = GameObject.FindGameObjectWithTag ("SubCamera").transform;
		hud.Find("Stage_Item_Key_A").GetComponent<SpriteRenderer>().enabled = itemKeyA;
		hud.Find("Stage_Item_Key_B").GetComponent<SpriteRenderer>().enabled = itemKeyB;
		hud.Find("Stage_Item_Key_C").GetComponent<SpriteRenderer>().enabled = itemKeyC;
	}

	protected override void Start() {
		base.Start ();

		seAnimationList = new AudioSource[5];
		seAnimationList [0] = AppSound.instance.SE_ATK_A1;
		seAnimationList [1] = AppSound.instance.SE_ATK_A2;
		seAnimationList [2] = AppSound.instance.SE_ATK_A3;
		seAnimationList [3] = AppSound.instance.SE_ATK_ARIAL;
		seAnimationList [4] = AppSound.instance.SE_MOV_JUMP;
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

#if xxx
		// Debug
		BoxCollider2D boxCol2D = GameObject.Find("Collider_EnemyActiveZone").GetComponent<BoxCollider2D>();
		Vector3 vecA = transform.position + new Vector3 (boxCol2D.center.x - boxCol2D.size.x / 2.0f, boxCol2D.center.y - boxCol2D.size.y / 2.0f);
		Vector3 vecB = transform.position + new Vector3 (boxCol2D.center.x + boxCol2D.size.x / 2.0f, boxCol2D.center.y + boxCol2D.size.y / 2.0f);
		Collider2D[] col2DList = Physics2D.OverlapAreaAll (vecA,vecB);
		foreach(Collider2D col2D in col2DList) {
			if (col2D.tag == "EnemyBody") {
				col2D.GetComponentInParent<EnemyMain>().cameraEnabled = true;
			}
		}
#endif		
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

		// 공격 중인지 검사
		if (stateInfo.nameHash == ANISTS_ATTACK_A || 
		    stateInfo.nameHash == ANISTS_ATTACK_B || 
		    stateInfo.nameHash == ANISTS_ATTACK_C || 
		    stateInfo.nameHash == ANISTS_ATTACKJUMP_A || 
		    stateInfo.nameHash == ANISTS_ATTACKJUMP_B) {
			// 이동 정지
			speedVx = 0;
		}

#if xxx
		// 캐릭터 방향（공격 중이나 점프 중에 돌아볼 수 없도록 금지한다）
		if (stateInfo.nameHash != ANISTS_ATTACK_A && 
		    stateInfo.nameHash != ANISTS_ATTACK_B && 
		    stateInfo.nameHash != ANISTS_ATTACK_C && 
		    stateInfo.nameHash != ANISTS_ATTACKJUMP_A && 
		    stateInfo.nameHash != ANISTS_ATTACKJUMP_B) {
			transform.localScale = new Vector3 (basScaleX * dir, transform.localScale.y, transform.localScale.z);
		}
#else
		// 캐릭터 방향
		transform.localScale = new Vector3 (basScaleX * dir, transform.localScale.y, transform.localScale.z);
#endif

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

		// 그 자리에서 돌아봤는지 검사
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
			//Debug.Log(string.Format("Jump 1 {0} {1} {2} {3}",jumped,transform.position,grounded,groundedPrev));
			//Debug.Log(groundCheckCollider[1].name);
			AppSound.instance.SE_MOV_JUMP.Play ();
		}
	}

	public void ActionAttack() {
		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
		if (stateInfo.nameHash == ANISTS_Idle || stateInfo.nameHash == ANISTS_Walk || stateInfo.nameHash == ANISTS_Run || 
		    stateInfo.nameHash == ANISTS_Jump || stateInfo.nameHash == ANISTS_ATTACK_C) {

			animator.SetTrigger ("Attack_A");
			if (stateInfo.nameHash == ANISTS_Jump || stateInfo.nameHash == ANISTS_ATTACK_C) {
				rigidbody2D.velocity     = new Vector2(0.0f,0.0f);
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

	public void ActionEtc() {
		Collider2D[] otherAll = Physics2D.OverlapPointAll (groundCheck_C.position);
		foreach (Collider2D other in otherAll) {
			if (other.tag == "EventTrigger") {
				StageTrigger_Link link = other.GetComponent<StageTrigger_Link>();
				if (link != null) {
					link.Jump();
				}
			} else
			if (other.tag == "KeyDoor") {
				StageObject_KeyDoor keydoor = other.GetComponent<StageObject_KeyDoor>();
				keydoor.OpenDoor();
			} else
			if (other.name == "Stage_Switch_Body") {
				StageObject_Switch sw = other.transform.parent.GetComponent<StageObject_Switch>();
				sw.SwitchTurn();
			}
		}
	}

	public void ActionDamage(float damage) {
		// 피격 처리해도 되는지 검사
		if (!activeSts) {
			return;
		}

#if xxx
		// 무작위로 타격음을 재생
		switch(Random.Range(0,3)) {
		case 0 : AppSound.instance.SE_HIT_A1.Play (); break;
		case 1 : AppSound.instance.SE_HIT_A2.Play (); break;
		case 2 : AppSound.instance.SE_HIT_A3.Play (); break;
		}
#else
		// 타격음을 재생
		AppSound.instance.SE_HIT_A1.Play ();
#endif

#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID)
		Handheld.Vibrate();
#endif

		animator.SetTrigger ("DMG_A");
		speedVx = 0;
		rigidbody2D.gravityScale = gravityScale;

		// Combo Reset
		comboCount = 0;
		comboTimer = 0.0f;

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
		if (stateInfo.nameHash == ANISTS_DEAD) {
			return;
		}

		base.Dead (gameOver);

		if (gameOver) {
			SetHP(0,hpMax);
			Invoke ("GameOver", 3.0f);
		} else {
			SetHP(hp / 2,hpMax);
			Invoke ("GameReset", 3.0f);
		}

		GameObject.Find ("HUD_Dead").GetComponent<MeshRenderer> ().enabled = true;
		GameObject.Find ("HUD_DeadShadow").GetComponent<MeshRenderer> ().enabled = true;
		if (GameObject.Find ("VRPad") != null) {
			GameObject.Find ("VRPad").SetActive(false);
		}
	}

	public void GameOver() {
		PlayerController.score = 0;
		PlayerController.nowHp = PlayerController.checkPointHp;

		Application.LoadLevel(Application.loadedLevelName);
	}

	void GameReset() {
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


