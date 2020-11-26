using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAnimatedMenu
{
    void OnAnimationOutComplete();
    void OnAnimationInComplete();

    void AnimateIn();
    void AnimateOut();
}
