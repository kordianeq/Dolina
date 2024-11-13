using TMPro;
using UnityEngine;

public class EnemyLogic : MonoBehaviour
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
        text.text = hp.ToString();
    }

    public void ApplyDamage(float damage)
    {
        hp = hp - (int)Mathf.Round(damage);
    }
}
