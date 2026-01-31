using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ScrollingUIBackground : MonoBehaviour
{
    public float speed;
    public Vector2 direction;

    Image img;
    void Start()
    {
        img = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        img.material.mainTextureOffset += -direction * speed * Time.deltaTime;
    }
}
