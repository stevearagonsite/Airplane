using System;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PhotonAnimatorView))]
[RequireComponent(typeof(PhotonTransformView))]
[RequireComponent(typeof(PhotonNetwork))]
public class CharacterModel: MonoBehaviour {
    private PhotonView _photonView;
    private Animator _animator;
    public EntityPlayer Entity { get; set; }

    private void Start() {
        _photonView = gameObject.GetComponent<PhotonView>();
        _animator = gameObject.GetComponent<Animator>();
        ManagerUpdate.Instance.Execute += Execution;
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