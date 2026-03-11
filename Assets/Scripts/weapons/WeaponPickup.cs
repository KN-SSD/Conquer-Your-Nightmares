using UnityEngine;
using System.Collections.Generic;

public class WeaponPickup : MonoBehaviour
{
    [Tooltip("Wszystkie bronie do wyboru na starcie.")]
    [SerializeField] private List<GameObject> allPickupObjects;

    [Tooltip("Obiekt z konkretna bronia wewnatrz Gracza (np. Weapon_Sword, Weapon_Axe).")]
    [SerializeField] private Weapon playerWeaponToEquip;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && GameManager.Instance.currentState == GameManager.GameState.WeaponSelection)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            
            if (player != null && playerWeaponToEquip != null)
            {
                player.EquipWeapon(playerWeaponToEquip);
                GameManager.Instance.StartRun();

                foreach (GameObject pickup in allPickupObjects)
                {
                    if (pickup != null) Destroy(pickup);
                }
            }
        }
    }
}