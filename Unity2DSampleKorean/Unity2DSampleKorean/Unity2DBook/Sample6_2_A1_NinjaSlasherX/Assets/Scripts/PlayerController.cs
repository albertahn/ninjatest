using UnityEngine;
using System.Collections;

public class PlayerController : BaseCharacterController {

	// === 외부 파라미터（Inspector 표시） =====================
						 public float 	initHpMax = 20.0f;
	[Range(0.1f,100.0f)] public float 	initSpeed = 12.0f;

	// === 외부 파라미터 ======================================
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

	// === 내부 파라미터 ======================================
	int 			jumpCount			= 0;
	bool			breakEnabled		= true;
	float 			groundFriction		= 0.0f;


	// === 코드（Monobehaviour기본 기능 구현） ================
	protected override void Awake () {
		base.Awake ();

		// 파라미터 초기화
		speed = initSpeed;
		SetHP(initHpMax,initHpMax);
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
			}
		} else {
			jumpCount = 0;
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
		Camera.main.transform.position = transform.position + Vector3.back;
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

		// 이동 체크
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
		switch(jumpCount) {
		case 0 :
			if (grounded) {
				animator.SetTrigger ("Jump");
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

	public void ActionAttack() {
		AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
		if (stateInfo.nameHash != ANISTS_ATTACK_A) {
			animator.SetTrigger ("Attack_A");
		} else {
			animator.SetTrigger ("Attack_B");
		}
	}

}


