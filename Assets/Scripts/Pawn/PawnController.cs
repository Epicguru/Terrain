
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PawnController : MonoBehaviour
{
    public CharacterController Controller
    {
        get
        {
            if (_cc == null)
                _cc = GetComponent<CharacterController>();
            return _cc;
        }
    }
    private CharacterController _cc;

    public float Speed = 7f;

    private void Update()
    {
        Vector2 flatInput = Vector2.zero;
        if (Input.GetKey(KeyCode.A))
            flatInput.x -= 1f;
        if (Input.GetKey(KeyCode.D))
            flatInput.x += 1f;
        if (Input.GetKey(KeyCode.S))
            flatInput.y -= 1f;
        if (Input.GetKey(KeyCode.W))
            flatInput.y += 1f;

        Vector3 worldInput = transform.TransformDirection(new Vector3(flatInput.x, 0f, flatInput.y));
        Vector3 final = worldInput.normalized * Speed;

        Controller.Move(final * Time.deltaTime);
    }
}
