using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayableMixer
{
    // 播放时的参数
    public PlayableAnimationParam mPlayParam;

    //Playable
    public AnimationClipPlayable mPrePlayable;//上一个Playable

    public AnimationClipPlayable mCurPlayable;//当前Playable
    public AnimationMixerPlayable mMixer;
    private PlayableGraph mGraph;

    // 运动播放状态
    public bool isFinishedPlay
    {
        get
        {
            //如果 playable 未初始化
            if (mCurPlayable.IsValid() == false)
                return false;

            // 如果超过预定播放时间，则认为播放结束。
            if (mCurPlayable.GetTime() > mCurPlayable.GetAnimationClip().length)
                return true;

            return false;
        }
    }

    // 过渡状态
    public bool mIsFinishedTrans { get; private set; }

    // 协程相关
    private PlayableController mController;
    //过度携程
    private Coroutine mFadeCoroutine = null;

    //构造函数
    public PlayableMixer(PlayableController rController, PlayableGraph rGraph)
    {
        this.mController = rController;
        this.mGraph = rGraph;
        //设置默认参数
        this.mPlayParam = new PlayableAnimationParam(0);
        this.mPrePlayable = AnimationClipPlayable.Create(mGraph, null);
        this.mMixer = AnimationMixerPlayable.Create(mGraph, 2);
    }

    // 切换动画剪辑
    public void Reconnect(AnimationClip clip)
    {
        // 断开连接
        this.mGraph.Disconnect(mMixer, 0);
        this.mGraph.Disconnect(mMixer, 1);
        if (this.mPrePlayable.IsValid())
            this.mPrePlayable.Destroy();

        //更新
        this.mPrePlayable = this.mCurPlayable;
        this.mCurPlayable = AnimationClipPlayable.Create(this.mGraph, clip);

        // 重新连接
        this.mMixer.ConnectInput(1, this.mPrePlayable, 0);
        this.mMixer.ConnectInput(0, this.mCurPlayable, 0);

        if (this.mPlayParam.mReverse)
        {
            //倒放时间设置
            this.mCurPlayable.SetTime(mCurPlayable.GetAnimationClip().length);
            this.mCurPlayable.SetSpeed(-1f);
        }
        else
        {
            //正常时间设置
            this.mCurPlayable.SetSpeed(1f);
        }

        //同步转换时的时间设置
        if (this.mPlayParam.mSyncTrans)
            this.mCurPlayable.SetTime(mPrePlayable.GetTime());

        //如果有过渡协程就停止（防止重复的过渡协程运行）
        if (this.mFadeCoroutine != null)
            this.mController.StopCoroutine(this.mFadeCoroutine);

        //在过渡期间开始运动混合
        this.mFadeCoroutine = this.mController.StartCoroutine(this.CrossFadeCoroutine(this.mPlayParam.mFadeDuration, this.mPlayParam.mWaitFade));
    }

    //设置播放参数
    public void SetPlayableParam(PlayableAnimationParam rParam)
    {
        this.mPlayParam = rParam;
    }

    //动画过度
    private IEnumerator CrossFadeCoroutine(float duration, bool rWaitCrossFade = false)
    {
        this.mIsFinishedTrans = false;
        if (rWaitCrossFade)
            this.mCurPlayable.SetSpeed(0f); // 停止过渡目标的时间

        // 在指定时间混合动画
        float fWaitTime = Time.time + duration;
        yield return new WaitWhile(() =>
        {
            var rDiff = fWaitTime - Time.time;
            if (rDiff <= 0)
            {
                this.mMixer.SetInputWeight(1, 0);
                this.mMixer.SetInputWeight(0, 1);
                return false;
            }
            else
            {
                var rate = Mathf.Clamp01(rDiff / duration);
                this.mMixer.SetInputWeight(1, rate);
                this.mMixer.SetInputWeight(0, 1 - rate);
                return true;
            }
        });

        if (rWaitCrossFade)
        {
            float play_speed = mPlayParam.mReverse ? -1f : 1f;
            this.mCurPlayable.SetSpeed(play_speed); // 恢复转场目的地的动作播放时间
        }

        this.mIsFinishedTrans = true;
        this.mFadeCoroutine = null;
    }

}
