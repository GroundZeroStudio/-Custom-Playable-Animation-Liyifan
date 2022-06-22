using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestController : MonoBehaviour
{
    public Toggle overrideToggle;
    public Toggle loopToggle;
    public Toggle reverseToggle;
    public Toggle waitToggle;
    public Toggle syncToggle;
    public Toggle simpleToggle;
    public Toggle LayerCrossfadeToggle;
    public InputField InputField;

    public Slider transSlider;

    public Slider layerTransSlider;

    const string ActorModleName = "TanZhiLang_001_L_";

    public List<AnimationClip> animationClips;

    public PlayableController mControllet;

    // Start is called before the first frame update
    void Start()
    {
        if (this.mControllet == null)
            this.mControllet = this.transform.GetComponent<PlayableController>();
    }

    public void SimplePlay(string rClipName)
    {
        this.ResetALLLayer();
        if (this.FindClipByName(rClipName, out AnimationClip rClip))
        {
            var rParam = new PlayableAnimationParam(0);
            this.mControllet.play(rClip,rParam);
        }
    }

    public void Play(string rClipName)
    {
        int.TryParse(InputField.text,out int rLayerCount);
        if (this.overrideToggle.isOn)
        {
            rLayerCount = 1;
        }
        var rParam = new PlayableAnimationParam(rLayerCount,this.loopToggle.isOn,this.reverseToggle.isOn,transSlider.value,this.waitToggle.isOn,this.syncToggle.isOn);

        if(this.FindClipByName(rClipName,out AnimationClip rClip))
        {
            if (this.LayerCrossfadeToggle.isOn)
                this.mControllet.CrossFadeLayer(rLayerCount,this.layerTransSlider.value, this.LayerCrossfadeToggle.isOn);

            this.mControllet.play(rClip, rParam);
        }
    }

    public void OnBtnClic(Button button)
    {
        if (this.simpleToggle.isOn)
            this.SimplePlay(ActorModleName + button.name);
        else
            this.Play(ActorModleName + button.name);
    }

    public bool FindClipByName(string rClipName,out AnimationClip rClip)
    {
        rClip = this.animationClips.Find(clip => clip.name == rClipName);
        if (rClip != null)
            return true;
        else
        {
            Debug.Log($"未找到Clips:{rClipName}");
            return false;
        }
    }

    public void ResetALLLayer()
    {
        this.mControllet.RessetAllLayer();   
    }
}
