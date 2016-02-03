using UnityEngine;
using System.Collections;

public class EnemySprite : MonoBehaviour {

	EnemyMain enemyMain;

	void Awake () {
		// EnemyMain을 검색
		enemyMain = GetComponentInParent<EnemyMain> ();
	}
	
	void OnBecameVisible()
	{
		// 카메라에 비치고 있다
	}
	void OnBecameInvisible()
	{
		// 키메라에 비치고 있지 않다
	}

	void OnWillRenderObject() {
		if (Camera.current.tag == "MainCamera") {
			// 처리
			enemyMain.cameraEnabled = true;
		}
	}

}
