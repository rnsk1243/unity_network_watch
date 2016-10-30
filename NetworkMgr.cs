using UnityEngine;
using System.Collections;

public class NetworkMgr : MonoBehaviour {

    //접속 IP
    private const string ip = "127.0.0.1";
    //접속 Port
    private const int port = 30000;
    //NAT 기능의 사용 여부
    private bool _useNat = false;
    // 플레이어 프리팹
    public GameObject player;
    void OnGUI()
    {
        //Debug.Log("OnGUI함수 호출");
        //현재 사용자의 네트워크에 접속 여부 판단
        if(Network.peerType == NetworkPeerType.Disconnected)
        {
            // 게임 서버 생성 버튼
            if(GUI.Button(new Rect(20,20,200,25), "Start Server"))
            {
                // 게임 서버 생성 : InitializeServer(접속자수, 포트번호, NAT사용여부)
                Network.InitializeServer(20, port, _useNat);
            }
            // 게임에 접속하는 버튼
            if(GUI.Button(new Rect(20,50, 200, 25), "Connect to Server"))
            {
                // 게임 서버 접속 : Connect(접속IP, 접속포트번호)
                Network.Connect(ip, port);
            }
        }
    }

    // 게임 서버로 구동시키고 서버 초기화가 정상적으로 완료됐을 때 자동 호출됨
    void OnServerInitialized()
    {
        CreatePlayer();
    }

    // 클라이언트로 게임 서버에 접속했을 때 자동 호출됨.
    void OnConnectedToServer()
    {
        CreatePlayer();
    }
    // 플레이어를 생성하는 함수

    void CreatePlayer()
    {
        // 플레이어의 무작위 생성 위치 산출
        Vector3 pos = new Vector3(Random.Range(-20.0f, 20.0f), 0.0f, Random.Range(-20.0f, 20.0f));
        // 네트워크 상에 플레이어를 동적 생성
        // 현재 게임에 접속한 모든 사용자에게 프리팹을 생성해주며 내부적으로
        // Buffered RPC를 호출해 나중에 접속한
        // 사용자도 미리 생성된 프리팹을 볼 수 있다.
        // Network.Instantiate(프리펩, 생성위치, 각도, 그릅) 
        // 그릅을 지정하면 그릅에만 생성되게 할 수 있다.
        Network.Instantiate(player, pos, Quaternion.identity, 0);
    }
    // 접속이 종료된 플레이어의 네트워크 객체를 모두 소멸 처리
    void OnPlayerDisconnected(NetworkPlayer netPlayer)
    {
        // 네트워크 플레이어의 모든 Buffered RPC를 소멸 처리
        Network.RemoveRPCs(netPlayer);
        // 네트워크 플레이어의 모든 네트워크 객체를 소멸 처리(게임 서버가 구동되고 있어야 이용 가능)
        Network.DestroyPlayerObjects(netPlayer);
    }
}
