using UnityEngine;
using System.Collections;


public class EnemyMain_C : EnemyMain {

	// === 외부 파라미터（Inspector 표시） =====================
	public int 		aiIfATTACKONSIGHT 		= 50;
	public int 		aiIfRUNTOPLAYER 		= 10;
	public int	 	aiIfESCAPE 				= 10;
	public float 	aiPlayerEscapeDistance = 0.0f;

	public int 		damageAttack_A 			= 1;

	public int 		fireAttack_A			= 3;
	public float 	waitAttack_A			= 10.0f;


	// === 내부 파라미터 ======================================
	int fireCountAttack_A = 0;

	// === 코드（AI사고 루틴 처리） =================================
	public override void FixedUpdateAI () {
		// 플레이어가 오면 도망간다
		enemyCtrl.ActionMoveToFar (player, aiPlayerEscapeDistance);

		// AI스테이트
//		Debug.Log (string.Format(">>> aists {0}",aiState)); // 항상 표시하면 처리가 느려지므로 주의
		switch (aiState) {
		case ENEMYAISTS.ACTIONSELECT	: // 사고 루틴 기점
			// 액션 선택
			int n = SelectRandomAIState();
			if (n < aiIfATTACKONSIGHT) {
				SetAIState(ENEMYAISTS.ATTACKONSIGHT,100.0f);
			} else
			if (n < aiIfATTACKONSIGHT + aiIfRUNTOPLAYER) {
				SetAIState(ENEMYAISTS.RUNTOPLAYER,3.0f);
			} else
			if (n < aiIfATTACKONSIGHT + aiIfRUNTOPLAYER + aiIfESCAPE) {
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

		case ENEMYAISTS.ATTACKONSIGHT 	: // 그 자리에서 공격
			Attack_A();
			break;

		case ENEMYAISTS.RUNTOPLAYER		: // 가까이 다가간다
			if (!enemyCtrl.ActionMoveToNear(player,10.0f)) {
				Attack_A();
			}
			break;

		case ENEMYAISTS.ESCAPE			: // 멀어진다
			if (!enemyCtrl.ActionMoveToFar(player,4.0f)) {
				Attack_A();
			}
			break;
		}
	}

	// === 코드（액션 처리） ==============================
	void Attack_A() {
		enemyCtrl.ActionLookup(player,0.1f);
		enemyCtrl.ActionMove (0.0f);
		enemyCtrl.ActionAttack("Attack_A",damageAttack_A);

		fireCountAttack_A ++;
		if (fireCountAttack_A >= fireAttack_A) {
			fireCountAttack_A = 0;
			SetAIState (ENEMYAISTS.FREEZ, waitAttack_A);
		}
	}
	
}
