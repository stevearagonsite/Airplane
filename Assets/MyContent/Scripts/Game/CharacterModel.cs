using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PhotonAnimatorView))]
[RequireComponent(typeof(PhotonTransformView))]
[RequireComponent(typeof(PhotonNetwork))]
public class CharacterModel: MonoBehaviour {
    private PhotonView _photonView;
    private Animator _animator;
    private const string AnimatorAnimation01 = "animation01";
    private const string AnimatorAnimation02 = "animation02";
    private const string AnimatorAnimation03 = "animation03";
    private readonly string[] _containerAnimations = new[] {
        AnimatorAnimation01,
        AnimatorAnimation02,
        AnimatorAnimation03
    };
    
    public EntityPlayer Entity { get; set; }

    private void Start() {
        _photonView = gameObject.GetComponent<PhotonView>();
        _animator = gameObject.GetComponent<Animator>();
        ManagerUpdate.Instance.Execute += Execution;
        StartCoroutine("RandomAnimationIteration");
    }

    private IEnumerator RandomAnimationIteration() {
        while (true) {
            yield return new WaitForSeconds(3);
            SetRandomAnimation();
        }
    }

    private void SetRandomAnimation() {
        var randomIndex = Random.Range(0,_containerAnimations.Length);
        var randomAnimation = _containerAnimations[randomIndex];
        DisableAllAnimation();
        _animator.SetBool(randomAnimation, true);
    }

    private void DisableAllAnimation() {
        _animator.SetBool(AnimatorAnimation01, false);
        _animator.SetBool(AnimatorAnimation02, false);
        _animator.SetBool(AnimatorAnimation03, false);
    }

    private void OnDestroy() {
        ManagerUpdate.Instance.Execute -= Execution;
    }

    private void Execution() {
        if (!Entity || !Entity.characterPosition) return;
        transform.position = Entity.characterPosition.position;
        transform.rotation = Entity.characterPosition.rotation;
    }
}