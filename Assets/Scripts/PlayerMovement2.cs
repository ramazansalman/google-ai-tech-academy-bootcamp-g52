using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerMovement2
{
    using UnityEngine;

    public class PlayerMovement2 : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 5.0f;     // Defailt Move Speed
        public float sprintMultiplier = 1.1f; // Sprint yapınca hız çarpanı
        public float jumpForce = 8.0f;     // Zıplama kuvveti
        public float gravity = 20.0f;      // Yer çekimi
        public float friction = 6.0f;      // Sürtünme (daha yumuşak durma)
        public float airControl = 0.1f;    // Hava kontrol oranı
        public float maxVelocity = 7.0f;  // Max hız sınırı (bunnyhop için)

        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;

        void Start()
        {
            controller = GetComponent<CharacterController>();
        }

        void Update()
        {
            Move();
            ApplyGravity();
            controller.Move(velocity * Time.deltaTime);
        }

        void Move()
        {
            isGrounded = controller.isGrounded;

            // Inputları al
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveZ = Input.GetAxisRaw("Vertical");
            Vector3 moveDirection = transform.right * moveX + transform.forward * moveZ;

            // Yerdeysen hızlanma ve sürtünme uygula
            if (isGrounded)
            {
                GroundMove(moveDirection);
            }
            else
            {
                AirMove(moveDirection);
            }

            // **Zıplama - Space veya Mouse Wheel Up (mwheelup)**
            if ((Input.GetKeyDown(KeyCode.Space) || Input.GetAxis("Mouse ScrollWheel") > 0) && isGrounded)
            {
                velocity.y = jumpForce;
            }

            // Sprint
            if (Input.GetKey(KeyCode.LeftShift) && isGrounded)
            {
                velocity.x *= sprintMultiplier;
                velocity.z *= sprintMultiplier;
            }
        }

        void GroundMove(Vector3 moveDirection)
        {
            // Yerdeyken sürtünme uygula
            velocity.x *= 1 - (friction * Time.deltaTime);
            velocity.z *= 1 - (friction * Time.deltaTime);

            // Hareket et
            Vector3 desiredVelocity = moveDirection.normalized * moveSpeed;
            velocity.x = Mathf.Lerp(velocity.x, desiredVelocity.x, Time.deltaTime * 10);
            velocity.z = Mathf.Lerp(velocity.z, desiredVelocity.z, Time.deltaTime * 10);
        }

        void AirMove(Vector3 moveDirection)
        {
            // Hava kontrolünü uygula
            velocity.x += moveDirection.x;
            velocity.z += moveDirection.z * airControl;

            // Maksimum hız sınırla
            velocity.x = Mathf.Clamp(velocity.x, -maxVelocity, maxVelocity);
            velocity.z = Mathf.Clamp(velocity.z, -maxVelocity, maxVelocity);
        }

        void ApplyGravity()
        {
            if (!isGrounded)
            {
                velocity.y -= gravity * Time.deltaTime;
            }
        }
    }
}
