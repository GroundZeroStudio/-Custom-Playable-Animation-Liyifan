using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayableController : MonoBehaviour
{
    public PlayableControllerParam mPlayableControllerParam;
    public PlayableLayerParam[] mPlayableLayerParams;

    //Playable API
    private AnimationLayerMixerPlayable mLayerMixer;
    private AnimationPlayableOutput mOutput;
    private PlayableGraph mGraph;

    //GetComponent()
    private Animator mAnimator;

    //Layer相关携程
    private Coroutine mLayerCoroutine;

    //混合器实际上负责运动过渡
    private List<PlayableMixer> mPlayableMixers = new List<PlayableMixer>();

    void Awake()
    {
        if (mPlayableControllerParam == null||mPlayableControllerParam!=null&&mPlayableControllerParam.layerCount!=mPlayableLayerParams.Length)
        {
            Debug.Log("请配置动画控制器参数或LayerCount与LayerParam配置不一致");
            return;
        }
        this.mAnimator = this.GetComponentInChildren<Animator>();
        //PlayableAPI
        this.mGraph = PlayableGraph.Create();
        //创建LayerMixer
        this.mLayerMixer = AnimationLayerMixerPlayable.Create(mGraph, 2);

        //初始化Layer和Mask
        for (int i = 0; i < mPlayableControllerParam.layerCount; i++)
        {
            var rParam = this.mPlayableLayerParams[i];
            this.mPlayableMixers.Add(new PlayableMixer(this, mGraph));
            if (rParam.LayerAvatarMask != null)
                this.mLayerMixer.SetLayerMaskFromAvatarMask(rParam.LayerIndex, rParam.LayerAvatarMask);
        }
        //初始化输出
        this.mOutput = AnimationPlayableOutput.Create(mGraph, "output", mAnimator);
        this.mGraph.Play();
    }

    private void Update()
    {
        //更新所有Mixer的播放时间，不是Loop的停止，是Loop的设置成0
        this.CheckClipLoop();
    }

    private void CheckClipLoop()
    {
        for (int i = 0; i < this.mPlayableMixers.Count; i++)
            if (this.mPlayableMixers[i].isFinishedPlay && this.mPlayableMixers[i].mPlayParam.mLoop)
                this.mPlayableMixers[i].mCurPlayable.SetTime(0f);// 重置时间并循环播放
            else if (this.mPlayableMixers[i].isFinishedPlay && this.mPlayableMixers[i].mPlayParam.mLoop == false)
                Debug.Log(this.mPlayableMixers[i].mCurPlayable.GetAnimationClip().name+":已经播放完毕");
    }

    void OnDestroy()
    {
        this.mGraph.Destroy();
    }

    #region 功能
    // 播放动作
    public bool play(AnimationClip rClip, PlayableAnimationParam rParam)
    {
        if (rClip == null || rParam == null)
            return false;

        PlayableMixer rPlayableMixer = this.mPlayableMixers[rParam.mLayer];
        rPlayableMixer.SetPlayableParam(rParam);

        //切断
        this.mGraph.Disconnect(this.mLayerMixer, rParam.mLayer);

        // 重新连接
        rPlayableMixer.Reconnect(rClip);
        this.mLayerMixer.ConnectInput(rParam.mLayer, rPlayableMixer.mMixer, sourceOutputIndex: 0, weight: 1f);

        //输出
        this.mOutput.SetSourcePlayable(this.mLayerMixer);

        return true;
    }

    // 播放(播放单一动画)
    public bool play(AnimationClip rClip, bool rLoop, int rLayer = 0)
    {
        PlayableAnimationParam param = new PlayableAnimationParam(rLayer, rLoop);
        return play(rClip, param);
    }
    
    //外层调用层级过度
    public void CrossFadeLayer(int rLayer, float rDuration, bool rEnable/*开启或关闭*/)
    {
        if (this.mLayerCoroutine != null)
            StopCoroutine(this.mLayerCoroutine);

        this.mLayerCoroutine = StartCoroutine(this.CrossFadeLayerWeight(rLayer,rDuration,rEnable));
    }

    //层级融合过度
    private IEnumerator CrossFadeLayerWeight(int rLayer, float rDuration, bool rEnable/*开启或关闭*/)
    {
        Debug.Log(rEnable + " " + this.mLayerMixer.GetInputWeight(rLayer));
        // 如果已经启用/禁用则不处理
        if (rEnable && this.mLayerMixer.GetInputWeight(rLayer) >= 1f)
            yield break;
        else if (rEnable == false && this.mLayerMixer.GetInputWeight(rLayer) <= 0f)
            yield break;

        // 在指定时间混合动画
        float waitTime = Time.time + rDuration;
        yield return new WaitWhile(() =>
        {
            float diff = waitTime - Time.time;
            float rate = Mathf.Clamp01(diff / rDuration);
            float weight = (rEnable) ? 1 - rate : rate;
            this.mLayerMixer.SetInputWeight(rLayer, weight);

            if (diff <= 0)
                return false;
            else
                return true;
        });
    }

    //瞬切设置层级
    public void RessetLayerWeight(int rLayerCount,bool rEnable/*开启或关闭*/)
    {
        Debug.Log(rEnable + " " + this.mLayerMixer.GetInputWeight(rLayerCount));
        // 如果已经启用/禁用则不处理
        if (rEnable && this.mLayerMixer.GetInputWeight(rLayerCount) >= 1f)
            return;
        else if (rEnable == false && this.mLayerMixer.GetInputWeight(rLayerCount) <= 0f)
            return;

        float rWeight = (rEnable) ? 1: 0;
        this.mLayerMixer.SetInputWeight(rLayerCount, rWeight);
        //0为默认层
        this.mLayerMixer.SetInputWeight(0, 1);
    }

    //重置所有层级
    public void RessetAllLayer()
    {
        for (int i = 0; i <this.mPlayableControllerParam.layerCount; i++)
            this.mLayerMixer.SetInputWeight(i, 0);
        //0为默认层
        this.mLayerMixer.SetInputWeight(0, 1);
    }
    #endregion

}
