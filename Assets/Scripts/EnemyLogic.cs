using TMPro;
using UnityEngine;

public class EnemyLogic : MonoBehaviour, IDamagable
{
    public int hp;
    public int maxhp;

    public TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {
        hp = maxhp;
    }

    // Update is called once per frame
    void Update()
    {
        if( hp <= 0 )
        {
            Destroy(gameObject);
        }
        if(text) text.text = hp.ToString();
    }

    public void Damaged(float damage)
    {
        //Debug.Log("Enemy damaged for " + damage);
        hp = hp - (int)Mathf.Round(damage);
    }
}
