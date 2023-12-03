using UnityEngine;
using UnityEngine.UI;

public class ExplodedImage : MonoBehaviour
{
    public Sprite explodedImage;
    
    public void ReplaceImage() => GetComponent<Image>().sprite = explodedImage;
}
