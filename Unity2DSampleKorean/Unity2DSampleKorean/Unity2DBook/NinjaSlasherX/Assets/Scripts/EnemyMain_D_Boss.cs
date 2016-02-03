using UnityEngine;
using System.Collections;

public class EnemyMain_D_Boss : EnemyMain {

	// === 외부 파라미터(Inspector 표시) =====================
	public int aiIfRUNTOPLAYER 			= 30;
	public int aiIfJUMPTOPLAYER 		= 10;
	public int aiIfESCAPE 				= 20;
	public int aiIfRETURNTODOGPILE 		= 10;

	// === 캐시 ==========================================
	GameObject		bossHud;
	LineRenderer	hudHpBar;
	Transform 		playerTrfm;

	// === 내부 파라미터 ======================================
	float dogPileCheckTime 	= 0.0f;
	float jumpCheckTime 	= 0.0f;

	// === 코드（AI 사고 논리 처리） =================================
	public override void Start() {
		base.Start();
		bossHud  	= GameObject.Find ("BossHud");
		hudHpBar 	= GameObject.Find ("HUD_HPBar_Boss").GetComponent<LineRenderer> ();
		playerTrfm  = PlayerController.GetTranform ();
	}

	public override void Update() {
		base.Update();
		// 상태 표시
		if (enemyCtrl.hp > 0) {
			hudHpBar.SetPosition (1, new Vector3 (15.0f * ((float)enemyCtrl.hp / (float)enemyCtrl.hpMax), 0.0f, 0.0f));
		} else {
			if (bossHud != null) {
				bossHud.SetActive(false);
				bossHud = null;
			}
		}
	}

	public override void FixedUpdateAI () {
		// 플레이어가 스테이지 양쪽 끝으로 도망간 상태라면 도그파일까지 돌아온다
		if (Time.fixedTime - dogPileCheckTime > 3.0f &&
		    (playerTrfm.position.x < 32.0f || playerTrfm.position.x > 48.0f)) {
			if (transform.position.x < 34.0f || transform.position.x > 46.0f) {
				if (dogPile != null) {
					SetAIState(ENEMYAISTS.RETURNTODOGPILE,Random.Range(2.0f,3.0f));
				}
			} else {
				Attack_A();
				SetAIState(ENEMYAISTS.WAIT,3.0f);
			}

			dogPileCheckTime = Time.fixedTime;
			jumpCheckTime = Time.fixedTime + 3.0f;
		} else
		// 플레이어가 올라타고 있을 때나 깔려 있을 때를 위한 긴급 처리
		if (Time.fixedTime - jumpCheckTime > 1.0f &&
			enemyCtrl.hp > enemyCtrl.hpMax / 2.0f && 
		    GetDistanePlayer () < 4.0f) {
			Attack_Jump();
			SetAIState(ENEMYAISTS.WAIT,3.0f);
			jumpCheckTime = Time.fixedTime;
		}

		// AI 상태
		//Debug.Log (string.Format(">>> aists {0}",aiState));
		switch (aiState) {
		case ENEMYAISTS.ACTIONSELECT	: // 생각의 시작점
			// 액션 선택
			int n = SelectRandomAIState();
			if (n < aiIfRUNTOPLAYER) {
				SetAIState(ENEMYAISTS.RUNTOPLAYER,3.0f);
			} else
			if (n < aiIfRUNTOPLAYER + aiIfJUMPTOPLAYER) {
				SetAIState(ENEMYAISTS.JUMPTOPLAYER,1.0f);
			} else
			if (n < aiIfRUNTOPLAYER + aiIfJUMPTOPLAYER + aiIfESCAPE) {
				SetAIState(ENEMYAISTS.ESCAPE,Random.Range(2.0f,5.0f));
			} else
			if (n < aiIfRUNTOPLAYER + aiIfJUMPTOPLAYER + aiIfESCAPE + aiIfRETURNTODOGPILE) {
				if (dogPile != null) {
					SetAIState(ENEMYAISTS.RETURNTODOGPILE,Random.Range(2.0f,3.0f));
				}
			} else {
				SetAIState(ENEMYAISTS.WAIT,1.0f + Random.Range(0.0f,1.0f));
			}
			enemyCtrl.ActionMove (0.0f);

			// 호밍 공격은 체력이 떨어졌을 때부터
			if (enemyCtrl.hp > enemyCtrl.hpMax / 2.0f) {
				if (aiState == ENEMYAISTS.ESCAPE) {
					Attack_Jump();
					SetAIState(ENEMYAISTS.WAIT,3.0f);
				}
			}
			break;
			
		case ENEMYAISTS.WAIT			: // 휴식
			enemyCtrl.ActionLookup(player,0.1f);
			enemyCtrl.ActionMove (0.0f);
			break;
			
		case ENEMYAISTS.RUNTOPLAYER		: // 다가간다
			if (!enemyCtrl.ActionMoveToNear(player,7.0f)) {
				Attack_A();
			}
			break;
			
		case ENEMYAISTS.JUMPTOPLAYER	: // 점프로 다가간다
			if (GetDistanePlayer() > 5.0f) {
				Attack_Jump();
			}  else {
				enemyCtrl.ActionLookup(player,0.1f);
				SetAIState(ENEMYAISTS.WAIT,3.0f);
			}
			break;
			
		case ENEMYAISTS.ESCAPE			: // 멀어진다
			if (!enemyCtrl.ActionMoveToFar(player,7.0f)) {
				Attack_B();
			}
			break;
			
		case ENEMYAISTS.RETURNTODOGPILE	: // 도그파일로 돌아온다
			if (enemyCtrl.ActionMoveToNear(dogPile,3.0f)) {
			} else {
				enemyCtrl.ActionMove (0.0f);
				SetAIState(ENEMYAISTS.ACTIONSELECT,1.0f);
			}
			break;
		}
	}

	// === 코드（액션 처리） ==============================
	void Attack_A() {
		enemyCtrl.ActionLookup(player,0.1f);
		enemyCtrl.ActionAttack("Attack_A",10);
		enemyCtrl.attackNockBackVector = new Vector2(1000.0f,100.0f);
		SetAIState(ENEMYAISTS.WAIT,3.0f);
	}

	void Attack_B() {
		enemyCtrl.ActionMove (0.0f);
		enemyCtrl.ActionAttack("Attack_B",0);
		SetAIState(ENEMYAISTS.WAIT,5.0f);
	}

	void Attack_Jump() {
		enemyCtrl.ActionLookup(player,0.1f);
		enemyCtrl.ActionMove (0.0f);
		enemyCtrl.attackEnabled = true;
		enemyCtrl.attackDamage 	= 1;
		enemyCtrl.attackNockBackVector = new Vector2(1000.0f,100.0f);
		enemyCtrl.ActionJump();
	}
}
