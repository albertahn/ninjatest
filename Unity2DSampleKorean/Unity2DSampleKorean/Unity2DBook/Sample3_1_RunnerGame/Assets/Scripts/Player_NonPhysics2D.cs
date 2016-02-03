using UnityEngine;
using System.Collections;

public class Player_NonPhysics2D : MonoBehaviour {

	// --- 선언 ------------------------------------------------------------

	// Inspector에서 조정하기 위한 속성
	public float 	speed = 15.0f;	// 플레이어 캐릭터 속도
	public Sprite[] run;			// 플레이어 캐릭터 달리기 스프라이트
	public Sprite[] jump;		// 플레이어 캐릭터 점프 스프라이트
	
	// 내부에서 다루는 변수
	float 			jumpVy;			// 플레이어 캐릭터 상승 속도
	int 			animIndex;		// 플레이어 캐릭터 애니메이션 재생 인덱스
	bool 			goalCheck;		// 들어왔는지 확인

	// --- 메세지에 대응한 코드-----------------------------------------
	
	// 컴포넌트 실행 시작
	void Start () {
		// 초기화
		jumpVy 		= 0.0f;
		animIndex 	= 0;
		goalCheck 	= false;
	}

	// 플레이어 캐릭터에 적용된 충돌판정 영역에 다른 게임 오브젝트의 충돌판정 영역이 겹쳤다
	void OnCollisionEnter2D(Collision2D col) {
		// 들어왔는지 검사
		if (col.gameObject.name == "Stage_Gate") {
			// 들어왔다
			goalCheck = true;
			return;
		}
		// 골인 지점이 아닌 곳이라면 스테이지를 다시 로드해서 리셋한다
		Application.LoadLevel (Application.loadedLevelName);
	}

	// 프레임 다시 쓰기
	void Update () {
		// 들어왔는지 검사
		if (goalCheck) {
			return; // 들어왔다면 처리를 정지한다
		}

		// 현재 플레이어 캐릭터가 얼만큼의 높이에 있는가를 계산
		float height = transform.position.y + jumpVy;
		// 접지 확인(높이가 0이면 접지한 것임)
		if (height <= 0.0f) {
			// 점프 초기화
			height = 0.0f;
			jumpVy = 0.0f;

			// 점프 확인
			if (Input.GetButtonDown ("Fire1")) {
				// 점프 처리
				jumpVy = +1.3f;
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
		} else {
			// 점프 후 떨어지는 도중
			jumpVy -= 0.2f;
			//jumpVy -= 6.0f * Time.deltaTime; // 제대로 된 처리는 이것이다
		}

		// 플레이어 캐릭터 이동(좌표 설정)
		transform.position = new Vector3 (transform.position.x + speed * Time.deltaTime, height, 0.0f);
		// 아래와 같이 상대적으로 이동하도록 해도 된다
		//transform.Translate (speed * Time.deltaTime, jumpVy, 0.0f);
		// transform.position += new Vector3 (speed * Time.deltaTime, jumpVy, 0.0f);
		// 단, 다음과 같은 방법으로는 움직이지 않으므로 주의해야 한다
		//transform.position.Set(transform.position.x + speed * Time.deltaTime, height, 0.0f);

		// 카메라 이동(좌표를 상대적으로 이동시킴)
		GameObject goCam = GameObject.Find ("Main Camera");
		goCam.transform.Translate (speed * Time.deltaTime, 0.0f, 0.0f);
	}

	// UnityGUI 표시
	void OnGUI() {
		// 디버그 텍스트
		GUI.TextField (new Rect(10,10,300,60), "[Unity2D Sample 3-1 A]\n마우스 왼쪽 버튼을 누르면 점프!");
		// 리셋 버튼
		if ( GUI.Button ( new Rect(10, 80, 100, 20), "리셋" ) ) {
			Application.LoadLevel (Application.loadedLevelName);
		}
		// 메뉴로 돌아간다
		if ( GUI.Button ( new Rect(10,110, 100, 20), "메뉴" ) ) {
			Application.LoadLevel ("SelectMenu");
		}
	}

}
