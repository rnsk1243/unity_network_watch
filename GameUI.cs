using UnityEngine;
// UI 컴포넌트에 접근하기 위해 추가한 네임스페이스
using UnityEngine.UI;
using System.Collections;

public class GameUI : MonoBehaviour {

    //Text UI 항목 연결을 위한 변수
    public Text txtScore;
    //Text UI 항목 연결을 위한 변수
    public Text tekiTxtScore;

    //Text UI 항목 연결을 위한 변수
    public Text myHp;
    //Text UI 항목 연결을 위한 변수
    public Text tekiHp;

    // 승리 또는 패배
    public Text textVictory;

    // 누적 점수를 기록하기 위한 변수
    private int myScore = 0;
    // 누적 점수를 기록하기 위한 변수
    private int tekiTotScore = 0;
    // 누가 승리인지 기록할 변수
    public bool isMyVictory = false;
    private bool isTekiVictory = false;
    // 승리패배 패널
    public GameObject isVictoryPanel;
    // 승리패배 판단 했니?
    public bool isTextVictoryCall = false;
    // 플레이어 컨트롤 스크립트 - 리셋 함수 호출 용도
    public PlayerCtrl playerCtrl;
    // Use this for initialization
    void Start () {
        // 처음 실행 후 저장된 스코어 정보 로드
        myScore = 0;
        // 나 점령시간 초기화
        DispScore(0);
        // 적 점령시간 저장 변수
        tekiTotScore = 0;
        // 적 점령시간 초기화
        TekiDispScore(0);
        // 승패 패널 숨기기
        isVictoryPanel.SetActive(false);
       // playerCtrl = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCtrl>();
    }
    //점수 누적 및 화면 표시
    [RPC]
    public void DispScore(int score)
    {
        // 승패 패널이 나오면 아무것도 안하고 리턴 시킴
        if (isTextVictoryCall)
        {
            return;
        }
        //  Debug.Log("점수 표시 함수 호출 score = " + score);
        myScore = score;
        // 텍스트 수정
        txtScore.text = "나 점령 <color=#ff0000>" + myScore.ToString() + "</color>/180 초";
    }
    [RPC]
    public void TekiDispScore(int score)
    {
        // 승패 패널이 나오면 아무것도 안하고 리턴 시킴
        if (isTextVictoryCall)
        {
            return;
        }
        //    Debug.Log("적 점수 표시 함수 호출 tekiTotScore = " + score);
        tekiTotScore = score;
        tekiTxtScore.text = "적 점령 <color=#ff0000>" + tekiTotScore.ToString() + "</color>/180 초";
    }
    //HP 화면 표시
    public void myHP(int score)
    {
        // 승패 패널 나오면 바로 리턴
        if (isTextVictoryCall)
        {
            return;
        }
        //yield return new WaitForSeconds(0.5f);
        // Debug.Log("나 HP = " + score);
        myHp.text = "나 HP <color=#ff0000>" + score.ToString() + "</color>/100";
    }
    public void TekiHP(int score)
    {
        if (isTextVictoryCall)
        {
            return;
        }
        Debug.Log("TekiHP 호출");
        // yield return new WaitForSeconds(0.5f);
        //   Debug.Log("적 HP = " + score);
        tekiHp.text = "적 HP <color=#ff0000>" + score.ToString() + "</color>/100";
    }
    // 승패 패널 출력 함수
    public void TextVictory(bool isVictory)
    {
        // 승패 패널 표시
        isVictoryPanel.SetActive(true);
        // 승리이면
        if (isVictory)
        {
          //  Debug.Log("승리 표시");
            textVictory.text = "승리";
        }// 패배이면
        else
        {
            textVictory.text = "패배";
        }
        // 승패 패널 호출 되었다고 true 저장
        isTextVictoryCall = true;
    }
    // 승패 판정 함수
    public void isVictory()
    {
       // Debug.Log("isVictory() 호출됨");
        if(isTextVictoryCall)
        {
            return;
        }
        // 내 스코어나 적 스코어가 180 이상이면
        if(myScore >= 180 || tekiTotScore >= 180)
        {
            // 내가 승리
            if(myScore > tekiTotScore)
            {
                isMyVictory = true;
            }else // 적이 승리
            {
                isTekiVictory = true;
            }
            // 승패 패널 호출 함수 호출함.
            TextVictory(isMyVictory);

        }
    }
    // 다시하기 이벤트 등록할 함수
    public void RePlay()
    {
        isVictoryPanel.SetActive(false);
        //isTextVictoryCall = false;
        isMyVictory = false;
        isTekiVictory = false;
        playerCtrl.GameReset();
        myScore = 0;
        tekiTotScore = 0;
        DispScore(0);
        TekiDispScore(0);
      //  PlayerCtrl.isReset = false;
      //  StartCoroutine(playerCtrl.RespawnPlayer(0.1f));
    }


    public bool GetVictoryPanel()
    {
        return isTextVictoryCall;
    }

    public void SetVictoryPanel(bool IsCall)
    {
        isTextVictoryCall = IsCall;
    }

    // 이벤트 등록
    void OnEnable()
    {
        // 내 체력 ui 변경 함수
        PlayerCtrl.OnPlayerHp += this.myHP;
        // 적 체력 ui 변경 함수
        PlayerCtrl.OnTekiHp += this.TekiHP;
        PlayerCtrl.OnMyTime += this.DispScore;
        PlayerCtrl.OnTekiTime += this.TekiDispScore;
        // 승패 판정 이벤트 등록
        PlayerCtrl.OnVictory += this.isVictory;
        // 다시하기 이벤트 등록
        PlayerCtrl.OnRiset += this.RePlay;
        //
        PlayerCtrl.OnResetPanel += this.GetVictoryPanel;
        PlayerCtrl.OnSetPanel += this.SetVictoryPanel;
    
    }
}
