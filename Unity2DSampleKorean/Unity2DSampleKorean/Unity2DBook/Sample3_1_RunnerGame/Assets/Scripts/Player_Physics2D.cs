using UnityEngine;
using System.Collections;

public class Player_Physics2D : MonoBehaviour {

	// --- 선언 ------------------------------------------------------------
	
	// Inspector에서 조정하기 위한 속성
	public float 	speed 		= 12.0f;	// 플레이어 캐릭터 속도
	public float 	jumpPower 	= 500.0f;	// 플레이어 캐릭터를 점프시켰을 때의 파워
	public Sprite[] run;					// 플레이어 캐릭터 달리기 스프라이트
	public Sprite[] jump;					// 플레이어 캐릭터 점프 스프라이트

	// 내부에서 다루는 변수
	int 			animIndex;		// 플레이어 캐릭터 애니메이션 재생 인덱스
	bool 			grounded;		// 접지 확인
	bool 			goalCheck;		// 들어왔는지 확인
	float 			goalTime;		// 들어온 시간

	// --- 메세지에 대응한 코드 -----------------------------------------
	
	// 컴포넌트 실행 시작
	void Start () {
		// 초기화
		animIndex 	= 0;
		grounded 	= false;
		goalCheck 	= false;
	}

	// 플레이어 캐릭터에 적용된 충돌판정 영역에 다른 게임 오브젝트의 충돌판정 영역이 겹쳤다
	void OnCollisionEnter2D(Collision2D col) {
		// 들어왔는지 확인
		if (col.gameObject.name == "Stage_Gate") {
			// 들어왔다
			goalCheck 	= true;
			goalTime 	= Time.timeSinceLevelLoad;
		}
	}

	// 프레임 다시 쓰기
	void Update () {
		// 들어왔는지 확인
		Transform 	groundCheck = transform.Find ("GroundCheck");
		grounded = (Physics2D.OverlapPoint (groundCheck.position) != null) ? true : false;  
		if (grounded) {
			// 점프
			if (Input.GetButtonDown ("Fire1")) {
				// 점프 처리
				rigidbody2D.AddForce ( new Vector2(0.0f,jumpPower) );
				// 점프 스프라이트 이미지로 전환
				GetComponent<SpriteRenderer>().sprite = jump[0];
			} else {
				// 달리기 처리
				animIndex ++;
				if (animIndex >= run.Length) {
					animIndex = 0;
				}
				// 달리기 스프라이트 이미지로 전환
				GetComponent<SpriteRenderer>().sprite = run[animIndex];
			}
		}
		// 구멍에 빠졌는지 확인
		if (transform.position.y < -10.0f) {
			// 구멍에 빠졌다면 스테이지를 다시 읽어들여서 리셋한다
			Application.LoadLevel (Application.loadedLevelName);
		}
	}

	// 프레임 다시 쓰기
	void FixedUpdate () {
		// 이동 계산
		rigidbody2D.velocity = new Vector2 (speed, rigidbody2D.velocity.y);
		// 카메라 이동
		GameObject goCam = GameObject.Find ("Main Camera");
		goCam.transform.position = new Vector3 (transform.position.x + 5.0f, goCam.transform.position.y, goCam.transform.position.z);
	}

	// UnityGUI 표시
	void OnGUI() {
		// 디버그 텍스트
		GUI.TextField(new Rect(10,10,300,60), "[Unity2D Sample 3-1 A]\n마우스 왼쪽 버튼을 누르면 점프!");
		if (goalCheck) {
			GUI.TextField (new Rect (10, 100, 330, 60), string.Format ("***** Goal!! *****\nTime {0}", goalTime));
		}
		// 리셋 버튼
		if ( GUI.Button( new Rect(10, 80, 100, 20), "리셋" ) ) {
			Application.LoadLevel (Application.loadedLevelName);
		}
		// 메뉴로 돌아간다
		if ( GUI.Button ( new Rect(10,110, 100, 20), "메뉴" ) ) {
			Application.LoadLevel ("SelectMenu");
		}
	}

}
