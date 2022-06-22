using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayableAnimationParam 
{
    public int mLayer = 0;

    //循环运动
    public bool mLoop = false;

    // 反向播放动作
    public bool mReverse = false;

    // 运动过渡所需的时间 [秒]
    public float mFadeDuration = 0.2f;

    // 转场的时候，播放转场前的运动和转场后的运动，不混合
    public bool mWaitFade = false;

    // 同步转场前动作的播放时间和转场后的动作播放时间
    public bool mSyncTrans = false;

    public PlayableAnimationParam(int layer_, bool loop_ = false, bool reverse_ = false, float fadeDuration_ = 0.2f, bool waitFade_ = false, bool syncTrans_ = false)
    {
        mLayer = layer_;
        mLoop = loop_;
        mReverse = reverse_;
        mFadeDuration = fadeDuration_;
        mWaitFade = waitFade_;
        mSyncTrans = syncTrans_;
    }
}
