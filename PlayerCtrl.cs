using UnityEngine;
using System.Collections;
//SmoothFollow 스크립트를 사용하기 위해 네임스페이스를 선언
using UnityStandardAssets.Utility;

public class PlayerCtrl : MonoBehaviour {

    // CharacterController 컴포넌트를 할당할 변수
    private CharacterController controller;

    // Transform 컴포넌트를 할당할 변수
    private Transform tr;
    // NetworkView 컴포넌트를 할당할 변수
    private NetworkView _networkView;

    // 위치 정보를 송수신할 때 사용할 변수 선언 및 초기값 설정
    // 캐릭터 위치정보 송수신 변수
    private Vector3 currPos = Vector3.zero;
    private Quaternion currRot = Quaternion.identity;

    // Bullet 프리팹 할당
    public GameObject bullet;
    // 총알 발사 위치
    public Transform firePos;

    // 사망 여부를 나타내는 변수
    private bool isDie = false;
    // 플레이어 생명치
    private int hp = 100;
    // 적 생명치
    //private int tekiHp = 100;
    // 부활 시간(Respawn Time)
    private float respawnTime = 3.0f;
    // 게임 유아이 객체 - 게임이 종료 되었는지 판단용
    public GameUI gameUI;
    // 나와 적의 점령 시간 저장용
    private float myTime = 0.0f;
    //private float tekitime = 0.0f;
    // 이벤트
    // UI 갱신용도
    public delegate void PlayerHpHandler(int hp);
    public static event PlayerHpHandler OnPlayerHp;
    public static event PlayerHpHandler OnTekiHp;
    public static event PlayerHpHandler OnMyTime;
    public static event PlayerHpHandler OnTekiTime;
    // 승패 판단용
    public delegate void PlayerVictory();
    public static event PlayerVictory OnVictory;
    // 다시하기 
    public delegate void PlayerRiset();
    public static event PlayerRiset OnRiset;
    // 리셋 했니?
    public static bool isReset = false;
    public static bool isResetRcv = false;
    // 상대 리셋 했나 판단용
    public delegate bool PlayerPanelReset();
    public static event PlayerPanelReset OnResetPanel;
    public delegate void SetPanel(bool isCall);
    public static event SetPanel OnSetPanel;
    void Awake()
    {
        // 접근할 컴포넌트를 할당한다.
        tr = GetComponent<Transform>();
        _networkView = GetComponent<NetworkView>();
        controller = GetComponent<CharacterController>();

        // 자신의 스크립트를 Observed 속성에 연결
        //_networkView.observed = this;
        // NetworkView가 자신의 것인지 확인한다.
        // 나 일때 
        if(_networkView.isMine)
        {
            //Main Camera가 추적해야 할 대상을 설정한다.
            Camera.main.GetComponent<SmoothFollow>().target = tr;
            gameUI = GameObject.FindGameObjectWithTag("UI").GetComponent<GameUI>();
           // if (gameUI)
           // {
                 // Debug.Log("gameUI 초기화 됨");
           // }
           // else
          //  {
                // Debug.Log("gameUI 초기화 안됨");
          //  }
        }
    }

    void Update()
    {
        // 나 업데이트
        if(_networkView.isMine)
        {
            if(Input.GetMouseButtonDown(0))
            {
                // 사망했을 때 발사 로직 및 이동 로직을 수행하지 않고 빠져나감
                if (isDie)
                    return;

                if (OnResetPanel())
                    return;

                //자신은 로컬 함수를 호출해 발사
                Fire();
                // 자신을 제외한 나머지 원격 사용자에게 Fire 함수를 원격 호출
                _networkView.RPC("Fire", RPCMode.Others);
               
                

            }
            // 리셋 버튼이 눌러져서 isReset이 true가 되었니?
            if (isReset)
            {
                // 리셋 함수 호출
                OnClickReset();
                tr.position = new Vector3(Random.Range(-20.0f, 20.0f), 0.0f, Random.Range(-20.0f, 20.0f));
                //   _networkView.RPC("OnClickReset", RPCMode.Others);
            }

            //CharacterController의 속도벡터를 로컬벡터로 변환
            Vector3 localVelocity = tr.InverseTransformDirection(controller.velocity);
            // 전진후진 방향의 가속도
            Vector3 forwardDir = new Vector3(0f, 0f, localVelocity.z);
            // 좌우방향의 가속도
            Vector3 rightDir = new Vector3(localVelocity.x, 0f, 0f);
           // Debug.Log("나 GameUI.isTextVictoryCall = " + OnResetPanel());
        }// 상대 객체 업데이트
        else // 원격 플레이어일때 수행
        {
            // 현재 좌표와 전송받은 새로운 좌표 간의 거리차가 크다면 바로 이동
            if(Vector3.Distance(tr.position, currPos) >= 2.0f)
            {
                tr.position = currPos;
                tr.rotation = currRot;
            }else
            {
                // 전송받아온 변경된 위치로 부드럽게 이동
                tr.position = Vector3.Lerp(tr.position, currPos, Time.deltaTime * 10.0f);
                // 전송받아온 변경된 각도로 부드럽게 회전
                tr.rotation = Quaternion.Slerp(tr.rotation, currRot, Time.deltaTime * 10.0f);
            }
           // Debug.Log("상대 GameUI.isTextVictoryCall = " + OnResetPanel());
        }
    }

    // RPC 함수 지정을 위해 반드시 [RPC] 어트리뷰트를 명시
    [RPC]
    void Fire()
    {
        // 총알 생성
        GameObject.Instantiate(bullet, firePos.position, firePos.rotation);
    }

    //NetworkView 컴포넌트에서 호출해 주는 콜백 함수
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        //  Debug.Log("OnSerializeNetworkView함수 호출");
        // 로컬 플레이어의 위치 및 회전 정보 송신
        if (stream.isWriting) // 데이터 송신중이면
        {
            // 보낼 데이터들 준비
            Vector3 pos = tr.position;
            Quaternion rot = tr.rotation;
            float time = myTime;
            int _hp = hp;
            bool isMoje = gameUI.isTextVictoryCall;
            // 리셋이면
            if (isReset)
            {
                //Debug.Log(" === 초기화 === ");
                // 초기화
                myTime = 0;
                hp = 100;
                _hp = 100;
                time = 0;
                isReset = false;
                isResetRcv = true;
            }
            //Debug.Log("isReset = " + isReset);
            //Debug.Log("내 점령시간 = " + myTime);
            // 데이터 전송
            stream.Serialize(ref _hp);
            stream.Serialize(ref time);
            stream.Serialize(ref pos);
            stream.Serialize(ref rot);
            stream.Serialize(ref isMoje);
            // 이벤트 발생 (내가 조종하는 객체 해당하는 것만)
            // 이곳에서 이벤트를 발생시키면 값이 반영된다.
            OnPlayerHp(hp);
            OnMyTime((int)myTime);
            OnVictory();

            
        }
        else
        {   // 데이터 수신 중이면(송신중이 아닐때)
            // 원격 플레이어의 위치 및 회전 정보 수신
            float revTime = 0.0f;
            Vector3 revPos = Vector3.zero;
            Quaternion revRot = Quaternion.identity;
            int revHp = 0;
            bool revMoje = false;
            // 데이터 수신
            stream.Serialize(ref revHp);
            stream.Serialize(ref revTime);
            stream.Serialize(ref revPos);
            stream.Serialize(ref revRot);
            stream.Serialize(ref revMoje);
            // 리셋이면
            if (isResetRcv)
            {
                //Debug.Log(" === 초기화 === ");
                // 초기화
                myTime = 0;
                revTime = 0.0f;
                hp = 100;
                revHp = 100;
                isResetRcv = false;
            }

            // Debug.Log("받은 적의 Hp = " + revHp);
            // 받은 데이터 적용
            hp = revHp;
            myTime = revTime;
            currPos = revPos;
            currRot = revRot;

            Debug.Log("revMoje = " + revMoje);
            // 이벤트 발생
            // 이벤트 발생 (상대가 조종하는 객체 해당하는 것만)
            // 이곳에서 이벤트를 발생시키면 값이 반영된다.
            // 상대 기기에서 얻어온 값은 이 else문 내에서만 유효 하다 따라서 상대 기기에서 얻어온값으로 가지고 놀고 싶으면
            // 여기서 받아온 값을 이용하여 함수를 호출한다.
            //Debug.Log("받은 적의 시간 = " + tekitime);
            OnTekiHp(hp);
            OnTekiTime((int)myTime);
            OnVictory();

        }
    }
    // 총알의 충돌 체크
    // OnTrigger함수는 충돌시 호출되는 콜벡함수이고 물리적인 반응은 하지 않는다.
    void OnTriggerEnter(Collider coll)
    {
        if(coll.gameObject.tag == "BULLET")
        {
            // 승패표시 활성화 상태일때는 무적
            if (OnResetPanel())
                return;

            Destroy(coll.gameObject);
            // 플레이어의 생명치를 차감
            hp -= 20;
            //Debug.Log("나의 hp = " + hp);
            
            
            // 생명치가 0 이하일 때 사망 및 Respawn 코루틴 함수 호출
            if (hp <= 0)
            {
                StartCoroutine(this.RespawnPlayer(respawnTime));
            }
        }
    }

    // 사망 처리 및 Respawn 철
    public IEnumerator RespawnPlayer(float waitTime)
    {
        isDie = true;
        // 플레이어의 Mesh Renderer를 비활성화하는 코루틴 함수 호출
        // 플레이어 안보이게 하기
        StartCoroutine(this.PlayerVisible(false, 0.0f));

        // Respawn 시간까지 기다림
        yield return new WaitForSeconds(waitTime);

        // Respawn 시간이 지난 후 플레이어의 위치를 무작위로 산출
        tr.position = new Vector3(Random.Range(-20.0f, 20.0f), 0.0f, Random.Range(-20.0f, 20.0f));
        // 생명치를 초기값으로 재성정
        hp = 100;

        // 플레이어를 컨트롤할 수 있게 변수 설정
        isDie = false;
        // 플레이어의 Mesh Renderer 활성화
        // 플레이어가 보이도록 함
        // 이때 0.5초 딜레이를 주는 이유는 사망위치에서 플레이어가 잠시 보였다가 사라지는 현상을 막기 위함.
        // 즉 플레이어가 새로운 좌표로 이동할 시간을 주는것
        StartCoroutine(this.PlayerVisible(true, 0.5f));
    }

    // 플레이어의 Mesh Renderer와 Character Controller의 활성/비활성 처리
    IEnumerator PlayerVisible(bool visibled, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        //플레이어 바디의 Skinned Mesh Renderer visibled 인자값에 따라 활성/비활성화 
        GetComponentInChildren<MeshRenderer>().enabled = visibled;
        //플레이어 Weapon의 Mesh Renderer 활성/비활성화
        GetComponentInChildren<MeshRenderer>().enabled = visibled;

        // 키보드 움직임에 반응하지 않게 MoveCtrl과 Charactor Controller를 활성/비활성화
        // _networkView.isMine 조건을 주어 내것만 움직이지 못하게 한다.
        if (_networkView.isMine)
        {
            // 스크립트 활성/비활성화
            GetComponent<MoveCtrl>().enabled = visibled;
            // 움직이게/못움직이게 하기
            // 위에서 스크립트를 활성/비활성화 했기때문에 굳이 안해도 될거 같지만 책에서 
            // 해주고 있으므로..
            controller.enabled = visibled;
        }
    }
    // 점령 함수
    void OnTriggerStay(Collider coll)
    {
        // 점령 중인가? 캐릭터가 닿은 tag가 Occupation 이면
        if (coll.gameObject.tag == "Occupation")
        {
            // Debug.Log("점령시작 myTime = " + (int)myTime);
            myTime += Time.deltaTime;
            
            // gameUI.Test();
            // Debug.Log(gameUI.tt);
        }
    }
    // 게임 리셋 함수
    public void GameReset()
    {
      //  Debug.Log("GameReset함수 호출");
      //  tekitime = 0;
        myTime = 0;
      //  tekiHp = 100;
        hp = 100;
    }
    // 리셋 이벤트 호출 함수
    // 리셋 과정 = 리셋버튼 클릭 -> StartReset()함수호출 -> OnClickReset()함수 호출-> GameUI에서 등록된 리셋함수 RePlay()함수 호출 -> GameReset()함수 호출
    [RPC]
    public void OnClickReset()
    {
        // 승패 패널 안불렀다로 
        OnSetPanel(false);
        //  Debug.Log("리셋 이벤트 발생");
        OnRiset();
    }

    public void StartReset()
    {
       // Debug.Log("startReset");
        isReset = true;
    }

}
