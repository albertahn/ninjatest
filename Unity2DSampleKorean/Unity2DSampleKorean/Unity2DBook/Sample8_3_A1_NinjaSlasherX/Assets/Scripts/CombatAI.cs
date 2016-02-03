using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CombatAI : MonoBehaviour {

	// === 외부 파라미터(Inspector 표시) =====================
	public int freeAIMax 		= 3;
	public int blockAttackAIMax = 10;

	// === 코드（Monobehaviour기본 기능 구현） ================
	void FixedUpdate () {
		var activeEnemyMainList = new List<EnemyMain>();

		// 카메라에 비치고 있는 적을 검색
		GameObject[] enemyList = GameObject.FindGameObjectsWithTag ("Enemy");
		if (enemyList == null) {
			return;
		}
		foreach (GameObject enemy in enemyList) {
			//Debug.Log (string.Format(">>> obj Name {0} position {1}",enemy.name,enemy.transform.position));
			EnemyMain enemyMain = enemy.GetComponent<EnemyMain> ();
			if (enemyMain != null) {
				if (enemyMain.combatAIOerder && enemyMain.cameraEnabled) {
					activeEnemyMainList.Add (enemyMain);
				}
			} else {
				Debug.LogWarning(string.Format("CombatAI : EnemyMain null : {0} {1}",enemy.name,enemy.transform.position));
			}
		}

		// 공격하는 적을 억제한다
		int i = 0;
		foreach (EnemyMain enemyMain in activeEnemyMainList) {
			if (i < freeAIMax) {
				// 그냥 자유롭게 행동하도록 둔다
			} else
			if (i < freeAIMax + blockAttackAIMax) {
				// 공격을 억제한다
				if (enemyMain.aiState == ENEMYAISTS.RUNTOPLAYER) {
					enemyMain.SetCombatAIState(ENEMYAISTS.WAIT);
				}
			} else {
				// 행동을 정지시킨다
				if (enemyMain.aiState != ENEMYAISTS.WAIT) {
					enemyMain.SetCombatAIState(ENEMYAISTS.WAIT);
				}
			}
			i ++;
		}

		//Debug.Log(string.Format(">>> Combat AI {0}",i));
	}
}
