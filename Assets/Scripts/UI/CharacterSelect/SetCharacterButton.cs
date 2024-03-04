using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class SetCharacterButton : NetworkBehaviour
{
    public void OnSelectCharacterButtonClick()
    {
        int characterId = (int)NetworkManager.Singleton.LocalClientId + 1;
        GameManager.Instance.SetCharacter(characterId);
    }
}
