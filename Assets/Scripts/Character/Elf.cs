using UnityEngine;
using System.Collections;

public class Elf : Companion
{
    [SerializeField] private GameObject _projectile;

    protected override void Awake()
    {
        // call base class
        base.Awake();
    }

    // protected override IEnumerator Attack()
    // {
    //     Anim.SetTrigger("Attack");

    //     // summon projectile 
    //     Vector3 projPos = transform.position+transform.up;
    //     GameObject proj = Instantiate(_projectile, projPos, Quaternion.identity);

    //     // launch projectile at opponent
    //     Vector3 opPos = CurrentOpponent.transform.position+(CurrentOpponent.transform.up/2f);
    //     float _distance = Vector2.Distance(opPos, proj.transform.position);
    //     while (_distance > 0.1f)
    //     {
    //         _distance = Vector2.Distance(opPos, proj.transform.position);
    //         Vector2 projVector = (opPos - proj.transform.position).normalized;
    //         // move projectile
    //         proj.GetComponent<Rigidbody2D>().linearVelocity = projVector * 6f;
    //         yield return new WaitForFixedUpdate();
    //     }

    //     yield return new WaitForSeconds(.01f);

    //     Destroy(proj);

    //     yield return new WaitForSeconds(.1f);

    //     // damage
    //     CurrentOpponent.Damage(Strength);
    // }
}

