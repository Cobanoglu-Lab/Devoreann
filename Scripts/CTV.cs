/* Copyright © 2019, University of Texas Southwestern Medical Center. All rights reserved.
 *  Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
 *  Department: Lyda Hill Department of Bioinformatics 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class CTV : MonoBehaviour {

    Animator animator;
    VideoPlayer video;

    private void Start()
    {
        animator = GetComponent<Animator>();
        video = GetComponentInChildren<VideoPlayer>();

        StartCoroutine(CheckIfVideoFinished());
    }

    IEnumerator CheckIfVideoFinished()
    {
        ulong numFrames = video.frameCount;

        while ((ulong)video.frame < numFrames-2)
        {
            if (OVRInput.Get(OVRInput.Button.Two) && OVRInput.Get(OVRInput.Button.Four))
            {
                animator.SetTrigger("bIsMovieFinished");
                yield break;
            }

            yield return new WaitForSeconds(0.05f);
        }
        animator.SetTrigger("bIsMovieFinished");
    }
}
