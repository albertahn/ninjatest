using UnityEngine;
using System.Collections;

public enum ENEMYAISTS // --- 적 AI 상태 ---
{
	ACTIONSELECT,		// 액션 선택（사고）
	WAIT,				// 일정 시간（멈춰서）기다린다
	RUNTOPLAYER,		// 달려서 플레어에 가까이 간다
	JUMPTOPLAYER,		// 점프해서  플레어에 가까이 간다
	ESCAPE,				// 플레이어로부터 도망친다
	ATTACKONSIGHT,		// 그 자리에서 이동하지 않고 공격한다（원거리 공격용）
	FREEZ,				// 행동 정지（단, 이동 처리는 계속한다）
}

public class EnemyMain : MonoBehaviour {

	// === 외부 파라미터（Inspector 표시） =====================
	public		int					debug_SelectRandomAIState = -1;

	// === 외부 파라미터 ======================================
	[System.NonSerialized] public ENEMYAISTS 	aiState			= ENEMYAISTS.ACTIONSELECT;

	// === 캐쉬 ==========================================
	protected 	EnemyController 	enemyCtrl;
	protected 	GameObject		 	player;
	protected 	PlayerController 	playerCtrl;

	// === 내부 파라미터 ======================================
	protected 	float				aiActionTimeLength		= 0.0f;
	protected 	float				aiActionTImeStart		= 0.0f;
	protected 	float				distanceToPlayer 		= 0.0f;
	protected 	float				distanceToPlayerPrev	= 0.0f;

	// === 코드（Monobehaviour기본 기능 구현） ================
	public virtual void Awake() {
		enemyCtrl 	 	= GetComponent <EnemyController>();
		player 			= PlayerController.GetGameObject ();
		playerCtrl 		= player.GetComponent<PlayerController>();
	}

	public virtual void Start () {
	}
	
	public virtual void Update () {
	}

	public virtual void FixedUpdate () {
		if (BeginEnemyCommonWork ()) {
			FixedUpdateAI();
			EndEnemyCommonWork ();
		}
	}

	public virtual void FixedUpdateAI () {
	}


	// === 코드（기본 AI 동작 처리） =============================
	public bool BeginEnemyCommonWork () {
		// 살아있는지 검사
		if (enemyCtrl.hp <= 0) {
			return false;
		}

		enemyCtrl.animator.enabled 	= true;


		// 상태 검사
		if (!CheckAction ()) {
			return false;
		}

		return true;
	}

	public void EndEnemyCommonWork() {
		// 액션 한계 시간 검사
		float time = Time.fixedTime - aiActionTImeStart;
		if (time > aiActionTimeLength) {
			aiState = ENEMYAISTS.ACTIONSELECT;
		}
	}

	public bool CheckAction() {
		// 상태 검사
		AnimatorStateInfo stateInfo = enemyCtrl.animator.GetCurrentAnimatorStateInfo(0);
		if (stateInfo.tagHash  == EnemyController.ANITAG_ATTACK ||
		    stateInfo.nameHash == EnemyController.ANISTS_DMG_A ||
		    stateInfo.nameHash == EnemyController.ANISTS_DMG_B ||
		    stateInfo.nameHash == EnemyController.ANISTS_Dead) {
			return false;
		}

		return true;
	}

	public int SelectRandomAIState() {
#if UNITY_EDITOR
		if (debug_SelectRandomAIState >= 0) {
			return debug_SelectRandomAIState;
		}
#endif
		return Random.Range (0, 100 + 1);
	}

	public void SetAIState(ENEMYAISTS sts,float t) {
		aiState 			= sts;
		aiActionTImeStart  	= Time.fixedTime;
		aiActionTimeLength 	= t;
	}
	
	public virtual void SetCombatAIState(ENEMYAISTS sts) {
		aiState 		  = sts;
		aiActionTImeStart = Time.fixedTime;
		enemyCtrl.ActionMove (0.0f);
	}

	// === 코드（AI 스크립트 지원 함수） ====================
	public float GetDistanePlayer() {
		distanceToPlayerPrev = distanceToPlayer;
		distanceToPlayer = Vector3.Distance (transform.position, playerCtrl.transform.position);
		return distanceToPlayer;
	}

	public bool IsChangeDistanePlayer(float l) {
		return (Mathf.Abs(distanceToPlayer - distanceToPlayerPrev) > l);
	}

	public float GetDistanePlayerX() {
		Vector3 posA = transform.position;
		Vector3 posB = playerCtrl.transform.position;
		posA.y = 0; posA.z = 0;
		posB.y = 0; posB.z = 0;
		return Vector3.Distance (posA, posB);
	}
	
	public float GetDistanePlayerY() {
		Vector3 posA = transform.position;
		Vector3 posB = playerCtrl.transform.position;
		posA.x = 0; posA.z = 0;
		posB.x = 0; posB.z = 0;
		return Vector3.Distance (posA, posB);
	}

}
