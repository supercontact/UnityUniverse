using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonControl : MonoBehaviour {

    public float speed = 5f;
    public float jumpPower = 5f;
    public float gravity = 10f;
    public Camera controlCamera;

    private CharacterController characterController;
    private Vector3 nonControlledVelocity = Vector3.zero;

	// Use this for initialization
	void Awake () {
        characterController = GetComponent<CharacterController>();
    }
	
	// Update is called once per frame
	void Update () {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float rotation = controlCamera.transform.eulerAngles.y;

        Vector3 movement = Quaternion.Euler(new Vector3(0, rotation, 0)) * new Vector3(horizontal, 0, vertical) * speed;
        if (characterController.isGrounded) {
            if (Input.GetButton("Jump")) {
                nonControlledVelocity.y += jumpPower;
            } else {
                nonControlledVelocity.y = 0;
            }
        } else {
            nonControlledVelocity.y -= gravity * Time.deltaTime;
        }
        movement += nonControlledVelocity;
        characterController.Move(movement * Time.deltaTime);
	}
}
