using UnityEngine;

public class YemmaAnim
{
    private Animator animator;
    private int currentStateId = -1;
    
    public YemmaAnim(Animator animator)
    {
        this.animator = animator;
    }
    
    public YemmaAnim(Transform ownerTransform)
    {
        animator = ownerTransform.GetComponent<Animator>();
        if (animator == null)
            animator = ownerTransform.GetComponentInChildren<Animator>();
    }
    
    /// <summary>
    /// Transita para uma nova animação por ID
    /// </summary>
    public void PlayAnimation(int stateId, float crossfadeTime = 0.2f)
    {
        if (animator == null || currentStateId == stateId) return;
        
        animator.CrossFadeInFixedTime(stateId, crossfadeTime);
        currentStateId = stateId;
    }
    
    /// <summary>
    /// Transita para uma nova animação por nome
    /// </summary>
    public void PlayAnimation(string stateName, float crossfadeTime = 0.2f)
    {
        if (animator == null) return;
        
        int stateId = Animator.StringToHash(stateName);
        PlayAnimation(stateId, crossfadeTime);
    }
    
    /// <summary>
    /// Define um parâmetro float no animator
    /// </summary>
    public void SetFloat(string parameterName, float value)
    {
        if (animator == null) return;
        animator.SetFloat(parameterName, value);
    }
    
    /// <summary>
    /// Define um parâmetro bool no animator
    /// </summary>
    public void SetBool(string parameterName, bool value)
    {
        if (animator == null) return;
        animator.SetBool(parameterName, value);
    }
    
    /// <summary>
    /// Transita para uma nova animação por enum
    /// </summary>
    public void PlayAnimation(YemmaAnimationState animState, float crossfadeTime = 0.2f)
    {
        string animationName = ConvertEnumToAnimatorName(animState);
        PlayAnimation(animationName, crossfadeTime);
    }
    
    /// <summary>
    /// Converte o enum para o nome correto no Animator
    /// </summary>
    private string ConvertEnumToAnimatorName(YemmaAnimationState animState)
    {
        switch (animState)
        {
            case YemmaAnimationState.BakeIdle: return "Bake-Idle";
            case YemmaAnimationState.BakeWalk: return "Bake-Walk";
            case YemmaAnimationState.BakeRun: return "Bake-Run";
            case YemmaAnimationState.BakeJump: return "Bake-Jump";
            case YemmaAnimationState.BakeFall: return "Bake-Fall";
            case YemmaAnimationState.BakeLand: return "Bake-Land";
            case YemmaAnimationState.BakeHandEdge: return "Bake-HandEdge";
            case YemmaAnimationState.BakeClimb: return "Bake-Climb";
            default: return "Bake-Idle";
        }
    }
    
    /// <summary>
    /// Dispara um trigger no animator
    /// </summary>
    public void SetTrigger(string triggerName)
    {
        if (animator == null) return;
        animator.SetTrigger(triggerName);
    }
    
    /// <summary>
    /// Retorna o ID do estado atual
    /// </summary>
    public int GetCurrentStateId()
    {
        return currentStateId;
    }
    
    /// <summary>
    /// Verifica se uma animação está tocando
    /// </summary>
    public bool IsPlayingAnimation(int stateId)
    {
        if (animator == null) return false;
        return animator.GetCurrentAnimatorStateInfo(0).shortNameHash == stateId;
    }
    
    /// <summary>
    /// Retorna o Animator
    /// </summary>
    public Animator GetAnimator()
    {
        return animator;
    }
}