using UnityEngine;
using System.Collections;

public class EnemyMain_B : EnemyMain {

	// === 외부 파라미터（Inspector 표시） =====================
	public int aiIfRUNTOPLAYER 			= 30;
	public int aiIfESCAPE 				= 20;

	public int damageAttack_A 			= 1;
	public int damageAttack_B 			= 2;

	// === 코드（AI사고 루틴 처리） =================================
	public override void FixedUpdateAI () {
		// AI 스테이트
//		Debug.Log (string.Format(">>> aists {0}",aiState)); // 항상 표시하면 처리가 느려지므로 주의
		switch (aiState) {
		case ENEMYAISTS.ACTIONSELECT	: // 사고 루틴 기점
			// 액션 선택
			int n = SelectRandomAIState();
			if (n < aiIfRUNTOPLAYER) {
				SetAIState(ENEMYAISTS.RUNTOPLAYER,3.0f);
			} else
			if (n < aiIfRUNTOPLAYER + aiIfESCAPE) {
				SetAIState(ENEMYAISTS.ESCAPE,Random.Range(2.0f,5.0f));
			} else {
				SetAIState(ENEMYAISTS.WAIT,1.0f + Random.Range(0.0f,1.0f));
			}
			enemyCtrl.ActionMove (0.0f);
			break;

		case ENEMYAISTS.WAIT			: // 휴식
			enemyCtrl.ActionLookup(player,0.1f);
			enemyCtrl.ActionMove (0.0f);
			break;

		case ENEMYAISTS.RUNTOPLAYER		: // 가까이 다가간다
			if (GetDistanePlayerY() < 3.0f) {
				if (!enemyCtrl.ActionMoveToNear(player,2.0f)) {
					Attack_A();
				}
			} else {
				if (GetDistanePlayerX() > 3.0f && !enemyCtrl.ActionMoveToNear(player,5.0f)) {
					Attack_A();
				}
			}
			break;

		case ENEMYAISTS.ESCAPE			: // 멀어진다
			if (!enemyCtrl.ActionMoveToFar(player,4.0f)) {
				Attack_B();
			}
			break;
		}
	}

	// === 코드（액션 처리） ==============================
	void Attack_A() {
		enemyCtrl.ActionLookup(player,0.1f);
		enemyCtrl.ActionMove (0.0f);
		enemyCtrl.ActionAttack("Attack_A",damageAttack_A);
		enemyCtrl.attackNockBackVector = new Vector2(500.0f,2000.0f);
		SetAIState(ENEMYAISTS.WAIT,3.0f);
	}

	void Attack_B() {
		enemyCtrl.ActionLookup(player,0.1f);
		enemyCtrl.ActionMove (0.0f);
		enemyCtrl.ActionAttack("Attack_B",damageAttack_B);
		enemyCtrl.attackNockBackVector = new Vector2(500.0f,1000.0f);
		SetAIState(ENEMYAISTS.FREEZ,5.0f);
	}

}
