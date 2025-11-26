using UnityEngine;
using UnityEngine.InputSystem;

public class Controls : MonoBehaviour
{
    CharacterController characterController;

    Vector2 CurMovInpt;
    Vector3 CurMov;

    float MoveSpeed = 10.0f;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }
    
    void Update()
    {
        Movement();
    }

    void Movement()
    {
        CurMovInpt.x = Input.GetAxis("Vertical");
        CurMovInpt.y = Input.GetAxis("Horizontal");

        Vector3 fwd = Camera.main.transform.forward;
        Vector3 rgt = Camera.main.transform.right;
        fwd.y = 0;
        rgt.y = 0;
        fwd = fwd.normalized;
        rgt = rgt.normalized;
        Vector3 RelVertMov = CurMovInpt.x * fwd;
        Vector3 RelHorMov = CurMovInpt.y * rgt;
        CurMov = RelVertMov + RelHorMov;

        if (characterController.isGrounded)
        {
            float groundgrav = -0.5f * Time.deltaTime;
            CurMov.y = groundgrav;
        }
        else
        {
            float grav = -50.0f * Time.deltaTime;
            CurMov.y += grav;
        }

        characterController.Move(CurMov * MoveSpeed * Time.deltaTime);
    }
}
