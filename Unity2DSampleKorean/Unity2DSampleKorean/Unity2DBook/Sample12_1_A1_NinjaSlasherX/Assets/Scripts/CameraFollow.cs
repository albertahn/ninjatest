using UnityEngine;
using System.Collections;

public enum CAMERATARGET { // --- 카메라에 대한 대상 유형 ---
	PLAYER,			// 플레이어 좌표
	PLAYER_MARGIN,	// 플레이어 좌표(앞쪽 시야를 확보한다 마진있음)
	PLAYER_GROUND,	// 과거에 플레이어가 접지한 지면 좌표(앞쪽 시야를 확보한다 마진있음)
}

public enum CAMERAHOMING { // --- 카메라 호밍 유형 ---
	DIRECT,			// 카메라 좌표에 대상 좌표를 직접 설정한다
	LERP,			// 카메라와 대상 좌표를 선형 보간한다
	SLERP,			// 카메라와 대상 좌표를 곡선 보간한다
	STOP,			// 카메라를 멈춘다
}

public class CameraFollow : MonoBehaviour 
{
	// === 외부 파라미터(Inspecotr 표시) =====================
	[System.Serializable]
	public class Param {
		public CAMERATARGET tragetType 			= CAMERATARGET.PLAYER_GROUND;
		public CAMERAHOMING homingType 			= CAMERAHOMING.LERP;
		public Vector2 		margin 				= new Vector2 (2.0f, 2.0f);
		public Vector2 		homing 				= new Vector2 (0.1f, 0.2f);
		public bool			borderCheck 		= false;
		public GameObject	borderLeftTop;
		public GameObject	borderRightBottom;
		public bool			viewAreaCheck		= true;
		public Vector2		viewAreaMinMargin	= new Vector2(0.0f,0.0f); 
		public Vector2		viewAreaMaxMargin	= new Vector2(0.0f,2.0f); 

		public bool			orthographicEnabled = true;
		public float		screenOGSize		= 5.0f;
		public float		screenOGSizeHoming	= 0.1f;
		public float		screenPSSize		= 50.0f;
		public float		screenPSSizeHoming	= 0.1f;
	}
	public Param param;

	// === 캐시 ==========================================
	GameObject 		 	player;
	Transform 		 	playerTrfm;
	PlayerController 	playerCtrl;

	float				screenOGSizeAdd = 0.0f;
	float				screenPSSizeAdd = 0.0f;

	// === 코드(Monobehaviour 기본 기능 구현) ================
	void Awake () {
		player 		= PlayerController.GetGameObject();
		playerTrfm 	= player.transform;
		playerCtrl  = player.GetComponent<PlayerController>();
	}

	void LateUpdate () {
		float targetX 	= playerTrfm.position.x;
		float targetY 	= playerTrfm.position.y;
		float pX 		= transform.position.x;
		float pY 		= transform.position.y;
		float screenOGSize = camera.orthographicSize;
		float screenPSSize = camera.fieldOfView;

		// 대상 설정
		switch (param.tragetType) {
		case CAMERATARGET.PLAYER			:
			targetX = playerTrfm.position.x;
			targetY = playerTrfm.position.y;
			break;
		case CAMERATARGET.PLAYER_MARGIN	:
			targetX = playerTrfm.position.x + param.margin.x * playerCtrl.dir;
			targetY = playerTrfm.position.y + param.margin.y;
			break;
		case CAMERATARGET.PLAYER_GROUND	:
			targetX = playerTrfm.position.x + param.margin.x * playerCtrl.dir;
			targetY = playerCtrl.groundY + param.margin.y;
			break;
		}

		// 카메라 이동 한계 경계선 검사
		if (param.borderCheck) {
			float cX = playerTrfm.transform.position.x;
			float cY = playerTrfm.transform.position.y;
			if (cX < param.borderLeftTop.transform.position.x ||
			    cX > param.borderRightBottom.transform.position.x ||
			    cY > param.borderLeftTop.transform.position.y || 
			    cY < param.borderRightBottom.transform.position.y) {
				return;
			}
		}

		// 플레이어가 카메라 프레임 안에 들어왔는지 검사
		if (param.viewAreaCheck) {
			float 	z   = playerTrfm.position.z - transform.position.z;
			Vector3 minMargin = param.viewAreaMinMargin;
			Vector3 maxMargin = param.viewAreaMaxMargin;
			Vector2 min = Camera.main.ViewportToWorldPoint(new Vector3(0.0f,0.0f,z)) - minMargin;
			Vector2 max = Camera.main.ViewportToWorldPoint(new Vector3(1.0f,1.0f,z)) - maxMargin;
			if (playerTrfm.position.x < min.x || playerTrfm.position.x > max.x) {
				targetX = playerTrfm.position.x;
			}
			if (playerTrfm.position.y < min.y || playerTrfm.position.y > max.y) {
				targetY = playerTrfm.position.y;
				playerCtrl.groundY = playerTrfm.position.y;
			}
		}

		// 카메라 이동(호밍)
		switch (param.homingType) {
		case CAMERAHOMING.DIRECT 		:
			pX = targetX;
			pY = targetY;
			screenOGSize = param.screenOGSize;
			screenPSSize = param.screenPSSize;
			break;
		case CAMERAHOMING.LERP 			:
			//pX = transform.position.x + (targetX - transform.position.x) * homing.x;
			//pY = transform.position.y + (targetY - transform.position.y) * homing.y;
			pX = Mathf.Lerp(transform.position.x,targetX,param.homing.x);
			pY = Mathf.Lerp(transform.position.y,targetY,param.homing.y);
			screenOGSize = Mathf.Lerp(screenOGSize,param.screenOGSize,param.screenOGSizeHoming);
			screenPSSize = Mathf.Lerp(screenPSSize,param.screenPSSize,param.screenPSSizeHoming);
			break;
		case CAMERAHOMING.SLERP 		:
			pX = Mathf.SmoothStep(transform.position.x,targetX,param.homing.x);
			pY = Mathf.SmoothStep(transform.position.y,targetY,param.homing.y);
			screenOGSize = Mathf.SmoothStep(screenOGSize,param.screenOGSize,param.screenOGSizeHoming);
			screenPSSize = Mathf.SmoothStep(screenPSSize,param.screenPSSize,param.screenPSSizeHoming);
			break;
		case CAMERAHOMING.STOP 			:
			break;
		}
		transform.position 		= new Vector3 (pX,pY, transform.position.z);
		camera.orthographic 	= param.orthographicEnabled;
		camera.orthographicSize = screenOGSize + screenOGSizeAdd;
		camera.fieldOfView 		= screenPSSize + screenPSSizeAdd;
		camera.orthographicSize = Mathf.Clamp (camera.orthographicSize,  2.5f,   10.0f);
		camera.fieldOfView		= Mathf.Clamp (camera.fieldOfView     , 30.0f,  100.0f);

		// 카메라 특수 줌 효과 계산
		screenOGSizeAdd *= 0.99f;
		screenPSSizeAdd *= 0.99f;
	}

	// === 코드(그 외) ====================================
	public void SetCamera(Param cameraPara ) {
		param = cameraPara;
	}

	public void AddCameraSize(float ogAdd,float psAdd) {
		screenOGSizeAdd += ogAdd;
		screenPSSizeAdd += psAdd;
	}	
}
