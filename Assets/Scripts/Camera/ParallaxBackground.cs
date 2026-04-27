using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxBackground : MonoBehaviour
{
    public Vector2 parallaxEffectMultiplier = Vector2.one;
    public bool infiniteHorizontal = true;
    public bool infiniteVertical;

    Transform cameraTransform;
    SpriteRenderer spriteRenderer;
    Vector3 lastCameraPosition;
    float textureUnitSizeX;
    float textureUnitSizeY;

    void Start()
    {
        if (Camera.main == null)
        {
            enabled = false;
            return;
        }

        cameraTransform = Camera.main.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastCameraPosition = cameraTransform.position;

        CacheTextureUnitSize();
    }

    void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        Vector3 cameraDelta = cameraTransform.position - lastCameraPosition;
        transform.position += new Vector3(
            cameraDelta.x * parallaxEffectMultiplier.x,
            cameraDelta.y * parallaxEffectMultiplier.y,
            0f);

        lastCameraPosition = cameraTransform.position;
        LoopIfNeeded();
    }

    void CacheTextureUnitSize()
    {
        if (spriteRenderer.sprite == null)
            return;

        Sprite sprite = spriteRenderer.sprite;
        textureUnitSizeX = sprite.rect.width / sprite.pixelsPerUnit;
        textureUnitSizeY = sprite.rect.height / sprite.pixelsPerUnit;
    }

    void LoopIfNeeded()
    {
        if (spriteRenderer.sprite == null)
            return;

        Vector3 position = transform.position;

        if (infiniteHorizontal && textureUnitSizeX > 0f)
        {
            float cameraDistanceX = cameraTransform.position.x - position.x;
            if (Mathf.Abs(cameraDistanceX) >= textureUnitSizeX)
                position.x += textureUnitSizeX * Mathf.Sign(cameraDistanceX);
        }

        if (infiniteVertical && textureUnitSizeY > 0f)
        {
            float cameraDistanceY = cameraTransform.position.y - position.y;
            if (Mathf.Abs(cameraDistanceY) >= textureUnitSizeY)
                position.y += textureUnitSizeY * Mathf.Sign(cameraDistanceY);
        }

        transform.position = position;
    }
}
