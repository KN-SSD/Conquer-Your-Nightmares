using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class EventClick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
 
    private Transform highlight;
    private Transform selection;
   private void Awake()
    {
    }
    public void OnPointerDown(PointerEventData eventData)
    {        
    }
    public void OnPointerUp(PointerEventData eventData)
    {
     Debug.Log("Z obiektu");
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Klik");
        SceneManager.LoadScene("SampleScene");
 
    }
    public void OnPointerEnter(PointerEventData eventData)
    {       Debug.Log("Najechano"); 
        
    }
    public void OnPointerExit(PointerEventData eventData)
    {}
}
