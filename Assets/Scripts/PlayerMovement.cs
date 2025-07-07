using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerMovement
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Tooltip("Normal yürüme hızı")]
        public float walkSpeed = 6f;
        [Tooltip("Sprint hızı (Left Shift basılı)")]
        public float sprintSpeed = 12f;
        [Tooltip("Zeminde ivmelenme katsayısı")]
        public float acceleration = 30f;
        [Tooltip("Havada ivmelenme katsayısı")]
        public float airAcceleration = 15f;
        [Tooltip("Yerde uygulanacak sürtünme")]
        public float friction = 6f;
        [Tooltip("Yerçekimi kuvveti")]
        public float gravity = 20f;
        [Tooltip("Zıplama kuvveti")]
        public float jumpForce = 6.5f;
        [Tooltip("Yatay maksimum hız (bunny hopping sırasında sınır)")]
        public float maxSpeed = 20f;
        [Tooltip("Havada hareketin kontrol faktörü (0-1 arası)")]
        public float airControl = 0.5f;

        private CharacterController controller;
        private Vector3 velocity;
        private Vector2 inputDir;
        private bool wishJump = false;

        void Start()
        {
            controller = GetComponent<CharacterController>();
        }

        // Inputları burada okuyarak fizik hesaplamalarından ayırıyoruz.
        void Update()
        {
            // Yatay input: Horizontal ve Vertical axis'lerden ham değerleri alıyoruz.
            inputDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            // Zıplama girişi: Jump tuşuna basıldığında (Space) zıplama isteğini işaretliyoruz.
            if (controller.isGrounded && (Input.GetButtonDown("Jump") || Input.GetAxis("Mouse ScrollWheel") > 0))
            {
                wishJump = true;
            }
        }

        // Fizik hesaplamaları için FixedUpdate kullanılıyor.
        void FixedUpdate()
        {
            bool isGrounded = controller.isGrounded;

            if (isGrounded)
            {
                // Zemine temas halinde, düşüşte oluşan negatif y hızını sıfırlıyoruz.
                if (velocity.y < 0)
                    velocity.y = -2f; // Karakterin zeminde "yapışık" kalması için hafif bir aşağı kuvvet

                // Yerdeyken sürtünme ile yavaşlatma
                ApplyFriction();

                // Hedef hız: sprint tuşuna bağlı olarak belirleniyor.
                float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

                // İstenilen hareket yönünü belirle (transform.right ve transform.forward baz alınarak)
                Vector3 wishDir = transform.right * inputDir.x + transform.forward * inputDir.y;
                if (wishDir.sqrMagnitude > 0.001f)
                {
                    wishDir.Normalize();
                    Accelerate(wishDir, targetSpeed, acceleration);
                }

                // Zıplama isteği varsa zıplama yap
                if (wishJump)
                {
                    velocity.y = jumpForce;
                    wishJump = false;
                }
            }
            else // Hava durumunda
            {
                // Yerçekimi uygulaması
                velocity.y -= gravity * Time.fixedDeltaTime;

                // Hava hareketi: Input yönüne göre ivmelenme, ancak havada kontrol daha sınırlı
                Vector3 wishDir = transform.right * inputDir.x + transform.forward * inputDir.y;
                if (wishDir.sqrMagnitude > 0.001f)
                {
                    wishDir.Normalize();
                    AirAccelerate(wishDir, airAcceleration, airControl);
                }
            }

            // Bunny hopping sırasında yatay hızın belirli bir maksimum değeri aşmaması için clamp uyguluyoruz.
            Vector3 horizontalVel = new Vector3(velocity.x, 0, velocity.z);
            if (horizontalVel.magnitude > maxSpeed)
            {
                horizontalVel = horizontalVel.normalized * maxSpeed;
                velocity.x = horizontalVel.x;
                velocity.z = horizontalVel.z;
            }

            // Son olarak, karakteri hareket ettiriyoruz.
            controller.Move(velocity * Time.fixedDeltaTime);
        }

        /// <summary>
        /// Zeminde verilen yönde ivmelenme uygular.
        /// </summary>
        void Accelerate(Vector3 wishDir, float targetSpeed, float accel)
        {
            // Mevcut hızın, istenen yöndeki bileşenini hesapla.
            float currentSpeed = Vector3.Dot(new Vector3(velocity.x, 0, velocity.z), wishDir);
            float addSpeed = targetSpeed - currentSpeed;
            if (addSpeed <= 0)
                return;

            float accelSpeed = accel * Time.fixedDeltaTime * targetSpeed;
            if (accelSpeed > addSpeed)
                accelSpeed = addSpeed;

            velocity += wishDir * accelSpeed;
        }

        /// <summary>
        /// Havada verilen yönde ivmelenme uygular (bunny hopping için önemli).
        /// </summary>
        void AirAccelerate(Vector3 wishDir, float accel, float controlFactor)
        {
            // Mevcut yatay hızın, istenen yöndeki bileşenini hesapla.
            float currentSpeed = Vector3.Dot(new Vector3(velocity.x, 0, velocity.z), wishDir);
            float addSpeed = walkSpeed - currentSpeed; // Hedef hız olarak normal yürüme hızı kullanılabilir.
            if (addSpeed <= 0)
                return;

            float accelSpeed = accel * Time.fixedDeltaTime * walkSpeed;
            if (accelSpeed > addSpeed)
                accelSpeed = addSpeed;

            // Air control faktörü ile ivmeyi sınırlandırıyoruz.
            velocity += wishDir * accelSpeed * controlFactor;
        }

        /// <summary>
        /// Zeminde hareket halindeyken sürtünme uygular (ivme yavaşlatma).
        /// </summary>
        void ApplyFriction()
        {
            Vector3 horizontalVel = new Vector3(velocity.x, 0, velocity.z);
            float speed = horizontalVel.magnitude;
            if (speed < 0.1f)
            {
                velocity.x = 0;
                velocity.z = 0;
                return;
            }
                
            float drop = speed * friction * Time.fixedDeltaTime;
            float newSpeed = speed - drop;
            if (newSpeed < 0)
                newSpeed = 0;

            horizontalVel *= newSpeed / speed;
            velocity.x = horizontalVel.x;
            velocity.z = horizontalVel.z;
        }
    }
}
