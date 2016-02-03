using UnityEngine;
using System.Collections;

public class StageObject_DogPile : MonoBehaviour {

	public GameObject[] enemyList;
	public GameObject[] destroyObjectList;
	
	void Start () {
		InvokeRepeating ("CheckEnemy",0.0f, 1.0f);
	}

	void CheckEnemy () {
		// 등록되어 있는 적 리스트를 통해 적이 생존 상태인지 여부를 확인
		// （1초에 한 번만 해도 된다）
		bool flag = true;
		foreach (GameObject enemy in enemyList) {
			if (enemy != null) {
				flag = false;
			}
		}

		// 모든 적이 쓰러져 있는지 확인
		if (flag) {
            // 삭제물 리스트에 포함된 오브젝트를 삭제한다
            foreach (GameObject destroyObject in destroyObjectList) {
				Destroy(destroyObject,1.0f);
			}
			CancelInvoke("CheckEnemy");
		}
	}
}
