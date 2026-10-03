using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyFSM fsm;
    [SerializeField] private Animator animator;

    private static readonly int StateHash = Animator.StringToHash("State");

    private EnemyFSM.EnemyState lastState;
    private bool initialized;

    private void Awake()
    {
        if (fsm == null)
            fsm = GetComponentInParent<EnemyFSM>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        // Kirim nilai enum hanya saat state berubah
        if (initialized && fsm.CurrentState == lastState)
            return;

        initialized = true;
        lastState = fsm.CurrentState;

        // Patrol = 0, Chase = 1, Attack = 2, Flee = 3, Dead = 4
        animator.SetInteger(StateHash, (int)lastState);
    }
}
