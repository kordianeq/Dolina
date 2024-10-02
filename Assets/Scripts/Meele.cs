using UnityEngine;

public class Meele : MonoBehaviour
{
    
    public int lightDamage, heavyDamage;
    public float timeBetweenStabs;
    bool attackL, attackH, canAttack;

    [Header("Keybinds")]
    public KeyCode lightAttack = KeyCode.Mouse0;
    public KeyCode hevyAttack = KeyCode.Mouse1;
    // Start is called before the first frame update
    void Start()
    {
        canAttack = true;
    }

    // Update is called once per frame
    void Update()
    {
        MyInput();
    }

    void MyInput()
    {

        attackL = Input.GetKey(lightAttack);
        attackH = Input.GetKey(hevyAttack);
    }

    private void OnTriggerStay(Collider other)
    {
        
        if (attackL && canAttack)
        {
            canAttack = false;
            if (other.gameObject.CompareTag("Enemy"))
            {
                
                other.gameObject.GetComponent<EnemyLogic>().ApplyDamage(lightDamage);
            }

            Invoke(nameof(AttackReset), timeBetweenStabs);
        }
        if (attackH && canAttack)
        {
            canAttack = false;
            if (other.gameObject.CompareTag("Enemy"))
            {
                
                other.gameObject.GetComponent<EnemyLogic>().ApplyDamage(heavyDamage);
            }

            Invoke(nameof(AttackReset), timeBetweenStabs);
        }
    }

    void AttackReset()
    {
        canAttack = true;
    }
}
