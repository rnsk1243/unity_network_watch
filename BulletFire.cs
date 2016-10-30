using UnityEngine;
using System.Collections;

public class BulletFire : MonoBehaviour {
    // 총알 유지 시간
    private float lifeTime = 5.0f;
	// Use this for initialization
	void Start () {
        //Rigidbody의 속도를 Forward 방향으로 설정
        GetComponent<Rigidbody>().velocity = transform.forward * 10.0f;
        // 일정 시간이 지난 후 제거
        Destroy(gameObject, lifeTime);
	}
}
