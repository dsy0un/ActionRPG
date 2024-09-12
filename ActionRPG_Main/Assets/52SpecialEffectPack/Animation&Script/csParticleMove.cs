using UnityEngine;
using System.Collections;

public class csParticleMove : MonoBehaviour
{
    public float speed = 0.1f;

    public GameObject explosion;
    public float expRange;

    bool expSign = true;

    float magicDamage = 40.0f;

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, expRange);
    }

    void Update () {
        transform.Translate(Vector3.forward * speed);
        MagicHit();
	}

    void MagicHit()
    {
        int layerMask = 1 << 7;
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, expRange, Vector3.up, 0, layerMask);
        if (hits.Length > 0)
        {
            foreach (RaycastHit hit in hits)
            {
                hit.transform.SendMessage("MonsterHPDown", magicDamage); // 충돌한 대상(몬스터)에게 MonsterHPDown 함수를 찾아 호출
                GameObject exp = Instantiate(explosion, hit.transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.transform.gameObject.layer == 7)
    //    {
    //        if (expSign)
    //        {
    //            other.transform.SendMessage("MonsterHPDown", magicDamage); // 충돌한 대상(몬스터)에게 MonsterHPDown 함수를 찾아 호출
    //            GameObject exp = Instantiate(explosion, other.transform.position, Quaternion.identity);
    //            Destroy(gameObject);
    //        }
    //    }
    //}
}
