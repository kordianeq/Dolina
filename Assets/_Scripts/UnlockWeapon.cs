using UnityEngine;

public interface IUnlockable
{
    void Unlock();
}
public class UnlockWeapon : MonoBehaviour, IUnlockable
{
    public GameObject objToUnlock;
    public bool isUnlocked = false;
    public void Unlock()
    {
        isUnlocked = true;
        objToUnlock.SetActive(true);
        //GameManager.Instance.SetGun();
    }

}
