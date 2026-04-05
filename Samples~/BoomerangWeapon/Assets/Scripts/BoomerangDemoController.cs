using UnityEngine;
using TMPro;
using GameplayMechanicsUMFOSS.Core;
using GameplayMechanicsUMFOSS.Combat;
#if UMF_NEW_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GameplayMechanicsUMFOSS.Samples.BoomerangWeapon
{
    /// <summary>
    /// Input controller for the Boomerang Weapon demo scene.
    /// Handles throw/recall input, crosshair aiming, and HUD state display.
    /// Works alongside PlayerController (movement) and WeaponFeedbackHandler (juice).
    /// Supports both Legacy Input Manager and new Input System package.
    /// </summary>
    public class BoomerangDemoController : MonoBehaviour
    {
        [Header("Weapons")]
        [SerializeField] private BoomerangWeapon_UMFOSS[] weapons;

        [Header("Aiming")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float aimAssistRadius = 0.5f;
        [SerializeField] private float maxAimDistance = 50f;
        [SerializeField] private LayerMask aimLayers;

        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI stateLabel;
        [SerializeField] private TextMeshProUGUI controlsLabel;
        [SerializeField] private RectTransform crosshair;

        [Header("Crosshair Colors")]
        [SerializeField] private Color defaultCrosshairColor = Color.white;
        [SerializeField] private Color targetLockedColor = Color.red;
        [SerializeField] private Color recallReadyColor = Color.cyan;

        private UnityEngine.UI.Image crosshairImage;
        private int currentWeaponIndex;
        private BoomerangWeapon_UMFOSS weapon;

        private void Awake()
        {
            if (crosshair != null)
                crosshairImage = crosshair.GetComponent<UnityEngine.UI.Image>();
        }

        private void Start()
        {
            if (weapons != null && weapons.Length > 0)
            {
                currentWeaponIndex = 0;
                ActivateWeapon(currentWeaponIndex);
            }

            if (controlsLabel != null)
                controlsLabel.text = "LMB: Throw  |  R: Recall  |  Q: Switch  |  WASD: Move  |  Shift: Sprint  |  Space: Jump  |  ESC: Unlock";
        }

        private void Update()
        {
            if (weapon == null) return;

            HandleInput();
            UpdateHUD();
        }

        private void HandleInput()
        {
            if (GetLeftMousePressed() && weapon.IsEquipped && Cursor.lockState == CursorLockMode.Locked)
            {
                weapon.Throw(GetAimDirection());
            }

            if (GetRecallPressed() && !weapon.IsEquipped)
            {
                weapon.Recall();
            }

            if (GetSwitchPressed() && weapon.IsEquipped && weapons.Length > 1)
            {
                SwitchWeapon();
            }
        }

        private void SwitchWeapon()
        {
            weapons[currentWeaponIndex].gameObject.SetActive(false);
            currentWeaponIndex = (currentWeaponIndex + 1) % weapons.Length;
            ActivateWeapon(currentWeaponIndex);
        }

        private void ActivateWeapon(int index)
        {
            weapon = weapons[index];
            weapon.gameObject.SetActive(true);
        }

        private Vector3 GetAimDirection()
        {
            if (playerCamera == null)
                return transform.forward;

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            // Aim assist: if a SphereCast hits a target, throw toward that point
            if (UnityEngine.Physics.SphereCast(ray, aimAssistRadius, out RaycastHit hit, maxAimDistance, aimLayers))
            {
                return (hit.point - weapon.transform.position).normalized;
            }

            // No target found: throw toward the point maxAimDistance away on the ray
            return (ray.GetPoint(maxAimDistance) - weapon.transform.position).normalized;
        }

        private void UpdateHUD()
        {
            if (stateLabel != null)
            {
                stateLabel.text = $"{weapon.gameObject.name} — {weapon.CurrentState}";
            }

            UpdateCrosshair();
        }

        private void UpdateCrosshair()
        {
            if (crosshairImage == null || playerCamera == null) return;

            switch (weapon.CurrentState)
            {
                case WeaponStateKey.Equipped:
                    // Check if aiming at a target
                    Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                    bool aimingAtTarget = UnityEngine.Physics.SphereCast(ray, aimAssistRadius, maxAimDistance, aimLayers);
                    crosshairImage.color = aimingAtTarget ? targetLockedColor : defaultCrosshairColor;
                    break;

                case WeaponStateKey.Thrown:
                case WeaponStateKey.Embedded:
                    crosshairImage.color = recallReadyColor;
                    break;

                case WeaponStateKey.Recalling:
                    crosshairImage.color = defaultCrosshairColor;
                    break;
            }
        }

        // ───────────────────────────────────────────────
        // Input Abstraction (Legacy + New Input System)
        // ───────────────────────────────────────────────

        private bool GetLeftMousePressed()
        {
#if UMF_NEW_INPUT_SYSTEM
            if (Mouse.current != null)
                return Mouse.current.leftButton.wasPressedThisFrame;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonDown(0);
#endif
            return false;
        }

        private bool GetRecallPressed()
        {
#if UMF_NEW_INPUT_SYSTEM
            if (Keyboard.current != null)
                return Keyboard.current.rKey.wasPressedThisFrame;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.R);
#endif
            return false;
        }

        private bool GetSwitchPressed()
        {
#if UMF_NEW_INPUT_SYSTEM
            if (Keyboard.current != null)
                return Keyboard.current.qKey.wasPressedThisFrame;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Q);
#endif
            return false;
        }
    }
}
