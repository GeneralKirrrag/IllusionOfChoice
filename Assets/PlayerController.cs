using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Internals")]
    private CharacterController controller;
    private Rigidbody RB;
    private CapsuleCollider CC;
    private Vector3 playerVelocity;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float playerSpeed = 2.0f;
    [SerializeField] private float groundDrag = 5f;
    [SerializeField] private float jumpHeight = 1.0f;
    private float gravityValue = -9.81f;

    [Header("Externals")]
    public GameObject Cube;
    private Vector3 CubePosition;
    private Quaternion CubeRotation;
    public Transform LowerRoom;
    public Transform UpperRoom;
    public Image UICursor;
    public GameObject ButtonStartPrompt;
    public Cinemachine.CinemachineVirtualCamera VirtualCamera;
    public bool canMove = false;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        RB = GetComponent<Rigidbody>();
        CC = GetComponent<CapsuleCollider>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        CubePosition = Cube.transform.position;
        CubeRotation = Cube.transform.rotation;

        ButtonStartPrompt.gameObject.SetActive(true);
        VirtualCamera.enabled = false;
        canMove = false;
    }

    void Update()
    {

        if (Input.anyKeyDown) {
            VirtualCamera.enabled = true;
            canMove = true;
            ButtonStartPrompt.gameObject.SetActive(false);
        }

        if (isButton() && (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Activate")) && !Cube.activeInHierarchy) {
            Cube.transform.position = CubePosition;
            Cube.transform.rotation = CubeRotation;
            Cube.SetActive(true);
        }

        if (isGrounded() && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        if (isGrounded()) RB.drag = groundDrag;
        else RB.drag = 0;

        if (!canMove) return;

        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        move = (Camera.main.transform.forward * move.z + Camera.main.transform.right * move.x).normalized;
        move.y = 0;
        RB.AddForce(move * playerSpeed * 10f, ForceMode.Force);

        //controller.Move(move * Time.deltaTime * playerSpeed);

        // if (move != Vector3.zero)
        // {
        //     gameObject.transform.forward = move;
        // }

        // Makes the player jump
        if (Input.GetButtonDown("Jump") && isGrounded())
        {
            RB.AddForce(transform.up * jumpHeight, ForceMode.Impulse);
        }

        Vector3 flatVel = new Vector3(RB.velocity.x, 0f, RB.velocity.z);

        if (flatVel.magnitude > playerSpeed) {
            Vector3 limitedVel = flatVel.normalized * playerSpeed;
            RB.velocity = new Vector3(limitedVel.x, RB.velocity.y, limitedVel.z);
        }
    }

    void OnTriggerEnter(Collider other) {
        Cube.SetActive(false);
        Vector3 relativePosition = UpperRoom.InverseTransformPoint(transform.position);
        transform.position = LowerRoom.position + relativePosition;
    }

    bool isGrounded() {
        return Physics.Raycast(transform.position + CC.center, Vector3.down, CC.center.y + 0.2f, whatIsGround);
    }

    bool isButton() {
        RaycastHit hit;
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.SphereCast(ray, 0.2f, out hit, 3f)) {
            if (hit.collider.gameObject.layer == 8) {
                Debug.Log("Can Reach Button");
                UICursor.gameObject.SetActive(true);
                return true;
            }
            else{
                UICursor.gameObject.SetActive(false);
                return false;
            } 
        }
        else {
            UICursor.gameObject.SetActive(false);
            return false;
        }
    }
}
