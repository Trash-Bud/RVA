using DefaultNamespace;
using UnityEngine;
using Vuforia;

public class DeckController : MonoBehaviour
{

    public GameObject DeckScreen;
    public GameObject Canvas;

    public UnityEngine.UI.Image Aquarihno;
    public UnityEngine.UI.Image Pyroscarab;
    public UnityEngine.UI.Image Pricklash;

    public Sprite AquarihnoSprite;
    public Sprite PyroscarabSprite;
    public Sprite PricklashSprite;

    
    public bool AquarihnoChanged;
    public bool PyroscarabChanged;
    public bool PricklashChanged;

    
    // Start is called before the first frame update


    public void OnBack()
    {
        DeckScreen.SetActive(false);
        Canvas.SetActive(true);
        VuforiaBehaviour.Instance.enabled = true;

    }
    
    public void UpdateCards()
    {
        VuforiaBehaviour.Instance.enabled = false;
        DeckScreen.SetActive(true);
        Canvas.SetActive(false);
        var captured = CaptureInfo.capturedTargets;

        foreach (var cap in captured)
        {
            switch (cap)
            {
                case Target.Aquarhin:
                    if(!AquarihnoChanged)
                    {
                        Aquarihno.sprite = AquarihnoSprite;
                        AquarihnoChanged = true;
                    }
                    break;
                case Target.Pyroscarab:
                    if(!PyroscarabChanged)
                    {
                        Pyroscarab.sprite = PyroscarabSprite;
                        PyroscarabChanged = true;
                    }
                    break;
                
                case Target.Pricklash:
                    if(!PricklashChanged)
                    {
                        Pricklash.sprite = PricklashSprite;
                        PricklashChanged = true;
                    }
                    break;
                
            }
        }

        
    }
}
