using UnityEngine;
using System;
using System.Collections;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _lifeTime;
    [SerializeField] private float _damage;

    public event Action<Bullet> Died;

    public void StartLifeTime()
    {
        StartCoroutine(LifeTime());
    }

    private IEnumerator LifeTime()
    {
        yield return new WaitForSeconds(_lifeTime);
        Died?.Invoke(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            collision.gameObject.GetComponent<Health>().TakeDamage(_damage);
        }

        Died?.Invoke(this);
    }
}
