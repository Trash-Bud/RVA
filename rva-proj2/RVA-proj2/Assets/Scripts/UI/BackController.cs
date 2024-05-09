
using Unity.VisualScripting;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
   public GameObject mainMenu;

   
   
   public void Back()
   {
      mainMenu.SetActive(true);
      this.GameObject().SetActive(false);
      
   }
}
