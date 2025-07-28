using DG.Tweening;
using UnityEngine;

public class Flipping : MonoBehaviour
{
    private bool flipped = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) Flip();
    }

    private void Flip()
    {
        flipped = !flipped;
        transform.DORotate(new(0, flipped ? 0f : 180f, 0), 0.25f);
    }

}