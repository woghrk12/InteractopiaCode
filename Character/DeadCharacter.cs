using UnityEngine;
using Photon.Pun;

public class DeadCharacter : MonoBehaviourPun
{
    #region Variables

    [SerializeField] private SpriteRenderer characterSprite = null;

    #endregion Variables

    #region Unity Events

    private void Awake()
    {
        Material material = Instantiate(characterSprite.material);
        characterSprite.material = material;
    }

    private void Start()
    {
        GameManager.InGame.DeadCharacterObjectDictionary.Add(photonView.Owner.ActorNumber, this);

        ECharacterColor color = (ECharacterColor)PhotonNetwork.CurrentRoom
            .Players[photonView.Owner.ActorNumber]
            .CustomProperties[PlayerProperties.PLYAER_COLOR];

        // Set player character's color
        characterSprite.material.SetColor(
            "_CharacterColor",
            CharacterColor.GetColor(color)
            );

        gameObject.SetActive(false);
    }

    #endregion Unity Events
}
