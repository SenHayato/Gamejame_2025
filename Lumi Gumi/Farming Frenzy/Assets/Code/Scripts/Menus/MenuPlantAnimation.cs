using System.Collections;
using UnityEngine;

public class MenuPlantAnimation : MonoBehaviour
{
    #region Editor Fields
    [SerializeField] private Animator animator;
    [SerializeField] private string animationTrigger = "PlantAnimation";
    [SerializeField] private float minDelay = 10f;
    [SerializeField] private float maxDelay = 15f;
    #endregion

    #region Properties
    #endregion

    #region Methods
    void Start()
    {
        StartCoroutine(PlayAnimationWithDelay());
    }

    private IEnumerator PlayAnimationWithDelay()
    {
        while (true)
        {
            float randomDelay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(randomDelay);

            animator.SetTrigger(animationTrigger);
        }
    }
    #endregion




}
