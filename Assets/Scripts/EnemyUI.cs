using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyFSM fsm;
    [SerializeField] private EnemyHealth health;
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private Slider healthBar;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;

        if (fsm == null)
            fsm = GetComponentInParent<EnemyFSM>();

        if (health == null)
            health = GetComponentInParent<EnemyHealth>();
    }

    private void LateUpdate()
    {
        // Billboard: selalu menghadap kamera
        if (cam != null)
            transform.forward = cam.transform.forward;

        EnemyFSM.EnemyState state = fsm.CurrentState;

        stateText.text = state.ToString();
        stateText.color = StateColor(state);

        healthBar.value = health.CurrentHealth / health.MaxHealth;
    }

    private Color StateColor(EnemyFSM.EnemyState state)
    {
        switch (state)
        {
            case EnemyFSM.EnemyState.Patrol: return Color.green;
            case EnemyFSM.EnemyState.Chase: return Color.yellow;
            case EnemyFSM.EnemyState.Attack: return Color.red;
            case EnemyFSM.EnemyState.Flee: return Color.cyan;
            default: return Color.gray;
        }
    }
}
