#if GPU_INSTANCER
using UnityEngine;

namespace GPUInstancer.CrowdAnimations
{
    public class GPUICrowdPrefab : GPUInstancerPrefab
    {
        public GPUICrowdRuntimeData runtimeData;
        public Animator animatorRef;

        public GPUIMecanimAnimator mecanimAnimator;
        public GPUICrowdAnimator crowdAnimator;

        public override void SetupPrefabInstance(GPUInstancerRuntimeData runtimeData, bool forceNew = false)
        {
            this.runtimeData = (GPUICrowdRuntimeData)runtimeData;
            if (animatorRef == null)
                animatorRef = GetAnimator();

            GPUICrowdPrototype prototype = (GPUICrowdPrototype)prefabPrototype;
            if (prototype.animationData.useCrowdAnimator)
            {
                if (crowdAnimator == null)
                    crowdAnimator = new GPUICrowdAnimator();
                if (crowdAnimator.activeClipCount == 0)
                    StartAnimation(prototype.animationData.clipDataList[prototype.animationData.crowdAnimatorDefaultClip].animationClip);
                else
                    crowdAnimator.UpdateIndex(this.runtimeData, gpuInstancerID - 1);
            }
            else
            {
                mecanimAnimator = new GPUIMecanimAnimator(GetAnimator());
                if (animatorRef != null)
                {
                    animatorRef.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    animatorRef.applyRootMotion = prototype.applyRootMotion;
                }
            }
        }

        public Animator GetAnimator()
        {
            return GetComponent<Animator>();
        }

        #region Crowd Animator Workflow
        public void StartAnimation(AnimationClip animationClip, float startTime = -1.0f, float speed = 1.0f, float transitionTime = 0)
        {
            crowdAnimator.StartAnimation(runtimeData, gpuInstancerID - 1, animationClip, startTime, speed, transitionTime); 
        }

        public void StartBlend(Vector4 animationWeights,
            AnimationClip animationClip1, AnimationClip animationClip2, AnimationClip animationClip3 = null, AnimationClip animationClip4 = null,
            float[] animationTimes = null, float[] animationSpeeds = null, float transitionTime = 0)
        {
            crowdAnimator.StartBlend(runtimeData, gpuInstancerID - 1, animationWeights, animationClip1, animationClip2, animationClip3, animationClip4, animationTimes, animationSpeeds, transitionTime);
        }

        public void SetAnimationWeights(Vector4 animationWeights)
        {
            crowdAnimator.SetAnimationWeights(runtimeData, gpuInstancerID - 1, animationWeights);
        }

        public void SetAnimationSpeed(float animationSpeed)
        {
            crowdAnimator.SetAnimationSpeed(runtimeData, gpuInstancerID - 1, animationSpeed);
        }

        public void SetAnimationSpeeds(float[] animationSpeeds)
        {
            crowdAnimator.SetAnimationSpeeds(runtimeData, gpuInstancerID - 1, animationSpeeds);
        }

        public float GetAnimationTime(AnimationClip animationClip)
        {
            return crowdAnimator.GetClipTime(runtimeData, animationClip);
        }

        public void SetAnimationTime(AnimationClip animationClip, float time)
        {
            crowdAnimator.SetClipTime(runtimeData, gpuInstancerID - 1, animationClip, time);
        }
        #endregion Crowd Animator Workflow
    }
}
#endif //GPU_INSTANCER